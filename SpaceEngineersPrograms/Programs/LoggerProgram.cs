using Sandbox.ModAPI.Ingame;
using System;
using System.Collections.Generic;
using System.Text;

namespace SpaceEngineersPrograms
{
    internal class LoggerProgram : MyGridProgram
    {
        private static readonly IReadOnlyDictionary<string, string> escapeCharacters = new Dictionary<string, string>
        {
            { @"\a", "\a" },
            { @"\b", "\b" },
            { @"\f", "\f" },
            { @"\n", "\n" },
            { @"\r", "\r" },
            { @"\t", "\t" },
            { @"\v", "\v" },
            { @"\", "\\" },
            { @"\'", "\'" },
            { "\\\"", "\"" }
        };

        private Commands<bool> commands = new Commands<bool>('|');

        private readonly Dictionary<string, StringBuilder> outputs = new Dictionary<string, StringBuilder>();

        public LoggerProgram()
        {
            commands.Add("append", "Append logging data", "This command appends data to logger.", AppendCommand, "application ID", "data");
            commands.Add("applications", "List applications", "This command lists all applications that have written to that logger.", ApplicationsCommand);
            commands.Add("clear", "Clear data", "This command clears logging data.", ClearCommand, ArgumentString.Optional("application ID"));
            commands.Add("help", "Help topics", "This command shows help topics.", HelpCommand, ArgumentString.Optional("help topic"));
            commands.Add("read", "Read logging data", "This command reads output by dumping output data to \"Cutsom Data\".", ReadCommand, "application ID");
            commands.Add("write", "Write logging data", "This command writes logging data.", WriteCommand, "application ID");
            commands.AddAliases("append", "a");
            commands.AddAliases("applications", "apps");
            commands.AddAliases("clear", "c");
            commands.AddAliases("help", "commands", "manual", "cmds", "cmd", "man", "h", "?");
            commands.AddAliases("read", "r");
            commands.AddAliases("write", "w");
        }

        private StringBuilder GetOrCreateAppOutput(string applicationID)
        {
            StringBuilder ret;
            if (outputs.ContainsKey(applicationID))
            {
                ret = outputs[applicationID];
            }
            else
            {
                ret = new StringBuilder();
                outputs.Add(applicationID, ret);
            }
            return ret;
        }

        private void AppendOutput(StringBuilder output, string escapedText)
        {
            string text = escapedText;
            foreach (KeyValuePair<string, string> escape_character in escapeCharacters)
            {
                text = text.Replace(escape_character.Key, escape_character.Value);
            }
            output.Append(text);
        }

        private void PrintHelp()
        {
            Echo(commands.GetHelpTopic());
        }

        private string GetApplicationIDArgument(IReadOnlyList<string> arguments)
        {
            if (arguments == null)
            {
                throw new ArgumentNullException(nameof(arguments));
            }
            return ((arguments.Count > 0) ? arguments[0].Trim() : "0");
        }

        private bool AppendCommand(IReadOnlyList<string> arguments)
        {
            StringBuilder output = GetOrCreateAppOutput(GetApplicationIDArgument(arguments));
            bool first = true;
            for (int i = 1; i < arguments.Count; i++)
            {
                if (first)
                {
                    first = false;
                }
                else
                {
                    output.Append('|');
                }
                AppendOutput(output, arguments[i]);
            }
            return true;
        }

        private bool ApplicationsCommand(IReadOnlyList<string> arguments)
        {
            StringBuilder output_string_builder = new StringBuilder();
            foreach (string application in outputs.Keys)
            {
                output_string_builder.AppendLine(application);
            }
            Me.CustomData = output_string_builder.ToString();
            output_string_builder.Clear();
            return true;
        }

        private bool ClearCommand(IReadOnlyList<string> arguments)
        {
            bool ret = true;
            if (arguments.Count > 0)
            {
                ret = outputs.Remove(GetApplicationIDArgument(arguments));
            }
            else
            {
                foreach (StringBuilder output in outputs.Values)
                {
                    output.Clear();
                }
                outputs.Clear();
            }
            return ret;
        }

        private bool HelpCommand(IReadOnlyList<string> arguments)
        {
            if (arguments.Count > 0)
            {
                Echo(commands.GetHelpTopic(arguments[0]));
            }
            else
            {
                PrintHelp();
            }
            return true;
        }

        private bool ReadCommand(IReadOnlyList<string> arguments)
        {
            string application_id = GetApplicationIDArgument(arguments);
            bool ret = outputs.ContainsKey(application_id);
            Me.CustomData = (ret ? outputs[application_id].ToString() : string.Empty);
            return ret;
        }

        private bool WriteCommand(IReadOnlyList<string> arguments)
        {
            StringBuilder output = GetOrCreateAppOutput(GetApplicationIDArgument(arguments));
            bool first = true;
            output.Clear();
            for (int i = 1; i < arguments.Count; i++)
            {
                if (first)
                {
                    first = false;
                }
                else
                {
                    output.Append('|');
                }
                AppendOutput(output, arguments[i]);
            }
            return true;
        }

        private void Main(string argument, UpdateType updateSource)
        {
            if (!(commands.Parse(argument)))
            {
                PrintHelp();
            }
        }
    }
}
