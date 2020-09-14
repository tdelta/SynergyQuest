using System.Linq;
using UnityEngine;
using WebSocketSharp;

#if UNITY_EDITOR
using UnityEditor;
#endif

/**
 * Some objects need to be uniquely identified across scenes, for example to remember, which keys have already been
 * collected by the players (see also `Key` behaviour).
 * 
 * This behavior ensures a unique id is generated for an object when it is placed in the editor.
 *
 * DO NOT COPY OBJECTS WITH THIS BEHAVIOUR IN MULTI SCENE EDITING, normal scene editing is fine though.
 * (since you might end up copying the GUID)
 */
[DisallowMultipleComponent]
public class Guid : MonoBehaviour
{
    [HideInInspector, SerializeField] private string _guid = null;
    public string guid => _guid;
    
    private void Awake()
    {
        if (!HasGuidBeenCreated())
        {
            Debug.LogError("GUID has not been generated in edit mode! Open/edit/create this object in edit mode to generate a GUID");
        }
        
        // If the position of this object had been saved, restore it
        if (DungeonDataKeeper.Instance.GetSavedPosition(this, out var position))
        {
            this.transform.position = position;
        }
    }
    
    private bool HasGuidBeenCreated()
    {
        return !_guid.IsNullOrEmpty();
    }
    
    #if UNITY_EDITOR
        private void OnValidate()
        {
            // If no GUID has been created or there is a duplicate one, create a new GUID
            // (We check for identical GUIDs, since this object might have been created by duplicating another)
            if (!HasGuidBeenCreated() || IsGuidDuplicateInScene())
            {
                CreateGuid();
            }
            
            // If a GUID had been created but this is a prefab, we need to remove it again
            // (We do not want GUIDs for prefabs, otherwise the GUID would be copied to every instance)
            else if (HasGuidBeenCreated() && this.IsPrefab())
            {
                _guid = null;
                // Mark this object as changed, so that the changed GUID will be serialized
                Undo.RecordObject(this, "Deleted GUID from prefab");
            }
        }
    
        /**
         * Creates a new Guid, if this object is not a prefab.
         * (We do not want GUIDs for prefabs, otherwise the GUID would be copied to every instance)
         *
         * This method has been extracted and adapted from a Unity Technologies project: https://github.com/Unity-Technologies/guid-based-reference
         * Hence, this method is licensed under the Unity Companion License as a derivative work: https://unity3d.com/legal/licenses/Unity_Companion_License
         */
        void CreateGuid()
        {
            if (this.IsPrefab())
            {
                return;
            }
            
            _guid = System.Guid.NewGuid().ToString();
            Undo.RecordObject(this, "Added GUID"); // Mark the object as changed, so that the new GUID will be saved
    
            // If we are creating a new GUID for a prefab instance of a prefab, but we have somehow lost our prefab connection
            // force a save of the modified prefab instance properties
            if (PrefabUtility.IsPartOfNonAssetPrefabInstance(this))
            {
                PrefabUtility.RecordPrefabInstancePropertyModifications(this);
            }
        }

        /**
         * Determines, if there is another object in the scene with the same GUID.
         */
        private bool IsGuidDuplicateInScene()
        {
            return HasGuidBeenCreated() && FindObjectsOfType<Guid>()
                .Any(otherGuid => otherGuid != this && otherGuid.guid == this.guid);
        }
    #endif
}

#if UNITY_EDITOR
/**
 * Displays the GUID string generated by the `Guid` behaviour above in the inspector, but does not allow editing.
 *
 * This class has been extracted and adapted from a Unity Technologies project: https://github.com/Unity-Technologies/guid-based-reference
 * Hence, it is licensed under the Unity Companion License as a derivative work: https://unity3d.com/legal/licenses/Unity_Companion_License
 */
[CustomEditor(typeof(Guid))]
public class GuidComponentDrawer : Editor
{
    private Guid _guidComp;

    public override void OnInspectorGUI()
    {
        if (_guidComp == null)
        {
            _guidComp = (Guid) target;
        }
       
        EditorGUILayout.LabelField("Guid:", _guidComp.guid.IsNullOrEmpty() ? "Not created or this object is a prefab." : _guidComp.guid);
    }
}
#endif