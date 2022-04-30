using System.Collections.Generic;
using HarmonyLib;

namespace SpeenChroma
{
    public class ChromaPatches
    {
        private static readonly List<GameplayColorBlender> ColorBlenders = new List<GameplayColorBlender>();
        private static readonly List<HSLColor> StartingColors = new List<HSLColor>();
        private static readonly List<HSLColor> Colors = new List<HSLColor>();
        private static bool _internalChromaUpdate = false;

        public static bool ChromaUpdate = true;
        public static float RainbowSpeed = Plugin.ModConfig.GetValueOrDefaultTo("Rainbow", "Speed", 1f);
        public static readonly List<bool> EnabledNotes = new List<bool>();

        [HarmonyPatch(typeof(Track), nameof(Track.Update))]
        [HarmonyPostfix]
        private static void UpdateChroma()
        {
            if (!_internalChromaUpdate || !ChromaUpdate) return;
            for (int i = 0; i < ColorBlenders.Count; i++)
            {
                if (i == 0 || !EnabledNotes[i - 1]) continue;
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
                float hue = (float) Utilities.GetInstanceField(blender.GetType(), blender, "hue");
                float saturation = (float) Utilities.GetInstanceField(blender.GetType(), blender, "saturation");
                float lightness = (float) Utilities.GetInstanceField(blender.GetType(), blender, "lightness");
                Colors.Add(new HSLColor
                {
                    H = hue,
                    S = saturation,
                    L = lightness,
                });
            }

            StartingColors.Clear();
            StartingColors.AddRange(Colors);

            _internalChromaUpdate = true;
        }

        public static void InitializeConfigFields()
        {
            string enabledNotes = Plugin.ModConfig.GetValueOrDefaultTo("Chroma", "AffectedNotes", "1111111");
            for (int i = 0; i < 7; i++)
            {
                EnabledNotes.Add(enabledNotes[i] == '1');
            }
        }

        [HarmonyPatch(typeof(XDOptionsMenu), nameof(XDOptionsMenu.OpenColors))]
        [HarmonyPostfix]
        private static void PauseChroma()
        {
            ResetColors();
            _internalChromaUpdate = false;
            NotificationSystemGUI.AddMessage("Chroma Updates don't run while the Note Color Overrides menu is open.");
        }

        public static void ResetColors()
        {
            for (int i = 0; i < ColorBlenders.Count; i++)
            {
                var col = StartingColors[i];
                ColorBlenders[i].SetHSL(col.H, col.S, col.L);
            }
        }

        [HarmonyPatch(typeof(XDOptionsMenu), "Awake")]
        [HarmonyPostfix]
        private static void AddResumeChromaCallback(XDOptionsMenu __instance)
        {
            __instance.closeColorTabButton.onClick.AddListener(UpdateColorArray);
        }
        
        // // Razer Chroma section
        // // Yes, there's a small limitation with the razer integration.
        // [HarmonyPatch(typeof(Track), nameof(Track.Update))]
        // [HarmonyPostfix]
        // private static void LookupKeyboard()
        // {
        //     
        // }
    }
}
