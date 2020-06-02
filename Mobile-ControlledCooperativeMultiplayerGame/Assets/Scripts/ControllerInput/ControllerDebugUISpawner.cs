using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ControllerDebugUISpawner : MonoBehaviour
{
    [SerializeField] private ControllerServer _controllerServer;
    [SerializeField] private GameObject _controllerDebugUiPrefab;
    
    [SerializeField] private Vector3 _nextSpawnLocation = Vector3.zero;
    
    // Start is called before the first frame update
    void OnEnable()
    {
        _controllerServer.OnNewController += OnNewController;
    }
    
    void OnDisable()
    {
        _controllerServer.OnNewController -= OnNewController;
    }
    
    void OnNewController(ControllerInput input)
    {
        var DebugUiObject = Instantiate(_controllerDebugUiPrefab, _nextSpawnLocation, Quaternion.identity);

        var DebugUi = DebugUiObject.GetComponent<ControllerDebugUI>();
        DebugUi.SetInput(input);
        
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
