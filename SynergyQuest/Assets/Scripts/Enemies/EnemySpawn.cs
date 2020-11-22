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

﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
 using DamageSystem;
 using UnityEngine;
using UnityEngine.Tilemaps;

[RequireComponent(typeof(Tilemap), typeof(Switch))]
public class EnemySpawn : MonoBehaviour {

    #pragma warning disable 0649
        [Serializable]
        struct EnemyTypeCountPair {
            public EnemyController Type;
            public int Count;
        }
    #pragma warning restore 0649

    // If true, the configured number of enemies will be spawned for every player
    [SerializeField] private bool spawnEnemiesPerPlayer = false;
    [SerializeField] List<EnemyTypeCountPair> enemies = default;

    /**
     * If this list is non-empty, enemies will be spawned at these points instead
     * of on random tiles
     */
    [SerializeField] private List<Transform> customSpawnPoints = new List<Transform>();

    private int _deadEnemies = 0;
    private int _spawnedEnemies = 0;
    private int _emptyRadius = 1;
    private float _totalSpawnTime = 1;
    private float _spawnTime;
    private Tilemap _tilemap;
    private Switch _switch;
    private System.Random _rand = new System.Random();

    // Start is called before the first frame update
    void Start()
    {
        _tilemap = GetComponent<Tilemap>();
        _switch = GetComponent<Switch>();
        
        _spawnTime = _totalSpawnTime / Math.Max(enemies.Sum(e => e.Count), 1);

        // Only spawn enemies, if this spawner has not already been defeated
        if (!_switch.Value)
        {
            StartCoroutine(SpawnEnemies());
        }
    }

    IEnumerator SpawnEnemies() {
        yield return new WaitForSeconds(_spawnTime);

        foreach (var enemy in enemies)
        {
            var spawnCount = enemy.Count;
            if (spawnEnemiesPerPlayer)
            {
                spawnCount *= PlayerDataKeeper.Instance.NumPlayers;
            }
            
            for (var i = 0; i < spawnCount; ++i)
            {
                SpawnWithRandomPosition(enemy.Type);
                yield return new WaitForSeconds(_spawnTime);
            }
        }
    }

    void SpawnWithRandomPosition(EnemyController obj)
    {
        var spawnPositions = GetValidSpawnPositions();
        if (spawnPositions.Count > 0) { // if no spawn position with no collider inside radius exists
            int index = _rand.Next(spawnPositions.Count);
            var instance = Instantiate(obj, spawnPositions[index], Quaternion.identity);
            instance.GetComponent<Health>().OnDeath += OnEnemyDead;
            instance.ShowParticles();
            _spawnedEnemies++;
        }
    }

    List<Vector3> GetValidSpawnPositions()
    {
        if (customSpawnPoints is null || customSpawnPoints.Count == 0)
        {
            var spawnPositions = new List<Vector3>();
            foreach (var position in _tilemap.cellBounds.allPositionsWithin)
            {
                var floatPosition = (Vector3) position;
                var collider = Physics2D.OverlapCircle(floatPosition, _emptyRadius);
                if (_tilemap.HasTile(position) && collider is null)
                {
                    spawnPositions.Add(floatPosition);
                }
            }
            return spawnPositions;
        }

        else
        {
            return customSpawnPoints.Select(obj => obj.position).ToList();
        }
    }

    void OnEnemyDead()
    {
      _deadEnemies++;
      if (_deadEnemies == _spawnedEnemies)
          _switch.Value = true;
    }

}
