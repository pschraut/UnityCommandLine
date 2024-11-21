//
// CommandLine for Unity. Copyright (c) 2020-2024 Peter Schraut (www.console-dev.de). See LICENSE.md
// https://github.com/pschraut/UnityCommandLine
//
#pragma warning disable IDE1006, IDE0017
#pragma warning disable IDE0051 // Remove unused private members
#pragma warning disable IDE0052 // Remove unread private members
#pragma warning disable IDE0032 // Use auto property
#pragma warning disable CS0649 // Field 'xxx' is never assigned to, and will always have its default value
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Oddworm.Framework
{
    /// <summary>
    /// The CommandLine class provides the ability to parse a "Key/Value-pair like" text document,
    /// whose values can be queried in strongly typed fashion. It supports the types string, int, float, bool and enum.
    /// It supports C# like single and multi-line comments, as well quoted arguments, if they span across multiple words.
    /// </summary>
    public static partial class CommandLine
    {
        /// <summary>
        /// Gets/sets whether the CommandLine is enabled. A disabled CommandLine returns the defaultValue always.
        /// Use the ODDWORM_COMMANDLINE_DISABLE scripting define symbol to let the compiler remove the CommandLine method bodies.
        /// If the ODDWORM_COMMANDLINE_DISABLE scripting define symbol has been set, it returns false always.
        /// </summary>
        public static bool isEnabled
        {
            get
            {
#if ODDWORM_COMMANDLINE_DISABLE
                return false;
#else
                return s_Enabled;
#endif
            }
            set
            {
#if ODDWORM_COMMANDLINE_DISABLE
                // Do nothing
#else
                s_Enabled = value;
#endif
            }
        }

        /// <summary>
        /// Gets the text that was passed to Init().
        /// If the ODDWORM_COMMANDLINE_DISABLE scripting define symbol has been set, it returns an empty string always.
        /// </summary>
        public static string text
        {
            get
            {
#if ODDWORM_COMMANDLINE_DISABLE
                return "";
#else
                return s_Text;
#endif
            }
        }

        /// <summary>
        /// An event that is invoked after Commandline.Init has been called.
        /// This event is invoked even if the ODDWORM_COMMANDLINE_DISABLE scripting define symbol has been set.
        /// </summary>
        public static Action onInitialized
        {
            // Didn't mark as "event" on purpose, so user-code can also reset it (clear all handlers)
            get;
            set;
        }

#if !ODDWORM_COMMANDLINE_DISABLE
        static bool s_Enabled; // Whether the command-line is enabled
        static string s_Text = ""; // The text passed to Init()
        static readonly List<string> s_Parsed = new List<string>(16); // The parses text as one "token" per entry
        static readonly List<Param> s_Cached = new List<Param>(16); // The params already queried from the user

        struct Param
        {
            public enum Type
            {
                None,
                String,
                Int,
                Float,
                Bool,
                Enum
            }

            public string key;
            public int keyHash;
            public Type type;

            public string stringValue;
            public int intValue;
            public float floatValue;
            public bool boolValue;
            public Enum enumValue;

            public override string ToString()
            {
                return string.Format("key: {0}, type: {1}", key, type);
            }
        }
#endif // !ODDWORM_COMMANDLINE_DISABLE

        static CommandLine()
        {
#if ODDWORM_COMMANDLINE_DISABLE
            isEnabled = false;
#else
            isEnabled = true;
#endif
        }

        /// <summary>
        /// Initialize the CommandLine with the specified text.
        /// </summary>
        /// <param name="text">The text that contains all the command-line parameters.</param>
        /// <remarks>
        /// A "LoadFromFile" method isn't provided, because it needs to be handled on various
        /// platforms differently, depending where the file is located.
        /// Thus, it's up to user-code to load the file and pass the content to Init() instead.
        /// </remarks>
        public static void Init(string text)
        {
#if !ODDWORM_COMMANDLINE_DISABLE
            // Clear the parsed and cached values always.
            // The parse list gets populated right in this method.
            // The cached list gets populated as values are queried from the CommandLine.
            s_Parsed.Clear();
            s_Cached.Clear();

            // Remember the text. We do this, in case user code wants to call
            // Init() multiple times (which is valid) to concatenate the command-line text, like:
            // CommandLine.Init(CommandLine.text + "\n" + newCommandLine);
            s_Text = text;

            var sb = new System.Text.StringBuilder(256); // the currently parsed word
            var i = 0; // the current index into the raw text

            // Parse the text...
            while (!string.IsNullOrEmpty(s_Text) && i < s_Text.Length)
            {
                var c = s_Text[i++];

                // A white-space character indicates a new argument on the command-line
                if (char.IsWhiteSpace(c))
                {
                    if (sb.Length > 0)
                    {
                        s_Parsed.Add(sb.ToString());
                        sb.Clear();
                    }
                    continue;
                }

                // A quote represents a string that is allowed to contain white-space characters
                if (c == '\"')
                {
                    // Read until a quote occurs again
                    while (i < s_Text.Length)
                    {
                        c = s_Text[i++];

                        // Two quotes in succession represent a single quote in a quoted string
                        // "This is ""great""."
                        if (c == '\"' && i < s_Text.Length && s_Text[i] == '\"')
                        {
                            sb.Append(c);
                            i++;
                            continue;
                        }
                        else if (c == '\"') // end-quote
                        {
                            break;
                        }

                        // Otherwise just use the character
                        sb.Append(c);
                    }

                    continue;
                }

                // Skip C-style block comments like /* This is a comment */
                // Check if we found the starting character sequence /*
                if (c == '/' && i < s_Text.Length && s_Text[i] == '*')
                {
                    // If there is no space between the last world and the comment,
                    // then we need to flush the current stringbuffer.
                    if (sb.Length > 0)
                    {
                        s_Parsed.Add(sb.ToString());
                        sb.Clear();
                    }

                    // Read until closing character sequence */ occurs
                    while (i < s_Text.Length)
                    {
                        c = s_Text[i++];

                        if (c == '*' && i < s_Text.Length && s_Text[i] == '/')
                        {
                            i++;
                            break;
                        }
                    }

                    continue;
                }

                // Skip C-style line comment like // This is a comment
                if (c == '/' && i < s_Text.Length && s_Text[i] == '/')
                {
                    // If there is no space between the last world and the comment,
                    // then we need to flush the current stringbuffer.
                    if (sb.Length > 0)
                    {
                        s_Parsed.Add(sb.ToString());
                        sb.Clear();
                    }

                    // Read until closing character sequence occurs
                    while (i < s_Text.Length)
                    {
                        c = s_Text[i++];

                        // Line end?
                        if (c == '\n' || c == '\r' || c == '\0')
                        {
                            i--; // decrement so char.IsWhiteSpace at the beginning of the loop kicks in
                            break;
                        }
                    }

                    continue;
                }

                sb.Append(c);
            }

            // In case there is no whitespace as last character, the string buffer
            // can still contain something. So we check if it has content.
            if (sb.Length > 0)
            {
                s_Parsed.Add(sb.ToString());
                sb.Clear();
            }
#endif
            // Raise the event always, even when disabled. This is to allow subscribed code
            // to get executed rather than ending up uninitialized. Subscribed code will then
            // most likely query CommandLine options and if ODDWORM_COMMANDLINE_DISABLE is present,
            // would receive default values as expected.
            onInitialized?.Invoke();
        }

        /// <summary>
        /// Gets whether the specified key exists in the command-line.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns>true if it exists, false otherwise.</returns>
        public static bool HasKey(string key)
        {
#if ODDWORM_COMMANDLINE_DISABLE
            return false;
#else
            if (!isEnabled)
                return false;

            for (var n = s_Parsed.Count - 1; n >= 0; --n)
            {
                if (string.Equals(s_Parsed[n], key, StringComparison.OrdinalIgnoreCase))
                    return true;
            }
            return false;
#endif
        }

        /// <summary>
        /// Gets the value corresponding to the specified key.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="defaultValue">The return value if the key does not exist.</param>
        /// <returns>The value if the key exists, otherwise the defaultValue.</returns>
        public static string GetString(string key, string defaultValue)
        {
#if ODDWORM_COMMANDLINE_DISABLE
            return defaultValue;
#else
            if (!isEnabled)
                return defaultValue;

            // Check if it's in the cache already
            if (TryGetParam(key, Param.Type.String, out Param param))
                return param.stringValue;

            // It's not in the cache yet. Scan the command-line strings for the requested key
            var value = defaultValue;
            var hasKey = false;
            for (var n = 0; n < s_Parsed.Count - 1; ++n)
            {
                if (!string.Equals(s_Parsed[n], key, StringComparison.OrdinalIgnoreCase))
                    continue;

                value = s_Parsed[n + 1];
                hasKey = true;
                break;
            }

            if (hasKey)
            {
                // Add the param to the cache only when the key was found.
                // This is to support to pass different default values when no key for it exists.
                var newParam = new Param();
                newParam.key = key;
                newParam.keyHash = Animator.StringToHash(key);
                newParam.type = Param.Type.String;
                newParam.stringValue = value;
                s_Cached.Add(newParam);
            }

            return value;
#endif
        }

        /// <summary>
        /// Gets the value corresponding to the specified key.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="defaultValue">The return value if the key does not exist.</param>
        /// <returns>The value if the key exists, otherwise the defaultValue.</returns>
        public static float GetFloat(string key, float defaultValue)
        {
#if ODDWORM_COMMANDLINE_DISABLE
            return defaultValue;
#else
            if (!isEnabled)
                return defaultValue;

            // Check if it's in the cache already
            if (TryGetParam(key, Param.Type.Float, out Param param))
                return param.floatValue;

            // It's not in the cache yet. Scan the command-line strings for the requested key
            var value = defaultValue;
            var hasKey = false;
            for (var n = 0; n < s_Parsed.Count - 1; ++n)
            {
                if (!string.Equals(s_Parsed[n], key, StringComparison.OrdinalIgnoreCase))
                    continue;

                if (!float.TryParse(s_Parsed[n + 1], out value))
                    value = defaultValue;

                hasKey = true;
                break;
            }

            if (hasKey)
            {
                // Add the param to the cache
                var newParam = new Param();
                newParam.key = key;
                newParam.keyHash = Animator.StringToHash(key);
                newParam.type = Param.Type.Float;
                newParam.floatValue = value;
                s_Cached.Add(newParam);
            }

            return value;
#endif
        }

        /// <summary>
        /// Gets the value corresponding to the specified key.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="defaultValue">The return value if the key does not exist.</param>
        /// <returns>The value if the key exists, otherwise the defaultValue.</returns>
        public static int GetInt(string key, int defaultValue)
        {
#if ODDWORM_COMMANDLINE_DISABLE
            return defaultValue;
#else
            if (!isEnabled)
                return defaultValue;

            // Check if it's in the cache already
            if (TryGetParam(key, Param.Type.Int, out Param param))
                return param.intValue;

            // It's not in the cache yet. Scan the command-line strings for the requested key
            var value = defaultValue;
            var hasKey = false;
            for (var n = 0; n < s_Parsed.Count - 1; ++n)
            {
                if (!string.Equals(s_Parsed[n], key, StringComparison.OrdinalIgnoreCase))
                    continue;

                if (!int.TryParse(s_Parsed[n + 1], out value))
                    value = defaultValue;

                hasKey = true;
                break;
            }

            if (hasKey)
            {
                // Add the param to the cache
                var newParam = new Param();
                newParam.key = key;
                newParam.keyHash = Animator.StringToHash(key);
                newParam.type = Param.Type.Int;
                newParam.intValue = value;
                s_Cached.Add(newParam);
            }

            return value;
#endif
        }

        /// <summary>
        /// Gets the value corresponding to the specified key.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="defaultValue">The return value if the key does not exist.</param>
        /// <returns>The value if the key exists, otherwise the defaultValue.</returns>
        public static bool GetBool(string key, bool defaultValue)
        {
#if ODDWORM_COMMANDLINE_DISABLE
            return defaultValue;
#else
            if (!isEnabled)
                return defaultValue;

            // Check if it's in the cache already
            if (TryGetParam(key, Param.Type.Bool, out Param param))
                return param.boolValue;

            // It's not in the cache yet. Scan the command-line strings for the requested key.
            var value = defaultValue;
            var hasKey = false;
            for (var n = 0; n < s_Parsed.Count - 1; ++n)
            {
                if (!string.Equals(s_Parsed[n], key, StringComparison.OrdinalIgnoreCase))
                    continue;

                // We allow to specify bool values as 0,false and 1,true
                if (int.TryParse(s_Parsed[n + 1], out int b))
                {
                    // Everything else than 0 is true
                    value = b != 0;
                }
                else
                {
                    // Everything else than false is true
                    value = !string.Equals(s_Parsed[n + 1], "false", StringComparison.OrdinalIgnoreCase);
                }

                hasKey = true;
                break;
            }

            if (hasKey)
            {
                // Add the param to the cache
                var newParam = new Param();
                newParam.key = key;
                newParam.keyHash = Animator.StringToHash(key);
                newParam.type = Param.Type.Bool;
                newParam.boolValue = value;
                s_Cached.Add(newParam);
            }

            return value;
#endif
        }

        /// <summary>
        /// Gets the value corresponding to the specified key.
        /// </summary>
        /// <typeparam name="TEnum">The Enum type.</typeparam>
        /// <param name="key">The key.</param>
        /// <param name="defaultValue">The return value if the key does not exist.</param>
        /// <returns>The value if the key exists, otherwise the defaultValue.</returns>
        public static TEnum GetEnum<TEnum>(string key, TEnum defaultValue)
            where TEnum : struct, System.Enum, IConvertible
        {
#if ODDWORM_COMMANDLINE_DISABLE
            return defaultValue;
#else
            if (!isEnabled)
                return defaultValue;

            // Check if it's in the cache already
            if (TryGetParam(key, Param.Type.Enum, out Param param))
                return (TEnum)param.enumValue;

            TEnum value = defaultValue;
            var hasKey = false;
            for (var n = 0; n < s_Parsed.Count - 1; ++n)
            {
                if (!string.Equals(s_Parsed[n], key, StringComparison.OrdinalIgnoreCase))
                    continue;

                if (!Enum.TryParse(s_Parsed[n + 1], true, out value))
                    value = defaultValue;

                hasKey = true;
                break;
            }

            // Add the param to the cache
            if (hasKey)
            {
                var newParam = new Param();
                newParam.key = key;
                newParam.keyHash = Animator.StringToHash(key);
                newParam.type = Param.Type.Enum;
                newParam.enumValue = value;
                s_Cached.Add(newParam);
            }

            return value;
#endif
        }

#if !ODDWORM_COMMANDLINE_DISABLE
        static bool TryGetParam(string key, Param.Type type, out Param param)
        {
            var keyHash = Animator.StringToHash(key);

            for (var n = 0; n < s_Cached.Count; ++n)
            {
                param = s_Cached[n];

                if (param.type != type)
                    continue;

                if (param.keyHash != keyHash)
                    continue;

                if (!string.Equals(param.key, key, StringComparison.OrdinalIgnoreCase))
                    continue;

                return true;
            }

            param = new Param();
            return false;
        }
#endif
    }
}
