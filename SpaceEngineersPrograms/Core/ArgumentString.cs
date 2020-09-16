using System;

namespace SpaceEngineersPrograms
{
    public struct ArgumentString : IEquatable<ArgumentString>
    {
        public string Argument { get; private set; }

        public bool IsOptional { get; private set; }

        public ArgumentString(string argument, bool isOptional)
        {
            if (argument == null)
            {
                throw new ArgumentNullException(nameof(argument));
            }
            Argument = argument;
            IsOptional = isOptional;
        }

        public static ArgumentString Optional(string argument) => new ArgumentString(argument, true);

        public static ArgumentString Required(string argument) => new ArgumentString(argument, false);

        public static implicit operator ArgumentString(string input) => Required(input);

        public override string ToString() => (Argument + (IsOptional ? " (optional)" : string.Empty));

        public bool Equals(ArgumentString other) => Argument.Equals(other.Argument);
    }
}
