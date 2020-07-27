using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/**
 * Creates instances of `ControllerDebugUI` game objects when new controllers connect.
 * They are only intended to display some debug information about the state of a controller connection and sent inputs.
 */
public class ControllerDebugUISpawner : MonoBehaviour
{
    [SerializeField] private GameObject _controllerDebugUiPrefab;
    
    [SerializeField] private Vector3 _nextSpawnLocation = Vector3.zero;
    
    // Start is called before the first frame update
    void OnEnable()
    {
        ControllerServer.Instance.OnNewController += OnNewController;
        foreach (var input in ControllerServer.Instance.GetInputs())
        {
            OnNewController(input);
        }
    }
    
    void OnDisable()
    {
        if (ControllerServer.Instance != null)
        {
            ControllerServer.Instance.OnNewController -= OnNewController;
        }
    }
    
    void OnNewController(ControllerInput input)
    {
        var DebugUiObject = Instantiate(_controllerDebugUiPrefab, _nextSpawnLocation, Quaternion.identity);

        var DebugUi = DebugUiObject.GetComponent<ControllerDebugUI>();
        DebugUi.SetInput(input);
        
        // Move the next spawning position to the right by the width of the last debug ui game object.
        Vector3[] worldCorners = new Vector3[4];
        DebugUiObject.GetComponent<RectTransform>().GetWorldCorners(worldCorners);
        var width = worldCorners[2].x - worldCorners[1].x;
        
        _nextSpawnLocation.Set(
            _nextSpawnLocation.x + width,
            _nextSpawnLocation.y,
            _nextSpawnLocation.z
        );
    }
}
