using UnityEngine;

#if UNITY_EDITOR

using HarmonyLib;
using System;
using UnityEditor;
using System.Runtime.InteropServices;
using WebSocketSharp;

#endif

/**
 * Allows to set a docstring for a Unity object / prefab.
 *
 * It will be displayed in the top of the inspector using <see cref="GameObjectInspectorPatch"/>
 */
public class ObjectDocs : MonoBehaviour
{
    [SerializeField, TextArea(5, 15)]
    private string docstring = null;

    public string Docstring => docstring;
}

#if UNITY_EDITOR


/**
 * <summary>
 * Patches the code of the Unity inspector GUI for GameObjects at runtime, so that we can insert custom GUI elements at
 * the top of the inspector.
 * 
 * Currently only displays docstrings which have been set using <see cref="ObjectDocs"/>
 * </summary>
 *
 * <remarks>
 * Uses the Harmony2 library (https://github.com/pardeike/Harmony) to inject custom code at the end of
 * <see cref="UnityEngine.GameObjectInspector.OnInspectorGUI"/>
 * (https://github.com/Unity-Technologies/UnityCsReference/blob/master/Editor/Mono/Inspector/GameObjectInspector.cs).
 * </remarks>
 */
static class GameObjectInspectorPatch
{
    // Get a handle to the UnityEditor.GameObjectInspector type
    // We need to use reflection here, since the type is marked as internal and can thus not be accessed by "typeof(..)"
    static Lazy<Type> inspectorType = new Lazy<Type>(() => 
        System.Reflection.Assembly.GetAssembly(typeof(Editor)).GetType("UnityEditor.GameObjectInspector")
    );
    
    // Get a handle to the "target" getter method of UnityEditor.GameObjectInspector
    static Lazy<_MethodInfo> mTarget = new Lazy<_MethodInfo>(() => 
        inspectorType.Value.GetProperty("target")?.GetGetMethod()
    );
    
    /**
     * <summary>
     * Appends a call to <see cref="Postfix"/> at the end of the
     * <see cref="UnityEngine.GameObjectInspector.OnInspectorGUI"/> method
     * </summary>
     * 
     * <remarks>
     * Is automatically executed when our scripts have been recompiled and loaded.
     * </remarks>
     */
    [UnityEditor.Callbacks.DidReloadScripts]
    static void PerformPatch()
    {
        var harmony = new Harmony("de.synergyquest.objectdocs");
        
        var mOriginal = AccessTools.Method(inspectorType.Value, "OnInspectorGUI");
        var mPostfix = SymbolExtensions.GetMethodInfo(() => Postfix(null));

        harmony.Patch(mOriginal, null, new HarmonyMethod(mPostfix));
    }
    
    /**
     * <summary>
     * A call to this method is appended to <see cref="UnityEngine.GameObjectInspector.OnInspectorGUI"/>.
     * </summary>
     * <param name="__instance">
     *     A reference to the <c>UnityEngine.GameObjectInspector</c> instance.
     *     It is not typed since <c>UnityEngine.GameObjectInspector</c> is marked as "internal", so the compiler would
     *     not accept the use of the type here.
     * </param>
     */
    static void Postfix(object __instance)
    {
        // Get a reference to the object the inspector is currently displaying
        GameObject target = (GameObject) mTarget.Value.Invoke(__instance, null);
        
        // If ObjectDocs have been added to the target object of this inspector, display them
        if (target.TryGetComponent(out ObjectDocs objectDocs) && !objectDocs.Docstring.IsNullOrEmpty())
        {
            GUILayout.BeginHorizontal();
                GUILayout.Space(10);
            
                GUILayout.BeginVertical();
                    GUILayout.Space(10);
                    GUILayout.Label("Docstring", EditorStyles.boldLabel);
                    GUILayout.Space(5);
                    GUILayout.Label(new GUIContent(objectDocs.Docstring, $"Edit docstring in {nameof(ObjectDocs)} component."), EditorStyles.wordWrappedLabel);
                    GUILayout.Space(10);
                GUILayout.EndVertical();
            
                GUILayout.Space(10);
            GUILayout.EndHorizontal();
        }
    }   
}

#endif
