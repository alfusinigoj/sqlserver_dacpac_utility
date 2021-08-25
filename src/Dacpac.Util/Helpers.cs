using System;

namespace Dacpac.CommandLine
{
    public class Helpers
    {
        public static string GetFromEnvironmentVariableIfSo(string argumentValue)
        {
            if (argumentValue.StartsWith("env:"))
            {
                var variableName = argumentValue.Substring(4, argumentValue.Length - 4);
                argumentValue = Environment.GetEnvironmentVariable(variableName);
                if (String.IsNullOrWhiteSpace(argumentValue))
                    throw new ArgumentException($"{variableName} is not set!");
            }

            return argumentValue;
        }
    }
}
