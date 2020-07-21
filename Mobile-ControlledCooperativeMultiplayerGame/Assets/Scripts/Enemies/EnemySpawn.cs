using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;

public class EnemySpawn : MonoBehaviour {

    [System.Serializable] struct EnemyTypeCountPair {
        public EnemyController Type;
        public int Count;
    }

    [SerializeField] List<EnemyTypeCountPair> enemies;

    int emptyRadius = 1;
    float totalSpawnTime = 1;
    float spawnTime;
    bool spawnComplete;
    Tilemap tilemap;
    Switch switcher;
    System.Random rand = new System.Random();
    List<EnemyController> gameObjects = new List<EnemyController>();

    // Start is called before the first frame update
    void Start() {
        tilemap = GetComponent<Tilemap>();
        switcher = GetComponent<Switch>();
        spawnTime = totalSpawnTime / Math.Max(enemies.Sum(e => e.Count), 1);
        StartCoroutine(SpawnEnemies());
    }

    void Update() {
        CheckExit();
    }

    void CheckExit() {
        if (spawnComplete && gameObjects.All(e => e == null)) {
            switcher.Value = true;
        }
    }

    IEnumerator SpawnEnemies() {
        yield return new WaitForSeconds(spawnTime);

        foreach (var enemy in enemies) {
            for (var i = 0; i < enemy.Count; i++) {
                SpawnWithRandomPosition(enemy.Type);
                yield return new WaitForSeconds(spawnTime);
            }
        }
        spawnComplete = true;
    }

    void SpawnWithRandomPosition(EnemyController obj) {
        var spawnPositions = GetValidSpawnPositions();
        if (spawnPositions.Count > 0) { // if no spawn position with no collider inside radius exists
            int index = rand.Next(spawnPositions.Count);
            var instance = Instantiate(obj, spawnPositions[index], Quaternion.identity);
            instance.ShowParticles();
            gameObjects.Add(instance);
        }
    }

    List<Vector3Int> GetValidSpawnPositions() {
        var spawnPositions = new List<Vector3Int>();
        foreach (var position in tilemap.cellBounds.allPositionsWithin) {
            var collider = Physics2D.OverlapCircle((Vector3) position, emptyRadius);
            if (tilemap.HasTile(position) && collider == null)
                spawnPositions.Add(position);
        }
        return spawnPositions;
    }
}
