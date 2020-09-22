// This file is part of the "Synergy Quest" game
// (github.com/tdelta/SynergyQuest).
// 
// Copyright (c) 2020
//   Marc Arnold     (m_o_arnold@gmx.de)
//   Martin Kerscher (martin_x@live.de)
//   Jonas Belouadi  (jonas.belouadi@posteo.net)
//   Anton W Haubner (anton.haubner@outlook.de)
// 
// This program is free software; you can redistribute it and/or modify it
// under the terms of the GNU General Public License as published by the Free
// Software Foundation; either version 3 of the License, or (at your option) any
// later version.
// 
// This program is distributed in the hope that it will be useful, but WITHOUT
// ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or
// FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for
// more details.
// 
// You should have received a copy of the GNU General Public License along with
// this program; if not, see <https://www.gnu.org/licenses>.
// 
// Additional permission under GNU GPL version 3 section 7 apply,
// see `LICENSE.md` at the root of this source code repository.

﻿using UnityEngine;

[RequireComponent(typeof(Guid))]
[RequireComponent(typeof(Interactive))]
[RequireComponent(typeof(SpriteRenderer))]
public class Chest : MonoBehaviour
{
    /**
     * Sprites to show when the box is either opened or closed
     */
    [SerializeField] private Sprite chestOpenSprite = default;

    private Interactive _interactive;
    private SpriteRenderer _renderer;
    public Guid Guid { get; private set; }

    private bool _hasBeenOpened = false;

    [SerializeField] private GameObject coin;
    [SerializeField] private int amountOfCoins;

    [SerializeField] private bool spawnCoinsWithDelay;

    private void Awake()
    {    
        _renderer = GetComponent<SpriteRenderer>();
        _interactive = GetComponent<Interactive>();
        Guid = GetComponent<Guid>();
    }

    private void Start()
    {
        // Determine, if this chest has already been opened
        _hasBeenOpened = DungeonDataKeeper.Instance.HasChestBeenOpened(this);
        // If so, open it
        if (_hasBeenOpened)
        {
            _renderer.sprite = chestOpenSprite;
            _interactive.enabled = false;
        }
    }

    private void OnEnable()
    {
        _interactive.OnInteractionTriggered += OnInteractionTriggered;
    }
    
    private void OnDisable()
    {
        _interactive.OnInteractionTriggered -= OnInteractionTriggered;
    }

    void OnInteractionTriggered(PlayerController _)
    {
        if (!_hasBeenOpened)
        {
            // Open the chest
            _hasBeenOpened = true;
            // remember that the chest has been opened across levels
            DungeonDataKeeper.Instance.SaveChestActivation(this, true);
            
            _renderer.sprite = chestOpenSprite;
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
