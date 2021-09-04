using BepInEx;
using BepInEx.IL2CPP;
using HarmonyLib;
using MewsToolbox;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace SpeenChroma
{
    [BepInPlugin(MOD_ID, MOD_NAME, MOD_VERSION)]
    public class ChromaMod : BasePlugin
    {
        public const string MOD_ID = "ChromaMod";
        public const string MOD_NAME = "Speen Chroma";
        public const string MOD_VERSION = "1.2.0-Crazy8s";

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
            Patches.RainbowSpeed = Convert.ToSingle(ModConfig.GetValueOrDefaultTo<decimal>("Rainbow", "Speed", 1), IniFile.Culture);
            if (Patches.RainbowSpeed <= 0) Patches.RainbowSpeed = 1;
            Patches.DefaultRainbowSpeed = Patches.RainbowSpeed;
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
                    "NoteTypes=000000\n"
                    );
            }
        }

        public class Patches
        {
            public static bool EnabledRainbow = true;
            public static char[] EnabledBlenders = { '1', '1', '1', '1', '1', '1'};
            public static List<GameplayColorBlender> blenders = new List<GameplayColorBlender>();
            public static ChromaMode ChromaMode;

            public static float RainbowSpeed = 1f;
            public static float DefaultRainbowSpeed = 1;
            public static List<float[]> baseColors;

            [HarmonyPatch(typeof(GameplayColorBlender), nameof(GameplayColorBlender.ApplyAllModifications))]
            [HarmonyPostfix]
            private static void ApplyModificationsPostfix(GameplayColorBlender __instance)
            {
                blenders.Add(__instance);
            }

            [HarmonyPatch(typeof(Track), nameof(Track.Update))]
            [HarmonyPostfix]
            private static void UpdatePostfix()
            {
                switch (ChromaMode)
                {
                    case ChromaMode.Normal:
                        if (!EnabledRainbow) return;
                        for (int i = 1; i < blenders.Count; i++)
                        {
                            if (EnabledBlenders[i - 1] == '1')
                            {
                                var blender = blenders[i];
                                blender.SetHSL((blender.hue >= 1f ? blender.hue - 1f : blender.hue) + 0.1f * Time.deltaTime * RainbowSpeed, blender.saturation, blender.lightness);
                            }
                        }
                        break;

                    case ChromaMode.FullRainbow:
                        for (int i = 1; i < blenders.Count; i++)
                        {
                            var blender = blenders[i];
                            blender.SetHSL((blender.hue >= 1f ? blender.hue - 1f : blender.hue) + 0.1f * Time.deltaTime * RainbowSpeed, 1, 0.5f);
                        }
                        break;

                    case ChromaMode.Monochrome:
                        for (int i = 1; i < blenders.Count; i++)
                        {
                            if (blenders[i].saturation > 0)
                                blenders[i].SetHSL(blenders[i].hue, 0, 0.5f);
                        }
                        break;
                }
            }

            [HarmonyPatch(typeof(XDLevelSelectMenuBase), nameof(XDLevelSelectMenuBase.Update))]
            [HarmonyPostfix]
            private static void CycleChromaMode()
            {
                if (!Input.GetKeyDown(KeyCode.F1)) return;
                switch (ChromaMode)
                {
                    case ChromaMode.Normal:
                        ChromaMode = ChromaMode.FullRainbow;
                        if (baseColors == null)
                        {
                            baseColors = new List<float[]>();
                            for (int i = 1; i < blenders.Count; i++)
                            {
                                baseColors.Add(new float[] { blenders[i].hue, blenders[i].saturation, blenders[i].lightness });
                            }
                        }
                        RainbowSpeed = 8;
                        break;

                    case ChromaMode.FullRainbow:
                        ChromaMode = ChromaMode.Monochrome;
                        RainbowSpeed = DefaultRainbowSpeed;
                        break;

                    case ChromaMode.Monochrome:
                        ChromaMode = ChromaMode.Normal;
                        for (int i = 1; i < blenders.Count; i++)
                        {
                            blenders[i].SetHSL(baseColors[i-1][0], baseColors[i-1][1], baseColors[i-1][2]);
                        }
                        break;
                }
                Logger.LogWarning("Chroma Mode: " + ChromaMode);
            }
        }

        public enum ChromaMode
        {
            Normal,
            Monochrome,
            FullRainbow
        }
    }
}
