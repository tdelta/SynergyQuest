﻿using UnityEngine;
using Random = UnityEngine.Random;

public class CoinController : MonoBehaviour
{
    [SerializeField] private float thrust = default;
    [SerializeField] private int deactivationTime = default;

    public void Init(int deactivationTimeOverride)
    {
        deactivationTime = deactivationTimeOverride;
    }
    
    // Start is called before the first frame update
    void Start()
    {
        this.GetComponent<Rigidbody2D>().AddForce(getRandomDirection() * thrust);
        
        if (deactivationTime > 0)
        {
            StartCoroutine(
                CoroutineUtils.Wait(
                    deactivationTime,
                    () => Destroy(this.gameObject)
                )
            );
        }
    }

    private Vector3 getRandomDirection() 
    {
        var xDirection = Random.Range(-1.0f, 1.0f);
        var yDirection = Random.Range(-1.0f, 1.0f);
        
        return new Vector3(xDirection, yDirection);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        var other = collision.collider;
        if (other.CompareTag("Player"))
        {
            other.GetComponent<PlayerController>().IncreaseGoldCounter();
            
            this.gameObject.PlaySoundAndDestroy();
        } 
    }
}