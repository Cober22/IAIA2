using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EconomyManager : MonoBehaviour
{
    //public GM gm;
    public Grid grid;

    private void Update()
    {
        //PurchasePhase();
        SeeForUnitInCastle();
    }

    public void GiveMoneyForUnit(Unit unit) //dada la unidad a la que ha matado
    {
        int money = 0;

        if (unit.name.Contains("Guerrero"))
        {
            //la mitad del coste de creación
            money += 20;
        }
        else if (unit.name.Contains("Volador"))
        {
            //la mitad del coste de creación
            money += 40;
        }
        else //unit.name.Contains("Tanque")
        {
            //la mitad del coste de creación
            money += 25;
        }

        if (GameObject.Find("Map Generator").GetComponent<GM>().playerTurn == 1) //turno enemigo (1)
        {
            GM.player1Gold += money;
        }
        else //turno aliado (2)
        {
            GM.player2Gold += money;
        }

        //actualizamos la cantidad de oro de ambos jugadores
        GameObject.Find("Map Generator").GetComponent<GM>().UpdateGoldText();
    }

    public int FeedUnits(List<GameObject> units, int gold) //dada una lista con las distintas unidades, aliadas o enemigas y el monedero correspondiente
    {
        int necessaryMoney = 0;
        int wallet = gold;
        List<GameObject> unitsToEliminate = new List<GameObject>();

        foreach (GameObject unit in units)
        {
            if (wallet - unit.transform.GetComponent<Unit>().feedingCost < 0)
            {
                unitsToEliminate.Add(unit);
            }
            else
            {
                necessaryMoney += unit.transform.GetComponent<Unit>().feedingCost;
                wallet -= unit.transform.GetComponent<Unit>().feedingCost;
            }
        }

        foreach (GameObject unit in unitsToEliminate)
        {
            units.Remove(unit);
            Destroy(unit);
        }

        return necessaryMoney;
    }

    private void PurchasePhase()
    {

        int tank = 0;
        int flying = 0;
        int warrior = 0;

        int totalFeedingCost = 0;

        //identificamos cuantas unidades de cada tipo tiene la IA
        for (int i = 0; i < MapGenerator.unitsEnemy.Count; i++)
        {
            if (MapGenerator.unitsEnemy[i].name.Contains("Tanque"))
            {
                tank++;
            }
            else if (MapGenerator.unitsEnemy[i].name.Contains("Guerrero"))
            {
                warrior++;
            }
            else
            {
                flying++;
            }

            totalFeedingCost += MapGenerator.unitsEnemy[i].gameObject.transform.GetComponent<Unit>().feedingCost;

        }

        if (flying == 0)
        {
            //si podemos asumir el gasto y además tenemos dinero suficiente como para mantener a las unidades por otros 4 turnos
            if (GM.player1Gold - 80 >= totalFeedingCost*4 + 5) //el coste total de manutención más el coste de manutención de la nueva unidad a comprar
            {

            }
        }
        else if(tank == 0)
        {

        }
        //else if(warrior == 0) //peor unidad, no me interesa comprarla a no ser que esté en una situación crítica
        //{

        //}
    }

    //private string WhatUnit(int count)
    //{

    //}

    private void SeeForUnitInCastle()
    {
        List<Nodo> vecinos;

        vecinos = GameObject.Find("Map Generator").GetComponent<Grid>().GetAllNeighbouringNodes(MapGenerator.nodoCastilloEnemigo);

        //para cada unidad enemiga vamos a comprobar si está cerca del castillo
        foreach (Nodo nodo in vecinos)
        {
            for (int i = 0; i < MapGenerator.unitsEnemy.Count; i++)
            {
                //Debug.Log(grid.NodeFromWorldPosition(MapGenerator.unitsEnemy[i].transform.position) == nodo);
                if (grid.NodeFromWorldPosition(MapGenerator.unitsEnemy[i].transform.position) == nodo)
                {

                }
            }
        }
    }

    //private bool UnitInADeterminateTile(Nodo nodo, Unit unit)
    //{
    //    if (nodo.tile.GetComponent<Tile>().

    //    {

    //    }

    //    return true;
    //}
}
