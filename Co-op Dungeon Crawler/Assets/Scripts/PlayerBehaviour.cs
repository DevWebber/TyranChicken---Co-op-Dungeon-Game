using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerBehaviour : MonoBehaviour {
    /// <summary>
    /// Controls the players behaviour, consisting of their health, attacking and interacting.
    /// </summary>
    /// 

    public delegate void SendHealthInfo(int health);
    public static event SendHealthInfo OnSendHealthInfo;

    [SerializeField]
    private int playerHealth;

    private bool invulnerable;
    private Material[] playerMaterial;
    private Color playerColor;
    private Animation playerAnim;
    private bool isAnimPlaying;

    public bool IsAnimationPlaying
    {
        get { return isAnimPlaying; }
    }

    private void OnEnable()
    {
        
    }

    private void OnDisable()
    {
        
    }

    void Start()
    {
        invulnerable = false;
        playerAnim = GetComponent<Animation>();
        //This lets us control the material of the player. Later on this will be more efficient.
        playerMaterial = new Material[transform.childCount - 1];

        for (int i = 0; i < playerMaterial.Length; i++)
        {
            playerMaterial[i] = transform.GetChild(i).GetComponent<Renderer>().material;
        }
        playerColor = playerMaterial[0].color;

        //Initially sends the health of the player to whatever is keeping an eye on it
        SendHealthData();
	}

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Mouse0))
        {
            playerAnim.Blend("Sword Attack");
            isAnimPlaying = playerAnim.isPlaying;
        }
    }

    void FixedUpdate()
    {
        if (playerAnim.isPlaying == false)
        {
            isAnimPlaying = false;
        }
    }

    public void TakeDamage(int damage)
    {
        //Invulnerability means the player doesn't constantly take damage when next to an enemy
        if (!invulnerable)
        {
            playerHealth -= damage;

            invulnerable = true;
            ChangeColor(Color.red);

            Invoke("ResetInvulnerable", 1.5f);

            SendHealthData();
        }
    }

    //Resets the invulnerability state
    private void ResetInvulnerable()
    {
        invulnerable = false;
        ChangeColor(playerColor);
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
}
