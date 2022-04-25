using System.Collections.Generic;
using HarmonyLib;

namespace SpeenChroma
{
    public class ChromaPatches
    {
        private static readonly List<GameplayColorBlender> ColorBlenders = new List<GameplayColorBlender>();
        private static readonly List<HSLColor> StartingColors = new List<HSLColor>();
        private static readonly List<HSLColor> Colors = new List<HSLColor>();
        private static bool _chromaUpdate = false;
        private static readonly float RainbowSpeed = Plugin.ModConfig.GetValueOrDefaultTo("Rainbow", "Speed", 1f);
        
        [HarmonyPatch(typeof(Track), nameof(Track.Update))]
        [HarmonyPostfix]
        private static void UpdateChroma()
        {
            if (!_chromaUpdate) return;
            for (int i = 0; i < ColorBlenders.Count; i++)
            {
                var col = Colors[i];
                col.H = (col.H >= 1f ? col.H - 1f : col.H) + 0.1f * Time.deltaTime * RainbowSpeed;
                ColorBlenders[i].SetHSL(col.H, col.S, col.L);
                Colors[i] = col;
            }
        }

        [HarmonyPatch(typeof(GameplayColorBlender), nameof(GameplayColorBlender.ApplyAllModifications))]
        [HarmonyPostfix]
        private static void AddColorBlender(GameplayColorBlender __instance)
        {
            ColorBlenders.Add(__instance);
        }

        [HarmonyPatch(typeof(Track), "Awake")]
        [HarmonyPostfix]
        private static void UpdateColorArray()
        {
            Colors.Clear();
            foreach (var blender in ColorBlenders)
            {
                float hue = (float)Utilities.GetInstanceField(blender.GetType(), blender, "hue");
                float saturation = (float)Utilities.GetInstanceField(blender.GetType(), blender, "saturation");
                float lightness = (float)Utilities.GetInstanceField(blender.GetType(), blender, "lightness");
                Colors.Add(new HSLColor
                {
                    H = hue,
                    S = saturation,
                    L = lightness,
                });
            }
            StartingColors.Clear();
            StartingColors.AddRange(Colors);

            _chromaUpdate = true;
        }

        [HarmonyPatch(typeof(XDOptionsMenu), nameof(XDOptionsMenu.OpenColors))]
        [HarmonyPostfix]
        private static void PauseChroma()
        {
            for (int i = 0; i < ColorBlenders.Count; i++)
            {
                var col = StartingColors[i];
                ColorBlenders[i].SetHSL(col.H, col.S, col.L);
            }
            _chromaUpdate = false;
            NotificationSystemGUI.AddMessage("Chroma Updates don't run while the Note Color Overrides menu is open.");
        }

        [HarmonyPatch(typeof(XDOptionsMenu), "Awake")]
        [HarmonyPostfix]
        private static void AddResumeChromaCallback(XDOptionsMenu __instance)
        {
            __instance.closeColorTabButton.onClick.AddListener(UpdateColorArray);
        }
    }
}
