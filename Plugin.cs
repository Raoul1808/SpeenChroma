using System;
using System.IO;
using BepInEx;
using HarmonyLib;
using MewsToolbox;

namespace SpeenChroma
{
    [BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
    public class Plugin : BaseUnityPlugin
    {
        public static IniFile ModConfig { get; private set; }
        public static readonly string ConfigDirPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "Speen Mods");
        public static readonly string ConfigFilePath = Path.Combine(ConfigDirPath, "SpeenChromaConfig.ini");
        
        private void Awake()
        {
            if (!Directory.Exists(ConfigDirPath))
                Directory.CreateDirectory(ConfigDirPath);
            if (!File.Exists(ConfigFilePath))
                File.Create(ConfigFilePath).Close();
            ModConfig = new IniFile(ConfigFilePath);
            
            Harmony harmony = new Harmony(PluginInfo.PLUGIN_GUID);
            harmony.PatchAll(typeof(ChromaPatches));
        }
    }
}
