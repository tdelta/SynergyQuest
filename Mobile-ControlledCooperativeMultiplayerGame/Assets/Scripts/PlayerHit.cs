using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerHit : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.tag == "Enemy") {
            EnemyController enemy = other.gameObject.GetComponent<EnemyController>();
            enemy.PutDamage(1, (other.transform.position - transform.position).normalized); 
        }
    }
}
