using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{

    public float speed;

    Animator animator;


    float vertical;
    float horizontal;
    Vector2 lookDirection = new Vector2(1,0);

    public Transform attackPointTop;
    public Transform attackPointBottom;
    public Transform attackPointLeft;
    public Transform attackPointRight;

    public float attackRadius;
    bool attacking = false;

    // Start is called before the first frame update
    void Start()
    {
        speed = 3.0f;
        animator = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space)) {
            attack();
        }   
    }

    void FixedUpdate ()
    {
        if(attacking == false) {
            move();
        }
    }

    public Vector2 getPosition()
    {
        return transform.position;
    }

    public int getDamage()
    {
        return 1; //TODO :)
    }

    private void attack()
    {
        animator.SetTrigger("Attack");
        StartCoroutine(attackCo());
    }

    private IEnumerator attackCo()
    {
        attacking = true;
        yield return new WaitForSeconds(0.3f);
        attacking = false;
    }

    private void move()
    {
        vertical = Input.GetAxis("Vertical");
        horizontal = Input.GetAxis("Horizontal");

        Vector2 move = new Vector2(horizontal, vertical);

        if (!Mathf.Approximately(move.x, 0.0f) || !Mathf.Approximately(move.y, 0.0f)) {
            lookDirection.Set(move.x, move.y);
            lookDirection.Normalize();
        }

        animator.SetFloat("Look x", lookDirection.x);
        animator.SetFloat("Look y", lookDirection.y);
        animator.SetFloat("Speed", move.magnitude);


        Vector2 newPosition = transform.position;
        newPosition.x = newPosition.x + horizontal * speed * Time.deltaTime;
        newPosition.y = newPosition.y + vertical * speed * Time.deltaTime;

        transform.position = newPosition;
    }
}
