using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Chest : MonoBehaviour
{

    /**
     * Sprites to show when the box is either opened or closed
     */
    [SerializeField] private Sprite chestOpenSprite;
    [SerializeField] private Sprite chestClosedSprite;

    private Interactive _interactive;
    private SpriteRenderer _renderer;

    public GameObject coin;
    public int amountOfCoins;

    public bool spawnCoinsWithDelay;

    private void Awake()
    {    
        _renderer = GetComponent<SpriteRenderer>();
        _interactive = GetComponent<Interactive>();
    }
    
    // Update is called once per frame
    void Update()
    {  
        if (_interactive.IsInteracting) {
            // Open the chest
            _renderer.sprite = chestOpenSprite;
            _interactive.SuppressSpeechBubble = true;
            _interactive.enabled = false;
            SpawnCoins();
        }
    }

    private void SpawnCoins() 
    {
        for(int i = 0; i < amountOfCoins; i++) {
            if (spawnCoinsWithDelay) {
                SpawnCoinWithDelay(amountOfCoins - i);    
            } else {
                SpawnCoin();
            }
        }
    }

    /**
     * Spawns a single coin in front of the chest with a given delay
     */
    private void SpawnCoinWithDelay(float delay) {
        StartCoroutine(
            CoroutineUtils.Wait(
                delay,
                () => SpawnCoin()
                )
            );
    }

    /**
     * Spawns a single coin in front of the chest
     */
    private void SpawnCoin() {
        // Modify the y position in order to spawn the coins in front of the chest
        Vector3 spawnPosition = transform.position;
        spawnPosition.y = spawnPosition.y - 0.5f;
        Instantiate(coin, spawnPosition, Quaternion.identity);
    }
}