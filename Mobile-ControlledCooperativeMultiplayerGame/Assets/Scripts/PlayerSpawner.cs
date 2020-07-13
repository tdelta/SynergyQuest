using System;
using System.Linq;
using Cinemachine;
using UnityEngine;

public abstract class PlayerSpawner : MonoBehaviour
{
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

    protected abstract bool IsSpawnerActive();

    private void Awake()
    {
        if (ReferenceEquals(targetGroup, null))
        {
            targetGroup = FindObjectOfType<CinemachineTargetGroup>();
        }
    }

    private void OnEnable()
    {
        // Register callback so that we get informed about new controllers
        ControllerServer.Instance.OnNewController += OnNewController;
    }
    
    private void OnDisable()
    {
        ControllerServer.Instance.OnNewController -= OnNewController;
    }

    // Start is called before the first frame update
    void Start()
    {
        if (IsSpawnerActive())
        {
            // Ensure game is in started state
            // (NOTE: This a weird place to set the game state, however, it makes debugging easier. Spawners can only
            //  be found in game scenes and we want controllers to register the game as started when running those scenes
            //  in the editor)
            SharedControllerState.Instance.SetGameState(GameState.Started);

            Spawn();
            
            // If we are in debug mode, and no players have been added manually to be managed,
            // we retrieve the preexisting player instances in the scene automatically
            if (DebugSettings.Instance.DebugMode && !managedPreexistingPlayers.Any())
            {
                managedPreexistingPlayers = FindObjectsOfType<PlayerController>();
            }

            // Register local player instances for respawning
            foreach (var player in managedPreexistingPlayers)
            {
                player.OnRespawn += Respawn;
            }
        }
    }

    public void Spawn()
    {
        PlayerManager.Instance
            .InstantiateExistingPlayers(this.transform.position)
            .ForEach(SpawnPlayer);

        SpawnConnectedInputs();
        SpawnDebugPlayers();
    }

    private void SpawnConnectedInputs()
    {
        // Spawn every already connected player, which has not yet been spawned
        var inputs = ControllerServer.Instance.GetInputs().Where(PlayerManager.Instance.IsInputAssignedToPlayer);
            
        foreach (var input in inputs)
        {
            var player = PlayerManager.Instance.InstantiateNewPlayer(input);
            SpawnPlayer(player);
        }
    }

    private void SpawnDebugPlayers()
    {
        // Spawn locally controlled players when this is enabled in the debug settings
        if (!DebugSettings.Instance.DebugPlayersWereSpawned)
        {
            foreach (var playerConfig in DebugSettings.Instance.LocalDebugPlayers)
            {
                var input = Instantiate(playerConfig.inputPrefab);
                input.SetColor(playerConfig.color);

                var player = PlayerManager.Instance.InstantiateNewPlayer(input, this.transform.position);
                // ReSharper disable once Unity.InstantiateWithoutParent
                input.transform.SetParent(player.gameObject.transform);
                
                SpawnPlayer(player);
            }

            DebugSettings.Instance.DebugPlayersWereSpawned = true;
        }
    }
    
    /**
     * Creates a character / player object at the current position and initializes it with the given controller
     */
    private void SpawnPlayer(PlayerController player)
    {
        if (targetGroup != null)
        {
            targetGroup.AddMember(player.transform, 1, 1);
        }

        player.ClearRespawnHandlers();
        player.OnRespawn += Respawn;
    }

    void OnNewController(ControllerInput input)
    {
        // Only let new controllers join if we are in debug mode.
        // Otherwise they must join via lobby.
        if (DebugSettings.Instance.DebugMode && IsSpawnerActive())
        {
            input.SetColor(_nextPlayerColor);
            _nextPlayerColor = _nextPlayerColor.NextColor();

            var player = PlayerManager.Instance.InstantiateNewPlayer(input, this.transform.position);
            SpawnPlayer(player);
        }
    }

    private void Respawn(PlayerController player)
    {
        player.GetComponent<PhysicsEffects>().Teleport(this.transform.position);

        // Make player visible again, if they have been invisible
        player.GetComponent<SpriteRenderer>().enabled = true;
        player.Reset();
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawIcon(transform.position, "spawnIcon.png", true);
    }
}
