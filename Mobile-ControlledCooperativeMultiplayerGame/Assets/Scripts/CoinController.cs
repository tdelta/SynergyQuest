using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CoinController : MonoBehaviour
{

    public float thrust;


    // Start is called before the first frame update
    void Start()
    {
        this.GetComponent<Rigidbody2D>().AddForce(getRandomDirection() * thrust);
    }

    private Vector3 getRandomDirection() 
    {
        float xDirection = Random.Range(-1.0f, 1.0f);
        float yDirection = Random.Range(-1.0f, 1.0f);
        return new Vector3(xDirection, yDirection);
    }


    private void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log("Der Trigger wird ausgeführt");
        if (other.gameObject.layer == LayerMask.NameToLayer("Player")) {
            other.GetComponent<PlayerController>().increaseGoldCounter();
            Destroy(this.gameObject);
            // ToDo: Play sound
        }
    }
}
