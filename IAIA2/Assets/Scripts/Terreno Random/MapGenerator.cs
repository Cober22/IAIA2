using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MapGenerator : MonoBehaviour {


	public float noiseScale;
	public int octaves;
	[Range(0,1)]
	public float persistance;
	public float lacunarity;

	public int seed;
	public Vector2 offset;

    [Range(0, 5)]
    public int marginX;
    [Range(0, 3)]
    public int marginY;

    public bool autoUpdate;

	public TerrainType[] regions;

	private int mapWidth;
	private int mapHeight;
	private int n_Hootchs = 6;
	private int n_Obstacles = 15;
	private GameObject hootchs;
	private GameObject obstacles;
    private GameObject environment;

    private Nodo[,] grid;
	private List<Nodo> hootchs_NodesAvailable = new List<Nodo>();
	private List<Nodo> obstacles_NodesAvailable = new List<Nodo>();
	private List<Nodo> pathfing_NodesAvailable = new List<Nodo>();
	
	private void Awake()
    {
		grid = GetComponent<Grid>().grid;
		mapWidth = GetComponent<Grid>().gridSizeX;
		mapHeight = GetComponent<Grid>().gridSizeY;

		hootchs = new GameObject();
		hootchs.name = "Hootchs";
		
		obstacles= new GameObject();
		obstacles.name = "Obstacles";

        environment = new GameObject();
        environment.name = "Environment";
    }

    public void GenerateMap()
    {
        float[,] noiseMap = Noise.GenerateNoiseMap(mapWidth, mapHeight, seed, noiseScale, octaves, persistance, lacunarity, offset);

        RestoreMap();

        for (int x = 0; x < mapWidth; x++)
        {
            for (int y = 0; y < mapHeight; y++)
            {
                float currentHeight = noiseMap[x, y];
                for (int region = 0; region < regions.Length; region++)
                {
                    if (currentHeight <= regions[region].height)
                    {
                        grid[x, y].tile.transform.GetComponent<SpriteRenderer>().color = regions[region].colour;
                        //-----------   LISTA DE NODOS PARA CADA REGION   -----------//
                        switch (region)
                        {
                            case 4:
                                hootchs_NodesAvailable.Add(grid[x, y]);
                                break;
                            case 3:
                                obstacles_NodesAvailable.Add(grid[x, y]);
                                break;
                            case 2:
                                DrawSprite(grid[x, y], "/Environment", 2);
                                pathfing_NodesAvailable.Add(grid[x, y]);
                                break;
                            case 1:
                                DrawSprite(grid[x, y], "/Environment", 1);
                                pathfing_NodesAvailable.Add(grid[x, y]);
                                break;
                            default:
                                pathfing_NodesAvailable.Add(grid[x, y]);
                                break;
                        }
                        break;
                    }
                }
            }
        }

        RenderNonPathSprites(obstacles_NodesAvailable, n_Obstacles, "/Obstacles", 3);

        RenderNonPathSprites(hootchs_NodesAvailable, n_Hootchs, "/Hootchs", 4);
    }

    /// <summary>
    /// Método para dibujar los sprites que corresponden a los nodos que no pertenecen a los que podrán recorrerse
    /// </summary>
    /// <param name="i">Index para recorrer la lista de nodos del elemento a dibujar</param>
    /// <param name="elements">Lista de cada uno de los nodos sobre los que se dibujará el elemento</param>
    /// <param name="numElements">Número de elementos máximo que se van a dibujar</param>
    /// <param name="parentName">Objecto que será padre de los elementos. Para tenerlos organizados en el inspector.</param>
    /// <param name="index">Index del elemento que queremos dibujar, siendo el 1 el más alto</param>
    /// <returns></returns>
    private void RenderNonPathSprites(List<Nodo> elements, int numElements, string parentName, int index)
    {
        float limXLeft = -mapWidth / 2 + marginX;
        float limXRight = mapWidth / 2 - marginX;
        float limYUp = mapHeight/ 2 - marginY;
        float limYDown = -mapHeight / 2 + marginY;

        int i = 0;
        while (i < elements.Count && i < numElements)
        {
            int pos = new System.Random(seed).Next(0, elements.Count);
            Nodo element = elements[pos];

            if  (elements.Count > 0 && 
                (limXLeft > element.position.x || element.position.x > limXRight) ||
                (limYUp < element.position.z || element.position.z < limYDown))
            {
                pathfing_NodesAvailable.Add(elements[pos]);
                elements.RemoveAt(pos);

                if (elements.Count > 0)
                {
                    pos = new System.Random(seed).Next(0, elements.Count);
                    element = elements[pos];
                }
            }

            if(elements.Count > 0)
            {
                DrawSprite(element, parentName, index);
                elements.RemoveAt(pos);
                i++;
            }
        }

        if (elements.Count > 0)
        {
            int left = elements.Count - 1;
            while (left >= 0)
            {
                pathfing_NodesAvailable.Add(elements[left]);
                elements.RemoveAt(left);
                left--;
            }
        }
    }

    private void DrawSprite(Nodo element, string parentName, int index)
    {
        GameObject renderHolder = new GameObject();
        SpriteRenderer renderSprite = renderHolder.AddComponent<SpriteRenderer>();
        renderHolder.transform.SetParent(GameObject.Find(parentName).transform);
        renderSprite.color = new Color(1.0f, 1.0f, 1.0f, 1.0f);

        renderSprite.sprite = regions[index].sprite;
        renderSprite.transform.position = element.position;
        renderSprite.transform.rotation = Quaternion.Euler(90f, 0f, 0f);
        renderSprite.transform.localScale = element.tile.transform.localScale;
    }

    private void RestoreMap()
    {
        hootchs_NodesAvailable.Clear();
        obstacles_NodesAvailable.Clear();
        pathfing_NodesAvailable.Clear();

        foreach (Transform child in hootchs.transform)
            Destroy(child.gameObject);
        foreach (Transform child in obstacles.transform)
            Destroy(child.gameObject);
        foreach (Transform child in environment.transform)
            Destroy(child.gameObject);
    }

    void OnValidate() {
		if (mapWidth < 1) {
			mapWidth = 1;
		}
		if (mapHeight < 1) {
			mapHeight = 1;
		}
		if (lacunarity < 1) {
			lacunarity = 1;
		}
		if (octaves < 0) {
			octaves = 0;
		}
	}

    private void OnDrawGizmos()
    {
        Gizmos.DrawWireCube(transform.position, new Vector3(mapWidth, 1, mapHeight));
        if (grid != null)
        {
            foreach (Nodo nodo in hootchs_NodesAvailable)
            {
				Gizmos.color = Color.white;
                Gizmos.DrawCube(nodo.position, Vector3.one * 1);
            }

			foreach (Nodo nodo in obstacles_NodesAvailable)
			{
				Gizmos.color = Color.red;
				Gizmos.DrawCube(nodo.position, Vector3.one * 1);
			}

			foreach (Nodo nodo in pathfing_NodesAvailable)
			{
				Gizmos.color = Color.cyan;
				Gizmos.DrawCube(nodo.position, Vector3.one * 1);
			}
		}
    }
}

[System.Serializable]
public struct TerrainType {
	public string name;
	public float height;
	public Color colour;
	public Sprite sprite;
}