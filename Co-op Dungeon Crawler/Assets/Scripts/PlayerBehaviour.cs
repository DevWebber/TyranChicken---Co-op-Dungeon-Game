using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class PlayerBehaviour : NetworkBehaviour {
    /// <summary>
    /// Controls the players behaviour, consisting of their health, attacking and interacting.
    /// </summary>
    /// 

    public delegate void SendHealthInfo(int health);
    public static event SendHealthInfo OnSendHealthInfo;

    [SerializeField][SyncVar(hook ="OnChangeHealth")]
    private int playerHealth;
    [SerializeField]
    private int playerDamage;

    private string[] playerBodyParts;
    private string playerID;

    private bool invulnerable;
    private Material[] playerMaterial;
    private Color playerColor;
    private Animation playerAnim;
    private bool isAnimPlaying;

    public bool IsAnimationPlaying
    {
        get { return isAnimPlaying; }
    }

    public int PlayerDamage
    {
        get { return playerDamage; }
    }
    void Start()
    {
        invulnerable = false;
        playerAnim = GetComponent<Animation>();
        playerBodyParts = new string[] { "Head", "Body", "LeftArm", "RightArm", "LeftLeg", "RightLeg" };
        //This lets us control the material of the player. Later on this will be more efficient.
        playerMaterial = new Material[playerBodyParts.Length];

        for (int i = 0; i < playerMaterial.Length; i++)
        {
            playerMaterial[i] = transform.Find(playerBodyParts[i]).GetComponent<Renderer>().material;
        }
        playerColor = playerMaterial[0].color;

        //Initially sends the health of the player to whatever is keeping an eye on it
        SendHealthData();
	}

    void Update()
    {
        if (!isLocalPlayer)
        {
            return;
        }

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

        if (playerAnim.isPlaying == false)
        {
            isAnimPlaying = false;
        }
    }

    [Command]
    void CmdStartAttack()
    {
        RpcAttack();
    }

    [ClientRpc]
    void RpcAttack()
    {
        playerAnim.Blend("Sword Attack");
        isAnimPlaying = playerAnim.isPlaying;
    }

    public void TakeDamage(int damage)
    {
        //Invulnerability means the player doesn't constantly take damage when next to an enemy
        if (!isServer)
        {
            return;
        }

        if (!invulnerable)
        {
            playerHealth -= damage;

            if (playerHealth <= 0)
            {
                RpcRespawn();
            }
            else
            {

                invulnerable = true;
                ChangeColor(Color.red);

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

    [ClientRpc]
    void RpcRespawn()
    {
        if (isLocalPlayer)
        {
            playerHealth = 100;
            transform.position = Vector3.zero;
        }
    }
    //Helper method to change all parts of the player to a certain color.
    private void ChangeColor(Color colorToChange)
    {
        for (int i = 0; i < playerMaterial.Length; i++)
        {
            playerMaterial[i].color = colorToChange;
        }
    }

    //Sends the health value to any listeners who might want it
    void SendHealthData()
    {
        if (OnSendHealthInfo != null)
        {
            OnSendHealthInfo(playerHealth);
        }
    }

    void OnChangeHealth(int health)
    {
        playerHealth = health;
        SendHealthData();
    }

    public override void OnStartClient()
    {
        SendHealthData();
    }
}
