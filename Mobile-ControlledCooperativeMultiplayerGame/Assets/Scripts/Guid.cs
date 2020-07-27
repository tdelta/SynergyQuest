using UnityEngine;

/**
 * Some objects need to be uniquely identified across scenes, for example to remember, which keys have already been
 * collected by the players (see also `Key` behaviour).
 * 
 * This behavior ensures a unique id is generated for an object when it is placed in the editor.
 */
public class Guid : MonoBehaviour
{
    [HideInInspector, SerializeField] private string _guid = null;
    public string guid => _guid;

    private void OnValidate()
    {
        // If no unique id has been generated yet, generate it now.
        if (_guid is null)
        {
            _guid = System.Guid.NewGuid().ToString();
        }
    }
}
