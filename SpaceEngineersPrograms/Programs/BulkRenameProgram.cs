using Sandbox.ModAPI.Ingame;
using System;
using System.Collections.Generic;

namespace SpaceEngineersPrograms
{
    internal class BulkRenameProgram : MyGridProgram
    {
        private List<IMyTerminalBlock> blocks = new List<IMyTerminalBlock>();

        private Logger standardLogger;

        private Logger errorLogger;

        public Commands<bool> commands = new Commands<bool>('|');

        public BulkRenameProgram()
        {
            standardLogger = new Logger(this, ELoggerType.StandardOutput);
            errorLogger = new Logger(this, ELoggerType.ErrorOutput);
            commands.Add("help", "Help topics", "This command shows help topics.", HelpCommand);
            commands.Add("renameblocks", "Rename blocks", "This command renames any block that matches the pattern.", RenameBlocksCommand, "pattern", "replacement");
            commands.Add("renamegroupblocks", "Rename group blocks", "This command renames any block within a group that matches the pattern.", RenameGroupBlocksCommand, "group name", "pattern", "replacement");
            commands.AddAliases("help", "commands", "manual", "cmds", "cmd", "man", "h", "?");
            commands.AddAliases("renameblocks", "rb");
            commands.AddAliases("renamegroupblocks", "rgb");
        }

        private void Log(string text)
        {
            standardLogger.AppendLine(text);
            Echo(text);
        }

        private void LogError(string text)
        {
            errorLogger.AppendLine(text);
            Echo(text);
        }

        private void LogHelp(string helpTopic) => Log(commands.GetHelpTopic(helpTopic));

        private bool HelpCommand(IReadOnlyList<string> arguments)
        {
            LogHelp((arguments.Count > 0) ? arguments[0] : string.Empty);
            return true;
        }

        private bool RenameBlocksCommand(IReadOnlyList<string> arguments)
        {
            string replacement = arguments[1].Trim();
            uint count = 0U;
            System.Text.RegularExpressions.Regex regex = new System.Text.RegularExpressions.Regex(arguments[0].Trim());
            GridTerminalSystem.GetBlocks(blocks);
            foreach (IMyTerminalBlock block in blocks)
            {
                if (regex.IsMatch(block.CustomName))
                {
                    block.CustomName = regex.Replace(block.CustomName, replacement);
                    ++count;
                }
            }
            blocks.Clear();
            if (count == 0U)
            {
                Log("No occurences found.");
            }
            if (count == 1U)
            {
                Log("Replaced one occurence.");
            }
            else
            {
                Log("Replaced " + count + " occurences.");
            }
            return true;
        }

        private bool RenameGroupBlocksCommand(IReadOnlyList<string> arguments)
        {
            string block_group_name = arguments[0].Trim();
            System.Text.RegularExpressions.Regex regex = new System.Text.RegularExpressions.Regex(arguments[0].Trim());
            IMyBlockGroup block_group = GridTerminalSystem.GetBlockGroupWithName(arguments[0]);
            if (block_group != null)
            {
                string replacement = arguments[1].Trim();
                uint count = 0U;
                block_group.GetBlocks(blocks);
                foreach (IMyTerminalBlock block in blocks)
                {
                    if (regex.IsMatch(block.CustomName))
                    {
                        block.CustomName = regex.Replace(block.CustomName, replacement);
                        ++count;
                    }
                }
                blocks.Clear();
                if (count == 0U)
                {
                    Log("No occurences found.");
                }
                if (count == 1U)
                {
                    Log("Replaced one occurence.");
                }
                else
                {
                    Log("Replaced " + count + " occurences.");
                }
            }
            else
            {
                LogError("Group \"" + block_group_name + "\" does not exist.");
            }
            return true;
        }

        private void Main(string argument, UpdateType updateSource)
        {
            try
            {
                string command_name;
                if (!(commands.Parse(argument, out command_name)))
                {
                    LogHelp(command_name);
                }
            }
            catch (Exception e)
            {
                LogError("An invalid regular expression has been specified");
                LogError(e.ToString());
            }
        }
    }
}
