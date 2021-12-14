using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EconomyManager : MonoBehaviour
{
    public Grid grid;
    public MapGenerator mapGenerator;
    public GM gm;
    private int totalFeedingCost;

    private void Update()
    {
        if (MapGenerator.unitsEnemy.Count < 6 && gm.playerTurn == 1)
        {
            PurchasePhase();
        }

        
    }

    public static void GiveMoneyForUnit(Unit unit) //dada la unidad a la que ha matado
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

        if (GameObject.FindObjectOfType<GM>().playerTurn == 1) //turno enemigo (1)
        {
            GM.player1Gold += money;
        }
        else //turno aliado (2)
        {
            GM.player2Gold += money;
        }

        //actualizamos la cantidad de oro de ambos jugadores
        GameObject.FindObjectOfType<GM>().UpdateGoldText();
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
            if (gm.playerTurn == 2)
            {
                MapGenerator.unitsPlayer.Remove(unit);
                Destroy(unit);
            }
        }

        return necessaryMoney;
    }

    private void PurchasePhase()
    {
        //si hay algún nodo libre alrrededor del castillo entonces comienza la fase de compra
        if (ClearNodeForUnits())
        {
            int tanks = 0;
            int dragons = 0;
            int warriors = 0;

            totalFeedingCost = 0;

            int necessaryTanks = tanks;
            int necessaryDragons = dragons;
            int necessaryWarriors = warriors;

            //identificamos cuantas unidades de cada tipo tiene la IA
            for (int i = 0; i < MapGenerator.unitsEnemy.Count; i++)
            {
                if (MapGenerator.unitsEnemy[i].name.Contains("Tanque"))
                {
                    tanks++;
                }
                else if (MapGenerator.unitsEnemy[i].name.Contains("Guerrero"))
                {
                    warriors++;
                }
                else
                {
                    dragons++;
                }

                totalFeedingCost += MapGenerator.unitsEnemy[i].gameObject.transform.GetComponent<Unit>().feedingCost;

            }

            if (tanks < 2)
            {
                //si podemos asumir el gasto y además tenemos dinero suficiente como para mantener a las unidades por otros 4 turnos
                if (canBuyTank())
                {
                    GM.player1Gold -= 50;
                    GameObject.FindObjectOfType<GM>().UpdateGoldText();

                    Nodo position = TakeNodoForUnit();

                    BuyEnemyUnit("Tanque Enemigo", position);
                }
            }
            //si tiene dos tanques, priorizará la obtención de voladores, estás rotisimos
            else if (canBuyDragon())
            {
                GM.player1Gold -= 80;
                GameObject.FindObjectOfType<GM>().UpdateGoldText();

                Nodo position = TakeNodoForUnit();

                BuyEnemyUnit("Volador Enemigo", position);
            }
            else //valoraremos si debe comprar un warrior, son manquisimos, si no, que se ahorre el dinero
            {
                if (canBuyWarrior())
                {
                    //si está en un momento crítico, tiene menos de 3 unidades
                    if (MapGenerator.unitsEnemy.Count < 3)
                    {
                        GM.player1Gold -= 40;
                        GameObject.FindObjectOfType<GM>().UpdateGoldText();

                        Nodo position = TakeNodoForUnit();

                        BuyEnemyUnit("Guerrero Enemigo", position);
                    }
                }
            }

        }
        else
            return;
    }

    private bool canBuyTank()
    {
        //el coste total de manutención más el coste de manutención de la nueva unidad a comprar
        return GM.player1Gold - 50 >= totalFeedingCost * 2 + 3;
    }

    private bool canBuyDragon()
    {
        //el coste total de manutención más el coste de manutención de la nueva unidad a comprar
        return GM.player1Gold - 80 >= totalFeedingCost * 2 + 5;
    }

    private bool canBuyWarrior()
    {
        //el coste total de manutención más el coste de manutención de la nueva unidad a comprar
        return GM.player1Gold - 40 >= totalFeedingCost * 2 + 2;
    }

    private void BuyEnemyUnit(string type, Nodo nodo)
    {
        foreach (UnitType unit in mapGenerator.unitsCollection)
        {
            if (!unit.aliado && unit.name.Contains(type))
            {
                GameObject nuevaUnidad = Instantiate(unit.unit);
                nuevaUnidad.transform.position = nodo.position;
                nuevaUnidad.transform.SetParent(GameObject.Find("/Units").transform);

                MapGenerator.unitsEnemy.Add(nuevaUnidad);
                GameObject.FindObjectOfType<GM>().unitsIAonScene.Add(nuevaUnidad);

                break;
            }
        }
    }

    private Nodo TakeNodoForUnit()
    {
        Nodo newUnitPosition;

        List<Nodo> vecinos;

        vecinos = GameObject.Find("Map Generator").GetComponent<Grid>().GetAllNeighbouringNodes(MapGenerator.nodoCastilloEnemigo);

        foreach (Nodo nodo in vecinos)
        {
            bool goodNode = true;

            for (int i = 0; i < GameObject.Find("/Units").transform.childCount; i++)
            {
                if (grid.NodeFromWorldPosition(GameObject.Find("/Units").transform.GetChild(i).transform.position) == nodo)
                {
                    goodNode = false;
                }
            }

            if (goodNode)
            {
                newUnitPosition = nodo;
                return newUnitPosition;
            }
        }

        return null;
    }

    private bool ClearNodeForUnits()
    {
        bool canSpawn = true;

        List<Nodo> vecinos;

        vecinos = GameObject.Find("Map Generator").GetComponent<Grid>().GetAllNeighbouringNodes(MapGenerator.nodoCastilloEnemigo);

        foreach (Nodo nodo in vecinos)
        {
            canSpawn = true;

            for (int i = 0; i < GameObject.Find("/Units").transform.childCount; i++)
            {
                if (grid.NodeFromWorldPosition(GameObject.Find("/Units").transform.GetChild(i).transform.position) == nodo)
                {
                    canSpawn = false;
                }
            }
        }

        return canSpawn;
    }

    public bool CheckNodeForUnits(Nodo nodoCastillo)
    {
        bool occupiedNode = false;

        List<Nodo> vecinos;

        vecinos = GameObject.Find("Map Generator").GetComponent<Grid>().GetAllNeighbouringNodes(nodoCastillo);

        foreach (Nodo nodo in vecinos)
        {
            for (int i = 0; i < GameObject.Find("/Units").transform.childCount; i++)
            {
                if (grid.NodeFromWorldPosition(GameObject.Find("/Units").transform.GetChild(i).transform.position) == nodo)
                {
                    occupiedNode = true;
                    Debug.Log(occupiedNode);
                }
            }
        }

        return occupiedNode;


    }
}