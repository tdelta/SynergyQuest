using UnityEngine;

public class BombItem : Item
{
    // FIXME: Report to ItemController, when new instance can be created

    private Throwable _throwable;

    private void Awake()
    {
        _throwable = GetComponent<Throwable>();
    }

    public override void OnActivate(PlayerController player)
    {
        _throwable.Pickup(player);
    }

    private void Update()
    {
        var carrier = _throwable.Carrier;
        
        if (_throwable.IsBeingCarried && carrier.Input.GetButtonUp(ItemDescription.UseButton))
        {
            // FIXME: Refactor throwing direction into property of player
            _throwable.Carrier.ThrowThrowable(_throwable, new Vector2(carrier.Input.GetHorizontal(), carrier.Input.GetVertical()));
        }
    }
}
