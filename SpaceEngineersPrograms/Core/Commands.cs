using System;
using System.Collections.Generic;
using System.Text;

namespace SpaceEngineersPrograms
{
    public class Commands<TResult, TContext>
    {
        private HashSet<Command<TResult, TContext>> commands = new HashSet<Command<TResult, TContext>>();

        private Dictionary<string, Command<TResult, TContext>> commandLookup = new Dictionary<string, Command<TResult, TContext>>();

        public IReadOnlyDictionary<string, Command<TResult, TContext>> CommandLookup => commandLookup;

        public char Delimiter { get; set; }

        public TResult DefaultParseReturnValue { get; private set; } = default(TResult);

        public string GetHelpTopic()
        {
            StringBuilder help_string_builder = new StringBuilder();
            help_string_builder.AppendLine("Help topics:");
            List<Command<TResult, TContext>> sorted_commands = new List<Command<TResult, TContext>>(commands);
            sorted_commands.Sort((left, right) => left.Name.CompareTo(right.Name));
            foreach (Command<TResult, TContext> command in sorted_commands)
            {
                if (!(command.IsAlias))
                {
                    help_string_builder.Append(command.ToString());
                    help_string_builder.Append(": ");
                    help_string_builder.AppendLine(command.Description);
                }
            }
            sorted_commands.Clear();
            help_string_builder.AppendLine();
            help_string_builder.Append("Usage: <command>");
            help_string_builder.Append(Delimiter);
            help_string_builder.AppendLine("<arguments>");
            string ret = help_string_builder.ToString();
            help_string_builder.Clear();
            return ret;
        }

        public string GetHelpTopic(string commandName)
        {
            if (commandName == null)
            {
                throw new ArgumentNullException(nameof(commandName));
            }
            string ret;
            string trimmed_command_name = commandName.Trim().ToLower();
            if (commandLookup.ContainsKey(trimmed_command_name))
            {
                Command<TResult, TContext> command = commandLookup[trimmed_command_name];
                StringBuilder help_string_builder = new StringBuilder();
                help_string_builder.Append("Help topic: ");
                help_string_builder.AppendLine(command.GetHelpString(Delimiter));
                ret = help_string_builder.ToString();
                help_string_builder.Clear();
            }
            else
            {
                ret = GetHelpTopic();
            }
            return ret;
        }

        public Commands(char delimiter = ' ', TResult defaultParseReturnValue = default(TResult))
        {
            Delimiter = delimiter;
            DefaultParseReturnValue = defaultParseReturnValue;
        }

        public bool Add(string commandName, CommandDelegate<TResult> onExecuteCommand, params ArgumentString[] arguments) => Add(commandName, (executionArguments, context) => ((onExecuteCommand == null) ? DefaultParseReturnValue : onExecuteCommand(executionArguments)), arguments);

        public bool Add(string commandName, string commandDescription, string commandFullDescription, CommandDelegate<TResult> onExecuteCommand, params ArgumentString[] arguments) => Add(commandName, commandDescription, commandFullDescription, (executionArguments, context) => ((onExecuteCommand == null) ? DefaultParseReturnValue : onExecuteCommand(executionArguments)), arguments);

        public bool Add(string commandName, CommandDelegate<TResult, TContext> onExecuteCommand, params ArgumentString[] arguments)
        {
            if (string.IsNullOrWhiteSpace(commandName))
            {
                throw new ArgumentNullException(nameof(commandName));
            }
            bool ret = false;
            Command<TResult, TContext> command = Command<TResult, TContext>.New(commandName.Trim().ToLower(), onExecuteCommand, arguments);
            if (commands.Add(command))
            {
                commandLookup.Add(command.Name, command);
                ret = true;
            }
            return ret;
        }

        public bool Add(string commandName, string commandDescription, string commandFullDescription, CommandDelegate<TResult, TContext> onExecuteCommand, params ArgumentString[] arguments)
        {
            bool ret = false;
            Command<TResult, TContext> command = Command<TResult, TContext>.New(commandName, commandDescription, commandFullDescription, onExecuteCommand, arguments);
            if (commands.Add(command))
            {
                commandLookup.Add(command.Name, command);
                ret = true;
            }
            return ret;
        }

        public bool AddAliases(string commandName, params string[] aliases)
        {
            if (string.IsNullOrWhiteSpace(commandName))
            {
                throw new ArgumentNullException(nameof(commandName));
            }
            if (aliases == null)
            {
                throw new ArgumentNullException(nameof(aliases));
            }
            bool ret = false;
            string trimmed_command_name = commandName.Trim().ToLower();
            if (commandLookup.ContainsKey(trimmed_command_name))
            {
                Command<TResult, TContext> command = commandLookup[trimmed_command_name];
                foreach (string alias in aliases)
                {
                    if (alias == null)
                    {
                        throw new ArgumentNullException(nameof(alias));
                    }
                    Command<TResult, TContext> alias_command = command.CreateAlias(alias);
                    if (commands.Add(alias_command))
                    {
                        commandLookup.Add(alias_command.Name, alias_command);
                        ret = true;
                    }
                    else
                    {
                        ret = false;
                        break;
                    }
                }
            }
            return ret;
        }

        public bool Remove(string commandName)
        {
            bool ret = false;
            if (string.IsNullOrWhiteSpace(commandName))
            {
                throw new ArgumentNullException(nameof(commandName));
            }
            string trimmed_command_name = commandName.Trim().ToLower();
            if (commandLookup.ContainsKey(trimmed_command_name))
            {
                if (commands.Remove(commandLookup[trimmed_command_name]))
                {
                    ret = commandLookup.Remove(trimmed_command_name);
                }
            }
            return ret;
        }

        public TResult Parse(string argument) => Parse(argument, default(TContext));

        public TResult Parse(string argument, out string commandName) => Parse(argument, default(TContext), out commandName);

        public TResult Parse(string argument, TContext context)
        {
            string command_name;
            return Parse(argument, context, out command_name);
        }

        public TResult Parse(string argument, TContext context, out string commandName)
        {
            TResult ret = DefaultParseReturnValue;
            if (argument == null)
            {
                throw new ArgumentNullException(nameof(argument));
            }
            commandName = string.Empty;
            string[] arguments = argument.Split(Delimiter);
            if (arguments.Length > 0)
            {
                string command_name = arguments[0].Trim();
                commandName = command_name;
                if (commandLookup.ContainsKey(command_name))
                {
                    Command<TResult, TContext> command = commandLookup[command_name];
                    uint required_arguments = 0U;
                    foreach (ArgumentString command_argument in command.Arguments)
                    {
                        if (command_argument.IsOptional)
                        {
                            break;
                        }
                        ++required_arguments;
                    }
                    if (arguments.Length > required_arguments)
                    {
                        string[] input_arguments = new string[arguments.Length - 1];
                        for (int i = 0; i < input_arguments.Length; i++)
                        {
                            input_arguments[i] = arguments[i + 1];
                        }
                        ret = command.OnExecute(input_arguments, context);
                    }
                }
            }
            return ret;
        }
    }

    public class Commands<T> : Commands<T, object>
    {
        public Commands(char delimiter = ' ', T defaultParseReturnValue = default(T)) : base(delimiter, defaultParseReturnValue)
        {
            // ...
        }
    }
}
