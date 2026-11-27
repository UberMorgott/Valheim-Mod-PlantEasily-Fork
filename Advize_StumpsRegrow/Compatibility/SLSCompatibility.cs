namespace Advize_StumpsRegrow;

using System;
using BepInEx;
using BepInEx.Bootstrap;
using BepInEx.Configuration;
using static StumpsRegrow;

static class SLSCompatibility
{
    private const string GUID = "MidnightsFX.StarLevelSystem";

    private static bool _isInitialized;
    private static bool _enableTreeScaling = false;
    private static float _treeSizeScalePerLevel = 0f;
    private static ConfigEntry<bool> _enableTreeScalingEntry;
    private static ConfigEntry<float> _treeSizeScalePerLevelEntry;

    internal static bool TreeScalingEnabled
    {
        get
        {
            if (!_isInitialized)
                Initialize();

            return _enableTreeScaling && _treeSizeScalePerLevel != 0;
        }
    }

    private static void Initialize()
    {
        _isInitialized = true;

        if (!Chainloader.PluginInfos.TryGetValue(GUID, out PluginInfo StarLevelSystem)) return;

        ConfigFile SLSConfig = StarLevelSystem.Instance.Config;

        if (SLSConfig.TryGetEntry(new("LevelSystem", "EnableTreeScaling"), out ConfigEntry<bool> enableTreeScaling))
        {
            _enableTreeScalingEntry = enableTreeScaling;
            _enableTreeScaling = enableTreeScaling.Value;

            enableTreeScaling.SettingChanged += OnEnableTreeScalingChanged;
        }

        if (SLSConfig.TryGetEntry(new("LevelSystem", "TreeSizeScalePerLevel"), out ConfigEntry<float> treeSizeScalePerLevel))
        {
            _treeSizeScalePerLevelEntry = treeSizeScalePerLevel;
            _treeSizeScalePerLevel = treeSizeScalePerLevel.Value;

            treeSizeScalePerLevel.SettingChanged += OnTreeSizeScalePerLevelChanged;
        }
    }

    private static void OnEnableTreeScalingChanged(object sender, EventArgs e)
    {
        _enableTreeScaling = _enableTreeScalingEntry.Value;
        Dbgl($"SLS changed EnableTreeScaling to {_enableTreeScaling.ToString()}");
    }

    private static void OnTreeSizeScalePerLevelChanged(object sender, EventArgs e)
    {
        _treeSizeScalePerLevel = _treeSizeScalePerLevelEntry.Value;
        Dbgl($"SLS changed TreeSizeScalePerLevel to {_treeSizeScalePerLevel.ToString()}");
    }
    internal static float GetScaleFactor(int level)
    {
        return 1f + _treeSizeScalePerLevel * level;
    }
}
