using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class GameController : NetworkBehaviour
{

    [SerializeField]
    private GameObject enemySpawner;

    public override void OnStartServer()
    {
        base.OnStartServer();

        NetworkServer.Spawn(enemySpawner);
    }
}
