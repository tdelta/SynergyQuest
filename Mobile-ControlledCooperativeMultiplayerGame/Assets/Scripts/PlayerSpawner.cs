using System;
using System.Linq;
using Cinemachine;
using UnityEngine;

/**
 * Base class for behaviors spawning players.
 * It contains all necessary logic. Sub-classes usually only have to decide whether the spawner is currently active or
 * not (see `IsSpawnerActive` method).
 */
public abstract class PlayerSpawner : MonoBehaviour
{
    /**
     * If you want the new players to be tracked by a specific camera, assign the camera target group here.
     * If it is not assigned, a CinemachineTargetGroup instance will be searched for automatically.
     */
    [SerializeField] private CinemachineTargetGroup targetGroup = default;

    /**
     * There can be player instances pre-created in the scene. Especially local instances for debugging.
     * Those instances should be added to a spawner in this field, so that they can be respawned
     */
    [SerializeField] private PlayerController[] managedPreexistingPlayers = default;

    /**
     * In debug mode, for newly connected controllers, we need to give them a colour, since they didn't join via the
     * lobby which assigns colors. Here we store the next color to assign to a new controller.
     */
    private PlayerColor _nextPlayerColor = PlayerColor.Red;

    /**
     * Decides whether the spawner shall be active.
     * If the spawner is active, it spawns players on `Start` and respawns them when they die.
     *
     * For example, a door spawner should only be active, if the players used the corresponding door to enter the
     * room.
     */
    protected abstract bool IsSpawnerActive();

    private void Awake()
    {
        // If no camera target group has been provided manually, we try to retrieve it ourselves
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
        if (ControllerServer.Instance != null)
        {
            ControllerServer.Instance.OnNewController -= OnNewController;
        }
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
            
            // register local managed player instances for respawning
            if (managedPreexistingPlayers != null)
            {
                foreach (var player in managedPreexistingPlayers)
                {
                    player.OnRespawn += Respawn;
                }
            }
        }
    }

    /**
     * Spawns all players from the previous scene, if there are any.
     * Spawns players for all connected remote inputs, for which no player instance has been created previously.
     * Spawns debugging player instances as instructed in the `DebuggingSettings` scriptable object singleton.
     */
    public void Spawn()
    {
        PlayerDataKeeper.Instance
            .InstantiateExistingPlayers(this.transform.position)
            .ForEach(RegisterSpawnedInstance);

        SpawnConnectedInputs();
        SpawnDebugPlayers();
    }

    /**
     * Spawns players for all connected remote inputs, for which no player instance has been created previously.
     */
    private void SpawnConnectedInputs()
    {
        // Collect all connected inputs which have not been assigned to a player prefab instance before
        var inputs = ControllerServer.Instance.GetInputs().Where(input => !PlayerDataKeeper.Instance.IsInputAssignedToPlayer(input));
            
        // Spawn instances for them
        foreach (var input in inputs)
        {
            var player = PlayerDataKeeper.Instance.InstantiateNewPlayer(input, this.transform.position);
            RegisterSpawnedInstance(player);
        }
    }

    /**
     * Spawns debugging player instances as instructed in the `DebuggingSettings` scriptable object singleton.
     */
    private void SpawnDebugPlayers()
    {
        // Spawn locally controlled players as specified in the debug settings, if they have not already been spawned
        if (DebugSettings.Instance.DebugMode && !DebugSettings.Instance.DebugPlayersWereSpawned)
        {
            foreach (var playerConfig in DebugSettings.Instance.LocalDebugPlayers)
            {
                var input = Instantiate(playerConfig.inputPrefab);
                input.SetColor(playerConfig.color);

                var player = PlayerDataKeeper.Instance.InstantiateNewPlayer(input, this.transform.position);
                // ReSharper disable once Unity.InstantiateWithoutParent
                input.transform.SetParent(player.gameObject.transform);
                
                RegisterSpawnedInstance(player);
            }

            // Remember that the debug player instances have been spawned
            DebugSettings.Instance.DebugPlayersWereSpawned = true;
        }
    }
    
    /**
     * Given a player prefab instance which has just been spawned, assign it to the camera target group and
     * ensure it is respawned by this spawner.
     */
    private void RegisterSpawnedInstance(PlayerController player)
    {
        if (targetGroup != null)
        {
            targetGroup.AddMember(player.transform, 1, 1);
        }

        player.ClearRespawnHandlers();
        player.OnRespawn += Respawn;
        OnSpawn(player);
    }

    void OnNewController(ControllerInput input)
    {
        // Only let new controllers join if we are in debug mode.
        // Otherwise they must join via lobby.
        if (DebugSettings.Instance.DebugMode && IsSpawnerActive())
        {
            input.SetColor(_nextPlayerColor);
            _nextPlayerColor = _nextPlayerColor.NextColor();

            var player = PlayerDataKeeper.Instance.InstantiateNewPlayer(input, this.transform.position);
            RegisterSpawnedInstance(player);
        }
    }

    private void Respawn(PlayerController player)
    {
        // Move player to position of the spawner when respawning
        player.GetComponent<PhysicsEffects>().Teleport(this.transform.position);

        // Make player visible again, if they have been invisible
        player.GetComponent<SpriteRenderer>().enabled = true;
        player.Reset();
    }

    private void OnDrawGizmos()
    {
        // Make spawner visible in the editor by displaying an icon
        Gizmos.DrawIcon(transform.position, "spawnIcon.png", true);
    }

    /**
     * Invoked whenever a player is spawned. May be overriden.
     */
    protected virtual void OnSpawn(PlayerController player)
    { }
}
