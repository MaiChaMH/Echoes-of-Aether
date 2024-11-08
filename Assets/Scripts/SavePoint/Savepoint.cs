using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Savepoint : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            Player player = collision.GetComponent<Player>();
            if (player != null)
            {
                player.SetSavepoint(transform.position);
                Debug.Log("Savepoint Activated");
            }
        }
    }
}
