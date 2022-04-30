using System;
using System.IO;
using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using MewsToolbox;
using SpinCore;

namespace SpeenChroma
{
    [BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
    public class Plugin : SpinPlugin
    {
        public static IniFile ModConfig { get; private set; }
        public static readonly string ConfigDirPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "Speen Mods");
        public static readonly string ConfigFilePath = Path.Combine(ConfigDirPath, "SpeenChromaConfig.ini");

        private static ManualLogSource _logger;

        protected override void Awake()
        {
            _logger = Logger;
            if (!Directory.Exists(ConfigDirPath))
                Directory.CreateDirectory(ConfigDirPath);
            if (!File.Exists(ConfigFilePath))
                File.Create(ConfigFilePath).Close();
            ModConfig = new IniFile(ConfigFilePath);
            
            Harmony harmony = new Harmony(PluginInfo.PLUGIN_GUID);
            harmony.PatchAll(typeof(ChromaPatches));
            ChromaPatches.InitializeConfigFields();
            base.Awake();
        }

        protected override void CreateMenus()
        {
            ChromaUI.CreateOptionsTab(CreateOptionsTab("Speen Chroma"));
        }

        public static void Log(LogLevel level, object msg) => _logger.Log(level, msg);
        public static void LogInfo(object msg) => Log(LogLevel.Info, msg);
        public static void LogDebug(object msg) => Log(LogLevel.Debug, msg);
        public static void LogMessage(object msg) => Log(LogLevel.Message, msg);
        public static void LogWarning(object msg) => Log(LogLevel.Warning, msg);
        public static void LogError(object msg) => Log(LogLevel.Error, msg);
        public static void LogFatal(object msg) => Log(LogLevel.Fatal, msg);
    }
}
