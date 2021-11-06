using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Grid : MonoBehaviour
{
    public LayerMask wallMask;
    public Vector2 gridWorldSize;
    public float nodeRadius;
    public float distance;
    public Nodo[,] grid;
    public float nodeDiameter;
    public int gridSizeX, gridSizeY;
    public GameObject tile;

    void Awake()
    {
        GameObject grid = new GameObject();
        grid.name = "Grid";

        nodeDiameter = nodeRadius * 2;
        gridSizeX = Mathf.RoundToInt(gridWorldSize.x / nodeDiameter);
        gridSizeY = Mathf.RoundToInt(gridWorldSize.y / nodeDiameter);
        CreateGrid();
    }

    void CreateGrid()
    {
        grid = new Nodo[gridSizeX, gridSizeY];
        Vector3 bottomLeft = transform.position - Vector3.right * gridWorldSize.x / 2 - Vector3.forward * gridWorldSize.y / 2;
        for(int x = 0; x < gridSizeX; x++) {
            for(int y = 0; y < gridSizeY; y++){
                Vector3 worldPoint = bottomLeft + Vector3.right * (x * nodeDiameter + nodeRadius) + Vector3.forward * (y * nodeDiameter + nodeRadius);
                bool Wall = true;

                if (Physics.CheckSphere(worldPoint, nodeRadius, wallMask))
                    Wall = false;

                //-----------   CREACION DE GRID DE TILES   -----------//
                GameObject newTile = Instantiate(tile);
                newTile.transform.SetParent(GameObject.Find("/Grid").transform);
                newTile.transform.rotation = Quaternion.Euler(90f, 0f, 0f);
                newTile.transform.position = new Vector3(x, 0f, y);
                Nodo nodo = new Nodo(Wall, worldPoint, x, y, newTile);
                grid[x, y] = nodo;
            }
        }
    }

    public List<Nodo> GetNeighbouringNodes(Nodo a_Node)
    {
        List<Nodo> NeighbouringNodes = new List<Nodo>();
        int xCheck;
        int yCheck;

        for (int x = -1; x <= 1; x++)
        {
            for (int y = -1; y <= 1; y++)
            {
                if (x == 0 && y == 0) //if we are on the node tha was passed in, skip this iteration.
                    continue;

                xCheck = a_Node.gridX + x;
                yCheck = a_Node.gridY + y;

                //Make sure the node is within the grid.
                if (xCheck >= 0 && xCheck < gridSizeX && yCheck >= 0 && yCheck < gridSizeY)
                {   
                    NeighbouringNodes.Add(grid[xCheck, yCheck]); //Adds to the neighbours list.
                }
            }
        }

        return NeighbouringNodes;
    }

    public Nodo NodeFromWorldPosition(Vector3 a_WorldPosition)
    {
        float xpoint = ((a_WorldPosition.x + gridWorldSize.x / 2) / gridWorldSize.x);
        float ypoint = ((a_WorldPosition.z + gridWorldSize.y / 2) / gridWorldSize.y);

        xpoint = Mathf.Clamp01(xpoint);
        ypoint = Mathf.Clamp01(ypoint);

        int x = Mathf.RoundToInt((gridSizeX - 1) * xpoint);
        int y = Mathf.RoundToInt((gridSizeY - 1) * ypoint);

        return grid[x, y];
    }

    //private void OnDrawGizmos()
    //{
    //    Gizmos.DrawWireCube(transform.position, new Vector3(gridWorldSize.x, 1, gridWorldSize.y));
    //    if (grid != null)
    //    {
    //        foreach (Nodo nodo in grid)
    //        {
    //            if (nodo.IsWall)
    //                Gizmos.color = Color.white;
    //            else
    //                Gizmos.color = Color.yellow;

    //            Gizmos.DrawCube(nodo.position, Vector3.one * (nodeDiameter - distance));
    //        }
    //    }
    //}

}

//[System.Serializable]
//public struct TerrainType
//{
//    public string name;
//    public float height;
//    public Color colour;
//}