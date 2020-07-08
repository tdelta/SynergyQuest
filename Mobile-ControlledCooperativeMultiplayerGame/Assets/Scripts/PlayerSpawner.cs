using System;
using System.Linq;
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
     * If players with local controls are spawned for debugging, which local input controls shall be used?
     * When spawning multiple locally controlled players, we will cycle through this list.
     */
    [SerializeField] private LocalInput[] localInputPrefabs;

    /**
     * If you want the new players to be tracked by the camera, assign the camera target group here
     */
    [SerializeField] private CinemachineTargetGroup targetGroup;
    
    /**
     * There can be player instances pre-created in the scene. Especially local instances for debugging.
     * Those instances should be added to a spawner in this field, so that they can be respawned
     */
    [SerializeField] private PlayerController[] managedPreexistingPlayers = new PlayerController[0];

    /**
     * In debug mode, for newly connected controllers, we need to give them a colour, since they didn't join via the
     * lobby which assigns colors. Here we store the next color to assign to a new controller.
     */
    private PlayerColor _nextPlayerColor = PlayerColor.Red;

    private void Awake()
    {
        // If we are in debug mode, and no players have been added manually to be managed,
        // we retrieve the preexisting player instances in the scene automatically
        if (DebugSettings.DebugMode && !managedPreexistingPlayers.Any())
        {
            managedPreexistingPlayers = FindObjectsOfType<PlayerController>();
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        // Ensure game is in started state
        // (NOTE: This a weird place to set the game state, however, it makes debugging easier. Spawners can only
        //  be found in game scenes and we want controllers to register the game as started when running those scenes
        //  in the editor)
        SharedControllerState.Instance.SetGameState(GameState.Started);
        
        // Register callback so that we get informed about new controllers
        ControllerServer.Instance.OnNewController += OnNewController;
        
        // Spawn every already connected player
        var inputs = ControllerServer.Instance.GetInputs();
        foreach (var input in inputs)
        {
            SpawnPlayer(input);
        }
        
        // Spawn locally controlled players when this is enabled in the debug settings
        {
            for (var i = 0; i < DebugSettings.LocalDebugPlayers.Length; ++i)
            {
                var layout = localInputPrefabs[i % 2];
                var input = Instantiate(layout);
                input.SetColor(DebugSettings.LocalDebugPlayers[i]);
                
                var player = SpawnPlayer(input);
                input.transform.SetParent(player.gameObject.transform);
            }
        }
        
        // Register local player instances for respawning
        foreach (var player in managedPreexistingPlayers)
        {
            player.OnRespawn += Respawn;
        }
    }

    void OnNewController(ControllerInput input)
    {
        // Only let new controllers join if we are in debug mode.
        // Otherwise they must join via lobby.
        if (DebugSettings.DebugMode)
        {
            input.SetColor(_nextPlayerColor);
            _nextPlayerColor = _nextPlayerColor.NextColor();
            
            SpawnPlayer(input);
        }
    }

    private void Respawn(PlayerController player)
    {
        player.GetComponent<PhysicsEffects>().Teleport(this.transform.position);

        // Make player visible again, if they have been invisible
        player.GetComponent<SpriteRenderer>().enabled = true;
        player.Reset();
    }

    /**
     * Creates a character / player object at the current position and initializes it with the given controller
     */
    private PlayerController SpawnPlayer(Input input)
    {
        var instance = Instantiate(playerPrefab);
        instance.transform.position = this.transform.position;

        if (targetGroup != null)
        {
            targetGroup.AddMember(instance.transform, 1, 1);
        }

        var controller = instance.GetComponent<PlayerController>();
        controller.OnRespawn += Respawn;
        controller.Init(input);

        return controller;
    }
}
