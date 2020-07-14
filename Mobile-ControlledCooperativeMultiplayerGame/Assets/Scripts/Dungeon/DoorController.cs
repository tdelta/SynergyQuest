using UnityEngine;
using System;
using System.Collections;

[Obsolete("Use `Door` script instead")]
public class DoorController: MonoBehaviour 
{
    [SerializeField] private Sprite openedDoor;
    [SerializeField] private Sprite closedDoor;
    
    private SpriteRenderer _renderer;
    private AudioSource _audioSource;
    private bool _open = false;
    
    public void OpenDoor()
    {
        _renderer.sprite = openedDoor;
        _audioSource.Play();
        _open = true;
    }

    // Start is called before the first frame update
    void Start()
    {
        _renderer = GetComponent<SpriteRenderer>();
        _audioSource = GetComponent<AudioSource>();
        _renderer.sprite = closedDoor;
    }

    void OnCollisionEnter2D(Collision2D other) {
        if (other.gameObject.tag == "Player" && _open) {
            DungeonController.Instance.LoadNextRoom();
       }
    }
}
