using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class SC_Death : MonoBehaviour
{
    public delegate void SpikeCollisionHandler();
    public static event SpikeCollisionHandler OnSpikeCollision;

    public delegate void SpikeCollisionGeneralHandler(GameObject _Collied);
    public static event SpikeCollisionGeneralHandler OnSpikeCollisionGeneral;


    void OnCollisionEnter2D(Collision2D col)
    {
        // Debug.Log("OnCollisionEnter2D " + col.gameObject.name);
        if(col.gameObject.CompareTag("Player"))
        {
            Debug.Log("Mario Collision!");
            // if(col.gameObject.GetComponent<SC_Player>() != null)
            //    col.gameObject.GetComponent<SC_Player>().ResetMarioPosition();

            if (OnSpikeCollision != null)
                OnSpikeCollision();
        }
        else
        {
            if (OnSpikeCollisionGeneral != null)
                OnSpikeCollisionGeneral(col.gameObject);
        }
    }
}
