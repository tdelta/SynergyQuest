using UnityEngine;

/**
 * <summary>
 * Instead of directly respawning, this behaviour turns players into ghosts when respawning, see
 * <see cref="PlayerGhost"/>.
 * This ghost first must be caught by the other players before the player respawns.
 *
 * This minigame is activated by the <see cref="Spawnable.OnRespawn"/> event of <see cref="Spawnable"/>.
 * </summary>
 * <seealso cref="Spawnable"/>
 * <see cref="PlayerGhost"/>
 */
[RequireComponent(typeof(Spawnable), typeof(PlayerController))]
public class ReviveMinigame : MonoBehaviour
{
    [SerializeField] private GameObject playerGhostPrefab = default;

    private PlayerController _player;
    private Spawnable _spawnable;

    private void Awake()
    {
        _player = GetComponent<PlayerController>();
        _spawnable = GetComponent<Spawnable>();
    }

    private void OnEnable()
    {
        _spawnable.OnRespawn += OnRespawn;
    }

    private void OnDisable()
    {
        _spawnable.OnRespawn -= OnRespawn;
    }

    private void OnRespawn(Vector3 respawnPosition)
    {
        if (!DebugSettings.Instance.DebugMode || !DebugSettings.Instance.DisableRevivalMinigame)
        {
            var instance = Instantiate(playerGhostPrefab, respawnPosition, Quaternion.identity)
              .GetComponentInChildren<PlayerGhost>();
            
            instance.Register(_player, transform.position);
        }
    }
}
