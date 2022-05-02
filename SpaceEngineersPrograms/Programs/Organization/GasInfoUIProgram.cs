using Sandbox.ModAPI.Ingame;
using System;
using System.Collections.Generic;
using System.Text;
using VRage.Game.GUI.TextPanel;
using VRageMath;

namespace SpaceEngineersPrograms
{
    internal class GasInfoUIProgram : MyGridProgram
    {
        private class GasDisplay : Display
        {
            private Label titleLabel;

            private Label currentVolumeLabel;

            private Label maximalVolumeLabel;

            private ProgressBar volumeProgressBar;

            private double lastCurrentVolume;

            private float lastMaximalVolume;

            public GasDisplay(IMyTextSurface textSurface) : base(textSurface, null)
            {
                Font = "DarkBlue";
                BackgroundColor = Color.Black;
                Vector2 position = Size * 0.5f;
                Vector2 offset = textSurface.MeasureStringInPixels(new StringBuilder("W"), Font, FontSize);
                new Image("LCD_Economy_SE_Logo_2", TextAlignment.CENTER, Size * 0.5f, Size, Color.White, this);
                titleLabel = new Label("Gas: 0%", Font, ForegroundColor, FontSize, TextAlignment.CENTER, position + (Vector2.UnitY * offset * 2.0f), this);
                currentVolumeLabel = new Label("Current volume: 0 L", Font, ForegroundColor, FontSize, TextAlignment.LEFT, new Vector2(offset.X, position.Y + (offset.Y * 4.0f)), this);
                maximalVolumeLabel = new Label("Maximal volume: 0 L", Font, ForegroundColor, FontSize, TextAlignment.LEFT, new Vector2(offset.X, position.Y + (offset.Y * 5.0f)), this);
                volumeProgressBar = new ProgressBar(position + (Vector2.UnitY * (offset.Y * 8.0f)), new Vector2(Size.X - (offset.X * 2.0f), offset.Y * 0.5f), Color.Darken(Color.Cyan, 0.875f), Color.Red, Color.Darken(Color.Cyan, 0.75f), offset.Y * 0.125f, 0.0f, this);
            }

            public void UpdateValues(double currentVolume, float maximalVolume)
            {
                if ((currentVolume != lastCurrentVolume) || (maximalVolume != lastMaximalVolume))
                {
                    titleLabel.Text = "Gas: " + ((Math.Abs(maximalVolume) > float.Epsilon) ? ((currentVolume * 100.0) / maximalVolume) : 0L).ToString("N2") + "%";
                    currentVolumeLabel.Text = "Current volume: " + currentVolume.ToString("N2") + " L";
                    maximalVolumeLabel.Text = "Maximal volume: " + maximalVolume.ToString("N2") + " L";
                    float progress = ((Math.Abs(maximalVolume) > float.Epsilon) ? (float)(currentVolume / maximalVolume) : 0.0f);
                    volumeProgressBar.Value = progress;
                    volumeProgressBar.ForegroundColor = ((progress < 0.5f) ? Color.Lerp(Color.Red, Color.Orange, progress * 2.0f) : Color.Lerp(Color.Orange, Color.Green, (progress - 0.5f) * 2.0f));
                    lastCurrentVolume = currentVolume;
                    lastMaximalVolume = maximalVolume;
                    Refresh(null);
                }
            }
        }

        private Dictionary<long, GasDisplay> storageDisplays = new Dictionary<long, GasDisplay>();

        private HashSet<long> missingStorageDisplays = new HashSet<long>();

        private List<IMyTextPanel> textPanels = new List<IMyTextPanel>();

        private List<IMyGasTank> gasTanks = new List<IMyGasTank>();

        public GasInfoUIProgram()
        {
            Runtime.UpdateFrequency = UpdateFrequency.Update10;
        }

        private void Main(string argument, UpdateType updateType)
        {
            double current_volume = 0.0f;
            float maximal_volume = 0.0f;
            textPanels.Clear();
            GridTerminalSystem.GetBlocksOfType(textPanels, (text_panel) => (text_panel.CustomData.Trim().ToLower() == "gasinfo"));
            foreach (long key in storageDisplays.Keys)
            {
                missingStorageDisplays.Add(key);
            }
            foreach (IMyTextPanel text_panel in textPanels)
            {
                if (!(missingStorageDisplays.Remove(text_panel.EntityId)))
                {
                    storageDisplays.Add(text_panel.EntityId, new GasDisplay(text_panel));
                }
            }
            foreach (long key in missingStorageDisplays)
            {
                storageDisplays.Remove(key);
            }
            missingStorageDisplays.Clear();
            gasTanks.Clear();
            GridTerminalSystem.GetBlocksOfType(gasTanks);
            foreach (IMyGasTank gas_tank in gasTanks)
            {
                current_volume += gas_tank.Capacity * gas_tank.FilledRatio;
                maximal_volume += gas_tank.Capacity;
            }
            foreach (GasDisplay storage_display in storageDisplays.Values)
            {
                storage_display.UpdateValues(current_volume, maximal_volume);
            }
        }
    }
}
