using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class PlayerBehaviour : NetworkBehaviour {
    /// <summary>
    /// Controls the players behaviour, consisting of their health, attacking and interacting.
    /// </summary>
    /// 

    //These methods act as data senders. Mainly for private and secure way of sending info between
    //scripts.
    public delegate void SendHealthInfo(int health);
    public static event SendHealthInfo OnSendHealthInfo;

    //SyncVar is for multiplayer, mainly for the server
    [SerializeField][SyncVar(hook ="OnChangeHealth")]
    private int playerHealth;
    [SerializeField]
    private int playerDamage;

    //This was used for the box model, if we have one consistent model then this isn't needed
    private string[] playerBodyParts;

    private bool invulnerable;
    //These two are for changing the player to red when they get hit
    private Renderer playerMaterial;
    private Color playerColor;
    //Animator and another bool to check if the animation is playing
    private Animator playerAnimator;
    private bool isAnimPlaying;

    //This is not to secure as the health data
    public bool IsAnimationPlaying
    {
        get { return isAnimPlaying; }
        set { isAnimPlaying = value; }
    }

    public int PlayerDamage
    {
        get { return playerDamage; }
    }
    void Start()
    {
        invulnerable = false;
        playerAnimator = GetComponent<Animator>();
        //Everything commented is for the system that turns the player red when they get hit.
        //This just needs to get the parts that need to be changed, find their renderer and color
        //
        //playerBodyParts = new string[] { "Head", "Body", "LeftArm", "RightArm", "LeftLeg", "RightLeg" };
        //This lets us control the material of the player. Later on this will be more efficient.
        playerMaterial = new Renderer();

        // for (int i = 0; i < playerMaterial.Length; i++)
        // {
        //     playerMaterial[i] = transform.Find(playerBodyParts[i]).GetComponent<Renderer>().material;
        // }
        playerMaterial = transform.GetComponentInChildren<Renderer>();
        playerColor = playerMaterial.material.color;

        //Initially sends the health of the player to whatever is keeping an eye on it
        SendHealthData();
	}

    void Update()
    {
        if (!isLocalPlayer)
        {
            return;
        }

        //Send a command to the server to attack
        if (Input.GetKeyDown(KeyCode.Mouse0))
        {
            CmdStartAttack();
        }
    }

    void FixedUpdate()
    {
        if (!isLocalPlayer)
        {
            return;
        }
        
        //if (playerAnimator.IsInTransition(0) == false)
        //{
        //    isAnimPlaying = false;
        //    playerAnimator.SetBool("isSwinging", false);
        //}
    }

    //Start the attack
    [Command]
    void CmdStartAttack()
    {
        RpcAttack();
    }

    //Client side attack, once the server has verified you can actually attack
    [ClientRpc]
    void RpcAttack()
    {
        //Play the sword attack animation
        //playerAnimator.Play("Sword Attack");
        //Set whether we can play or not to true
        //isAnimPlaying = playerAnimator.IsInTransition(0);
        //Change the bool state in the animator so it starts the sword swing.
        //This might be conflicting with the play method.
        playerAnimator.SetBool("IsSwinging", true);
        isAnimPlaying = true;
    }

    public void TakeDamage(int damage)
    {
        //Invulnerability means the player doesn't constantly take damage when next to an enemy
        //If it is the server, don't do anything.
        if (!isServer)
        {
            return;
        }

        if (!invulnerable)
        {
            playerHealth -= damage;

            //If you die, just respawn
            if (playerHealth <= 0)
            {
                RpcRespawn();
            }
            else
            {
                invulnerable = true;
                ChangeColor(Color.red);

                //After 1.5 seconds, allow the player to get hit again
                Invoke("ResetInvulnerable", 1.5f);
            }
        }
    }

    //Resets the invulnerability state
    private void ResetInvulnerable()
    {
        invulnerable = false;
        ChangeColor(playerColor);
    }

    //Client side respawn
    [ClientRpc]
    void RpcRespawn()
    {
        //If this is the local player
        if (isLocalPlayer)
        {
            playerHealth = 100;
            transform.position = Vector3.zero;
        }
    }
    //Helper method to change all parts of the player to a certain color.
    private void ChangeColor(Color colorToChange)
    {
        playerMaterial.material.color = colorToChange;
    }

    //Sends the health value to any listeners who might want it
    void SendHealthData()
    {
        if (OnSendHealthInfo != null)
        {
            OnSendHealthInfo(playerHealth);
        }
    }

    //When the health is change, update it and send it to the UI
    void OnChangeHealth(int health)
    {
        playerHealth = health;
        SendHealthData();
    }

    //When the client is started, send some health data
    public override void OnStartClient()
    {
        SendHealthData();
    }
}
