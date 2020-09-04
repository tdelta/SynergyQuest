using UnityEngine;

/**
 * <summary>
 * Moves an object periodically up and down, as if it were hovering in mid air
 * </summary>
 */
public class Hover : MonoBehaviour
{
    /**
     * How far up and down the object shall move when hovering
     */
    [SerializeField] private float range = 0.2f;

    /**
     * How fast it shall move
     */
    [SerializeField] private float speed = 0.5f;
    [SerializeField] private int framesPerSec = 60;

    /**
     * Caches the position of the object before we started to move.
     */
    private Vector3 _originalPosition;

    private void Awake()
    {
        _originalPosition = this.transform.position;
    }

    private void Start()
    {
        InvokeRepeating(nameof(Animate), 0.0f, 1.0f/framesPerSec);
    }

    private void Animate()
    {
        var newPosition = _originalPosition;
        newPosition.y += range * Mathf.Sin(speed * Time.time);

        this.transform.position = newPosition;
    }
}
