// Copyright (c) Improbable Worlds Ltd, All Rights Reserved

using System;
using System.Collections.Generic;

namespace Improbable.Unity.Util
{
    /// <summary>
    ///     Utilities to aid in parsing command line arguments and their default values.
    /// </summary>
    public static class CommandLineUtil
    {
        /// <summary>
        ///     Gets a value specified on the command line, in the form "+key" "value"
        /// </summary>
        /// <typeparam name="T">The type of the value.</typeparam>
        /// <param name="arguments">The arguments to inspect.</param>
        /// <param name="configKey">The name of the key, without the leading +, e.g. "key"</param>
        /// <param name="defaultValue">The value to return if the key was not specified on the command line.</param>
        /// <returns>The value of the key, or defaultValue if the key was not specified on the command line.</returns>
        public static T GetCommandLineValue<T>(IList<string> arguments, string configKey, T defaultValue)
        {
            var dict = ParseCommandLineArgs(arguments);
            T configValue;
            if (TryGetConfigValue(dict, configKey, out configValue))
            {
                return configValue;
            }

            return defaultValue;
        }

        /// <summary>
        ///     Tries to get a value specified on the command line, in the form "+key" "value"
        /// </summary>
        /// <typeparam name="T">The type of the value.</typeparam>
        /// <param name="arguments">The arguments to inspect.</param>
        /// <param name="configKey">The name of the key, without the leading +, e.g. "key"</param>
        /// <param name="configValue">The variable to store the result in, if found.</param>
        /// <returns>True if the key was found.</returns>
        public static bool TryGetCommandLineValue<T>(IList<string> arguments, string configKey, out T configValue)
        {
            var dict = ParseCommandLineArgs(arguments);
            return TryGetConfigValue(dict, configKey, out configValue);
        }

        /// <summary>
        ///     Try to get am explicitly-typed value from the dictionary.
        /// </summary>
        /// <returns>True if the value was found, false otherwise.</returns>
        public static bool TryGetConfigValue<T>(Dictionary<string, string> dictionary, string configName, out T value)
        {
            string strValue;
            var desiredType = typeof(T);
            if (dictionary.TryGetValue(configName, out strValue))
            {
                if (desiredType.IsEnum)
                {
                    try
                    {
                        value = (T) Enum.Parse(desiredType, strValue);
                        return true;
                    }
                    catch (Exception e)
                    {
                        throw new FormatException(string.Format("Unable to parse argument {0} as enum {1}.", strValue, desiredType.Name), e);
                    }
                }

                value = (T) Convert.ChangeType(strValue, typeof(T));
                return true;
            }

            value = default(T);
            return false;
        }

        /// <summary>
        ///     Parses a series of command line option pairs, beginning with '+' into a dictionary.
        /// </summary>
        /// <remarks>
        ///     The arguments must be in the form: "+flag1 value1 +flag2 value2".
        /// </remarks>
        public static Dictionary<string, string> ParseCommandLineArgs(IList<string> args)
        {
            var config = new Dictionary<string, string>();
            for (var i = 0; i < args.Count; i++)
            {
                var flag = args[i];
                if (flag.StartsWith("+"))
                {
                    var flagArg = args[i + 1];
                    var strippedOfPlus = flag.Substring(1, flag.Length - 1);
                    config[strippedOfPlus] = flagArg;
                    // We've already processed the next argument, so skip it.
                    i++;
                }
            }

            return config;
        }
    }
}
