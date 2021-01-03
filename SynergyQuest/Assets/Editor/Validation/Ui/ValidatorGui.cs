using UnityEditor;
using UnityEngine;

namespace Editor.Validation.Ui
{
    public class ValidatorGui: EditorWindow
    {
        private bool autofix = false;
        private CheckSelector checkSelector = new CheckSelector();

        [MenuItem("SynergyQuest Tools/Validators/Validator GUI")]
        public static void Display()
        {
            GetWindow<ValidatorGui>();
        }
        
        private void OnGUI()
        {
            GUILayout.Label("Issues to check for:");
            var checkFactories = checkSelector.OnGUI();

            autofix = EditorGUILayout.Toggle("Autofix", autofix);
            if (GUILayout.Button("Validate"))
            {
                foreach (var checkFactory in checkFactories)
                {
                    var check = checkFactory.Invoke();
                    var issue = check.PerformCheck();

                    if (issue != null)
                    {
                        if (autofix && issue.CanAutofix)
                        {
                            issue.Autofix();
                            Debug.LogWarning($"AUTOFIXED {issue.GetLogText()}");
                        }

                        else
                        {
                            Debug.LogError(issue.GetLogText());
                        }
                    }
                }
            }
        }
    }
}