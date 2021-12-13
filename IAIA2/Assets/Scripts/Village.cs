using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Village : MonoBehaviour
{
    public bool conqueredByPlayer = false;
    public bool conqueredByIA = false;
    public int goldPerTurn = 10;
    public int playerNumber;
    public int cost = 100;

    private Grid grid;

    //private bool pulsado;

    List<Nodo> vecinos;

    private SpriteRenderer spriteRenderer;
    public Sprite spriteEnemigo;
    public Sprite spriteAliado;

    public void Start()
    {
        conqueredByIA = false;
        conqueredByPlayer = false;

        //pulsado = false;

        grid = GameObject.Find("Map Generator").GetComponent<Grid>();

        vecinos = grid.GetNeighbouringNodes(grid.NodeFromWorldPosition(this.transform.position));

        spriteRenderer = gameObject.GetComponent<SpriteRenderer>();
    }

    public void Update()
    {
        Unit[] units = FindObjectsOfType<Unit>();

        foreach (Unit unit in units)
        {
            foreach (Nodo vecino in vecinos)
            {
                //Debug.Log(grid.NodeFromWorldPosition(unit.position) == vecino);
                if (grid.NodeFromWorldPosition(unit.position) == vecino)
                {
                    if (unit.gameObject.layer == 7 && unit.gameObject.GetComponent<BTCharacter>().conquistarVilla)
                    {
                        conqueredByIA = true;
                        conqueredByPlayer = false;
                    }
                    else if (unit.gameObject.layer != 7 && Input.GetKeyDown(KeyCode.E))
                    {
                        conqueredByIA = false;
                        conqueredByPlayer = true;
                        //pulsado = false;
                    }
                }
            }
        }

        if (conqueredByPlayer)
        {
            spriteRenderer.sprite = spriteAliado;
            playerNumber = 2;
        }
        else if (conqueredByIA)
        {
            spriteRenderer.sprite = spriteEnemigo;
            playerNumber = 1;
        }
    }

    /*private void OnMouseDown()
    {
        pulsado = true;
        Debug.Log("pulsado: " + pulsado);
    }*/

}