using Sandbox.ModAPI.Ingame;
using System.Collections.Generic;

namespace SpaceEngineersPrograms
{
    public class Logger
    {
        private static readonly IReadOnlyDictionary<string, string> escapeCharacters = new Dictionary<string, string>
        {
            { "\a", @"\a" },
            { "\b", @"\b" },
            { "\f", @"\f" },
            { "\n", @"\n" },
            { "\r", @"\r" },
            { "\t", @"\t" },
            { "\v", @"\v" },
            { "\\", @"\" },
            { "\'", @"\'" },
            { "\"", "\\\"" }
        };

        private long appID;

        private List<IMyProgrammableBlock> outputProgrammableBlocks = new List<IMyProgrammableBlock>();

        public Logger(MyGridProgram gridProgram, ELoggerType loggerType)
        {
            if (gridProgram != null)
            {
                appID = gridProgram.Me.EntityId;
                string logger_name = loggerType.ToString();
                gridProgram.GridTerminalSystem.GetBlocksOfType(outputProgrammableBlocks, (outputProgrammableBlock) => outputProgrammableBlock.CustomName.Trim() == logger_name);
            }
        }

        private bool AppendText(string text)
        {
            bool ret = false;
            if (text != null)
            {
                string escaped_text = text;
                foreach (KeyValuePair<string, string> escape_character in escapeCharacters)
                {
                    escaped_text = escaped_text.Replace(escape_character.Key, escape_character.Value);
                }
                foreach (IMyProgrammableBlock output_programmable_block in outputProgrammableBlocks)
                {
                    if (output_programmable_block.IsWorking)
                    {
                        ret = output_programmable_block.TryRun("append|" + appID + "|" + text);
                    }
                }
            }
            return ret;
        }

        public bool Append(object obj) => AppendText((obj == null) ? string.Empty : obj.ToString());

        public bool AppendLine(object obj)
        {
            bool ret = Append(obj);
            if (ret)
            {
                ret = AppendText("\n");
            }
            return ret;
        }
    }
}
