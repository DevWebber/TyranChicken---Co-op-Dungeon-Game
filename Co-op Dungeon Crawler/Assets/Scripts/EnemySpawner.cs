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
    [SerializeField]
    private int timesToSpawn;

    public bool finalSpawner;

    private Transform mainSpawnPoint;

    private Vector3 spawnPosition;
    private Quaternion spawnRotation;
    private GameObject tempEnemy;

    private bool notSpawned = false;
    private bool finishedSpawning = false;
    private int numberSpawned = 0;

    private void OnEnable()
    {
        mainSpawnPoint = transform.GetChild(0);
    }

    private void OnTriggerEnter(Collider collider)
    {
        if (collider.tag == "Player")
        {
            notSpawned = true;
            transform.GetComponent<BoxCollider>().enabled = false;

            //If this is the final spawner, trigger the boss trigger script.
            if (finalSpawner)
            {
                BossTrigger tempBossTrigger = transform.parent.GetChild(0).GetComponent<BossTrigger>();
                tempBossTrigger.StartBossFight();
            }
        }
    }

    void FixedUpdate()
    {
        //Checks to see if the cooldown is off and if the fixed amount has already spawned
        if (notSpawned && numberSpawned < timesToSpawn)
        {
            Invoke("SpawnEnemies", 1f);
            notSpawned = false;
        }

        if (numberSpawned == timesToSpawn)
        {
            finishedSpawning = true;
        }
    }

    private void SpawnEnemies()
    {
        for (int i = 0; i < numberOfEnemies; i++)
        {
            spawnPosition = new Vector3(mainSpawnPoint.position.x + Random.Range(-8.0f, 8.0f), 0, mainSpawnPoint.position.z + Random.Range(-8.0f, 8.0f));
            spawnRotation = Quaternion.Euler(0.0f, Random.Range(0, 180f), 0.0f);

            tempEnemy = Instantiate(enemyPrefab, spawnPosition, spawnRotation);
            //Calls for the network to spawn them, so they are tracked server side
            NetworkServer.Spawn(tempEnemy);
        }

        Invoke("SpawnCooldown", 5f);
        numberSpawned++;
    }

    private void SpawnCooldown()
    {
        notSpawned = true;
    }

    public bool FinishedSpawning
    {
        get
        {
            return finishedSpawning;
        }
        set
        {
            finishedSpawning = value;
        }
    }
}
