using UnityEngine;

public class ChasmContactTracker : MonoBehaviour
{
    public Optional<Vector3> ChasmEntryPoint { get; private set; }
    public bool IsOnChasm => ChasmEntryPoint.IsSome();

    private Collider2D _collider;

    private void Awake()
    {
        _collider = GetComponent<Collider2D>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Chasm"))
        {
            if (ChasmEntryPoint.IsNone())
            {
                ChasmEntryPoint = Optional<Vector3>.Some(_collider.bounds.center);
            }
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Chasm") && !Chasm.IsOnChasm(_collider))
        {
            ChasmEntryPoint = Optional<Vector3>.None();
        }
    }
}
