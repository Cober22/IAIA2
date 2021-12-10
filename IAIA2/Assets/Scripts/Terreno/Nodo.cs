using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Nodo
{
    public int gridX;
    public int gridY;

    public bool IsWall;
    public Vector3 position;
    
    public Nodo Parent;

    public bool visited;
    
    public float gCost;
    public float hCost;
    public float influencia;
    public GameObject tile;
    
    public float FCost { get { return gCost + hCost; } }
    
    public Nodo(bool is_Wall, Vector3 a_Pos, int a_gridX, int a_gridY, GameObject a_tile)
    {   
        IsWall = is_Wall;
        position = a_Pos;
        gridX = a_gridX;
        gridY = a_gridY;

        tile = a_tile;
        tile.transform.position = a_Pos;
    }
}
