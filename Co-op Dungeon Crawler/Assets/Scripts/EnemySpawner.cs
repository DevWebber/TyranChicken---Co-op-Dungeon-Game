using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class EnemySpawner : NetworkBehaviour {

    [SerializeField]
    private GameObject enemyPrefab;
    [SerializeField]
    private int numberOfEnemies;

    private Vector3 spawnPosition;
    private Quaternion spawnRotation;
    private GameObject tempEnemy;

    private bool notSpawned = false;

	public override void OnStartServer()
    {
        SpawnEnemies();
    }

    void FixedUpdate()
    {
        if (notSpawned)
        {
            Invoke("SpawnEnemies", 1f);
            notSpawned = false;
        }
    }

    private void SpawnEnemies()
    {
        for (int i = 0; i < numberOfEnemies; i++)
        {
            spawnPosition = new Vector3(Random.Range(-8.0f, 8.0f), 0, Random.Range(92.0f, 108.0f));
            spawnRotation = Quaternion.Euler(0.0f, Random.Range(0, 180f), 0.0f);

            tempEnemy = Instantiate(enemyPrefab, spawnPosition, spawnRotation);
            NetworkServer.Spawn(tempEnemy);
        }

        Invoke("SpawnCooldown", 10f);
    }

    private void SpawnCooldown()
    {
        notSpawned = true;
    }
}
