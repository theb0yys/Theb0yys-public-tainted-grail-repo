using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using Awaken.TG.Main.Heroes;
using BepInEx;
using UnityEngine;

namespace TGCommunity.Example.SidecarPinbook;

[BepInPlugin(PluginGuid, PluginName, PluginVersion)]
public sealed class Plugin : BaseUnityPlugin
{
    public const string PluginGuid = "community.taintedgrail.example.sidecar-pinbook";
    public const string PluginName = "TG Example - Sidecar Pinbook";
    public const string PluginVersion = "0.1.0";

    private const string FileName = PluginGuid + ".pins.tsv";
    private readonly List<Pin> _pins = new();
    private string _path = string.Empty;
    private bool _showHud = true;

    private void Awake()
    {
        _path = Path.Combine(Paths.ConfigPath, FileName);
        LoadPins();
        Logger.LogInfo($"{PluginName} loaded. F6 save pin, F7 toggle HUD, F9 delete nearest.");
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.F6))
        {
            SaveCurrentPosition();
        }

        if (Input.GetKeyDown(KeyCode.F7))
        {
            _showHud = !_showHud;
        }

        if (Input.GetKeyDown(KeyCode.F9))
        {
            DeleteNearest();
        }
    }

    private void OnGUI()
    {
        if (!_showHud || Hero.Current == null)
        {
            return;
        }

        Vector3 current = Hero.Current.Coords;
        float y = 80f;

        GUI.Box(new Rect(Screen.width - 330f, 40f, 300f, 38f + Math.Min(8, _pins.Count) * 24f), "Pinbook");

        foreach (Pin pin in NearestPins(current, 8))
        {
            float distance = Vector3.Distance(current, pin.Position);
            GUI.Label(new Rect(Screen.width - 315f, y, 270f, 22f), $"{pin.Name}  {distance:0}m");
            y += 24f;
        }
    }

    private void SaveCurrentPosition()
    {
        Hero? hero = Hero.Current;
        if (hero == null)
        {
            Logger.LogWarning("Cannot save pin: Hero.Current unavailable.");
            return;
        }

        var pin = new Pin($"Pin {_pins.Count + 1}", hero.Coords);
        _pins.Add(pin);

        if (SavePins())
        {
            Logger.LogInfo($"Saved {pin.Name} at {pin.Position}.");
        }
    }

    private void DeleteNearest()
    {
        Hero? hero = Hero.Current;
        if (hero == null || _pins.Count == 0)
        {
            return;
        }

        int nearest = 0;
        float nearestDistance = float.MaxValue;

        for (int i = 0; i < _pins.Count; i++)
        {
            float distance = (_pins[i].Position - hero.Coords).sqrMagnitude;
            if (distance < nearestDistance)
            {
                nearest = i;
                nearestDistance = distance;
            }
        }

        string name = _pins[nearest].Name;
        _pins.RemoveAt(nearest);
        SavePins();
        Logger.LogInfo($"Deleted nearest pin: {name}.");
    }

    private IEnumerable<Pin> NearestPins(Vector3 current, int limit)
    {
        var ordered = new List<Pin>(_pins);
        ordered.Sort((left, right) =>
            (left.Position - current).sqrMagnitude.CompareTo((right.Position - current).sqrMagnitude));

        for (int i = 0; i < ordered.Count && i < limit; i++)
        {
            yield return ordered[i];
        }
    }

    private void LoadPins()
    {
        _pins.Clear();

        if (!File.Exists(_path))
        {
            return;
        }

        foreach (string line in File.ReadAllLines(_path))
        {
            if (TryParse(line, out Pin? pin) && pin != null)
            {
                _pins.Add(pin);
            }
        }
    }

    private bool SavePins()
    {
        string temp = _path + ".tmp";

        try
        {
            Directory.CreateDirectory(Paths.ConfigPath);
            var lines = new List<string>(_pins.Count);

            foreach (Pin pin in _pins)
            {
                lines.Add(Format(pin));
            }

            File.WriteAllLines(temp, lines, Encoding.UTF8);

            if (File.Exists(_path))
            {
                string backup = _path + ".bak";
                File.Replace(temp, _path, backup, ignoreMetadataErrors: true);
                if (File.Exists(backup))
                {
                    File.Delete(backup);
                }
            }
            else
            {
                File.Move(temp, _path);
            }

            return true;
        }
        catch (Exception ex)
        {
            if (File.Exists(temp))
            {
                File.Delete(temp);
            }

            Logger.LogWarning($"Pin save failed: {ex.GetType().Name}: {ex.Message}");
            return false;
        }
    }

    private static string Format(Pin pin)
    {
        return string.Join(
            "\t",
            "v1",
            Convert.ToBase64String(Encoding.UTF8.GetBytes(pin.Name)),
            pin.Position.x.ToString("R", CultureInfo.InvariantCulture),
            pin.Position.y.ToString("R", CultureInfo.InvariantCulture),
            pin.Position.z.ToString("R", CultureInfo.InvariantCulture));
    }

    private static bool TryParse(string line, out Pin? pin)
    {
        pin = null;
        string[] parts = line.Split('\t');

        if (parts.Length != 5 || parts[0] != "v1")
        {
            return false;
        }

        try
        {
            string name = Encoding.UTF8.GetString(Convert.FromBase64String(parts[1]));
            float x = float.Parse(parts[2], NumberStyles.Float, CultureInfo.InvariantCulture);
            float y = float.Parse(parts[3], NumberStyles.Float, CultureInfo.InvariantCulture);
            float z = float.Parse(parts[4], NumberStyles.Float, CultureInfo.InvariantCulture);
            pin = new Pin(name, new Vector3(x, y, z));
            return true;
        }
        catch
        {
            return false;
        }
    }

    private sealed class Pin
    {
        internal Pin(string name, Vector3 position)
        {
            Name = name;
            Position = position;
        }

        internal string Name { get; }
        internal Vector3 Position { get; }
    }
}
