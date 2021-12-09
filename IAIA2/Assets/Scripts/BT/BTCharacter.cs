using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BTCharacter : MonoBehaviour
{   
    //Ahora mismo es una FSM, se puede adaptar para el BT 
    
    Mode mode;
    GameObject enemyCloser;
    GameObject hootchCloser;
    Grid grid;
    public enum Percept
    {
        UnitMoneySupply, //Dinero >= 20 para alimentar a la unidad
        CastleUnderAttack, //Castillo siendo atacado
        EnemyClose, //Enemigo en rango
        WeakEnemy, //Enemigo a menos del 50% de vida
        VilleRangeToConquer, //Villa en rango cercano que se puede conquistar
        Dead,
        None
    }
    public enum Mode
    {
        Ataque,
        Defensa
    }
    public enum Direction
    {
        Up, Down, Left, Right
    }

    public BTCharacter()
    {

    }
    private void Awake()
    {
        grid = GameObject.Find("Map Generator").GetComponent<Grid>();
    }


    private IEnumerable<Percept> GetPerceptsAnalysis()
    {
        //Analizar situacion, añadiendo en result todos los elementos que percibe el agente
        var result = new List<Percept>();
        if (!IsUnitMoneySupply())
            result.Add(Percept.Dead);
        else
        {
            result.Add(Percept.UnitMoneySupply);
            if (IsCastleUnderAttack())
                result.Add(Percept.CastleUnderAttack);
            if (IsEnemyClose())
                result.Add(Percept.EnemyClose);
            if (IsWeakEnemy())
                result.Add(Percept.WeakEnemy);
            if (IsVilleRangeToConquer())
                result.Add(Percept.VilleRangeToConquer);
        }
        return result;
    }

    private Mode GetMode()
    {
        return mode;
    }
    private void SetMode(Mode m)
    {
        mode = m;
    }

    #region "Metodos Percepts"
    private bool IsUnitMoneySupply()
    {
        //Se comprueba que el dinero de la IA es >= 20 para mantener viva a la unidad
        return true;
    }
    private bool IsCastleUnderAttack()
    {
        //Si es un guerrero se mantiene en modo ataque, si no se pasa a defensa
        List<Nodo> adyacenteCastillo = grid.GetNeighbouringNodes(MapGenerator.nodoCastilloEnemigo);

        foreach (Nodo adyacente in adyacenteCastillo)
            foreach(GameObject unidad in MapGenerator.unitsPlayer)
                if (adyacente == grid.NodeFromWorldPosition(unidad.transform.position))
                    return true;
        return false;
    }
    private bool IsEnemyClose()
    {

        Tile[] tiles = GetComponent<Unit>().GetTilesInRange();

        foreach (Tile tile in tiles)
            foreach (GameObject unit in MapGenerator.unitsPlayer)
                if (unit.transform.position.x == tile.transform.position.x &&
                    tile.transform.position.y == unit.transform.position.y)
                {
                    enemyCloser = unit;
                    return true;
                }
        enemyCloser = null;
        return false;
    }

    private bool IsWeakEnemy()
    {
        //Accede al enemigo que esta cerca y comprueba si tiene poca vida
        return enemyCloser.GetComponent<Unit>().health < enemyCloser.GetComponent<Unit>().healthTotal / 2;
    }

    private bool IsVilleRangeToConquer()
    {
        Tile[] tiles = GetComponent<Unit>().GetTilesInRange();

        foreach (Tile tile in tiles)
            foreach (Nodo hootch in MapGenerator.hootchsNodes)
                if (hootch.position.x == tile.transform.position.x && tile.transform.position.y == hootch.position.y)
                {
                    GameObject[] allHootchs = GameObject.Find("Hootchs").GetComponentsInChildren<GameObject>();
                    foreach (GameObject checkHootch in allHootchs)
                        if (hootch.position.x == checkHootch.transform.position.x && checkHootch.transform.position.y == hootch.position.y)
                            hootchCloser = checkHootch;
                    return true;
                }
        hootchCloser = null;
        return false;
    }
    #endregion

    public void Analysis()
    {
        var percepts = GetPerceptsAnalysis();
        if (percepts.Contains(Percept.UnitMoneySupply))
        {
            //Quita 20 monedas
        }
        if (percepts.Contains(Percept.CastleUnderAttack))
        {
            //Va al castillo
            SetMode(Mode.Defensa);
        }
        else if(percepts.Contains(Percept.EnemyClose))
        {
            if (this.name.Contains("Guerrero"))
                SetMode(Mode.Ataque);
            else if (percepts.Contains(Percept.WeakEnemy))
                SetMode(Mode.Ataque);
            else
                SetMode(Mode.Defensa);

            //Si es un guerrero modo ataque
            //else if (percepts.Contains(Percept.WeakEnemy)){ //Modo Ataque}
            //else { Modo Defensa }
        }
    }


    public void Action()
    {
        Analysis();
        var ActualMode = GetMode();
        var percepts = GetPerceptsAnalysis();
        if (ActualMode == Mode.Ataque)
        {
            if (percepts.Contains(Percept.EnemyClose))
            {
                //Ir a la unidad enemiga con baja vida más cercana y atacar (mapa de infuencia) (variable closerenemy)
            }
            else if (percepts.Contains(Percept.VilleRangeToConquer))
            {
                //Ir a villa en rango no conquistada (hootchCloser) y conquistarla
            }
            else //No hay villa ni enemigo cercano
            {
                //Avanza sin mas si NO es un tanque
                //Usando mapa de influencias?
            }
        }
        else if (ActualMode == Mode.Defensa)
        {
            Nodo posInicial = grid.NodeFromWorldPosition(this.transform.position);
            Nodo posFinal = MapGenerator.nodoCastilloEnemigo;
            GameObject.FindObjectOfType<PathfindingAStar>().Pathfinding(posInicial, posFinal, ref GetComponent<Unit>().finalPath);
        }
    }

    
}

