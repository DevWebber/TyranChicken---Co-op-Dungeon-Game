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

    private string[] playerBodyParts;
    private string playerID;
    //Stores all possible weapon names for switching them
    [SerializeField]
    private string[] possibleWeapons;

    private bool invulnerable;
    //These two are for changing the player to red when they get hit
    private Renderer playerMaterial;
    private Color playerColor;
    //Animator and another bool to check if the animation is playing
    private Animator playerAnimator;

    private bool canAttack = true;
    private bool isAnimPlaying;
    private bool isSecondSwing;
    private bool isThirdSwing;
    private bool isFinalHit;

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
        if (!isLocalPlayer)
        {
            return;
        }

        invulnerable = false;
        playerAnimator = GetComponent<Animator>();
        //Gets the renderer of the player so we can use it's material and change it's color
        playerMaterial = new Renderer();

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
        if (Input.GetKeyDown(KeyCode.Mouse0) && canAttack)
        {
            if (!isSecondSwing && !isThirdSwing)
            {
                CmdStartAttack("IsFirstSwing");
                isSecondSwing = true;
            }
            else if (isSecondSwing)
            {
                CancelInvoke("ResetFullAttack");

                CmdStartAttack("IsSecondSwing");
                isThirdSwing = true;
                isSecondSwing = false;
            }
            else if (isThirdSwing)
            {
                CancelInvoke("ResetFullAttack");
                CmdStartAttack("IsThirdSwing");

                isThirdSwing = false;
                isFinalHit = true;
            }
        }
    }

    void FixedUpdate()
    {
        if (!isLocalPlayer)
        {
            return;
        }
    }

    //Start the attack
    [Command]
    void CmdStartAttack(string attackType)
    {
        RpcAttack(attackType);
        isAnimPlaying = true;
    }

    //Client side attack, once the server has verified you can actually attack
    [ClientRpc]
    void RpcAttack(string attackType)
    {
        //Change the bool state in the animator so it starts the sword swing.
        //This might be conflicting with the play method.
        canAttack = false;

        if (!isFinalHit)
        {
            Invoke("ResetAttack", 0.5f);
            playerAnimator.SetBool(attackType, true);
            Invoke("ResetFullAttack", 1f);
        }
        else
        {
            Invoke("ResetAttack", 1f);
            playerAnimator.SetBool(attackType, true);
            Invoke("ResetFullAttack", 1f);
        }
    }

    private void ResetAttack()
    {
        canAttack = true;
    }

    private void ResetFullAttack()
    {
        isSecondSwing = false;
        isThirdSwing = false;

        playerAnimator.SetBool("IsFirstSwing", false);
        playerAnimator.SetBool("IsSecondSwing", false);
        playerAnimator.SetBool("IsThirdSwing", false);
        isFinalHit = false;

        isAnimPlaying = false;
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
