using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.U2D;

public class SC_Coin : MonoBehaviour
{
    public delegate void CoinCollisionHandler();
    public static event CoinCollisionHandler OnCoinCollision;

    void OnTriggerEnter2D(Collider2D col)
    {
        // Debug.Log("OnCollisionEnter2D " + col.gameObject.name);
        if (col.gameObject.CompareTag("Player"))
        {
            Debug.Log("Mario Collision!");
            if (OnCoinCollision != null)
                OnCoinCollision();

            this.gameObject.SetActive(false);
        }
    }
}
