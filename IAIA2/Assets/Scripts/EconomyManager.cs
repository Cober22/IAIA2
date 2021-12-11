using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EconomyManager : MonoBehaviour
{
    //public GM gm;

    private void Update()
    {

    }

    public void GiveMoneyForUnit(Unit unit) //dada la unidad a la que ha matado
    {
        int money = 0;

        //dinero bajo
        if (unit.unitType == "Guerrero Aliado" || unit.unitType == "Guerrero Enemigo")
        {
            //la mitad del coste de creación
            money += 20;
        }
        //dinero alto
        else if (unit.unitType == "Volador Aliado" || unit.unitType == "Volador Enemigo")
        {
            //la mitad del coste de creación
            money += 40;
        }
        //dinero medio
        else //la unidad es Tanque Enemigo o Tanque Aliado
        {
            //la mitad del coste de creación
            money += 35;
        }

        if (GameObject.Find("Map Generator").GetComponent<GM>().playerTurn == 1) //turno enemigo
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
}
