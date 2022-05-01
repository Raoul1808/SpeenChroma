using SpinCore.UI;

namespace SpeenChroma
{
    public class ChromaUI
    {
        public static void CreateOptionsTab(CustomSpinTab tab)
        {
            SpinUI.CreateToggle("Enable Chroma Effects", tab.UIRoot,
                Plugin.ModConfig.GetValueOrDefaultTo("Chroma", "Enabled", true),
                onValueChanged: x =>
                {
                    Plugin.ModConfig.SetValue("Chroma", "Enabled", x);
                    ChromaPatches.ChromaUpdate = x;
                    ChromaLogic.ResetCurrentColors();
                });

            SpinUI.CreateSlider("Rainbow Speed", tab.UIRoot, 0f, 25f,
                Plugin.ModConfig.GetValueOrDefaultTo("Rainbow", "Speed", 1f),
                onValueChanged: x =>
                {
                    Plugin.ModConfig.SetValue("Rainbow", "Speed", x);
                    ChromaLogic.RainbowSpeed = x;
                });

            // for (int i = 0; i < ChromaPatches.EnabledNotes.Count; i++)
            // {
            //     SpinUI.CreateToggle($"Apply Effects on {}");
            // }
        }
    }
}
