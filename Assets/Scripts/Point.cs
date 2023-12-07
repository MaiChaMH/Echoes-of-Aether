using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Point : MonoBehaviour
{
    private Transform enemyObjectTransform;
    public GameObject enemyOj;
    // Start is called before the first frame update
    void Start()
    {
        enemyObjectTransform = enemyOj.transform;
    }

    // Update is called once per frame
    void Update()
    {
        if (enemyObjectTransform == null)
        {
            Destroy(gameObject);
            return;
        }
        Vector3 newPosition = transform.position;
        newPosition.y = enemyObjectTransform.position.y;
        transform.position = newPosition;
    }
}
