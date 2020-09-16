using System;
using System.Collections.Generic;
using System.Text;

namespace SpaceEngineersPrograms
{
    public class Command<TResult, TContext>
    {
        public string Name { get; private set; }

        public string Description { get; private set; }

        public string FullDescription { get; private set; }

        public Command<TResult, TContext> AliasTo { get; private set; }

        public bool IsAlias => (AliasTo != null);

        public CommandDelegate<TResult, TContext> OnExecute { get; private set; }

        public IReadOnlyList<ArgumentString> Arguments { get; private set; }

        public string GetHelpString(char delimiter)
        {
            StringBuilder help_string_builder = new StringBuilder();
            help_string_builder.AppendLine(Description);
            help_string_builder.AppendLine();
            help_string_builder.AppendLine(FullDescription);
            help_string_builder.AppendLine();
            help_string_builder.Append("Usage: ");
            help_string_builder.Append(Name);
            foreach (ArgumentString argument in Arguments)
            {
                help_string_builder.Append(delimiter);
                help_string_builder.Append("<");
                help_string_builder.Append(argument);
                help_string_builder.Append(">");
            }
            string ret = help_string_builder.ToString();
            help_string_builder.Clear();
            return ret;
        }

        public Command(string name, string description, string fullDescription, CommandDelegate<TResult, TContext> onExecute, params ArgumentString[] arguments)
        {
            if (name == null)
            {
                throw new ArgumentNullException(nameof(name));
            }
            if (description == null)
            {
                throw new ArgumentNullException(nameof(description));
            }
            if (fullDescription == null)
            {
                throw new ArgumentNullException(nameof(fullDescription));
            }
            if (onExecute == null)
            {
                throw new ArgumentNullException(nameof(onExecute));
            }
            if (arguments == null)
            {
                throw new ArgumentNullException(nameof(arguments));
            }
            Name = name;
            Description = description;
            FullDescription = fullDescription;
            AliasTo = null;
            OnExecute = onExecute;
            Arguments = arguments;
        }

        private Command(string alias, Command<TResult, TContext> command)
        {
            if (alias == null)
            {
                throw new ArgumentNullException(nameof(alias));
            }
            Name = alias;
            Description = command.Description;
            FullDescription = command.FullDescription;
            AliasTo = command;
            OnExecute = command.OnExecute;
            Arguments = command.Arguments;
        }

        public static Command<TResult, TContext> New(string name, CommandDelegate<TResult, TContext> onExecute, params ArgumentString[] arguments) => new Command<TResult, TContext>(name, "Command: " + name, "No help topic available.", onExecute, arguments);

        public static Command<TResult, TContext> New(string name, string description, string fullDescription, CommandDelegate<TResult, TContext> onExecute, params ArgumentString[] arguments) => new Command<TResult, TContext>(name, description, fullDescription, onExecute, arguments);

        public Command<TResult, TContext> CreateAlias(string alias) => new Command<TResult, TContext>(alias, this);

        public override bool Equals(object obj) => ((obj == null) ? string.Empty : obj.ToString()).Equals(Name);

        public override int GetHashCode() => Name.GetHashCode();

        public override string ToString() => Name;
    }

    public class Command<TResult> : Command<TResult, object>
    {
        public Command(string name, string description, string fullDescription, CommandDelegate<TResult, object> onExecute, params ArgumentString[] arguments) : base(name, description, fullDescription, onExecute, arguments)
        {
            // ...
        }
    }
}
