using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class EnemySpawn : MonoBehaviour {
    public EnemyController slime;
    public EnemyController bat;
    public EnemyController knight;

    public int numberOfSlimes = 1;
    public int numberOfBats = 1;
    public int numberOfKnights = 1;

    int emptyRadius = 2;

    // Start is called before the first frame update
    void Start() {
        var tilemap = GetComponent<Tilemap>();
        var rand = new System.Random();
        var spawnPositions = getValidSpawnPositions(tilemap, emptyRadius);

        spawnPositions = spawnRandomly(slime, numberOfSlimes, spawnPositions, rand);
        spawnPositions = spawnRandomly(bat, numberOfBats, spawnPositions, rand);
        spawnPositions = spawnRandomly(knight, numberOfKnights, spawnPositions, rand);
    }

    List<Vector3Int> spawnRandomly(EnemyController obj, int amount, List<Vector3Int> spawnPositions, System.Random rand) {
        for (int i = 0; i < amount; i++) {
            int index = rand.Next(spawnPositions.Count);
            var instance = Instantiate(obj, spawnPositions[index], Quaternion.identity);
            instance.ShowParticles();
            spawnPositions.RemoveAt(index);
        }
        return spawnPositions;
    }

    List<Vector3Int> getValidSpawnPositions(Tilemap tilemap, int radius) {
        var spawnPositions = new List<Vector3Int>();
        foreach (var position in tilemap.cellBounds.allPositionsWithin) {
            var collider = Physics2D.OverlapCircle((Vector3) position, radius);
            if (tilemap.HasTile(position) && collider == null)
                spawnPositions.Add(position);
        }
        return spawnPositions;
    }
}
