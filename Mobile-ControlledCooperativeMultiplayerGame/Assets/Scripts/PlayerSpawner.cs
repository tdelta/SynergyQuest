using Cinemachine;
using UnityEngine;

/**
 * Spawns character objects for every connected controller at its current position.
 *
 * If debug mode is set to true, a character will also be spawned for every newly connected controller
 */
public class PlayerSpawner : MonoBehaviour
{
    /**
     * Character prefab object spawned for every controller. Must have a `PlayerController` component
     */
    [SerializeField] private GameObject playerPrefab;

    /**
     * If you want the new players to be tracked by the camera, assign the camera target group here
     */
    [SerializeField] private CinemachineTargetGroup targetGroup;
    
    /**
     * Whether this spawner runs in debug mode. If yes, newly connected controllers which did not join via the lobby
     * are also accepted.
     */
    [SerializeField] private bool _debugMode = false;

    /**
     * In debug mode, for newly connected controllers, we need to give them a colour, since they didn't join via the
     * lobby which assigns colors. Here we store the next color to assign to a new controller.
     */
    private PlayerColor _nextPlayerColor = PlayerColor.Red;
    
    // Start is called before the first frame update
    void Start()
    {
        // Register callback so that we get informed about new controllers
        ControllerServer.Instance.OnNewController += OnNewController;
        
        // Spawn every already connected player
        var inputs = ControllerServer.Instance.GetInputs();
        foreach (var input in inputs)
        {
            SpawnPlayer(input);
        }
    }

    void OnNewController(ControllerInput input)
    {
        // Only let new controllers join if we are in debug mode
        if (_debugMode)
        {
            input.SetColor(_nextPlayerColor);
            _nextPlayerColor = _nextPlayerColor.NextColor();
            
            SpawnPlayer(input);
            input.SetGameState(GameState.Started);
        }
    }

    private void Respawn(PlayerController player)
    {
        player.transform.position = this.transform.position;
    }

    /**
     * Creates a character / player object at the current position and initializes it with the given controller
     */
    private void SpawnPlayer(ControllerInput input)
    {
        var instance = Instantiate(playerPrefab);
        instance.transform.position = this.transform.position;

        if (targetGroup != null)
        {
            targetGroup.AddMember(instance.transform, 1, 1);
        }

        var controller = instance.GetComponent<PlayerController>();
        controller.respawn = Respawn;
        controller.Init(input);
    }
}
