using System;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Editor.Validation.Ui
{
    public class CheckSelector
    {
        private Vector2 scrollPosition = Vector2.zero;
        private readonly bool[] enabledChecks = Enumerable.Repeat(true, CheckSharedMethods.CheckFactories.Value.Count).ToArray();
        
        public Func<Check>[] OnGUI()
        {
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition, GUILayout.ExpandHeight(false));
            CheckSharedMethods
                .CheckFactories
                .Value
                .ForEach((issueInfo, idx) =>
                    enabledChecks[idx] = EditorGUILayout.ToggleLeft(issueInfo.Item1, enabledChecks[idx], GUILayout.ExpandWidth(true))
                );
            EditorGUILayout.EndScrollView();

            return CheckSharedMethods
                .CheckFactories
                .Value
                .Zip(enabledChecks, (issueInfo, enabled) =>
                    enabled ? issueInfo.Item2 : null
                )
                .Where(issueFactory => issueFactory != null)
                .ToArray();
        }
    }
}