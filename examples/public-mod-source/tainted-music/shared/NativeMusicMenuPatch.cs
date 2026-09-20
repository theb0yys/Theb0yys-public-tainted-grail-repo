using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using BepInEx.Logging;
using HarmonyLib;
using UnityEngine;

namespace TaintedMusic;

internal static class NativeMusicMenuPatch
{
    private const string ButtonObjectName = "TaintedMusicMenuButton";
    private const string ButtonText = "Tainted Music";

    private static ManualLogSource? s_logger;

    internal static void Apply(Harmony harmony, ManualLogSource logger)
    {
        s_logger = logger;
        PatchMenuType(harmony, logger, "Awaken.TG.Main.UI.Menu.VMenuUI");
        PatchMenuType(harmony, logger, "Awaken.TG.Main.UI.TitleScreen.VTitleScreenUI");
    }

    private static void PatchMenuType(Harmony harmony, ManualLogSource logger, string typeName)
    {
        Type? viewType = SafeTypeByName(typeName);
        MethodInfo? target = viewType == null ? null : AccessTools.Method(viewType, "OnInitialize");
        MethodInfo? postfix = AccessTools.Method(typeof(NativeMusicMenuPatch), nameof(MenuInitializePostfix));
        if (viewType == null || target == null || postfix == null)
        {
            logger.LogWarning($"Could not patch {typeName}.OnInitialize for the separate Tainted Music menu.");
            return;
        }

        harmony.Patch(target, postfix: new HarmonyMethod(postfix));
        logger.LogInfo($"Patched {typeName}.OnInitialize for the separate Tainted Music menu.");
    }

    private static Type? SafeTypeByName(string typeName)
    {
        foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
        {
            try
            {
                Type? type = assembly.GetType(typeName, throwOnError: false, ignoreCase: false);
                if (type != null)
                {
                    return type;
                }
            }
            catch (Exception ex) when (ex is TypeLoadException || ex is ReflectionTypeLoadException || ex is BadImageFormatException)
            {
                // Generated IL2CPP assemblies can contain unrelated metadata that cannot be loaded.
            }
        }

        return null;
    }

    private static void MenuInitializePostfix(object __instance)
    {
        AddMusicButton(__instance);
    }

    private static void AddMusicButton(object view)
    {
        try
        {
            if (view == null)
            {
                return;
            }

            Type viewType = view.GetType();
            Component? optionsComponent = FindMenuButtonComponent(view, viewType);
            if (optionsComponent == null || optionsComponent.gameObject == null)
            {
                s_logger?.LogWarning($"{viewType.Name} options/settings button was unavailable. The separate Tainted Music button was not added.");
                return;
            }

            Transform? parent = optionsComponent.transform.parent;
            if (parent == null || parent.Find(ButtonObjectName) != null)
            {
                return;
            }

            GameObject buttonObject = UnityEngine.Object.Instantiate(optionsComponent.gameObject, parent);
            buttonObject.name = ButtonObjectName;
            buttonObject.transform.SetSiblingIndex(optionsComponent.transform.GetSiblingIndex() + 1);

            Component? musicButton = buttonObject.GetComponent(optionsComponent.GetIl2CppType()) as Component;
            if (musicButton == null)
            {
                UnityEngine.Object.Destroy(buttonObject);
                s_logger?.LogWarning($"{viewType.Name} cloned menu button is missing the expected component.");
                return;
            }

            ClearButtonEvents(musicButton);
            InvokeInitializeButton(musicButton, viewType.Name);
            s_logger?.LogInfo($"Tainted Music separate native menu button added. View={viewType.Name}; Renderer=Tainted Interface.");
        }
        catch (Exception ex)
        {
            s_logger?.LogWarning($"Failed to add the separate Tainted Music menu button: {ex.GetType().Name}: {ex.Message}");
        }
    }

    private static Component? FindMenuButtonComponent(object view, Type viewType)
    {
        foreach (string fieldName in new[] { "options", "settings", "optionsButton", "settingsButton" })
        {
            Component? exact = ReadFieldComponent(view, viewType, fieldName);
            if (IsUsableButtonComponent(exact))
            {
                return exact;
            }
        }

        foreach (MemberInfo member in EnumerateViewMembers(viewType, optionNamesOnly: true))
        {
            Component? component = ReadMemberComponent(view, member);
            if (IsUsableButtonComponent(component))
            {
                return component;
            }
        }

        foreach (MemberInfo member in EnumerateViewMembers(viewType, optionNamesOnly: false))
        {
            Component? component = ReadMemberComponent(view, member);
            if (IsUsableButtonComponent(component))
            {
                return component;
            }
        }

        return null;
    }

    private static Component? ReadFieldComponent(object view, Type viewType, string fieldName)
    {
        try
        {
            return viewType
                .GetField(fieldName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                ?.GetValue(view) as Component;
        }
        catch
        {
            return null;
        }
    }

    private static IEnumerable<MemberInfo> EnumerateViewMembers(Type viewType, bool optionNamesOnly)
    {
        BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
        for (Type? current = viewType; current != null; current = current.BaseType)
        {
            FieldInfo[] fields;
            PropertyInfo[] properties;
            try
            {
                fields = current.GetFields(flags | BindingFlags.DeclaredOnly);
                properties = current.GetProperties(flags | BindingFlags.DeclaredOnly);
            }
            catch
            {
                continue;
            }

            foreach (FieldInfo field in fields)
            {
                if (IsCandidateButtonMember(field.Name, field.FieldType, optionNamesOnly))
                {
                    yield return field;
                }
            }

            foreach (PropertyInfo property in properties)
            {
                if (property.GetIndexParameters().Length == 0 &&
                    IsCandidateButtonMember(property.Name, property.PropertyType, optionNamesOnly))
                {
                    yield return property;
                }
            }
        }
    }

    private static bool IsCandidateButtonMember(string memberName, Type memberType, bool optionNamesOnly)
    {
        bool optionLike = memberName.IndexOf("option", StringComparison.OrdinalIgnoreCase) >= 0 ||
                          memberName.IndexOf("setting", StringComparison.OrdinalIgnoreCase) >= 0;
        bool buttonLike = (memberType.FullName ?? string.Empty).IndexOf("ButtonConfig", StringComparison.OrdinalIgnoreCase) >= 0 ||
                          typeof(Component).IsAssignableFrom(memberType);
        return buttonLike && (!optionNamesOnly || optionLike);
    }

    private static Component? ReadMemberComponent(object view, MemberInfo member)
    {
        try
        {
            return member switch
            {
                FieldInfo field => field.GetValue(view) as Component,
                PropertyInfo property => property.GetValue(view) as Component,
                _ => null
            };
        }
        catch
        {
            return null;
        }
    }

    private static bool IsUsableButtonComponent(Component? component)
    {
        return component != null && component.gameObject != null && component.GetType()
            .GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
            .Any(IsInitializeButtonMethod);
    }

    private static bool IsInitializeButtonMethod(MethodInfo method)
    {
        ParameterInfo[] parameters = method.GetParameters();
        return method.Name == "InitializeButton" && parameters.Length == 2 && parameters[1].ParameterType == typeof(string);
    }

    private static void ClearButtonEvents(Component buttonConfig)
    {
        FieldInfo? buttonField = buttonConfig.GetType().GetField("button", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        object? button = buttonField?.GetValue(buttonConfig);
        button?.GetType()
            .GetMethod("ClearAllOnClickEvents", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
            ?.Invoke(button, Array.Empty<object>());
    }

    private static void InvokeInitializeButton(Component buttonConfig, string source)
    {
        MethodInfo? initialize = buttonConfig.GetType()
            .GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
            .FirstOrDefault(IsInitializeButtonMethod);
        if (initialize == null)
        {
            return;
        }

        initialize.Invoke(buttonConfig, new object[] { (Action)(() => Plugin.ShowMenuFromNativeMenu(source)), ButtonText });
    }
}
