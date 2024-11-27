using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class TileMapRenderer : MonoBehaviour
{
    public Color newColor = Color.blue; 

    void Start()
    {
        var renderer = GetComponent<TilemapRenderer>();
        if (renderer != null)
        {
            renderer.material.color = newColor;
        }
    }

    
}
