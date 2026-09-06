using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SC_Floor : MonoBehaviour
{
    public delegate void FloorCollisionHandler();
    public static event FloorCollisionHandler OnFloorCollision;

    void OnCollisionEnter2D(Collision2D col)
    {
        // Debug.Log("OnCollisionEnter2D " + col.gameObject.name);
        if (col.gameObject.CompareTag("Player"))
        {
            Debug.Log("Mario Collision!");

            float _playerY = col.gameObject.transform.position.y;   
            float _tileY = transform.position.y;

            // Debug.Log(_playerY + " " + _tileY);
            if (_playerY > _tileY + 0.45f)
            {
                if (OnFloorCollision != null)
                    OnFloorCollision(); 
            }
        }
    }
}
