using BepInEx;
using BepInEx.IL2CPP;
using BepInEx.IL2CPP.UnityEngine;
using HarmonyLib;
using MewsToolbox;
using System;
using System.Collections.Generic;
using System.IO;

namespace SpeenChroma
{
    [BepInPlugin(MOD_ID, MOD_NAME, MOD_VERSION)]
    public class ChromaMod : BasePlugin
    {
        public const string MOD_ID = "ChromaMod";
        public const string MOD_NAME = "Speen Chroma";
        public const string MOD_VERSION = "1.0.1";

        public static string ConfigFilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "Speen Mods", "SpeenChromaSettings.ini");

        public static BepInEx.Logging.ManualLogSource Logger;
        public static IniFile ModConfig;

        public override void Load()
        {
            Logger = BepInEx.Logging.Logger.CreateLogSource(MOD_ID);

            if (!Directory.Exists(ConfigFilePath)) Directory.CreateDirectory(Directory.GetParent(ConfigFilePath).FullName);
            if (!File.Exists(ConfigFilePath)) GenerateDefaultConfigFile();

            ModConfig = new IniFile(ConfigFilePath);
            InitializeModSettings();

            Logger.LogMessage("Rainbow speed set to: " + Patches.RainbowSpeed);

            Harmony.CreateAndPatchAll(typeof(ChromaMod.Patches));
        }

        private void InitializeModSettings()
        {
            Patches.RainbowSpeed = ModConfig.GetValueOrDefaultTo("Rainbow", "Speed", 1);
            if (Patches.RainbowSpeed <= 0) Patches.RainbowSpeed = 1;
            Patches.EnabledRainbow = ModConfig.GetValueOrDefaultTo("Rainbow", "Enabled", true);
            var noteTypes = ModConfig.GetValueOrDefaultTo("Rainbow", "NoteTypes", "111111");
            Patches.EnabledBlenders = noteTypes.ToCharArray();
            if (Patches.EnabledBlenders.Length != 6) Patches.EnabledBlenders = new char[] { '1', '1', '1', '1', '1', '1'};
        }

        private void GenerateDefaultConfigFile()
        {
            using (StreamWriter writer = File.CreateText(ConfigFilePath))
            {
                writer.Write(
                    "; Speen Chroma config file\n" +
                    "; Edit only values after = signs!\n" +
                    "; If you are unsure of which value to edit, read the comments!\n" +
                    "; Comments start with a semi-colon (;)" +
                    "\n" +
                    "\n" +
                    "[Rainbow]\n" +
                    "; Whether to enable the rainbow effect or not. Possible values: True / False (case sensitive!)\n" +
                    "Enabled=True\n" +
                    "\n" +
                    "; Rainbow Speed as a multiplier. Can be a floating point number. Only numbers above 0. Write with a dot (.) not a comma (,)\n" +
                    "Speed=1\n" +
                    "\n" +
                    "; Notes to enable rainbow effect on. 1 to enable, 0 to disable.\n" +
                    "; Each digit represent a note type, respectively: Note A, Note B, Beat Bar, Spin Left, Spin Right, Scratch\n" +
                    "NoteTypes=111111\n"
                    );
            }
        }

        public class Patches
        {
            public static bool EnabledRainbow = true;
            public static char[] EnabledBlenders = { '1', '1', '1', '1', '1', '1'};
            public static List<GameplayColorBlender> blenders = new List<GameplayColorBlender>();

            public static float RainbowSpeed = 1f;

            [HarmonyPatch(typeof(GameplayColorBlender), nameof(GameplayColorBlender.ApplyAllModifications))]
            [HarmonyPostfix]
            private static void ApplyModificationsPostfix(GameplayColorBlender __instance)
            {
                Logger.LogMessage("Added color blender");
                blenders.Add(__instance);
            }

            [HarmonyPatch(typeof(Track), nameof(Track.Update))]
            [HarmonyPostfix]
            private static void UpdatePostfix()
            {
                if (!EnabledRainbow) return;
                for (int i = 1; i < blenders.Count; i++)
                {
                    if (EnabledBlenders[i - 1] == '1')
                    {
                        var blender = blenders[i];
                        blender.SetHSL((blender.hue >= 1f ? blender.hue - 1f : blender.hue) + 0.1f * Time.deltaTime * RainbowSpeed, blender.saturation, blender.lightness);
                    }
                }
            }
        }
    }
}
