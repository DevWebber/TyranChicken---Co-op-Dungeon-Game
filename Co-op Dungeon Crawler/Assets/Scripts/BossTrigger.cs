using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.AI;

public class BossTrigger : MonoBehaviour {
    /// <summary>
    /// Have this class manage everything to do with the boss and such
    /// </summary>
    /// 

    private NetworkControl controller;

    [SerializeField]
    private EnemySpawner enemySpawner;
    [SerializeField]
    private Transform barrierObject;
    [SerializeField]
    private AudioSource mainAudioPlaying;

    [SerializeField]
    private Text objectiveText;
    [SerializeField]
    private GameObject minimapOrb;

    private bool barrierLifted = false;
    private bool notAlreadyLifted = false;

	void Awake()
    {
        controller = GameObject.FindGameObjectWithTag("NetworkManager").GetComponent<NetworkControl>();
	}

    private void OnTriggerEnter(Collider collision)
    {
        if (collision.gameObject.tag == "Player" && barrierLifted)
        {
            GetComponent<AudioSource>().Stop();
            controller.StopHost();
        }
    }

    private void FixedUpdate()
    {
        if (enemySpawner.FinishedSpawning)
        {
            Invoke("LiftBarrier", 5f);
            enemySpawner.FinishedSpawning = false;
        }

        if (barrierLifted && !notAlreadyLifted)
        {
            barrierObject.gameObject.SetActive(false);
            notAlreadyLifted = true;
        }
    }

    public void StartBossFight()
    {
        //Do some other stuff here later
        mainAudioPlaying.Stop();
        GetComponent<AudioSource>().Play();

        objectiveText.text = "-Defeat the enemies and Kill the Necromancer!";
        minimapOrb.GetComponent<MeshRenderer>().enabled = false;
    }

    private void LiftBarrier()
    {
        barrierLifted = true;
        objectiveText.text = "-The barrier has been lifted! Attack the Necromancer now!";
    }

}
