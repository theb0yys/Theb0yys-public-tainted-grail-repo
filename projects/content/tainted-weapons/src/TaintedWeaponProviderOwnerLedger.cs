using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;

namespace TaintedWeapons;

internal sealed class TaintedWeaponProviderOwnerLedger
{
    private readonly Dictionary<string, PackageOwner> _packages = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, BundleOwner> _bundles = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, AssetOwner> _assets = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, DependencyOwner> _dependencies = new(StringComparer.OrdinalIgnoreCase);

    internal IReadOnlyList<TaintedWeaponProviderOwnerLedgerRow> BuildRows(string label)
    {
        var rows = new List<TaintedWeaponProviderOwnerLedgerRow>();
        rows.AddRange(_packages.Values
            .OrderBy(owner => owner.OwnerId, StringComparer.OrdinalIgnoreCase)
            .Select(owner => owner.ToRow(label)));
        rows.AddRange(_bundles.Values
            .OrderBy(owner => owner.OwnerId, StringComparer.OrdinalIgnoreCase)
            .Select(owner => owner.ToRow(label)));
        rows.AddRange(_assets.Values
            .OrderBy(owner => owner.OwnerId, StringComparer.OrdinalIgnoreCase)
            .Select(owner => owner.ToRow(label)));
        rows.AddRange(_dependencies.Values
            .OrderBy(owner => owner.OwnerId, StringComparer.OrdinalIgnoreCase)
            .Select(owner => owner.ToRow(label)));
        return rows;
    }

    internal void RecordPackageMount(TaintedWeaponDefinition definition, string bundlePath)
    {
        string packageId = Normalize(definition.PackageId);
        if (string.IsNullOrWhiteSpace(packageId))
        {
            return;
        }

        if (!_packages.TryGetValue(packageId, out PackageOwner owner))
        {
            owner = new PackageOwner(packageId, definition);
        }

        owner = owner.WithMount(bundlePath);
        _packages[packageId] = owner;
    }

    internal void RecordBundleLoaded(TaintedWeaponDefinition definition, string bundlePath, int assetNameCount)
    {
        string ownerId = BundleOwnerId(definition, bundlePath);
        if (!_bundles.TryGetValue(ownerId, out BundleOwner owner))
        {
            owner = new BundleOwner(ownerId, definition, bundlePath);
        }

        owner = owner.WithLoad(assetNameCount);
        _bundles[ownerId] = owner;
    }

    internal void RecordAssetLoaded(TaintedWeaponDefinition definition, string bundlePath, GameObject prefab)
    {
        if (prefab == null)
        {
            return;
        }

        string ownerId = AssetOwnerId(definition, bundlePath, definition.EquippedPrefabAssetPath);
        if (!_assets.TryGetValue(ownerId, out AssetOwner owner))
        {
            owner = new AssetOwner(ownerId, definition, bundlePath, definition.EquippedPrefabAssetPath);
        }

        owner = owner.WithLoad(prefab.GetInstanceID(), ComponentProfileHash(prefab));
        _assets[ownerId] = owner;
    }

    internal void RecordAssetConsumerReleased(TaintedWeaponDefinition definition, string bundlePath)
    {
        string ownerId = AssetOwnerId(definition, bundlePath, definition.EquippedPrefabAssetPath);
        if (_assets.TryGetValue(ownerId, out AssetOwner owner))
        {
            _assets[ownerId] = owner.WithConsumerReleased();
        }
    }

    internal void RecordDependencyClosure(TaintedWeaponDefinition definition, string dependencyId)
    {
        string ownerId = DependencyOwnerId(definition, dependencyId);
        if (!_dependencies.TryGetValue(ownerId, out DependencyOwner owner))
        {
            owner = new DependencyOwner(ownerId, definition, dependencyId);
        }

        _dependencies[ownerId] = owner.WithResolved();
    }

    internal bool RecordEarlyBundleReleaseDenied(string bundlePath, out string reason)
    {
        int activeChildCount = _assets.Values.Count(owner =>
            string.Equals(owner.BundlePath, bundlePath, StringComparison.OrdinalIgnoreCase) &&
            owner.ActiveConsumerCount > 0 &&
            !owner.Released);
        if (activeChildCount <= 0)
        {
            reason = "no-active-child-owner";
            return false;
        }

        foreach (string ownerId in _bundles.Keys.ToArray())
        {
            BundleOwner owner = _bundles[ownerId];
            if (string.Equals(owner.BundlePath, bundlePath, StringComparison.OrdinalIgnoreCase))
            {
                _bundles[ownerId] = owner.WithReleaseBlocked();
            }
        }

        reason = "activeChildCount=" + activeChildCount.ToString(CultureInfo.InvariantCulture);
        return true;
    }

    internal void RecordBundleReleaseRequested(string bundlePath)
    {
        foreach (string ownerId in _packages.Keys.ToArray())
        {
            PackageOwner owner = _packages[ownerId];
            if (string.Equals(owner.LastBundlePath, bundlePath, StringComparison.OrdinalIgnoreCase))
            {
                _packages[ownerId] = owner.WithReleaseRequested();
            }
        }

        foreach (string ownerId in _bundles.Keys.ToArray())
        {
            BundleOwner owner = _bundles[ownerId];
            if (string.Equals(owner.BundlePath, bundlePath, StringComparison.OrdinalIgnoreCase))
            {
                _bundles[ownerId] = owner.WithReleaseRequested();
            }
        }

        foreach (string ownerId in _assets.Keys.ToArray())
        {
            AssetOwner owner = _assets[ownerId];
            if (string.Equals(owner.BundlePath, bundlePath, StringComparison.OrdinalIgnoreCase))
            {
                _assets[ownerId] = owner.WithAllConsumersReleased();
            }
        }

        foreach (string ownerId in _dependencies.Keys.ToArray())
        {
            DependencyOwner owner = _dependencies[ownerId];
            if (PackageHasBundle(owner.PackageId, bundlePath))
            {
                _dependencies[ownerId] = owner.WithReleaseRequested();
            }
        }
    }

    internal void RecordBundleUnloaded(string bundlePath)
    {
        foreach (string ownerId in _packages.Keys.ToArray())
        {
            PackageOwner owner = _packages[ownerId];
            if (string.Equals(owner.LastBundlePath, bundlePath, StringComparison.OrdinalIgnoreCase))
            {
                _packages[ownerId] = owner.WithReleased();
            }
        }

        foreach (string ownerId in _bundles.Keys.ToArray())
        {
            BundleOwner owner = _bundles[ownerId];
            if (string.Equals(owner.BundlePath, bundlePath, StringComparison.OrdinalIgnoreCase))
            {
                _bundles[ownerId] = owner.WithUnloaded();
            }
        }

        foreach (string ownerId in _dependencies.Keys.ToArray())
        {
            DependencyOwner owner = _dependencies[ownerId];
            if (PackageHasBundle(owner.PackageId, bundlePath))
            {
                _dependencies[ownerId] = owner.WithReleased();
            }
        }
    }

    internal void Clear()
    {
        _packages.Clear();
        _bundles.Clear();
        _assets.Clear();
        _dependencies.Clear();
    }

    private static string BundleOwnerId(TaintedWeaponDefinition definition, string bundlePath)
    {
        return Normalize(definition.PackageId) + "::bundle::" + Normalize(bundlePath);
    }

    private static string AssetOwnerId(TaintedWeaponDefinition definition, string bundlePath, string assetPath)
    {
        return Normalize(definition.PackageId) + "::asset::" + Normalize(bundlePath) + "::" + Normalize(assetPath);
    }

    private static string DependencyOwnerId(TaintedWeaponDefinition definition, string dependencyId)
    {
        return Normalize(definition.PackageId) + "::dependency::" + Normalize(dependencyId);
    }

    private bool PackageHasBundle(string packageId, string bundlePath)
    {
        return _packages.Values.Any(owner =>
            string.Equals(owner.PackageId, packageId, StringComparison.OrdinalIgnoreCase) &&
            string.Equals(owner.LastBundlePath, bundlePath, StringComparison.OrdinalIgnoreCase));
    }

    private static string ComponentProfileHash(GameObject prefab)
    {
        var builder = new StringBuilder();
        foreach (Component component in prefab.GetComponentsInChildren<Component>(true)
                     .Where(component => component != null)
                     .OrderBy(component => TransformPath(component.transform), StringComparer.Ordinal)
                     .ThenBy(component => component.GetType().FullName ?? component.GetType().Name, StringComparer.Ordinal))
        {
            builder
                .Append(TransformPath(component.transform))
                .Append('|')
                .Append(component.GetType().FullName ?? component.GetType().Name)
                .Append('\n');
        }

        using SHA256 sha256 = SHA256.Create();
        byte[] hash = sha256.ComputeHash(Encoding.UTF8.GetBytes(builder.ToString()));
        return string.Concat(hash.Select(value => value.ToString("x2", CultureInfo.InvariantCulture)));
    }

    private static string TransformPath(Transform transform)
    {
        var names = new Stack<string>();
        for (Transform? current = transform; current != null; current = current.parent)
        {
            names.Push(current.name);
        }

        return "/" + string.Join("/", names);
    }

    private static string Normalize(string value)
    {
        return (value ?? string.Empty).Trim();
    }

    private readonly struct PackageOwner
    {
        private readonly int _mountCount;
        private readonly string _lastBundlePath;
        private readonly bool _releaseRequested;
        private readonly bool _released;

        internal PackageOwner(string ownerId, TaintedWeaponDefinition definition)
        {
            OwnerId = ownerId;
            PackageId = definition.PackageId;
            WeaponId = definition.WeaponId;
            _mountCount = 0;
            _lastBundlePath = string.Empty;
            _releaseRequested = false;
            _released = false;
        }

        internal string OwnerId { get; }

        internal string PackageId { get; }

        private string WeaponId { get; }

        internal string LastBundlePath => _lastBundlePath;

        internal PackageOwner WithMount(string bundlePath)
        {
            return new PackageOwner(OwnerId, PackageId, WeaponId, _mountCount + 1, bundlePath, releaseRequested: false, released: false);
        }

        internal PackageOwner WithReleaseRequested()
        {
            return new PackageOwner(OwnerId, PackageId, WeaponId, _mountCount, _lastBundlePath, releaseRequested: true, _released);
        }

        internal PackageOwner WithReleased()
        {
            return new PackageOwner(OwnerId, PackageId, WeaponId, _mountCount, _lastBundlePath, _releaseRequested, released: true);
        }

        private PackageOwner(string ownerId, string packageId, string weaponId, int mountCount, string lastBundlePath, bool releaseRequested, bool released)
        {
            OwnerId = ownerId;
            PackageId = packageId;
            WeaponId = weaponId;
            _mountCount = mountCount;
            _lastBundlePath = lastBundlePath;
            _releaseRequested = releaseRequested;
            _released = released;
        }

        internal TaintedWeaponProviderOwnerLedgerRow ToRow(string label)
        {
            string state = _released ? "released" : "mounted";
            return new TaintedWeaponProviderOwnerLedgerRow(
                label,
                "package",
                OwnerId,
                PackageId,
                WeaponId,
                _lastBundlePath,
                string.Empty,
                string.Empty,
                string.Empty,
                state,
                _released ? 0 : _mountCount,
                _released,
                _releaseRequested && !_released,
                "packageMountId=" + OwnerId + "; mountCount=" + _mountCount.ToString(CultureInfo.InvariantCulture));
        }
    }

    private readonly struct BundleOwner
    {
        private readonly int _mountCount;
        private readonly int _assetNameCount;
        private readonly bool _releaseRequested;
        private readonly bool _releaseBlocked;
        private readonly bool _released;

        internal BundleOwner(string ownerId, TaintedWeaponDefinition definition, string bundlePath)
        {
            OwnerId = ownerId;
            PackageId = definition.PackageId;
            WeaponId = definition.WeaponId;
            BundlePath = bundlePath;
            _mountCount = 0;
            _assetNameCount = 0;
            _releaseRequested = false;
            _releaseBlocked = false;
            _released = false;
        }

        internal string OwnerId { get; }

        internal string BundlePath { get; }

        private string PackageId { get; }

        private string WeaponId { get; }

        internal BundleOwner WithLoad(int assetNameCount)
        {
            return new BundleOwner(OwnerId, PackageId, WeaponId, BundlePath, _mountCount + 1, assetNameCount, releaseRequested: false, releaseBlocked: false, released: false);
        }

        internal BundleOwner WithReleaseRequested()
        {
            return new BundleOwner(OwnerId, PackageId, WeaponId, BundlePath, _mountCount, _assetNameCount, releaseRequested: true, _releaseBlocked, _released);
        }

        internal BundleOwner WithReleaseBlocked()
        {
            return new BundleOwner(OwnerId, PackageId, WeaponId, BundlePath, _mountCount, _assetNameCount, _releaseRequested, releaseBlocked: true, _released);
        }

        internal BundleOwner WithUnloaded()
        {
            return new BundleOwner(OwnerId, PackageId, WeaponId, BundlePath, _mountCount, _assetNameCount, _releaseRequested, _releaseBlocked, released: true);
        }

        private BundleOwner(string ownerId, string packageId, string weaponId, string bundlePath, int mountCount, int assetNameCount, bool releaseRequested, bool releaseBlocked, bool released)
        {
            OwnerId = ownerId;
            PackageId = packageId;
            WeaponId = weaponId;
            BundlePath = bundlePath;
            _mountCount = mountCount;
            _assetNameCount = assetNameCount;
            _releaseRequested = releaseRequested;
            _releaseBlocked = releaseBlocked;
            _released = released;
        }

        internal TaintedWeaponProviderOwnerLedgerRow ToRow(string label)
        {
            string state = _released ? "unloaded" : "mounted";
            return new TaintedWeaponProviderOwnerLedgerRow(
                label,
                "bundle",
                OwnerId,
                PackageId,
                WeaponId,
                BundlePath,
                string.Empty,
                "UnityEngine.AssetBundle",
                string.Empty,
                state,
                _released ? 0 : _mountCount,
                _released,
                (_releaseRequested || _releaseBlocked) && !_released,
                "bundleOwner=" + OwnerId +
                "; mountCount=" + _mountCount.ToString(CultureInfo.InvariantCulture) +
                "; assetNameCount=" + _assetNameCount.ToString(CultureInfo.InvariantCulture));
        }
    }

    private readonly struct AssetOwner
    {
        private readonly int _loadCount;
        private readonly int _activeConsumerCount;
        private readonly int _lastInstanceId;
        private readonly string _componentProfileHash;
        private readonly bool _consumerReleased;

        internal AssetOwner(string ownerId, TaintedWeaponDefinition definition, string bundlePath, string assetPath)
        {
            OwnerId = ownerId;
            PackageId = definition.PackageId;
            WeaponId = definition.WeaponId;
            BundlePath = bundlePath;
            AssetPath = assetPath;
            _loadCount = 0;
            _activeConsumerCount = 0;
            _lastInstanceId = 0;
            _componentProfileHash = string.Empty;
            _consumerReleased = false;
        }

        internal string OwnerId { get; }

        internal string BundlePath { get; }

        internal int ActiveConsumerCount => _consumerReleased ? 0 : _activeConsumerCount;

        internal bool Released => _consumerReleased;

        private string PackageId { get; }

        private string WeaponId { get; }

        private string AssetPath { get; }

        internal AssetOwner WithLoad(int instanceId, string componentProfileHash)
        {
            int activeConsumerCount = _consumerReleased ? 1 : _activeConsumerCount + 1;
            return new AssetOwner(OwnerId, PackageId, WeaponId, BundlePath, AssetPath, _loadCount + 1, activeConsumerCount, instanceId, componentProfileHash, consumerReleased: false);
        }

        internal AssetOwner WithConsumerReleased()
        {
            int remainingConsumerCount = Math.Max(_activeConsumerCount - 1, 0);
            return new AssetOwner(
                OwnerId,
                PackageId,
                WeaponId,
                BundlePath,
                AssetPath,
                _loadCount,
                remainingConsumerCount,
                _lastInstanceId,
                _componentProfileHash,
                consumerReleased: remainingConsumerCount == 0);
        }

        internal AssetOwner WithAllConsumersReleased()
        {
            return new AssetOwner(
                OwnerId,
                PackageId,
                WeaponId,
                BundlePath,
                AssetPath,
                _loadCount,
                0,
                _lastInstanceId,
                _componentProfileHash,
                consumerReleased: true);
        }

        private AssetOwner(string ownerId, string packageId, string weaponId, string bundlePath, string assetPath, int loadCount, int activeConsumerCount, int lastInstanceId, string componentProfileHash, bool consumerReleased)
        {
            OwnerId = ownerId;
            PackageId = packageId;
            WeaponId = weaponId;
            BundlePath = bundlePath;
            AssetPath = assetPath;
            _loadCount = loadCount;
            _activeConsumerCount = activeConsumerCount;
            _lastInstanceId = lastInstanceId;
            _componentProfileHash = componentProfileHash;
            _consumerReleased = consumerReleased;
        }

        internal TaintedWeaponProviderOwnerLedgerRow ToRow(string label)
        {
            return new TaintedWeaponProviderOwnerLedgerRow(
                label,
                "asset",
                OwnerId,
                PackageId,
                WeaponId,
                BundlePath,
                AssetPath,
                "UnityEngine.GameObject",
                _componentProfileHash,
                _consumerReleased ? "released" : "loaded",
                ActiveConsumerCount,
                _consumerReleased,
                releaseBlocked: false,
                "assetPath=" + AssetPath +
                "; loadedAssetId=" + _lastInstanceId.ToString(CultureInfo.InvariantCulture) +
                "; duplicateLoadCount=" + _loadCount.ToString(CultureInfo.InvariantCulture));
        }
    }

    private readonly struct DependencyOwner
    {
        private readonly string _dependencyId;
        private readonly bool _releaseRequested;
        private readonly bool _released;

        internal DependencyOwner(string ownerId, TaintedWeaponDefinition definition, string dependencyId)
        {
            OwnerId = ownerId;
            PackageId = definition.PackageId;
            WeaponId = definition.WeaponId;
            _dependencyId = dependencyId;
            _releaseRequested = false;
            _released = false;
        }

        internal string OwnerId { get; }

        internal string PackageId { get; }

        private string WeaponId { get; }

        internal DependencyOwner WithResolved()
        {
            return new DependencyOwner(OwnerId, PackageId, WeaponId, _dependencyId, releaseRequested: false, released: false);
        }

        internal DependencyOwner WithReleaseRequested()
        {
            return new DependencyOwner(OwnerId, PackageId, WeaponId, _dependencyId, releaseRequested: true, _released);
        }

        internal DependencyOwner WithReleased()
        {
            return new DependencyOwner(OwnerId, PackageId, WeaponId, _dependencyId, _releaseRequested, released: true);
        }

        private DependencyOwner(string ownerId, string packageId, string weaponId, string dependencyId, bool releaseRequested, bool released)
        {
            OwnerId = ownerId;
            PackageId = packageId;
            WeaponId = weaponId;
            _dependencyId = dependencyId;
            _releaseRequested = releaseRequested;
            _released = released;
        }

        internal TaintedWeaponProviderOwnerLedgerRow ToRow(string label)
        {
            return new TaintedWeaponProviderOwnerLedgerRow(
                label,
                "dependency",
                OwnerId,
                PackageId,
                WeaponId,
                string.Empty,
                _dependencyId,
                "provider-dependency",
                string.Empty,
                _released ? "released" : "resolved",
                _released ? 0 : 1,
                _released,
                _releaseRequested && !_released,
                "declaredDependency=" + _dependencyId + "; resolvedDependency=" + _dependencyId);
        }
    }
}

internal readonly struct TaintedWeaponProviderOwnerLedgerRow
{
    internal const string Headers = "label\townerKind\townerId\tpackageId\tweaponId\tbundlePath\tassetPath\tassetType\tcomponentProfileHash\townerState\tconsumerCount\treleased\treleaseBlocked\tdetails";

    internal TaintedWeaponProviderOwnerLedgerRow(
        string label,
        string ownerKind,
        string ownerId,
        string packageId,
        string weaponId,
        string bundlePath,
        string assetPath,
        string assetType,
        string componentProfileHash,
        string ownerState,
        int consumerCount,
        bool released,
        bool releaseBlocked,
        string details)
    {
        Label = label;
        OwnerKind = ownerKind;
        OwnerId = ownerId;
        PackageId = packageId;
        WeaponId = weaponId;
        BundlePath = bundlePath;
        AssetPath = assetPath;
        AssetType = assetType;
        ComponentProfileHash = componentProfileHash;
        OwnerState = ownerState;
        ConsumerCount = consumerCount;
        Released = released;
        ReleaseBlocked = releaseBlocked;
        Details = details;
    }

    internal string Label { get; }

    internal string OwnerKind { get; }

    internal string OwnerId { get; }

    internal string PackageId { get; }

    internal string WeaponId { get; }

    internal string BundlePath { get; }

    internal string AssetPath { get; }

    internal string AssetType { get; }

    internal string ComponentProfileHash { get; }

    internal string OwnerState { get; }

    internal int ConsumerCount { get; }

    internal bool Released { get; }

    internal bool ReleaseBlocked { get; }

    internal string Details { get; }

    internal string ToTsv()
    {
        return string.Join("\t", new[]
        {
            Tsv(Label),
            Tsv(OwnerKind),
            Tsv(OwnerId),
            Tsv(PackageId),
            Tsv(WeaponId),
            Tsv(BundlePath),
            Tsv(AssetPath),
            Tsv(AssetType),
            Tsv(ComponentProfileHash),
            Tsv(OwnerState),
            ConsumerCount.ToString(CultureInfo.InvariantCulture),
            Released.ToString(CultureInfo.InvariantCulture),
            ReleaseBlocked.ToString(CultureInfo.InvariantCulture),
            Tsv(Details)
        });
    }

    private static string Tsv(string value)
    {
        return (value ?? string.Empty)
            .Replace('\r', ' ')
            .Replace('\n', ' ')
            .Replace('\t', ' ');
    }
}
