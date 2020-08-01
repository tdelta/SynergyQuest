#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.Experimental.SceneManagement;
using UnityEngine;
#endif

public static class EditorUtils
{
    #if UNITY_EDITOR
        /**
         * Determines, if a MonoBehaviour is currently executing on an object instance in prefab mode.
         *
         * This method has been extracted and adapted from a Unity Technologies project: https://github.com/Unity-Technologies/guid-based-reference
         * Hence, this method is licensed under the Unity Companion License: https://unity3d.com/legal/licenses/Unity_Companion_License
         */
        public static bool IsEditingInPrefabMode(this MonoBehaviour self)
        {
            if (EditorUtility.IsPersistent(self))
            {
                // if the game object is stored on disk, it is a prefab of some kind, despite not returning true for IsPartOfPrefabAsset =/
                return true;
            }
            
            else
            {
                // If the GameObject is not persistent let's determine which stage we are in first because getting Prefab info depends on it
                var mainStage = StageUtility.GetMainStageHandle();
                var currentStage = StageUtility.GetStageHandle(self.gameObject);
                if (currentStage != mainStage)
                {
                    var prefabStage = PrefabStageUtility.GetPrefabStage(self.gameObject);
                    if (prefabStage != null)
                    {
                        return true;
                    }
                }
            }
            return false;
        }
    
        /**
         * Determines, if a MonoBehaviour instance is part of a prefab object.
         *
         * This method has been extracted and adapted from a Unity Technologies project: https://github.com/Unity-Technologies/guid-based-reference
         * Hence, this method is licensed under the Unity Companion License: https://unity3d.com/legal/licenses/Unity_Companion_License
         */
        public static bool IsPrefab(this MonoBehaviour self)
        {
            return PrefabUtility.IsPartOfPrefabAsset(self) || self.IsEditingInPrefabMode();
        }
    #endif
}
