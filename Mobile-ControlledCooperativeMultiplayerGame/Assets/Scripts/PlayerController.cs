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


    // Start is called before the first frame update
    void Start()
    {
        speed = 3.0f;
        animator = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void FixedUpdate ()
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
