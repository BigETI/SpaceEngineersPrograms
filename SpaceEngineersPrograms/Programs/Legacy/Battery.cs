//using Sandbox.ModAPI;
using Sandbox.ModAPI.Ingame;
using System;
using System.Collections.Generic;
using System.Text;

namespace SpaceEngineersPrograms
{
    public class Battery : MyGridProgram
    {
        private List<IMyTerminalBlock> blocks = new List<IMyTerminalBlock>();

        private List<IMyTextPanel> textPanels = new List<IMyTextPanel>();

        private StringBuilder textBuilder = new StringBuilder();

        public Battery()
        {
            Runtime.UpdateFrequency = UpdateFrequency.Update1;
        }

        public void Save()
        {
            // ...
        }

        private void AppendFlag(string flag, bool state, ref bool hasFlag)
        {
            if (state)
            {
                if (hasFlag)
                {
                    textBuilder.Append(" | ");
                }
                else
                {
                    hasFlag = true;
                    textBuilder.Append(" ( ");
                }
                textBuilder.Append(flag);
            }
        }

        private static string GetWattString(float watt)
        {
            StringBuilder text_builder = new StringBuilder();
            if (watt >= 1000000000.0f)
            {
                text_builder.Append((watt * 0.000000001f).ToString());
                text_builder.Append("G");
            }
            else if (watt >= 1000000.0f)
            {
                text_builder.Append((watt * 0.000001f).ToString());
                text_builder.Append("M");
            }
            else if (watt >= 1000.0f)
            {
                text_builder.Append((watt * 0.001f).ToString());
                text_builder.Append("K");
            }
            else
            {
                text_builder.Append(watt.ToString());
            }
            text_builder.Append("W");
            string ret = text_builder.ToString();
            text_builder.Clear();
            return ret;
        }

        public void Main(string argument, UpdateType updateSource)
        {
            bool no_battery = true;
            blocks.Clear();
            textPanels.Clear();
            GridTerminalSystem.GetBlocks(blocks);
            foreach (IMyTerminalBlock block in blocks)
            {
                if (block is IMyTextPanel)
                {
                    IMyTextPanel text_panel = (IMyTextPanel)block;
                    if (text_panel.CustomData.Trim() == "showBatteryStates")
                    {
                        textPanels.Add(text_panel);
                    }
                }
                else if (block is IMyBatteryBlock)
                {
                    IMyBatteryBlock battery = (IMyBatteryBlock)block;
                    bool has_flag = false;
                    textBuilder.Append(battery.CustomName);
                    textBuilder.Append(": ");
                    textBuilder.Append(Math.Round((battery.CurrentStoredPower * 100.0f) / battery.MaxStoredPower).ToString());
                    textBuilder.Append("% ( ");
                    textBuilder.Append(GetWattString(battery.CurrentStoredPower * 1000.0f));
                    textBuilder.Append("h");
                    textBuilder.Append(" / ");
                    textBuilder.Append(GetWattString(battery.MaxStoredPower * 1000.0f));
                    textBuilder.AppendLine("h )");
                    textBuilder.Append("\tOutput: ");
                    textBuilder.Append(GetWattString(battery.CurrentOutput * 1000.0f));
                    textBuilder.Append(" / ");
                    textBuilder.AppendLine(GetWattString(battery.MaxOutput * 1000.0f));
                    textBuilder.Append("\tInput: ");
                    textBuilder.Append(GetWattString(battery.CurrentInput * 1000.0f));
                    textBuilder.Append(" / ");
                    textBuilder.AppendLine(GetWattString(battery.MaxInput * 1000.0f));
                    textBuilder.Append("Flags: ");
                    AppendFlag("W", battery.IsWorking, ref has_flag);
                    AppendFlag("C", battery.IsCharging, ref has_flag);
                    AppendFlag("H", battery.IsBeingHacked, ref has_flag);
                    if (has_flag)
                    {
                        textBuilder.Append(" )");
                    }
                    textBuilder.AppendLine();
                    no_battery = false;
                }
            }
            if (no_battery)
            {
                textBuilder.Append("No battery found!");
            }
            string text = textBuilder.ToString();
            textBuilder.Clear();
            foreach (IMyTextPanel text_panel in textPanels)
            {
                text_panel.WriteText(text);
            }
        }
    }
}
