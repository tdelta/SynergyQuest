using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerHit : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.tag == "Enemy") {
            EnemyController enemy = other.gameObject.GetComponent<EnemyController>();
            enemy.putDamage(1); 
        }
    }
}
