using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using static influenceMap;

public class MapGenerator : MonoBehaviour {

	public float noiseScale;
	public int octaves;
	[Range(0,1)]
	public float persistance;
	public float lacunarity;

	public int seed;
	public Vector2 offset;

    [Range(2, 5)]
    public int marginX;
    [Range(1, 5)]
    public int marginY;
    [Range(1, 3)]
    public int marginHootchs;

    public GameObject castilloAliado;
    public GameObject castilloEnemigo;
    public static Nodo nodoCastilloAliado;
    public static Nodo nodoCastilloEnemigo;

    public bool autoUpdate;

	public TerrainType[] regions;
    public UnitType[] unitsCollection;

    private int mapWidth;
	private int mapHeight;
	private int n_Hootchs = 6;
	private int n_Obstacles = 15;
    private int neutralPosPosition = 1;
    private int limXLeft;
    private int limXRight;
    private int limYUp;
    private int limYDown;
    private float midX;
    private int i = 0;
    private GameObject hootchs;
	private GameObject obstacles;
    private GameObject environment;
    private GameObject units;

    private Nodo[,] grid;
    private GameObject tileObject;
	private List<Nodo> hootchs_NodesAvailable = new List<Nodo>();
    private List<Nodo> obstacles_NodesAvailable = new List<Nodo>();
	public List<Nodo> pathfing_NodesAvailable = new List<Nodo>();

    public static List<GameObject> unitsPlayer = new List<GameObject>();
    public static List<GameObject> unitsEnemy = new List<GameObject>();
    public static List<Nodo> nodeUnitsPlayer= new List<Nodo>();
    public static List<Nodo> nodeUnitsEnemy = new List<Nodo>();

    [SerializeField]
    public static List<Nodo> hootchsNodes = new List<Nodo>();
    [SerializeField]
    public List<Nodo> obstaclesNodes = new List<Nodo>();

    [SerializeField]
    float _decay = 0.3f;

    [SerializeField]
    float _momentum = 0.8f;

    [SerializeField]
    int _updateFrequency = 3;

    [HideInInspector]
    public static InfluenceMap _influenceMap;

    private void Start()
    {
		grid = GetComponent<Grid>().grid;
        tileObject = GetComponent<Grid>().tile;
        mapWidth = GetComponent<Grid>().gridSizeX;
		mapHeight = GetComponent<Grid>().gridSizeY;
        midX = grid[mapWidth / 2, mapHeight / 2].position.x;

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

        units = new GameObject();
        units.name = "Units";

        _influenceMap = new InfluenceMap(mapWidth, mapHeight, _decay, _momentum);

        GenerateMap();

        int totalUnits = 0;
        foreach(UnitType unit in unitsCollection)
            totalUnits += unit.cantidad;

        while (units.transform.childCount < 6)
        {
            Debug.Log("Generate");
            GenerateMap();
        }

        InvokeRepeating("PropagationUpdate", 0.001f, 1.0f / _updateFrequency);
    }

    void PropagationUpdate()
    {
        //_influenceMap.Propagate();
        _influenceMap.GetInfluencesConsole();
    }

    private void CreateUnits(GameObject unit, GameObject castillo, List<GameObject> unitsList, Vector3 positionNewUnit)
    {
        if (positionNewUnit != castillo.transform.position)
        {
            bool correcPos = true;
            foreach (GameObject unitInMap in unitsList)
                if (unitInMap.transform.position == positionNewUnit)
                {
                    correcPos = false;
                    break;
                }
            if (correcPos)
                InstantiateUnits(unit, unitsList, positionNewUnit);
        }
    }

    private void InstantiateUnits(GameObject unit, List<GameObject> unitsList, Vector3 newUnitPosition)
    {
        GameObject newUnit = Instantiate(unit);
        newUnit.transform.position = newUnitPosition;
        newUnit.transform.SetParent(GameObject.Find("/Units").transform);
        SpriteRenderer[] newUnitsRenderer = newUnit.GetComponentsInChildren<SpriteRenderer>();

        foreach (SpriteRenderer renderer in newUnitsRenderer)
            renderer.sortingOrder = 1;

        unitsList.Add(newUnit);
    }

    /*public void ActualicePositionUnits(UnitType unit, Vector2 pos)
    {
        unit.position = pos;
    }*/

    public void GenerateMap()
    {
        seed = Random.Range(0, 100000);
        RestoreMap();
        float[,] noiseMap = Noise.GenerateNoiseMap(mapWidth, mapHeight, seed, noiseScale, octaves, persistance, lacunarity, offset);


        //-----------   CREACIÓN DE UNIDADES Y DE CASTILLO   -----------//
        int posX = Random.Range(0, marginX);
        int posY = Random.Range(0, mapHeight - marginY);
        nodoCastilloAliado = grid[posX, posY];
        castilloAliado.transform.position = new Vector3(grid[posX, posY].position.x, grid[posX, posY].position.y, 1f);

        posX = mapWidth - 1 - posX;
        posY = mapHeight - 1 - marginY - posY;
        nodoCastilloEnemigo = grid[posX, posY];
        castilloEnemigo.transform.position = new Vector3(grid[posX, posY].position.x, grid[posX, posY].position.y, 1f);

        Vector3 positionNewUnit;

        // Crear 'x' unidades de cada tipo para el enemigo y el jugador
        foreach (UnitType unit in unitsCollection)
        {
            int auxCantidad = unit.cantidad;
            while (auxCantidad > 0)
            {
                if (unit.aliado)
                {
                    posX = Random.Range(0, marginX);
                    posY = Random.Range(0, mapHeight - marginY);
                    nodeUnitsPlayer.Add(grid[posX, posY]);
                    positionNewUnit = new Vector3(grid[posX, posY].position.x, grid[posX, posY].position.y, 1f);
                    CreateUnits(unit.unit, castilloAliado, unitsPlayer, positionNewUnit);
                    auxCantidad--;
                }
                else if (!unit.aliado)
                {
                    posX = Random.Range(mapWidth - marginX, mapWidth);
                    posY = Random.Range(0, mapHeight - marginY);
                    nodeUnitsEnemy.Add(grid[posX, posY]);
                    positionNewUnit = new Vector3(grid[posX, posY].position.x, grid[posX, posY].position.y, 1f);
                    CreateUnits(unit.unit, castilloEnemigo, unitsEnemy, positionNewUnit);
                    auxCantidad--;
                }
            }
        }

        //-----------   CREACION ELEMENTOS DEL ENTORNO   -----------//
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
        while (elements.Count > numElements && i < numElements)
        {
            pos = Random.Range(0, elements.Count);
            element = elements[pos];

            // Bucle para respetar margenes, se eliminan los nodos que no los cumplen
            while (elements.Count > 0 &&
                (limXLeft > element.position.x || element.position.x > limXRight) ||
                (limYUp < element.position.y || element.position.y < limYDown))
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
            HomogenizeDistribution(elements, finalElements, element, parentName, index);
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

    private void HomogenizeDistribution(List<Nodo> elements, List<Nodo> finalElements, Nodo element, string parentName, int index)
    {
        // Equilibrio entre los lados
   
        // Parte derecha "jugador" del mapa
        if (element.position.x < midX && neutralPosPosition != -1)
        {
            if(parentName == "/Hootchs")
            {
                bool marginXHootch = true;
                // Margen X entre cabañas
                foreach (Nodo sprite in finalElements)
                    if (Vector3.Distance(sprite.position, element.position) <= marginHootchs)
                    {
                        marginXHootch = false;
                        break;
                    }
                if (marginXHootch)
                {
                    CreateFinalElement(elements, finalElements, element, parentName, index);
                    NotXCoincidenteInHootches(elements, element);
                }
            }
            else
            {
                CreateFinalElement(elements, finalElements, element, parentName, index);
            }
        }

        // Parte izquierda "enemiga" del mapa
        if (element.position.x > midX && neutralPosPosition != 1)
        {
            if (parentName == "/Hootchs")
            {
                bool marginXHootch = true;
                // Margen X entre cabañas
                foreach (Nodo sprite in finalElements)
                    if (Vector3.Distance(sprite.position, element.position) <= marginHootchs)
                    {
                        marginXHootch = false;
                        break;
                    }
                if (marginXHootch)
                {
                    CreateFinalElement(elements, finalElements, element, parentName, index);
                    NotXCoincidenteInHootches(elements, element);
                }
            }
            else
            {
                CreateFinalElement(elements, finalElements, element, parentName, index);
            }
        }
    }

    private void CreateFinalElement(List<Nodo> elements, List<Nodo> finalElements, Nodo element, string parentName, int index)
    {
        int posX = element.gridX;
        int posY = element.gridY;
        DrawSprite(parentName, element, index);
        element.tile.transform.gameObject.layer = 9;
        finalElements.Add(element);
        elements.Remove(element);
        grid[posX, posY].IsWall = true;
        neutralPosPosition = neutralPosPosition == 1 ? -1 : 1;
        i++;
    }

    private void NotXCoincidenteInHootches(List<Nodo> elements, Nodo element)
    {
        int c = 0;
        while (c < elements.Count)
        {
            if (elements[c].position.x == element.position.x)
            {
                pathfing_NodesAvailable.Add(elements[c]);
                elements.Remove(elements[c]);
            }
            else
            {
                c++;
            }
        }
    }

    private void DrawSprite(string parentName, Nodo element, int index)
    {
        GameObject parent = GameObject.Find(parentName);

        if (regions[index].element != null)
        {
            GameObject newElement = Instantiate(regions[index].element, parent.transform);
            newElement.transform.position = element.position;
            newElement.transform.localScale = element.tile.transform.localScale;
        } 
        else
        {
            GameObject renderHolder = Instantiate(tileObject);
            //Se destruye el render de los tiles para poner el nuevo elemento correspondiente
            DestroyImmediate(renderHolder.GetComponent<SpriteRenderer>());
            DestroyImmediate(renderHolder.GetComponent<RandomSprite>());
            DestroyImmediate(renderHolder.GetComponent<Tile>());

            // Se crea el objeto y se organiza en el inspector
            SpriteRenderer renderSprite = renderHolder.AddComponent<SpriteRenderer>();
            renderHolder.transform.SetParent(GameObject.Find(parentName).transform);
            renderSprite.color = new Color(1.0f, 1.0f, 1.0f, 1.0f);

            // Se crea el nuevo sprite correspondiente
            renderSprite.sprite = regions[index].sprite;
            renderSprite.transform.position = new Vector3(element.position.x, element.position.y, 0f);
            renderSprite.transform.localScale = element.tile.transform.localScale;

        }



    }

    private void RestoreMap()
    {
        hootchs_NodesAvailable.Clear();
        obstacles_NodesAvailable.Clear();
        pathfing_NodesAvailable.Clear();
        hootchsNodes.Clear();
        obstaclesNodes.Clear();
        unitsPlayer.Clear();
        unitsEnemy.Clear();

        foreach (Transform child in hootchs.transform)
            Destroy(child.gameObject);
        foreach (Transform child in obstacles.transform)
            Destroy(child.gameObject);
        foreach (Transform child in environment.transform)
            Destroy(child.gameObject);
        foreach (Transform child in units.transform)
            Destroy(child.gameObject);

        foreach (Nodo nodo in grid)
            nodo.IsWall = false;
        
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

    //private void OnDrawGizmos()
    //{
    //    Gizmos.DrawWireCube(transform.position, new Vector3(mapWidth, 1, mapHeight));
    //    if (grid != null)
    //    {
    //        foreach (Nodo nodo in hootchsNodes)
    //        {
    //            Gizmos.color = Color.white;
    //            Gizmos.DrawCube(nodo.position, Vector3.one * 1);
    //        }

    //        foreach (Nodo nodo in obstaclesNodes)
    //        {
    //            Gizmos.color = Color.red;
    //            Gizmos.DrawCube(nodo.position, Vector3.one * 1);
    //        }

    //        foreach (Nodo nodo in pathfing_NodesAvailable)
    //        {
    //            Gizmos.color = Color.cyan;
    //            Gizmos.DrawCube(nodo.position, Vector3.one * 1);
    //        }
    //    }
    //}
}

[System.Serializable]
public struct TerrainType {
	public string name;
	public float height;
	public Color colour;
	public Sprite sprite;
    public GameObject element;
}

[System.Serializable]
public struct UnitType
{
    public string name;
    [Range(1, 5)]
    public int cantidad;
    public bool aliado;
    //public float influenceValue;
    public GameObject unit;
    //public Vector2 position;
}