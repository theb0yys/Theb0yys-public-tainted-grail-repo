using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json;
using UnityEngine;

namespace Tainted.Armour.FoA;

internal sealed class FoaKandraRuntimeRegistrationInvoker
{
    public const string StageVersion = "tainted-armour.foa.kandra-runtime-registration-invoker.v1";
    public const string ProbeGameObjectPrefix = "TaintedArmourKandraRuntimeRegistration_";

    private readonly Action<string> logInfo;
    private readonly Action<string> logWarning;

    public FoaKandraRuntimeRegistrationInvoker(Action<string> logInfo, Action<string> logWarning)
    {
        this.logInfo = logInfo ?? (_ => { });
        this.logWarning = logWarning ?? (_ => { });
    }

    public PendingInvocation Begin(
        KandraRuntimeRegistrationInvocationContext context,
        string templateRendererPathContains,
        int maxPollFrames)
    {
        if (context == null) throw new ArgumentNullException(nameof(context));

        var blockers = new List<string>();
        var warnings = new List<string>();
        ValidateContext(context, blockers);

        RuntimeSurface? surface = RuntimeSurface.TryResolve(blockers);
        if (blockers.Count != 0 || surface == null)
        {
            return PendingInvocation.Completed(CreateFailureAttempt(
                context,
                blockers,
                warnings,
                "blocked:runtime_surface_unavailable",
                "blocked:runtime_surface_unavailable"));
        }

        Component? template = FindTemplateRenderer(surface, templateRendererPathContains, blockers, warnings);
        if (template == null)
        {
            return PendingInvocation.Completed(CreateFailureAttempt(
                context,
                blockers,
                warnings,
                "blocked:template_renderer_unavailable",
                "blocked:template_renderer_unavailable"));
        }

        GameObject? probeObject = null;
        Component? probeRenderer = null;
        UnityEngine.Object? customMesh = null;
        try
        {
            object templateRendererData = surface.RendererDataField.GetValue(template);
            customMesh = CreateCustomMesh(surface, context, templateRendererData, blockers, warnings);
            if (blockers.Count != 0 || customMesh == null)
            {
                return PendingInvocation.Completed(CreateFailureAttempt(
                    context,
                    blockers,
                    warnings,
                    "blocked:custom_mesh_population_failed",
                    "blocked:custom_mesh_population_failed"));
            }

            string probeName = ProbeGameObjectPrefix + SanitizeObjectName(context.Name);
            probeObject = new GameObject(probeName);
            probeObject.SetActive(false);
            UnityEngine.Object.DontDestroyOnLoad(probeObject);

            probeRenderer = probeObject.AddComponent(surface.RendererType);
            object probeRendererData = surface.RendererDataCopyMethod.Invoke(templateRendererData, new object[] { probeObject });
            surface.RendererDataMeshField.SetValue(probeRendererData, customMesh);
            surface.RendererDataField.SetValue(probeRenderer, probeRendererData);

            probeObject.SetActive(true);
            uint renderingId = surface.ReadRenderingId(probeRenderer);
            warnings.Add("foa_kandra_runtime_registration_invoked_by_activation:KandraRenderer.OnEnable");
            logInfo("Tainted Armour queued custom Kandra runtime registration. RequestId=" + context.RequestId + "; RenderingId=" + renderingId.ToString(CultureInfo.InvariantCulture));

            int pollLimit = Math.Max(1, Math.Min(3600, maxPollFrames));
            return new PendingInvocation(context, surface, probeObject, probeRenderer, customMesh, pollLimit, warnings, logWarning);
        }
        catch (Exception exception)
        {
            blockers.Add("foa_kandra_runtime_registration_begin_exception:" + exception.GetType().Name);
            warnings.Add(exception.Message);
            DestroyProbe(probeObject, probeRenderer);
            if (customMesh != null)
            {
                UnityEngine.Object.Destroy(customMesh);
            }

            return PendingInvocation.Completed(CreateFailureAttempt(
                context,
                blockers,
                warnings,
                "blocked:begin_exception",
                "blocked:begin_exception"));
        }
    }

    private static void ValidateContext(KandraRuntimeRegistrationInvocationContext context, List<string> blockers)
    {
        if (context.VertexCount <= 0)
        {
            blockers.Add("foa_kandra_runtime_registration_vertex_count_missing");
        }
        else if (context.VertexCount > ushort.MaxValue)
        {
            blockers.Add("foa_kandra_runtime_registration_vertex_count_exceeds_kandra_ushort_limit:" + context.VertexCount.ToString(CultureInfo.InvariantCulture));
        }

        if (context.IndicesCount <= 0)
        {
            blockers.Add("foa_kandra_runtime_registration_index_count_missing");
        }
        else if (context.IndicesCount > uint.MaxValue)
        {
            blockers.Add("foa_kandra_runtime_registration_index_count_exceeds_kandra_uint_limit:" + context.IndicesCount.ToString(CultureInfo.InvariantCulture));
        }

        if (context.BindposesCount < 0)
        {
            blockers.Add("foa_kandra_runtime_registration_bindpose_count_negative");
        }
        else if (context.BindposesCount > ushort.MaxValue)
        {
            blockers.Add("foa_kandra_runtime_registration_bindpose_count_exceeds_kandra_ushort_limit:" + context.BindposesCount.ToString(CultureInfo.InvariantCulture));
        }

        if (context.BlendshapeCount < 0)
        {
            blockers.Add("foa_kandra_runtime_registration_blendshape_count_negative");
        }
        else if (context.BlendshapeCount > ushort.MaxValue)
        {
            blockers.Add("foa_kandra_runtime_registration_blendshape_count_exceeds_kandra_ushort_limit:" + context.BlendshapeCount.ToString(CultureInfo.InvariantCulture));
        }
    }

    private static Component? FindTemplateRenderer(
        RuntimeSurface surface,
        string selector,
        List<string> blockers,
        List<string> warnings)
    {
        string trimmedSelector = selector?.Trim() ?? string.Empty;
        UnityEngine.Object[] objects = Resources.FindObjectsOfTypeAll(surface.RendererType);
        var candidates = new List<Component>();

        foreach (UnityEngine.Object obj in objects)
        {
            if (obj is not Component component || !component)
            {
                continue;
            }

            GameObject gameObject = component.gameObject;
            if (!gameObject || !gameObject.scene.IsValid() || !gameObject.activeInHierarchy)
            {
                continue;
            }

            if (gameObject.name.StartsWith(ProbeGameObjectPrefix, StringComparison.Ordinal))
            {
                continue;
            }

            if (!HasUsableRendererData(surface, component))
            {
                continue;
            }

            string path = BuildPath(component.transform);
            if (trimmedSelector.Length == 0 ||
                path.IndexOf(trimmedSelector, StringComparison.OrdinalIgnoreCase) >= 0 ||
                gameObject.name.IndexOf(trimmedSelector, StringComparison.OrdinalIgnoreCase) >= 0)
            {
                candidates.Add(component);
            }
        }

        if (candidates.Count == 1)
        {
            warnings.Add("foa_kandra_runtime_registration_template_renderer:" + BuildPath(candidates[0].transform));
            return candidates[0];
        }

        if (trimmedSelector.Length == 0)
        {
            blockers.Add("foa_kandra_runtime_registration_template_selector_required:candidates=" + candidates.Count.ToString(CultureInfo.InvariantCulture));
        }
        else if (candidates.Count == 0)
        {
            blockers.Add("foa_kandra_runtime_registration_template_renderer_not_found:" + trimmedSelector);
        }
        else
        {
            blockers.Add("foa_kandra_runtime_registration_template_renderer_ambiguous:" + trimmedSelector + ":matches=" + candidates.Count.ToString(CultureInfo.InvariantCulture));
        }

        return null;
    }

    private static bool HasUsableRendererData(RuntimeSurface surface, Component renderer)
    {
        try
        {
            object rendererData = surface.RendererDataField.GetValue(renderer);
            if (surface.RendererDataRigField.GetValue(rendererData) == null ||
                surface.RendererDataMeshField.GetValue(rendererData) == null)
            {
                return false;
            }

            if (surface.RendererDataMaterialsField.GetValue(rendererData) is not Array materials || materials.Length == 0)
            {
                return false;
            }

            if (surface.RendererDataBonesField.GetValue(rendererData) is not Array bones || bones.Length == 0)
            {
                return false;
            }

            return true;
        }
        catch
        {
            return false;
        }
    }

    private static UnityEngine.Object? CreateCustomMesh(
        RuntimeSurface surface,
        KandraRuntimeRegistrationInvocationContext context,
        object templateRendererData,
        List<string> blockers,
        List<string> warnings)
    {
        object? templateMesh = surface.RendererDataMeshField.GetValue(templateRendererData);
        if (templateMesh == null)
        {
            blockers.Add("foa_kandra_runtime_registration_template_mesh_missing");
            return null;
        }

        var customMesh = (UnityEngine.Object)ScriptableObject.CreateInstance(surface.MeshType);
        customMesh.name = context.Name;

        surface.SetMeshField(customMesh, "modDirectory", context.ModDirectory);
        surface.SetOptionalMeshField(customMesh, "_name", context.Name);
        surface.SetMeshField(customMesh, "vertexCount", checked((ushort)context.VertexCount));
        surface.SetMeshField(customMesh, "indicesCount", checked((uint)context.IndicesCount));
        surface.SetMeshField(customMesh, "bindposesCount", checked((ushort)context.BindposesCount));

        CopyMeshField(surface, templateMesh, customMesh, "meshLocalBounds", blockers);
        CopyMeshField(surface, templateMesh, customMesh, "localBoundingSphere", blockers);
        CopyMeshField(surface, templateMesh, customMesh, "reciprocalUvDistribution", blockers);

        Array submeshes = Array.CreateInstance(surface.SubmeshDataType, 1);
        object submesh = Activator.CreateInstance(surface.SubmeshDataType);
        surface.SubmeshIndexStartField.SetValue(submesh, 0u);
        surface.SubmeshIndexCountField.SetValue(submesh, checked((uint)context.IndicesCount));
        submeshes.SetValue(submesh, 0);
        surface.SetMeshField(customMesh, "submeshes", submeshes);
        warnings.Add("foa_kandra_runtime_registration_single_submesh:indexStart=0:indexCount=" + context.IndicesCount.ToString(CultureInfo.InvariantCulture));

        string[] blendshapeNames = Enumerable.Range(0, context.BlendshapeCount)
            .Select(index => context.Name + "_blendshape_" + index.ToString("0000", CultureInfo.InvariantCulture))
            .ToArray();
        surface.SetMeshField(customMesh, "blendshapesNames", blendshapeNames);
        if (blendshapeNames.Length != 0)
        {
            warnings.Add("foa_kandra_runtime_registration_generated_blendshape_names:count=" + blendshapeNames.Length.ToString(CultureInfo.InvariantCulture));
        }

        if (blockers.Count == 0)
        {
            return customMesh;
        }

        UnityEngine.Object.Destroy(customMesh);
        return null;
    }

    private static void CopyMeshField(RuntimeSurface surface, object sourceMesh, object targetMesh, string fieldName, List<string> blockers)
    {
        FieldInfo? field = surface.FindMeshField(fieldName);
        if (field == null)
        {
            blockers.Add("foa_kandra_runtime_registration_mesh_field_missing:" + fieldName);
            return;
        }

        field.SetValue(targetMesh, field.GetValue(sourceMesh));
    }

    private static KandraRuntimeRegistrationInvocationAttempt CreateFailureAttempt(
        KandraRuntimeRegistrationInvocationContext context,
        IEnumerable<string> blockers,
        IEnumerable<string> warnings,
        string isRegisteredStatus,
        string tryGetMeshMemoryStatus) =>
        new KandraRuntimeRegistrationInvocationAttempt(
            registrationMethodInvoked: false,
            canRegisterMethodsInvoked: false,
            createdOrActivatedKandraObject: false,
            runtimeRegistrationExecuted: false,
            runtimeRegistrationSucceeded: false,
            kandraIsRegisteredStatus: isRegisteredStatus,
            kandraTryGetMeshMemoryStatus: tryGetMeshMemoryStatus,
            registeredRendererIdentity: string.Empty,
            registeredMeshMemoryIdentity: string.Empty,
            blockers: blockers,
            warnings: warnings.Append("foa_kandra_runtime_registration_context:" + context.ModDirectory + "/" + context.Name));

    private static void DestroyProbe(GameObject? probeObject, Component? probeRenderer)
    {
        try
        {
            if (probeRenderer != null)
            {
                probeRenderer.gameObject.SetActive(false);
            }
        }
        catch
        {
        }

        if (probeObject != null)
        {
            UnityEngine.Object.Destroy(probeObject);
        }
    }

    private static string BuildPath(Transform transform)
    {
        var names = new Stack<string>();
        Transform? current = transform;
        while (current != null)
        {
            names.Push(current.name);
            current = current.parent;
        }

        return "/" + string.Join("/", names.ToArray());
    }

    private static string SanitizeObjectName(string value)
    {
        char[] chars = value.ToCharArray();
        for (int i = 0; i < chars.Length; i++)
        {
            if (char.IsControl(chars[i]) || chars[i] == '/' || chars[i] == '\\' || chars[i] == ':')
            {
                chars[i] = '_';
            }
        }

        return new string(chars);
    }

    internal sealed class PendingInvocation : IDisposable
    {
        private readonly KandraRuntimeRegistrationInvocationContext? context;
        private readonly RuntimeSurface? surface;
        private readonly GameObject? probeObject;
        private readonly Component? probeRenderer;
        private readonly object? customMesh;
        private readonly int maxPollFrames;
        private readonly List<string> warnings;
        private readonly Action<string> logWarning;
        private readonly KandraRuntimeRegistrationInvocationAttempt? immediateAttempt;
        private int pollFrames;
        private bool completed;

        internal PendingInvocation(
            KandraRuntimeRegistrationInvocationContext context,
            RuntimeSurface surface,
            GameObject probeObject,
            Component probeRenderer,
            object customMesh,
            int maxPollFrames,
            IEnumerable<string> warnings,
            Action<string> logWarning)
        {
            this.context = context;
            this.surface = surface;
            this.probeObject = probeObject;
            this.probeRenderer = probeRenderer;
            this.customMesh = customMesh;
            this.maxPollFrames = maxPollFrames;
            this.warnings = warnings.ToList();
            this.logWarning = logWarning;
        }

        private PendingInvocation(KandraRuntimeRegistrationInvocationAttempt immediateAttempt)
        {
            this.immediateAttempt = immediateAttempt;
            warnings = new List<string>();
            logWarning = _ => { };
        }

        public static PendingInvocation Completed(KandraRuntimeRegistrationInvocationAttempt attempt) =>
            new PendingInvocation(attempt);

        public bool TryComplete(out KandraRuntimeRegistrationInvocationAttempt? attempt)
        {
            if (immediateAttempt != null)
            {
                attempt = immediateAttempt;
                return true;
            }

            attempt = null;
            if (completed || context == null || surface == null || probeRenderer == null || customMesh == null)
            {
                return false;
            }

            pollFrames++;
            string isRegisteredStatus = surface.ReadIsRegisteredStatus(probeRenderer, out bool isRegistered);
            string meshMemoryStatus = surface.ReadTryGetMeshMemoryStatus(customMesh, out bool meshMemoryFound, out string meshMemoryIdentity);
            string rendererIdentity = BuildPath(probeRenderer.transform) + ":renderingId=" + surface.ReadRenderingId(probeRenderer).ToString(CultureInfo.InvariantCulture);

            if (isRegistered && meshMemoryFound)
            {
                completed = true;
                attempt = new KandraRuntimeRegistrationInvocationAttempt(
                    registrationMethodInvoked: true,
                    canRegisterMethodsInvoked: false,
                    createdOrActivatedKandraObject: true,
                    runtimeRegistrationExecuted: true,
                    runtimeRegistrationSucceeded: true,
                    kandraIsRegisteredStatus: isRegisteredStatus,
                    kandraTryGetMeshMemoryStatus: meshMemoryStatus,
                    registeredRendererIdentity: rendererIdentity,
                    registeredMeshMemoryIdentity: meshMemoryIdentity,
                    blockers: Array.Empty<string>(),
                    warnings: warnings);
                return true;
            }

            if (pollFrames >= maxPollFrames)
            {
                completed = true;
                var blockers = new[]
                {
                    "foa_kandra_runtime_registration_proof_timeout:frames=" + pollFrames.ToString(CultureInfo.InvariantCulture),
                };
                attempt = new KandraRuntimeRegistrationInvocationAttempt(
                    registrationMethodInvoked: true,
                    canRegisterMethodsInvoked: false,
                    createdOrActivatedKandraObject: true,
                    runtimeRegistrationExecuted: true,
                    runtimeRegistrationSucceeded: false,
                    kandraIsRegisteredStatus: isRegisteredStatus,
                    kandraTryGetMeshMemoryStatus: meshMemoryStatus,
                    registeredRendererIdentity: rendererIdentity,
                    registeredMeshMemoryIdentity: meshMemoryIdentity,
                    blockers: blockers,
                    warnings: warnings);
                Dispose();
                return true;
            }

            return false;
        }

        public void Dispose()
        {
            if (probeRenderer == null)
            {
                return;
            }

            try
            {
                probeRenderer.gameObject.SetActive(false);
            }
            catch (Exception exception)
            {
                logWarning("Tainted Armour Kandra probe deactivate failed. Exception=" + exception.GetType().Name + "; Message=" + exception.Message);
            }

            try
            {
                if (probeObject != null)
                {
                    UnityEngine.Object.Destroy(probeObject);
                }
            }
            catch (Exception exception)
            {
                logWarning("Tainted Armour Kandra probe destroy failed. Exception=" + exception.GetType().Name + "; Message=" + exception.Message);
            }
        }
    }

    internal sealed class RuntimeSurface
    {
        private RuntimeSurface(
            Type rendererType,
            Type meshType,
            Type submeshDataType,
            FieldInfo rendererDataField,
            FieldInfo rendererDataMeshField,
            FieldInfo rendererDataRigField,
            FieldInfo rendererDataMaterialsField,
            FieldInfo rendererDataBonesField,
            MethodInfo rendererDataCopyMethod,
            PropertyInfo renderingIdProperty,
            object managerInstance,
            MethodInfo isRegisteredMethod,
            PropertyInfo meshManagerProperty,
            MethodInfo tryGetMeshMemoryMethod,
            FieldInfo submeshIndexStartField,
            FieldInfo submeshIndexCountField)
        {
            RendererType = rendererType;
            MeshType = meshType;
            SubmeshDataType = submeshDataType;
            RendererDataField = rendererDataField;
            RendererDataMeshField = rendererDataMeshField;
            RendererDataRigField = rendererDataRigField;
            RendererDataMaterialsField = rendererDataMaterialsField;
            RendererDataBonesField = rendererDataBonesField;
            RendererDataCopyMethod = rendererDataCopyMethod;
            RenderingIdProperty = renderingIdProperty;
            ManagerInstance = managerInstance;
            IsRegisteredMethod = isRegisteredMethod;
            MeshManagerProperty = meshManagerProperty;
            TryGetMeshMemoryMethod = tryGetMeshMemoryMethod;
            SubmeshIndexStartField = submeshIndexStartField;
            SubmeshIndexCountField = submeshIndexCountField;
        }

        public Type RendererType { get; }

        public Type MeshType { get; }

        public Type SubmeshDataType { get; }

        public FieldInfo RendererDataField { get; }

        public FieldInfo RendererDataMeshField { get; }

        public FieldInfo RendererDataRigField { get; }

        public FieldInfo RendererDataMaterialsField { get; }

        public FieldInfo RendererDataBonesField { get; }

        public MethodInfo RendererDataCopyMethod { get; }

        public PropertyInfo RenderingIdProperty { get; }

        public object ManagerInstance { get; }

        public MethodInfo IsRegisteredMethod { get; }

        public PropertyInfo MeshManagerProperty { get; }

        public MethodInfo TryGetMeshMemoryMethod { get; }

        public FieldInfo SubmeshIndexStartField { get; }

        public FieldInfo SubmeshIndexCountField { get; }

        public static RuntimeSurface? TryResolve(List<string> blockers)
        {
            Type? rendererType = FindType("Awaken.Kandra.KandraRenderer");
            Type? meshType = FindType("Awaken.Kandra.KandraMesh");
            Type? submeshDataType = FindType("Awaken.Kandra.SubmeshData");
            Type? managerType = FindType("Awaken.Kandra.KandraRendererManager");

            if (rendererType == null) blockers.Add("foa_kandra_runtime_registration_type_missing:Awaken.Kandra.KandraRenderer");
            if (meshType == null) blockers.Add("foa_kandra_runtime_registration_type_missing:Awaken.Kandra.KandraMesh");
            if (submeshDataType == null) blockers.Add("foa_kandra_runtime_registration_type_missing:Awaken.Kandra.SubmeshData");
            if (managerType == null) blockers.Add("foa_kandra_runtime_registration_type_missing:Awaken.Kandra.KandraRendererManager");
            if (rendererType == null || meshType == null || submeshDataType == null || managerType == null)
            {
                return null;
            }

            FieldInfo? rendererDataField = FindField(rendererType, "rendererData");
            PropertyInfo? renderingIdProperty = rendererType.GetProperty("RenderingId", BindingFlags.Public | BindingFlags.Instance);
            if (rendererDataField == null) blockers.Add("foa_kandra_runtime_registration_renderer_data_field_missing");
            if (renderingIdProperty == null) blockers.Add("foa_kandra_runtime_registration_rendering_id_property_missing");
            if (rendererDataField == null || renderingIdProperty == null)
            {
                return null;
            }

            Type rendererDataType = rendererDataField.FieldType;
            FieldInfo? rendererDataMeshField = FindField(rendererDataType, "mesh");
            FieldInfo? rendererDataRigField = FindField(rendererDataType, "rig");
            FieldInfo? rendererDataMaterialsField = FindField(rendererDataType, "materials");
            FieldInfo? rendererDataBonesField = FindField(rendererDataType, "bones");
            MethodInfo? rendererDataCopyMethod = rendererDataType.GetMethod(
                "Copy",
                BindingFlags.Public | BindingFlags.Instance,
                null,
                new[] { typeof(GameObject) },
                null);

            if (rendererDataMeshField == null) blockers.Add("foa_kandra_runtime_registration_renderer_data_member_missing:mesh");
            if (rendererDataRigField == null) blockers.Add("foa_kandra_runtime_registration_renderer_data_member_missing:rig");
            if (rendererDataMaterialsField == null) blockers.Add("foa_kandra_runtime_registration_renderer_data_member_missing:materials");
            if (rendererDataBonesField == null) blockers.Add("foa_kandra_runtime_registration_renderer_data_member_missing:bones");
            if (rendererDataCopyMethod == null) blockers.Add("foa_kandra_runtime_registration_renderer_data_copy_method_missing");

            PropertyInfo? instanceProperty = managerType.GetProperty("Instance", BindingFlags.Public | BindingFlags.Static);
            object? managerInstance = instanceProperty?.GetValue(null);
            MethodInfo? isRegisteredMethod = managerType.GetMethod(
                "IsRegistered",
                BindingFlags.Public | BindingFlags.Instance,
                null,
                new[] { typeof(uint) },
                null);
            PropertyInfo? meshManagerProperty = managerType.GetProperty("MeshManager", BindingFlags.Public | BindingFlags.Instance);
            object? meshManager = meshManagerProperty?.GetValue(managerInstance);
            MethodInfo? tryGetMeshMemoryMethod = meshManager == null
                ? null
                : meshManager.GetType().GetMethods(BindingFlags.Public | BindingFlags.Instance)
                    .FirstOrDefault(method =>
                    {
                        if (method.Name != "TryGetMeshMemory")
                        {
                            return false;
                        }

                        ParameterInfo[] parameters = method.GetParameters();
                        return parameters.Length == 2 && parameters[0].ParameterType.IsAssignableFrom(meshType) && parameters[1].IsOut;
                    });

            if (managerInstance == null) blockers.Add("foa_kandra_runtime_registration_manager_instance_missing");
            if (isRegisteredMethod == null) blockers.Add("foa_kandra_runtime_registration_is_registered_method_missing");
            if (meshManagerProperty == null || meshManager == null) blockers.Add("foa_kandra_runtime_registration_mesh_manager_missing");
            if (tryGetMeshMemoryMethod == null) blockers.Add("foa_kandra_runtime_registration_try_get_mesh_memory_method_missing");

            FieldInfo? submeshIndexStartField = FindField(submeshDataType, "indexStart");
            FieldInfo? submeshIndexCountField = FindField(submeshDataType, "indexCount");
            if (submeshIndexStartField == null) blockers.Add("foa_kandra_runtime_registration_submesh_field_missing:indexStart");
            if (submeshIndexCountField == null) blockers.Add("foa_kandra_runtime_registration_submesh_field_missing:indexCount");

            if (blockers.Count != 0 ||
                rendererDataMeshField == null ||
                rendererDataRigField == null ||
                rendererDataMaterialsField == null ||
                rendererDataBonesField == null ||
                rendererDataCopyMethod == null ||
                managerInstance == null ||
                isRegisteredMethod == null ||
                meshManagerProperty == null ||
                tryGetMeshMemoryMethod == null ||
                submeshIndexStartField == null ||
                submeshIndexCountField == null)
            {
                return null;
            }

            return new RuntimeSurface(
                rendererType,
                meshType,
                submeshDataType,
                rendererDataField,
                rendererDataMeshField,
                rendererDataRigField,
                rendererDataMaterialsField,
                rendererDataBonesField,
                rendererDataCopyMethod,
                renderingIdProperty,
                managerInstance,
                isRegisteredMethod,
                meshManagerProperty,
                tryGetMeshMemoryMethod,
                submeshIndexStartField,
                submeshIndexCountField);
        }

        public FieldInfo? FindMeshField(string name) =>
            FindField(MeshType, name);

        public void SetMeshField(object mesh, string fieldName, object value)
        {
            FieldInfo? field = FindMeshField(fieldName);
            if (field == null)
            {
                throw new MissingFieldException(MeshType.FullName, fieldName);
            }

            field.SetValue(mesh, value);
        }

        public void SetOptionalMeshField(object mesh, string fieldName, object value)
        {
            FieldInfo? field = FindMeshField(fieldName);
            field?.SetValue(mesh, value);
        }

        public uint ReadRenderingId(Component renderer)
        {
            object? value = RenderingIdProperty.GetValue(renderer);
            return value == null ? 0u : Convert.ToUInt32(value, CultureInfo.InvariantCulture);
        }

        public string ReadIsRegisteredStatus(Component renderer, out bool observed)
        {
            uint renderingId = ReadRenderingId(renderer);
            object? value = IsRegisteredMethod.Invoke(ManagerInstance, new object[] { renderingId });
            observed = value is bool flag && flag;
            return "observed:" + observed.ToString(CultureInfo.InvariantCulture) + ":renderingId=" + renderingId.ToString(CultureInfo.InvariantCulture);
        }

        public string ReadTryGetMeshMemoryStatus(object mesh, out bool observed, out string identity)
        {
            object meshManager = MeshManagerProperty.GetValue(ManagerInstance);
            object?[] args = { mesh, null };
            object? value = TryGetMeshMemoryMethod.Invoke(meshManager, args);
            observed = value is bool flag && flag;
            identity = observed && args[1] != null
                ? args[1]!.GetType().FullName + "#" + args[1]!.GetHashCode().ToString(CultureInfo.InvariantCulture)
                : string.Empty;
            return "observed:" + observed.ToString(CultureInfo.InvariantCulture) + (identity.Length == 0 ? string.Empty : ":meshMemory=" + identity);
        }

        private static Type? FindType(string fullName)
        {
            foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                Type? type = assembly.GetType(fullName, throwOnError: false);
                if (type != null)
                {
                    return type;
                }
            }

            return null;
        }

        private static FieldInfo? FindField(Type type, string name) =>
            type.GetField(name, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
    }
}

internal sealed class KandraRuntimeRegistrationInvocationContextPacket
{
    public string RequestId { get; set; } = string.Empty;

    public string DryRunRequestId { get; set; } = string.Empty;

    public string ApprovalId { get; set; } = string.Empty;

    public string ModDirectory { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public int VertexCount { get; set; }

    public long IndicesCount { get; set; }

    public int BindposesCount { get; set; }

    public int BlendshapeCount { get; set; }

    public string LayoutVersion { get; set; } = string.Empty;

    public string KandraDirectory { get; set; } = string.Empty;

    public string MeshDataPath { get; set; } = string.Empty;

    public string IndicesDataPath { get; set; } = string.Empty;

    public long MeshDataByteCount { get; set; }

    public long IndicesDataByteCount { get; set; }

    public string MeshDataSha256 { get; set; } = string.Empty;

    public string IndicesDataSha256 { get; set; } = string.Empty;

    public string RegistrationOwner { get; set; } = string.Empty;

    public string RegistrationMethodSignature { get; set; } = string.Empty;

    public string RegistrationMethodFingerprint { get; set; } = string.Empty;

    public string RegistrationAssemblySha256 { get; set; } = string.Empty;

    public static KandraRuntimeRegistrationInvocationContextPacket From(KandraRuntimeRegistrationInvocationContext context) =>
        new KandraRuntimeRegistrationInvocationContextPacket
        {
            RequestId = context.RequestId,
            DryRunRequestId = context.DryRunRequestId,
            ApprovalId = context.ApprovalId,
            ModDirectory = context.ModDirectory,
            Name = context.Name,
            VertexCount = context.VertexCount,
            IndicesCount = context.IndicesCount,
            BindposesCount = context.BindposesCount,
            BlendshapeCount = context.BlendshapeCount,
            LayoutVersion = context.LayoutVersion,
            KandraDirectory = context.KandraDirectory,
            MeshDataPath = context.MeshDataPath,
            IndicesDataPath = context.IndicesDataPath,
            MeshDataByteCount = context.MeshDataByteCount,
            IndicesDataByteCount = context.IndicesDataByteCount,
            MeshDataSha256 = context.MeshDataSha256,
            IndicesDataSha256 = context.IndicesDataSha256,
            RegistrationOwner = context.RegistrationOwner,
            RegistrationMethodSignature = context.RegistrationMethodSignature,
            RegistrationMethodFingerprint = context.RegistrationMethodFingerprint,
            RegistrationAssemblySha256 = context.RegistrationAssemblySha256,
        };

    public KandraRuntimeRegistrationInvocationContext ToContext() =>
        new KandraRuntimeRegistrationInvocationContext(
            RequestId,
            DryRunRequestId,
            ApprovalId,
            ModDirectory,
            Name,
            VertexCount,
            IndicesCount,
            BindposesCount,
            BlendshapeCount,
            LayoutVersion,
            KandraDirectory,
            MeshDataPath,
            IndicesDataPath,
            MeshDataByteCount,
            IndicesDataByteCount,
            MeshDataSha256,
            IndicesDataSha256,
            RegistrationOwner,
            RegistrationMethodSignature,
            RegistrationMethodFingerprint,
            RegistrationAssemblySha256);
}

internal sealed class KandraRuntimeRegistrationHostReceiptPacket
{
    public string StageVersion { get; set; } = FoaKandraRuntimeRegistrationInvoker.StageVersion;

    public string WrittenAtUtc { get; set; } = string.Empty;

    public string Trigger { get; set; } = string.Empty;

    public KandraRuntimeRegistrationInvocationContextPacket Context { get; set; } = new KandraRuntimeRegistrationInvocationContextPacket();

    public KandraRuntimeRegistrationInvocationAttempt? Attempt { get; set; }

    public bool CandidateMapApplicationExecuted { get; set; }

    public bool ConversionExecuted { get; set; }

    public bool ItemEquipMutationExecuted { get; set; }

    public bool SaveMutationExecuted { get; set; }

    public bool NativeGameWriteExecuted { get; set; }

    public bool NonRegistrationDownstreamWritesExecuted { get; set; }

    public static KandraRuntimeRegistrationHostReceiptPacket From(
        string trigger,
        KandraRuntimeRegistrationInvocationContext context,
        KandraRuntimeRegistrationInvocationAttempt attempt) =>
        new KandraRuntimeRegistrationHostReceiptPacket
        {
            WrittenAtUtc = DateTime.UtcNow.ToString("O", CultureInfo.InvariantCulture),
            Trigger = trigger,
            Context = KandraRuntimeRegistrationInvocationContextPacket.From(context),
            Attempt = attempt,
            CandidateMapApplicationExecuted = false,
            ConversionExecuted = false,
            ItemEquipMutationExecuted = false,
            SaveMutationExecuted = false,
            NativeGameWriteExecuted = false,
            NonRegistrationDownstreamWritesExecuted = false,
        };

    public string ToJson() =>
        JsonConvert.SerializeObject(this, Formatting.Indented);
}
