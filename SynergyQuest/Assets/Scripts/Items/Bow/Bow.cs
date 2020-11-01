using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bow : Item
{

    [SerializeField] BowProjectile arrowPrefab;

    protected override void OnActivate(PlayerController player) {
        ActivateBowSprite(player);
        var spawnPoint = player.viewDirection.ToVector() + player.Rigidbody2D.position;
        var instance = BowProjectile.Launch(arrowPrefab, spawnPoint, player.viewDirection.ToVector());
        Physics2D.IgnoreCollision(instance.Collider, player.GetComponent<Collider2D>());
    }

    private void ActivateBowSprite(PlayerController player) {
        string searchfor = "";
        switch (player.viewDirection) {
            case Direction.Up:
                searchfor = "BowUp";
                break;
            case Direction.Down:
                searchfor = "BowDown";
                break;
            case Direction.Left:
                searchfor = "BowLeft";
                break;
            case Direction.Right:
                searchfor = "BowRight";
                break;
        }
        Transform trans = player.transform.Find(searchfor);
        SpriteRenderer sr = trans.GetComponent<SpriteRenderer>();
        sr.enabled = true;
        player.GetComponent<Animator>().SetTrigger("HoldHandOut");
        player.SetBowFiringMode();
        StartCoroutine(DeactivateBow(sr, player));
    }

    IEnumerator DeactivateBow(SpriteRenderer sr, PlayerController player) {
        yield return new WaitForSeconds(0.33f);
        sr.enabled = false;
        player.OnFiringBowFinished();
        Destroy(this.gameObject);
    }

}
