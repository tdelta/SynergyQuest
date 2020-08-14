using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerHit : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("Enemy"))
        {
            if (other.gameObject.GetComponent<NecromancerController>() is NecromancerController neko) {
                var player = gameObject.GetComponentInParent(typeof(PlayerController)) as PlayerController;
                neko.AttackingPlayer = player;
            }

            var enemy = other.gameObject.GetComponent<EnemyController>();
            enemy.PutDamage(1, (other.transform.position - transform.position).normalized); 
        }
        else if (other.gameObject.CompareTag("Switch"))
            other.gameObject.GetComponent<ShockSwitch>().Activate();
    }
}
