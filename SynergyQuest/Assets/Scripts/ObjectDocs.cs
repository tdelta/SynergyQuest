using UnityEngine;

#if UNITY_EDITOR

using System;
using UnityEditor;
using System.Runtime.InteropServices;
using WebSocketSharp;

#endif

/**
 * Allows to set a docstring for a Unity object / prefab.
 *
 * It will be displayed in the top of the inspector using <see cref="GameObjectInspector"/>
 */
public class ObjectDocs : MonoBehaviour
{
    [SerializeField, TextArea(5, 15)]
    private string docstring = null;

    public string Docstring => docstring;
}

#if UNITY_EDITOR

/**
 * Custom editor for GameObject.
 * 
 * Allows to display custom GUI elements at the top of the inspector.
 * Based on https://forum.unity.com/threads/inject-gui-code-in-inspectorwindow.313359/
 *
 * Currently only displays docstrings which have been set using <see cref="objectDocs"/>
 *
 * TODO: It would be cleaner to directly patch the OnInspectorGUI method of the original GameObjectInspector
 *   (https://github.com/Unity-Technologies/UnityCsReference/blob/master/Editor/Mono/Inspector/GameObjectInspector.cs)
 *   instead. Could maybe be done like this
 *     https://docs.microsoft.com/en-us/dotnet/api/system.reflection.emit.methodrental.swapmethodbody?view=netframework-4.8
 *   or like this
 *     https://stackoverflow.com/questions/7299097/dynamically-replace-the-contents-of-a-c-sharp-method
 */
[CanEditMultipleObjects, CustomEditor(typeof(GameObject))]
public class GameObjectInspector : Editor 
{
    // Variables to capture handles to the original GameObjectInspector instead
    private Type inspectorType;
    private Editor editorInstance;
    private _MethodInfo defaultHeaderGUI;
    private _MethodInfo defaultPreviewGUI;

    private ObjectDocs objectDocs;
   
    GameObjectInspector()
    {
        // Capture handles to the original GameObjectInspector methods
        inspectorType = System.Reflection.Assembly.GetAssembly(typeof(Editor)).GetType("UnityEditor.GameObjectInspector");
        
        defaultHeaderGUI = inspectorType.GetMethod(
            "OnHeaderGUI",
            System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic
        );
        
        defaultPreviewGUI = inspectorType.GetMethod(
            "OnPreviewGUI",
            System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic
        );
    }
    
    private void OnEnable()
    {
        // If we dont have an instance of the original GameObjectInspector, create one
        if (editorInstance == null)
        {
            editorInstance = Editor.CreateEditor(target, inspectorType);
        }
        
        // If there is a ObjectDocs component in the target object of this inspector, grab it
        var typedTarget = (GameObject) target;
        if (typedTarget.TryGetComponent(out ObjectDocs objectDocs))
        {
            this.objectDocs = objectDocs;
        }

        else
        {
            this.objectDocs = null;
        }
    }
    
    protected override void OnHeaderGUI()
    {
        // Draw the original header of the GameObjectInspector
        defaultHeaderGUI.Invoke(editorInstance, null);
    }

    public override void OnInspectorGUI()
    {
        // If ObjectDocs have been added to the target object of this inspector, display them
        if (!ReferenceEquals(this.objectDocs, null) && !this.objectDocs.Docstring.IsNullOrEmpty())
        {
            GUILayout.Space(10);
            GUILayout.Label("Docstring", EditorStyles.boldLabel);
            GUILayout.Space(5);
            GUILayout.Label(new GUIContent(this.objectDocs.Docstring, $"Edit docstring in {nameof(ObjectDocs)} component."), EditorStyles.wordWrappedLabel);
            GUILayout.Space(10);
        }
    }

    public virtual void OnPreviewGUI(Rect r, GUIStyle background)
    {
        defaultPreviewGUI.Invoke(editorInstance, new object[] {r, background});
    }
}

#endif
