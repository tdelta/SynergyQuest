// This file is part of the "Synergy Quest" game
// (github.com/tdelta/SynergyQuest).
// 
// Copyright (c) 2021
//   Marc Arnold     (m_o_arnold@gmx.de)
//   Martin Kerscher (martin_x@live.de)
//   Jonas Belouadi  (jonas.belouadi@posteo.net)
//   Anton W Haubner (anton.haubner@outlook.de)
// 
// This program is free software; you can redistribute it and/or modify it
// under the terms of the GNU General Public License as published by the Free
// Software Foundation; either version 3 of the License, or (at your option) any
// later version.
// 
// This program is distributed in the hope that it will be useful, but WITHOUT
// ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or
// FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for
// more details.
// 
// You should have received a copy of the GNU General Public License along with
// this program; if not, see <https://www.gnu.org/licenses>.
// 
// Additional permission under GNU GPL version 3 section 7 apply,
// see `LICENSE.md` at the root of this source code repository.

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
                            Debug.Log($"AUTOFIXED {issue.GetLogText()}");
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