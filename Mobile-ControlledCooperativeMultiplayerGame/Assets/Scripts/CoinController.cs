using UnityEngine;
using Random = UnityEngine.Random;

public class CoinController : MonoBehaviour
{
    [SerializeField] private float thrust;
    [SerializeField] private int deactivationTime;
    
    // Start is called before the first frame update
    void Start()
    {
        this.GetComponent<Rigidbody2D>().AddForce(getRandomDirection() * thrust);
        
        if (deactivationTime > 0)
        {
            CoroutineUtils.Wait(
                deactivationTime,
                () => Destroy(this.gameObject)
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
