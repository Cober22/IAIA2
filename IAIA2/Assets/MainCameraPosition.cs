using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainCameraPosition : MonoBehaviour
{
    Grid grid;

    private void Awake()
    {
        grid = GameObject.FindObjectOfType<MapGenerator>().GetComponent<Grid>();
    }
    // Start is called before the first frame update
    void Start()
    {
        transform.position = new Vector3(0, 2, -1f);
        transform.GetComponent<Camera>().orthographicSize = grid.gridSizeX / 2;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
