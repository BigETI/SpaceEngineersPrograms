using Sandbox.ModAPI.Ingame;
using System.Collections.Generic;
using System.Text;
using VRage;
using VRage.Game.GUI.TextPanel;
using VRage.Game.ModAPI.Ingame;
using VRageMath;

namespace SpaceEngineersPrograms
{
    public class StorageInfoUIProgram : MyGridProgram
    {
        private class StorageDisplay : Display
        {
            private Label titleLabel;

            private Label currentVolumeLabel;

            private Label maximalVolumeLabel;

            private Label currentMassLabel;

            private ProgressBar volumeProgressBar;

            private MyFixedPoint lastCurrentVolume;

            private MyFixedPoint lastMaximalVolume;

            private MyFixedPoint lastCurrentMass;

            public StorageDisplay(IMyTextSurface textSurface) : base(textSurface, null)
            {
                Font = "DarkBlue";
                BackgroundColor = Color.Black;
                Vector2 position = Size * 0.5f;
                Vector2 offset = textSurface.MeasureStringInPixels(new StringBuilder("W"), Font, FontSize);
                new Image("LCD_Economy_SE_Logo_2", TextAlignment.CENTER, Size * 0.5f, Size, Color.White, this);
                titleLabel = new Label("Storage: 0%", Font, ForegroundColor, FontSize, TextAlignment.CENTER, position + (Vector2.UnitY * offset * 2.0f), this);
                currentVolumeLabel = new Label("Current volume: 0 L", Font, ForegroundColor, FontSize, TextAlignment.LEFT, new Vector2(offset.X, position.Y + (offset.Y * 4.0f)), this);
                maximalVolumeLabel = new Label("Maximal volume: 0 L", Font, ForegroundColor, FontSize, TextAlignment.LEFT, new Vector2(offset.X, position.Y + (offset.Y * 5.0f)), this);
                currentMassLabel = new Label("Storage mass: 0 t", Font, ForegroundColor, FontSize, TextAlignment.LEFT, new Vector2(offset.X, position.Y + (offset.Y * 6.0f)), this);
                volumeProgressBar = new ProgressBar(position + (Vector2.UnitY * (offset.Y * 8.0f)), new Vector2(Size.X - (offset.X * 2.0f), offset.Y * 0.5f), Color.Darken(Color.Cyan, 0.875f), Color.Green, Color.Darken(Color.Cyan, 0.75f), offset.Y * 0.125f, 0.0f, this);
            }

            public void UpdateValues(MyFixedPoint currentVolume, MyFixedPoint maximalVolume, MyFixedPoint currentMass)
            {
                if ((currentVolume != lastCurrentVolume) || (maximalVolume != lastMaximalVolume) || (currentMass != lastCurrentMass))
                {
                    titleLabel.Text = "Storage: " + ((maximalVolume.RawValue > 0L) ? ((currentVolume.RawValue * 100L) / maximalVolume.RawValue) : 0L) + "%";
                    currentVolumeLabel.Text = "Current volume: " + (currentVolume.RawValue / 1000L) + " L";
                    maximalVolumeLabel.Text = "Maximal volume: " + (maximalVolume.RawValue / 1000L) + " L";
                    currentMassLabel.Text = "Storage mass: " + (currentMass.RawValue / 1000000L) + " t";
                    float progress = ((maximalVolume.RawValue > 0L) ? ((float)(currentVolume.RawValue) / maximalVolume.RawValue) : 0.0f);
                    volumeProgressBar.Value = progress;
                    volumeProgressBar.ForegroundColor = ((progress < 0.5f) ? Color.Lerp(Color.Cyan, Color.Orange, progress * 2.0f) : Color.Lerp(Color.Orange, Color.Red, (progress - 0.5f) * 2.0f));
                    lastCurrentVolume = currentVolume;
                    lastMaximalVolume = maximalVolume;
                    lastCurrentMass = currentMass;
                    Refresh(null);
                }
            }
        }

        private Dictionary<long, StorageDisplay> storageDisplays = new Dictionary<long, StorageDisplay>();

        private HashSet<long> missingStorageDisplays = new HashSet<long>();

        private List<IMyTextPanel> textPanels = new List<IMyTextPanel>();

        private List<IMyCargoContainer> cargoContainerBlocks = new List<IMyCargoContainer>();

        public StorageInfoUIProgram()
        {
            Runtime.UpdateFrequency = UpdateFrequency.Update10;
        }

        private void Main(string argument, UpdateType updateType)
        {
            MyFixedPoint current_volume = MyFixedPoint.Zero;
            MyFixedPoint maximal_volume = MyFixedPoint.Zero;
            MyFixedPoint current_mass = MyFixedPoint.Zero;
            textPanels.Clear();
            GridTerminalSystem.GetBlocksOfType(textPanels, (text_panel) => (text_panel.CustomData.Trim().ToLower() == "storageinfo"));
            foreach (long key in storageDisplays.Keys)
            {
                missingStorageDisplays.Add(key);
            }
            foreach (IMyTextPanel text_panel in textPanels)
            {
                if (!(missingStorageDisplays.Remove(text_panel.EntityId)))
                {
                    storageDisplays.Add(text_panel.EntityId, new StorageDisplay(text_panel));
                }
            }
            foreach (long key in missingStorageDisplays)
            {
                storageDisplays.Remove(key);
            }
            missingStorageDisplays.Clear();
            cargoContainerBlocks.Clear();
            GridTerminalSystem.GetBlocksOfType(cargoContainerBlocks);
            foreach (IMyCargoContainer cargo_container_block in cargoContainerBlocks)
            {
                for (int i = 0; i < cargo_container_block.InventoryCount; i++)
                {
                    IMyInventory inventory = cargo_container_block.GetInventory();
                    current_volume += inventory.CurrentVolume;
                    maximal_volume += inventory.MaxVolume;
                    current_mass += inventory.CurrentMass;
                }
            }
            foreach (StorageDisplay storage_display in storageDisplays.Values)
            {
                storage_display.UpdateValues(current_volume, maximal_volume, current_mass);
            }
        }
    }
}
