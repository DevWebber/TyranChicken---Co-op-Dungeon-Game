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

    private void OnEnable()
    {
        
    }

    private void OnDisable()
    {
        
    }

    void Start()
    {
        invulnerable = false;
        playerMaterial = new Material[transform.childCount - 1];

        for (int i = 0; i < playerMaterial.Length; i++)
        {
            playerMaterial[i] = transform.GetChild(i).GetComponent<Renderer>().material;
        }
        playerColor = playerMaterial[0].color;

        SendHealthData();
	}

    public void TakeDamage(int damage)
    {
        if (!invulnerable)
        {
            playerHealth -= damage;

            invulnerable = true;
            ChangeColor(Color.red);

            Invoke("ResetInvulnerable", 1.5f);

            SendHealthData();
        }
    }

    private void ResetInvulnerable()
    {
        invulnerable = false;
        ChangeColor(playerColor);
    }

    private void ChangeColor(Color colorToChange)
    {
        for (int i = 0; i < playerMaterial.Length; i++)
        {
            playerMaterial[i].color = colorToChange;
        }
    }

    void SendHealthData()
    {
        if (OnSendHealthInfo != null)
        {
            OnSendHealthInfo(playerHealth);
        }
    }
}
