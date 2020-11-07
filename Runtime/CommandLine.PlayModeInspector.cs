//
// CommandLine for Unity. Copyright (c) 2020 Peter Schraut (www.console-dev.de). See LICENSE.md
// https://github.com/pschraut/UnityCommandLine
//
#pragma warning disable IDE1006, IDE0017
#pragma warning disable IDE0051 // Remove unused private members
#pragma warning disable IDE0052 // Remove unread private members
#pragma warning disable IDE0032 // Use auto property
#pragma warning disable CS0649 // Field 'xxx' is never assigned to, and will always have its default value

namespace Oddworm.Framework
{
    public static partial class CommandLine
    {
#if PLAYMODEINSPECTOR_PRESENT && UNITY_EDITOR
        // Install the PlayMode Inspector package to use this code:
        // https://github.com/pschraut/UnityPlayModeInspector
        [Oddworm.Framework.PlayModeInspectorMethod]
        static void PlayModeInspectorMethod()
        {
            UnityEditor.EditorGUILayout.Toggle("Is Enabled", isEnabled);

            // Display the cached/retrieved params.
            // Here we actually know type, so we can display them properly.
            // We know the type, because the particuler Get... method stored it.
            UnityEditor.EditorGUILayout.LabelField("Cached", UnityEditor.EditorStyles.boldLabel);
            for (var n = 0; n < s_Cached.Count; ++n)
            {
                UnityEditor.EditorGUI.indentLevel++;
                var param = s_Cached[n];
                switch (param.type)
                {
                    case Param.Type.String:
                        param.stringValue = UnityEditor.EditorGUILayout.TextField(param.key, param.stringValue);
                        break;

                    case Param.Type.Bool:
                        param.boolValue = UnityEditor.EditorGUILayout.Toggle(param.key, param.boolValue);
                        break;

                    case Param.Type.Int:
                        param.intValue = UnityEditor.EditorGUILayout.IntField(param.key, param.intValue);
                        break;

                    case Param.Type.Float:
                        param.floatValue = UnityEditor.EditorGUILayout.FloatField(param.key, param.floatValue);
                        break;

                    case Param.Type.Enum:
                        if (param.enumValue.GetType().GetCustomAttributes(typeof(System.FlagsAttribute), true).Length > 0)
                            param.enumValue = UnityEditor.EditorGUILayout.EnumFlagsField(param.key, param.enumValue);
                        else
                            param.enumValue = UnityEditor.EditorGUILayout.EnumPopup(param.key, param.enumValue);
                        break;
                }
                UnityEditor.EditorGUI.indentLevel--;

                s_Cached[n] = param;
            }

            UnityEditor.EditorGUILayout.Space();

            // The raw text that was passed to CommandLine to parse
            UnityEditor.EditorGUILayout.LabelField("RAW (Read only)", UnityEditor.EditorStyles.boldLabel);
            UnityEditor.EditorGUI.indentLevel++;
            UnityEditor.EditorGUILayout.TextArea(s_Text);
            UnityEditor.EditorGUI.indentLevel--;

            UnityEditor.EditorGUILayout.Space();

            // Display parsed paramters. We don't know the type, thus we just display each one as text.
            UnityEditor.EditorGUILayout.LabelField("Parsed (Read only)", UnityEditor.EditorStyles.boldLabel);
            UnityEditor.EditorGUI.indentLevel++;
            for (var n = 0; n < s_Parsed.Count; ++n)
                UnityEditor.EditorGUILayout.TextField(s_Parsed[n]);
            UnityEditor.EditorGUI.indentLevel--;
        }
#endif // PLAYMODEINSPECTOR_PRESENT
    }
}
