using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CharacterCreation : MonoBehaviour
{

    GM gm;

    //para tomar las posiciones donde aparecen los dos castillos
    //public MapGenerator mapGenerator;
    //para acceder a los métodos de comprobar nodos
    public Grid grid;

    public Button player1openButton;
    public Button player2openButton;

    public GameObject player1Menu;
    public GameObject player2Menu;


    private void Start()
    {
        gm = FindObjectOfType<GM>();
    }

    private void Update()
    {
        if (gm.playerTurn == 1)
        {
            player1openButton.interactable = true;
            player2openButton.interactable = false;
        }
        else
        {
            player2openButton.interactable = true;
            player1openButton.interactable = false;
        }
    }

    public void ToggleMenu(GameObject menu) {
        menu.SetActive(!menu.activeSelf);
    }

    public void CloseCharacterCreationMenus() {
        player1Menu.SetActive(false);
        player2Menu.SetActive(false);
    }

    public void BuyUnit (Unit unit) {

        if (unit.playerNumber == 1 && unit.cost <= GM.player1Gold)
        {
            player1Menu.SetActive(false);
            GM.player1Gold -= unit.cost;
        } else if (unit.playerNumber == 2 && unit.cost <= GM.player2Gold)
        {
            player2Menu.SetActive(false);
            GM.player2Gold -= unit.cost;
        } else {
            print("NOT ENOUGH GOLD, SORRY!");
            return;
        }

        gm.UpdateGoldText();
        gm.createdUnit = unit;

        //turno aliado entonces a la lista de unidades aliadas
        if (gm.playerTurn == 2)
        {
            MapGenerator.unitsPlayer.Add(unit.gameObject);
        }
        else
        {
            MapGenerator.unitsEnemy.Add(unit.gameObject);
        }

        DeselectUnit();
        SetCreatableTiles();
    }

    public void BuyVillage(Village village) {
        if (village.playerNumber == 1 && village.cost <= GM.player1Gold)
        {
            player1Menu.SetActive(false);
            GM.player1Gold -= village.cost;
        }
        else if (village.playerNumber == 2 && village.cost <= GM.player2Gold)
        {
            player2Menu.SetActive(false);
            GM.player2Gold -= village.cost;
        }
        else
        {
            print("NOT ENOUGH GOLD, SORRY!");
            return;
        }
        gm.UpdateGoldText();
        gm.createdVillage = village;

        DeselectUnit();

        SetCreatableTiles();

    }

    void SetCreatableTiles() {
        gm.ResetTiles();

        List<Nodo> vecinos;

        if (gm.playerTurn == 2)
        {
            vecinos = grid.GetAllNeighbouringNodes(MapGenerator.nodoCastilloAliado);
        }
        else
        {
            vecinos = grid.GetAllNeighbouringNodes(MapGenerator.nodoCastilloEnemigo);
        }

        foreach (Nodo nodo in vecinos)
        {
            if (nodo.tile.GetComponent<Tile>().isClear())
            {
                nodo.tile.GetComponent<Tile>().SetCreatable();
            }
        }
    }

    void DeselectUnit() {
        if (gm.selectedUnit != null)
        {
            gm.selectedUnit.isSelected = false;
            gm.selectedUnit = null;
        }
    }




}
