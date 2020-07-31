using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BombItem : ThrowableItem
{

    [SerializeField] private Bomb _instance;
    private Bomb _copiedInstance;

    public override bool Ready()
    {
        return _copiedInstance?.isDestroyed() ?? true;
    }
    
    public override Button GetButton()
    {
        return Button.Bomb;
    }

    public override Throwable PickUp(PlayerController player){
        Vector2 position = new Vector2(player.Rigidbody2D.position.x, player.Renderer.bounds.max.y);
        if (_instance.Instantiate(position) is Bomb throwableItem){
            player.PickUpThrowable(throwableItem);
            _copiedInstance = throwableItem;
            return throwableItem;
        } else {
            return null;
        }
    }

    protected override void Throw(PlayerController player, Throwable throwableItem){
        player.ThrowThrowable(throwableItem, new Vector2(player.Input.GetHorizontal(), player.Input.GetVertical()));
    }
}
