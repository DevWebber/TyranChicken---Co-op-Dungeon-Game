using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class EnemySpawner : NetworkBehaviour {
    /// <summary>
    /// Simple class to spawn a desired enemy any number of times
    /// </summary>
    [SerializeField]
    private GameObject enemyPrefab;
    [SerializeField]
    private int numberOfEnemies;

    private Vector3 spawnPosition;
    private Quaternion spawnRotation;
    private GameObject tempEnemy;

    private bool notSpawned = false;
    private int maxAmountToSpawn = 10;
    private int numberSpawned;

    //At the server start, spawn some enemies.
	public override void OnStartServer()
    {
        SpawnEnemies();
    }

    void FixedUpdate()
    {
        //Checks to see if the cooldown is off and if the fixed amount has already spawned
        if (notSpawned && numberSpawned < maxAmountToSpawn)
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
            //Calls for the network to spawn them, so they are tracked server side
            NetworkServer.Spawn(tempEnemy);
        }

        Invoke("SpawnCooldown", 10f);
        numberSpawned++;
    }

    private void SpawnCooldown()
    {
        notSpawned = true;
    }
}
