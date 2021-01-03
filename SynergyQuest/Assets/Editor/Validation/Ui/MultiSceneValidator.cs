using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Editor.Validation.Ui;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Editor.Validation
{
    public class MultiSceneValidator: EditorWindow
    {
        private Vector2 scrollPosition = Vector2.zero;
        private HashSet<string> scenes = new HashSet<string>();
        private bool autofix = false;
        private CheckSelector checkSelector = new CheckSelector();

        [MenuItem("SynergyQuest Tools/Validators/Multi Scene Validator")]
        public static void Display()
        {
            GetWindow<MultiSceneValidator>();
        }
        
        private void OnGUI()
        {
            GUILayout.Label("Issues to check for:");
            var issueFactories = checkSelector.OnGUI();
            
            GUILayout.Label("Scenes to validate:");
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
            foreach (var scene in scenes)
            {
                EditorGUILayout.BeginHorizontal();
                if (!GUILayout.Button("X", GUILayout.ExpandWidth(false)))
                {
                    GUILayout.Label(Path.GetFileName(scene.CorrectFsSlashes()));
                }

                else
                {
                    scenes.Remove(scene);
                    Repaint();
                    return;
                }
                EditorGUILayout.EndHorizontal();
            }
            EditorGUILayout.EndScrollView();

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Add Scene"))
            {
                var selectedPath =
                    EditorUtility.OpenFilePanel("Select scene file", PathUtils.GetInstallDirectory(), "unity");
                if (selectedPath != null)
                {
                    scenes.Add(selectedPath);
                }
            }
            
            if (GUILayout.Button("Add Folder"))
            {
                var selectedPath =
                    EditorUtility.OpenFolderPanel("Select scene folder", PathUtils.GetInstallDirectory(), null);
                
                if (selectedPath != null)
                {
                    Directory
                        .EnumerateFiles(selectedPath.CorrectFsSlashes(), "*.unity", SearchOption.AllDirectories)
                        .ForEach(scenePath => scenes.Add(scenePath.WinToNixPath()));
                }
            }

            if (GUILayout.Button("Clear"))
            {
                scenes.Clear();
                Repaint();
                return;
            }
            
            EditorGUILayout.EndHorizontal();

            autofix = EditorGUILayout.Toggle("Autofix", autofix);
            if (GUILayout.Button("Validate"))
            {
                RunChecks(issueFactories, autofix);
            }
        }

        private void RunChecks(Func<Check>[] checkFactories, bool autofix)
        {
            var installPath = PathUtils.GetInstallDirectory();

            var originalLoadedScenes = Enumerable.Range(0, EditorSceneManager.sceneCount)
                .Select(i => EditorSceneManager.GetSceneAt(i))
                .ToArray();

            var originalLoadedScenePaths = originalLoadedScenes
                .Select(scene => scene.path)
                .ToArray();

            EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo();
            originalLoadedScenes.Skip(1).ForEach(scene => EditorSceneManager.CloseScene(scene, true));

            foreach (var scenePath in scenes)
            {
                var relativeScenePath = scenePath.WinToNixPath().Remove(0, installPath.Length + 1);
                
                EditorSceneManager.OpenScene(relativeScenePath);
                checkFactories
                    .Select(f => f.Invoke())
                    .SelectNotNull(check => check.PerformCheck())
                    .ForEach(issue =>
                    {
                        var baseMsg = $"In file {relativeScenePath}: {issue.GetLogText()}";

                        if (autofix && issue.CanAutofix)
                        {
                            issue.Autofix();
                            Debug.LogWarning($"AUTOFIXED {baseMsg}");
                        }

                        else
                        {
                            Debug.LogError(baseMsg);
                        }
                    });

                if (autofix)
                {
                    EditorSceneManager.MarkAllScenesDirty();
                    EditorSceneManager.SaveOpenScenes();
                }
            }

            if (originalLoadedScenePaths.Length > 0)
            {
                EditorSceneManager.OpenScene(originalLoadedScenePaths[0]);
            }
            originalLoadedScenePaths.Skip(1).ForEach(path => EditorSceneManager.OpenScene(path, OpenSceneMode.Additive));
        }

    }
}