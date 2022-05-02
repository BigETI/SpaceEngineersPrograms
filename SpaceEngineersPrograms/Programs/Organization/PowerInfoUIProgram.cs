using Sandbox.ModAPI.Ingame;
using System;
using System.Collections.Generic;
using System.Text;
using VRage.Game.GUI.TextPanel;
using VRageMath;

namespace SpaceEngineersPrograms
{
    internal class PowerInfoUIProgram : MyGridProgram
    {
        private class PowerDisplay : Display
        {
            private Label titleLabel;

            private Label batteryOutputLabel;

            private Label batteryInputLabel;

            private Label nonBatteryOutputLabel;

            private ProgressBar storedPowerProgressBar;

            float lastCurrentBatteryOutput;

            float lastMaximalBatteryOutput;

            float lastCurrentNonBatteryOutput;

            float lastMaximalNonBatteryOutput;

            float lastCurrentBatteryInput;

            float lastMaximalBatteryInput;

            float lastCurrentBatteryStoredPower;

            float lastMaximalBatteryStoredPower;

            public PowerDisplay(IMyTextSurface textSurface) : base(textSurface, null)
            {
                Font = "DarkBlue";
                BackgroundColor = Color.Black;
                Vector2 position = Size * 0.5f;
                Vector2 offset = textSurface.MeasureStringInPixels(new StringBuilder("W"), Font, FontSize);
                new Image("LCD_Economy_SE_Logo_2", TextAlignment.CENTER, Size * 0.5f, Size, Color.White, this);
                titleLabel = new Label("Stored power: 0% -> 0 MWh / 0 MWh", Font, ForegroundColor, FontSize * 0.75f, TextAlignment.CENTER, position + (Vector2.UnitY * offset * 2.0f), this);
                batteryOutputLabel = new Label("Battery output: 0% -> 0 MW / 0 MW", Font, ForegroundColor, FontSize * 0.75f, TextAlignment.LEFT, new Vector2(offset.X, position.Y + (offset.Y * 4.0f)), this);
                batteryInputLabel = new Label("Battery input: 0% -> 0 MW / 0 MW", Font, ForegroundColor, FontSize * 0.75f, TextAlignment.LEFT, new Vector2(offset.X, position.Y + (offset.Y * 5.0f)), this);
                nonBatteryOutputLabel = new Label("Other output: 0% -> 0 MW / 0 MW", Font, ForegroundColor, FontSize * 0.75f, TextAlignment.LEFT, new Vector2(offset.X, position.Y + (offset.Y * 6.0f)), this);
                storedPowerProgressBar = new ProgressBar(position + (Vector2.UnitY * (offset.Y * 8.0f)), new Vector2(Size.X - (offset.X * 2.0f), offset.Y * 0.5f), Color.Darken(Color.Cyan, 0.875f), Color.Green, Color.Darken(Color.Cyan, 0.75f), offset.Y * 0.125f, 0.0f, this);
            }

            public void UpdateValues(float currentBatteryOutput, float currentNonBatteryOutput, float maximalBatteryOutput, float maximalNonBatteryOutput, float currentBatteryInput, float maximalBatteryInput, float currentBatteryStoredPower, float maximalBatteryStoredPower)
            {
                if ((currentBatteryOutput != lastCurrentBatteryOutput) || (currentNonBatteryOutput != lastCurrentNonBatteryOutput) || (maximalBatteryOutput != lastMaximalBatteryOutput) || (maximalNonBatteryOutput != lastMaximalNonBatteryOutput) || (currentBatteryInput != lastCurrentBatteryInput) || (maximalBatteryInput != lastMaximalBatteryInput) || (currentBatteryStoredPower != lastCurrentBatteryStoredPower) || (maximalBatteryStoredPower != lastMaximalBatteryStoredPower))
                {
                    titleLabel.Text = "Stored power: " + ((Math.Abs(maximalBatteryStoredPower) > float.Epsilon) ? ((currentBatteryStoredPower * 100.0f) / maximalBatteryStoredPower) : 0.0f).ToString("N2") + "% -> " + currentBatteryStoredPower.ToString("N2") + " MWh / " + maximalBatteryStoredPower.ToString("N2") + " MWh";
                    batteryOutputLabel.Text = "Battery output: " + ((Math.Abs(maximalBatteryOutput) > float.Epsilon) ? ((currentBatteryOutput * 100.0f) / maximalBatteryOutput) : 0.0f).ToString("N2") + "% -> " + currentBatteryOutput.ToString("N2") + " MW / " + maximalBatteryOutput.ToString("N2") + " MW";
                    batteryInputLabel.Text = "Battery input: " + ((Math.Abs(maximalBatteryInput) > float.Epsilon) ? ((currentBatteryInput * 100.0f) / maximalBatteryInput) : 0.0f).ToString("N2") + "% -> " + currentBatteryInput.ToString("N2") + " MW / " + maximalBatteryInput.ToString("N2") + " MW";
                    nonBatteryOutputLabel.Text = "Other output: " + ((Math.Abs(maximalNonBatteryOutput) > float.Epsilon) ? ((currentNonBatteryOutput * 100.0f) / maximalNonBatteryOutput) : 0.0f).ToString("N2") + "% -> " + currentNonBatteryOutput.ToString("N2") + " MW / " + maximalNonBatteryOutput.ToString("N2") + " MW";
                    float progress = ((Math.Abs(maximalBatteryStoredPower) > float.Epsilon) ? (currentBatteryStoredPower / maximalBatteryStoredPower) : 0.0f);
                    storedPowerProgressBar.Value = progress;
                    storedPowerProgressBar.ForegroundColor = ((progress < 0.5f) ? Color.Lerp(Color.Red, Color.Orange, progress * 2.0f) : Color.Lerp(Color.Orange, Color.Green, (progress - 0.5f) * 2.0f));
                    lastCurrentBatteryOutput = currentBatteryOutput;
                    lastCurrentNonBatteryOutput = currentNonBatteryOutput;
                    lastMaximalBatteryOutput = maximalBatteryOutput;
                    lastMaximalNonBatteryOutput = maximalNonBatteryOutput;
                    lastCurrentBatteryInput = currentBatteryInput;
                    lastMaximalBatteryInput = maximalBatteryInput;
                    lastCurrentBatteryStoredPower = currentBatteryStoredPower;
                    lastMaximalBatteryStoredPower = maximalBatteryStoredPower;
                    Refresh(null);
                }
            }
        }

        private Dictionary<long, PowerDisplay> storageDisplays = new Dictionary<long, PowerDisplay>();

        private HashSet<long> missingStorageDisplays = new HashSet<long>();

        private List<IMyTextPanel> textPanels = new List<IMyTextPanel>();

        private List<IMyPowerProducer> powerProducers = new List<IMyPowerProducer>();

        public PowerInfoUIProgram()
        {
            Runtime.UpdateFrequency = UpdateFrequency.Update10;
        }

        private void Main(string argument, UpdateType updateType)
        {
            float current_battery_output = 0.0f;
            float maximal_battery_output = 0.0f;
            float current_battery_input = 0.0f;
            float maximal_battery_input = 0.0f;
            float current_battery_stored_power = 0.0f;
            float maximal_battery_stored_power = 0.0f;
            float current_non_battery_output = 0.0f;
            float maximal_non_battery_output = 0.0f;
            textPanels.Clear();
            GridTerminalSystem.GetBlocksOfType(textPanels, (text_panel) => (text_panel.CustomData.Trim().ToLower() == "powerinfo"));
            foreach (long key in storageDisplays.Keys)
            {
                missingStorageDisplays.Add(key);
            }
            foreach (IMyTextPanel text_panel in textPanels)
            {
                if (!(missingStorageDisplays.Remove(text_panel.EntityId)))
                {
                    storageDisplays.Add(text_panel.EntityId, new PowerDisplay(text_panel));
                }
            }
            foreach (long key in missingStorageDisplays)
            {
                storageDisplays.Remove(key);
            }
            missingStorageDisplays.Clear();
            powerProducers.Clear();
            GridTerminalSystem.GetBlocksOfType(powerProducers);
            foreach (IMyPowerProducer power_producer in powerProducers)
            {
                if (power_producer.IsWorking)
                {
                    if (power_producer is IMyBatteryBlock)
                    {
                        IMyBatteryBlock battery_block = (IMyBatteryBlock)power_producer;
                        current_battery_output += battery_block.CurrentOutput;
                        maximal_battery_output += battery_block.MaxOutput;
                        current_battery_input += battery_block.CurrentInput;
                        maximal_battery_input += battery_block.MaxInput;
                        current_battery_stored_power += battery_block.CurrentStoredPower;
                        maximal_battery_stored_power += battery_block.MaxStoredPower;
                    }
                    else
                    {
                        current_non_battery_output += power_producer.CurrentOutput;
                        maximal_non_battery_output += power_producer.MaxOutput;
                    }
                }
            }
            foreach (PowerDisplay storage_display in storageDisplays.Values)
            {
                storage_display.UpdateValues(current_battery_output, current_non_battery_output, maximal_battery_output, maximal_non_battery_output, current_battery_input, maximal_battery_input, current_battery_stored_power, maximal_battery_stored_power);
            }
        }
    }
}
