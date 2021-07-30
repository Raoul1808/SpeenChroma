using BepInEx;
using BepInEx.IL2CPP;
using HarmonyLib;
using System.Collections.Generic;
using UnityEngine;

namespace SpeenChroma
{
    [BepInPlugin(MOD_ID, MOD_NAME, MOD_VERSION)]
    public class ChromaMod : BasePlugin
    {
        public const string MOD_ID = "ChromaMod";
        public const string MOD_NAME = "Speen Chroma";
        public const string MOD_VERSION = "1.0.0";

        public static BepInEx.Logging.ManualLogSource Logger;

        public override void Load()
        {
            Logger = BepInEx.Logging.Logger.CreateLogSource(MOD_ID);
            Harmony.CreateAndPatchAll(typeof(ChromaMod.Patches));
        }

        public class Patches
        {
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
                foreach (GameplayColorBlender blender in blenders)
                {
                    blender.SetHSL((blender.hue >= 1f ? blender.hue - 1f : blender.hue) + 0.1f * Time.deltaTime, blender.saturation, blender.lightness);
                }
            }
        }
    }
}
