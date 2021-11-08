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
    private int neutralPosElements = 0;
    private float limXLeft;
    private float limXRight;
    private float limYUp;
    private float limYDown;
    private int i = 0;
    private GameObject hootchs;
	private GameObject obstacles;
    private GameObject environment;

    private Nodo[,] grid;
    private GameObject tileObject;
	private List<Nodo> hootchs_NodesAvailable = new List<Nodo>();
	private List<Nodo> obstacles_NodesAvailable = new List<Nodo>();
	private List<Nodo> pathfing_NodesAvailable = new List<Nodo>();
    [SerializeField]
    public List<Nodo> hootchsNodes = new List<Nodo>();
    [SerializeField]
    public List<Nodo> obstaclesNodes = new List<Nodo>();

    private void Awake()
    {
		grid = GetComponent<Grid>().grid;
        tileObject = GetComponent<Grid>().tile;
        mapWidth = GetComponent<Grid>().gridSizeX;
		mapHeight = GetComponent<Grid>().gridSizeY;

        limXLeft = -mapWidth / 2 + marginX;
        limXRight = mapWidth / 2 - marginX;
        limYUp = mapHeight / 2 - marginY;
        limYDown = -mapHeight / 2 + marginY;

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
                        //grid[x, y].tile.transform.GetComponent<SpriteRenderer>().color = regions[region].colour;
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
                                DrawSprite("/Environment", grid[x, y], 2);
                                pathfing_NodesAvailable.Add(grid[x, y]);
                                break;
                            case 1:
                                DrawSprite("/Environment", grid[x, y], 1);
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

        RenderNonPathSprites(obstacles_NodesAvailable, obstaclesNodes, n_Obstacles, "/Obstacles", 3);

        RenderNonPathSprites(hootchs_NodesAvailable, hootchsNodes, n_Hootchs, "/Hootchs", 4);
    }

    private void RenderNonPathSprites(List<Nodo> elements, List<Nodo> finalElements, int numElements, string parentName, int index)
    {
        i = 0;
        int pos;
        Nodo element;

        // Bucle para dibujar cada elemento en la posicion correcta.
        // Se detendrá si el mapa no tiene el número de casillas sufientes del tipo de territorio necesario
        while (elements.Count > 0 && i < numElements)
        {
            pos = Random.Range(0, elements.Count);
            element = elements[pos];

            // Bucle para respetar margenes, se eliminan los nodos que no los cumplen
            while (elements.Count > 0 &&
                (limXLeft > element.position.x || element.position.x > limXRight) ||
                (limYUp < element.position.z || element.position.z < limYDown))
            {
                pathfing_NodesAvailable.Add(elements[pos]);
                elements.RemoveAt(pos);

                if (elements.Count > 1)
                {
                    pos = new System.Random(seed).Next(0, elements.Count);
                    element = elements[pos];
                }
            }

            // Se recorrera la lista completa de elementos y se irán guardando
            // uno por uno en la lista final que usaremos para dibujar
            HomogenizeDistribution(elements, finalElements, element, parentName, pos, index);
        }

        // Borramos nodos sobrantes y los añadimos a la lista que usaremos para el pathfinding
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

        foreach (Nodo invalid in hootchsNodes)
            if(pathfing_NodesAvailable.Count > 0)
                pathfing_NodesAvailable.Remove(invalid);

        foreach (Nodo invalid in obstaclesNodes)
            if (pathfing_NodesAvailable.Count > 0)
                pathfing_NodesAvailable.Remove(invalid);
    }

    private void HomogenizeDistribution(List<Nodo> elements, List<Nodo> finalElements, Nodo element, string parentName, int pos, int index)
    {
        float midX = grid[mapWidth/2, mapHeight/2].position.x;

        int aux = neutralPosElements;
        if (element.position.x < midX)
            aux--;
        else if(element.position.x > midX)
            aux++;

        // Equilibrio entre los lados
        if(aux == 1 || aux == 0 || aux == -1) 
        {
            DrawSprite(parentName, element, index);
            finalElements.Add(element);
            elements.Remove(element);
            neutralPosElements = aux;
            i++;
        } else
        {
            pathfing_NodesAvailable.Add(element);
            elements.Remove(element);
        }
    }

    private void DrawSprite(string parentName, Nodo element, int index)
    {
        GameObject renderHolder = Instantiate(tileObject);

        // Se destruye el render de los tiles para poner el nuevo elemento correspondiente
        DestroyImmediate(renderHolder.GetComponent<SpriteRenderer>());
        DestroyImmediate(renderHolder.GetComponent<RandomSprite>());

        // Se crea el objeto y se organiza en el inspector
        SpriteRenderer renderSprite = renderHolder.AddComponent<SpriteRenderer>();
        renderHolder.transform.SetParent(GameObject.Find(parentName).transform);
        renderSprite.color = new Color(1.0f, 1.0f, 1.0f, 1.0f);

        // Se crea el nuevo sprite correspondiente
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
        hootchsNodes.Clear();
        obstaclesNodes.Clear();

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
            foreach (Nodo nodo in hootchsNodes)
            {
                Gizmos.color = Color.white;
                Gizmos.DrawCube(nodo.position, Vector3.one * 1);
            }

            foreach (Nodo nodo in obstaclesNodes)
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