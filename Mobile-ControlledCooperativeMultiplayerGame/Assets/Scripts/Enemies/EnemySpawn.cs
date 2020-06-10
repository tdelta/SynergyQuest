using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;

public class EnemySpawn : MonoBehaviour {
    public EnemyController slime;
    public EnemyController bat;
    public EnemyController knight;

    public int numberOfSlimes = 1;
    public int numberOfBats = 1;
    public int numberOfKnights = 1;

    int emptyRadius = 1;
    float totalSpawnTime = 1;
    float spawnTime;
    Tilemap tilemap;
    System.Random rand = new System.Random();
    List<EnemyController> gameObjects = new List<EnemyController>();
    

    // Start is called before the first frame update
    void Start() {
        tilemap = GetComponent<Tilemap>();
        spawnTime = totalSpawnTime / (numberOfSlimes + numberOfBats + numberOfKnights);
        Invoke("SpawnEnemies", spawnTime);
    }

    void Update() {
        CheckExit();
    }

    void CheckExit() {
        if (gameObjects.All(e => e == null)) {
            // open door, activate teleporter, whatever...
        }
    }

    void SpawnEnemies() {
        if (numberOfSlimes-- > 0)
            SpawnWithRandomPosition(slime);
        else if (numberOfBats-- > 0)
            SpawnWithRandomPosition(bat);
        else if (numberOfKnights-- > 0)
            SpawnWithRandomPosition(knight);
        else    
            return;
        Invoke("SpawnEnemies", spawnTime);
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
