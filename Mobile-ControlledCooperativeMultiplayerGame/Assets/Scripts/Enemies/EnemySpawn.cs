using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;

[RequireComponent(typeof(Tilemap), typeof(Switch))]
public class EnemySpawn : MonoBehaviour {

    [Serializable]
    struct EnemyTypeCountPair {
        public EnemyController Type;
        public int Count;
    }

    // If true, the configured number of enemies will be spawned for every player
    [SerializeField] private bool spawnEnemiesPerPlayer = false;
    [SerializeField] List<EnemyTypeCountPair> enemies;
    /**
     * If this list is non-empty, enemies will be spawned at these points instead
     * of on random tiles
     */
    [SerializeField] private List<Transform> customSpawnPoints = null;

    private int _emptyRadius = 1;
    private float _totalSpawnTime = 1;
    private float _spawnTime;
    private bool _spawnComplete;
    private Tilemap _tilemap;
    private Switch _switch;
    private System.Random _rand = new System.Random();
    private List<EnemyController> _spawnedEnemies = new List<EnemyController>();

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

    void Update()
    {
        CheckExit();
    }

    void CheckExit()
    {
        if (_spawnComplete && _spawnedEnemies.All(e => e == null)) {
            _switch.Value = true;
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
        
        _spawnComplete = true;
    }

    void SpawnWithRandomPosition(EnemyController obj)
    {
        var spawnPositions = GetValidSpawnPositions();
        if (spawnPositions.Count > 0) { // if no spawn position with no collider inside radius exists
            int index = _rand.Next(spawnPositions.Count);
            var instance = Instantiate(obj, spawnPositions[index], Quaternion.identity);
            instance.ShowParticles();
            _spawnedEnemies.Add(instance);
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
}
