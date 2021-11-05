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

	public bool autoUpdate;

	public TerrainType[] regions;

	private int mapWidth;
	private int mapHeight;
	private int n_Hootchs = 5;
	private int n_Obstacles = 10;

	private Nodo[,] grid;
	private List<Nodo> pathfing_NodesAvailable = new List<Nodo>();
	private List<Nodo> obstacles_NodesAvailable = new List<Nodo>();
	private List<Nodo> hootchs_NodesAvailable = new List<Nodo>();
	private SpriteRenderer rendSprite;
	
	private void Awake()
    {
		grid = GetComponent<Grid>().grid;
		mapWidth = GetComponent<Grid>().gridSizeX;
		mapHeight = GetComponent<Grid>().gridSizeY;
	}

    public void GenerateMap() {
		float[,] noiseMap = Noise.GenerateNoiseMap (mapWidth, mapHeight, seed, noiseScale, octaves, persistance, lacunarity, offset);

		for (int x = 0; x < mapWidth; x++) {
			for (int y = 0; y < mapHeight; y++) {
				float currentHeight = noiseMap[x, y];
				for (int region = 0; region < regions.Length; region++) {
					if (currentHeight <= regions[region].height) {

						//-----------   CONFIGURACION CUBOS SEGUN ALGORITMO   -----------//
						//grid[x, y].Cube.GetComponent<Renderer>().material.color = regions[region].colour;

						grid[x, y].tile.transform.GetComponent<SpriteRenderer>().color = regions[region].colour;
						//-----------   LISTA DE NODOS PARA CADA REGION   -----------//
						switch (region)
                        {
                            case 5:
                                hootchs_NodesAvailable.Add(grid[x, y]);
								break;
							case 4:
								obstacles_NodesAvailable.Add(grid[x, y]);
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
		System.Random randomPos = new System.Random(seed);

		// Se colocan las camaasn
		/*int i = 0;
		while (i < hootchs_NodesAvailable.Count && i < n_Hootchs)
        {
			int pos = randomPos.Next(0, hootchs_NodesAvailable.Count);
			//------- AQUI AÑADIR CABAÑA



			//-------
			pathfing_NodesAvailable.Add(hootchs_NodesAvailable[pos]);
			hootchs_NodesAvailable.RemoveAt(pos);
			i++;
        }
		i = 0;
		
		while(i < obstacles_NodesAvailable.Count && i < n_Obstacles)
        {
			int pos = randomPos.Next(0, obstacles_NodesAvailable.Count);
			//------- AQUI AÑADIR OBSTACULO


			//-------
			pathfing_NodesAvailable.Add(hootchs_NodesAvailable[pos]);
			hootchs_NodesAvailable.RemoveAt(pos);
			i++;
		}*/
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
}

[System.Serializable]
public struct TerrainType {
	public string name;
	public float height;
	public Color colour;
	public Sprite sprite;
}