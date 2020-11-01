using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BowProjectile : AbstractProjectile
{
    protected override void OnCollisionEnter2D(Collision2D other)
    {
        if (other.gameObject.CompareTag("Enemy"))
        {
            var enemy = other.gameObject.GetComponent<EntityController>();
            enemy.PutDamage(damageFactor, (other.transform.position - transform.position).normalized); 
            
        }
        Destroy(this.gameObject);
    }
}
