using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AssemblyDefender.Common.Collections;

namespace AssemblyDefender.Common.CommandLine
{
    public class CommandLineParser
    {
        #region Fields

        private char _keyCharacter = '/';
        private char _valueCharacter = ':';
        private Argument _defaultArgument;
        private Dictionary<string, Argument> _arguments = new Dictionary<string, Argument>(StringComparer.OrdinalIgnoreCase);

        #endregion

        #region Ctors

        public CommandLineParser()
        {
        }

        #endregion

        #region Properties

        public char KeyCharacter
        {
            get { return _keyCharacter; }
            set { _keyCharacter = value; }
        }

        public char ValueCharacter
        {
            get { return _valueCharacter; }
            set { _valueCharacter = value; }
        }

        public Argument DefaultArgument
        {
            get { return _defaultArgument; }
            set { _defaultArgument = value; }
        }

        #endregion

        #region Methods

        public void AddArgument(string name, Argument argument)
        {
            AddArgument(name, null, argument);
        }

        public void AddArgument(string name, string shortName, Argument argument)
        {
            _arguments.Add(name, argument);

            if (!string.IsNullOrEmpty(shortName))
            {
                _arguments.Add(shortName, argument);
            }
        }

        public void Parse()
        {
            var args = Environment.GetCommandLineArgs();

            // First argument is app path.
            if (args.Length < 2)
                return;

            // Remove first argument.
            var enumerator = new ArrayEnumerator<string>(args, 1, args.Length - 1);
            Parse(enumerator);
        }

        public void Parse(IEnumerable<string> args)
        {
            foreach (string arg in args)
            {
                if (string.IsNullOrEmpty(arg))
                    continue;

                if (arg[0] == _keyCharacter)
                {
                    string name;
                    string value = null;
                    int index = arg.IndexOf(_valueCharacter, 1);
                    if (index > 1)
                    {
                        if (index == arg.Length - 1)
                        {
                            throw new CommandLineException(string.Format(SR.CommandLineUnrecognizedOption, arg));
                        }

                        name = arg.Substring(1, index - 1);
                        value = arg.Substring(index + 1);

                        Argument argument;
                        if (!_arguments.TryGetValue(name, out argument))
                        {
                            throw new CommandLineException(string.Format(SR.CommandLineUnrecognizedOption, arg));
                        }

                        if (argument is BoolArgument)
                        {
                            throw new CommandLineException(string.Format(SR.CommandLineUnrecognizedOption, arg));
                        }

                        SetValue(argument, name, value);
                    }
                    else
                    {
                        bool boolValue = true;
                        bool hasSign = false;
                        int nameLength = arg.Length - 1;
                        if (arg.Length > 2)
                        {
                            char lastChar = arg[arg.Length - 1];
                            if (lastChar == '+')
                            {
                                hasSign = true;
                                boolValue = true;
                                nameLength--;
                            }
                            else if (lastChar == '-')
                            {
                                hasSign = true;
                                boolValue = false;
                                nameLength--;
                            }
                        }

                        name = arg.Substring(1, nameLength);

                        Argument argument;
                        if (!_arguments.TryGetValue(name, out argument))
                        {
                            throw new CommandLineException(string.Format(SR.CommandLineUnrecognizedOption, arg));
                        }

                        var boolArgument = argument as BoolArgument;
                        if (boolArgument == null)
                        {
                            throw new CommandLineException(string.Format(SR.CommandLineUnrecognizedOption, arg));
                        }

                        if (hasSign && !boolArgument.AllowSign)
                        {
                            throw new CommandLineException(string.Format(SR.CommandLineUnrecognizedOption, arg));
                        }

                        boolArgument.Value = boolValue;
                    }
                }
                else
                {
                    if (_defaultArgument != null)
                    {
                        SetValue(_defaultArgument, "default", arg);
                    }
                    else
                    {
                        throw new CommandLineException(string.Format(SR.CommandLineUnrecognizedOption, arg));
                    }
                }
            }

            // Ensure required properties are specified.
            foreach (var kvp in _arguments)
            {
                var argument = kvp.Value;
                if (argument.Required && !argument.SeenValue)
                {
                    throw new CommandLineException(string.Format(SR.CommandLineMissingOption, kvp.Key));
                }
            }
        }

        private void SetValue(Argument argument, string name, string value)
        {
            if (argument.AtMostOnce && argument.SeenValue)
            {
                throw new CommandLineException(string.Format(SR.CommandLineDuplicateOption, name));
            }

            argument.SeenValue = true;

            try
            {
                argument.SetValue(value);
            }
            catch (Exception)
            {
                throw new CommandLineException(string.Format(SR.CommandLineInvalidValue, name, value));
            }
        }

        #endregion
    }
}
