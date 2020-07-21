using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CoinController : MonoBehaviour
{

    public float thrust;

    public int deactivationTime;
    AudioSource source;
    public AudioClip coinCollect;


    // Start is called before the first frame update
    void Start()
    {
        source = this.GetComponent<AudioSource>();
        this.GetComponent<Rigidbody2D>().AddForce(getRandomDirection() * thrust);
        StartCoroutine(NotCollectedDestroyCoroutine());
    }

    private Vector3 getRandomDirection() 
    {
        float xDirection = Random.Range(-1.0f, 1.0f);
        float yDirection = Random.Range(-1.0f, 1.0f);
        return new Vector3(xDirection, yDirection);
    }


    private void OnCollisionEnter2D(Collision2D collision)
    {
        Collider2D other = collision.collider;
        if (other.gameObject.layer == LayerMask.NameToLayer("Player")) {
            other.GetComponent<PlayerController>().IncreaseGoldCounter();
            source.PlayOneShot(coinCollect, 1.0f);
            this.GetComponent<SpriteRenderer>().enabled = false;
            this.GetComponent<BoxCollider2D>().enabled = false;
            StartCoroutine(DestroyCoroutine());
        } 
    }

    public IEnumerator DestroyCoroutine() {
        yield return new WaitForSeconds(1f);
        Destroy(this.gameObject);
    }

    public IEnumerator NotCollectedDestroyCoroutine() {
        yield return new WaitForSeconds(deactivationTime);
        Destroy(this.gameObject);
    }
}
