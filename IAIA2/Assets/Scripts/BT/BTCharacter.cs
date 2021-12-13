using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static influenceMap;

public class BTCharacter : MonoBehaviour
{
    //Ahora mismo es una FSM, se puede adaptar para el BT 

    public Mode mode;
    GameObject enemyCloser;
    GameObject hootchCloser;
    PathfindingAStar pathfinding;
    Grid grid;

    Nodo nodoInicio;
    Nodo nodoFinal;
    List<Nodo> path;
    public bool actionInitialized = false; 
    //float influenciaMin, abajo, derecha, arriba, izquierda;
    public bool conquistarVilla;
    public List<Unit> enemiesInRange = new List<Unit>();
    public bool atacar;
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
        grid = GameObject.FindObjectOfType<Grid>();
        pathfinding = GameObject.FindObjectOfType<PathfindingAStar>();
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
            if (IsEnemyClose())
                result.Add(Percept.EnemyClose);
            if (IsCastleUnderAttack())
                result.Add(Percept.CastleUnderAttack);
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
        return GM.player1Gold - GetComponent<Unit>().feedingCost > 0;
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
        List<Tile> tiles = GetComponent<Unit>().GetTilesInRange();
        List<GameObject> unitsToPersecute = new List<GameObject>();

        int numUnits = GameObject.Find("/Units").transform.childCount;

        for (int i = 0; i < numUnits; i++)
            if(GameObject.Find("/Units").transform.name.Contains("Aliado"))
                unitsToPersecute.Add(GameObject.Find("/Units").transform.GetChild(i).gameObject);

        foreach (Tile tile in tiles)
        {
            //Nodo test = GameObject.FindObjectOfType<Grid>().NodeFromWorldPosition(tile.transform.position);
            foreach (GameObject unit in unitsToPersecute)
            {
                //Nodo nodoUnidad = GameObject.FinObjectOfType<Grid>().NodeFromWorldPosition(unit.transform.position);
                if (Vector2.Distance(new Vector2(unit.transform.position.x, unit.transform.position.y), new Vector2(tile.transform.position.x, tile.transform.position.y)) < 1.1f)
                {
                    Debug.Log("enemigo cerca");
                    enemyCloser = unit.transform.GetComponent<Unit>().gameObject;
                    return true;
                }
            }
        }

        enemyCloser = null;
        return false;
    }

    private bool IsWeakEnemy()
    {
        //Accede al enemigo que esta cerca y comprueba si tiene poca vida
        if(enemyCloser != null)
            return enemyCloser.GetComponent<Unit>().health < enemyCloser.GetComponent<Unit>().healthTotal / 2;
        return false;
    }

    private bool IsVilleRangeToConquer()
    {
        List<Tile> tiles = GetComponent<Unit>().GetTilesInRange();

        foreach (Tile tile in tiles)
            foreach (Nodo hootch in MapGenerator.hootchsNodes)
                if (Vector2.Distance(new Vector2(hootch.position.x, hootch.position.y), new Vector2(tile.transform.position.x, tile.transform.position.y)) < 1.1f)
                {
                    List<GameObject> allHootchs = new List<GameObject>();

                    int childCount = GameObject.Find("/Hootchs").transform.childCount;
                    for (int i = 0; i < childCount; i++)
                        allHootchs.Add(GameObject.Find("/Hootchs").transform.GetChild(i).gameObject);

                    foreach (GameObject checkHootch in allHootchs)
                        if (hootch.position.x == checkHootch.transform.position.x && checkHootch.transform.position.y == hootch.position.y && !checkHootch.GetComponent<Village>().conqueredByIA)
                        {
                            hootchCloser = checkHootch;
                            return true;
                        }
                }
        hootchCloser = null;
        return false;
    }
    #endregion

    public void Analysis()
    {
        atacar = false;
        conquistarVilla = false;
        var percepts = GetPerceptsAnalysis();
        if (percepts.Contains(Percept.Dead))
        {
            if (GameObject.FindObjectOfType<GM>().unitsIAonScene.Count > 0)
                GameObject.FindObjectOfType<GM>().unitsIAonScene.Remove(gameObject);
            Destroy(gameObject);
        } else
        {
            if (percepts.Contains(Percept.UnitMoneySupply))
            {
                GM.player1Gold -= GetComponent<Unit>().feedingCost;
                GameObject.FindObjectOfType<GM>().UpdateGoldText();
            }
            if(percepts.Contains(Percept.EnemyClose))
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
            else if (percepts.Contains(Percept.CastleUnderAttack))
            {
                //Debug.Log("ENTRAAAAA");

                //Va al castillo
                SetMode(Mode.Defensa);
            }
            else
            {
                SetMode(Mode.Ataque);
            }
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
                Debug.Log(name + " " + "ENEMIGO CERCA");
                //Ir a la unidad enemiga con baja vida más cercana y atacar (mapa de infuencia) (variable closerenemy)
                nodoInicio = grid.NodeFromWorldPosition(transform.position);
                nodoFinal = grid.NodeFromWorldPosition(enemyCloser.transform.position);

                pathfinding.Pathfinding(nodoInicio, nodoFinal, ref GetComponent<Unit>().finalPath);
                atacar = true;
            }
            else if (percepts.Contains(Percept.VilleRangeToConquer))
            {
                Debug.Log(name + " " + "CONQUISTA");
                //Ir a villa en rango no conquistada (hootchCloser) y conquistarla
                nodoInicio = grid.NodeFromWorldPosition(transform.position);
                Debug.Log(hootchCloser);
                nodoFinal = grid.NodeFromWorldPosition(hootchCloser.transform.position);

                Nodo nodoDestino = GameObject.FindObjectOfType<Grid>().NodeFromWorldPosition(hootchCloser.transform.position);
                nodoDestino.IsWall = false;
                if (Vector3.Distance(nodoInicio.position, nodoFinal.position) > 1f)
                    pathfinding.Pathfinding(nodoInicio, nodoFinal, ref GetComponent<Unit>().finalPath);
                else
                {
                    GetComponent<Unit>().finalPath = null;
                    GetComponent<Unit>().actionDone = true;
                }

                //pathfinding.Pathfinding(nodoInicio, nodoFinal, ref GetComponent<Unit>().finalPath);
                nodoDestino.IsWall = true;
                conquistarVilla = true;
            }
            else //No hay villa ni enemigo cercano
            {
                Debug.Log(name + " " + "AVANZA CASTILLO");
                //Avanza sin mas si NO es un tanque
                nodoInicio = grid.NodeFromWorldPosition(transform.position);

                List<Nodo> pathNodesAvailable = GameObject.FindObjectOfType<MapGenerator>().pathfing_NodesAvailable;

                int posFinal = UnityEngine.Random.Range(0, pathNodesAvailable.Count);
                //nodoFinal = grid.NodeFromWorldPosition(pathNodesAvailable[posFinal].position);
                //nodoInicio.IsWall = false;
                //nodoInicio.tile.GetComponent<Tile>().unitInTile = null;
                //pathfinding.Pathfinding(nodoInicio, nodoFinal, ref GetComponent<Unit>().finalPath);
                Debug.Log("HOLAx2");
                if (!this.name.Contains("Tanque"))
                {
                    Debug.Log("HOLA");
                    nodoFinal = MapGenerator.nodoCastilloAliado;

                    if (Vector3.Distance(nodoInicio.position, nodoFinal.position) > 1)
                        pathfinding.Pathfinding(nodoInicio, nodoFinal, ref GetComponent<Unit>().finalPath);
                    else
                    { // CODIGO DE ACCIÓN CUANDO EL TANQUE ESTA JUNTO AL CASTILLO
                        GetComponent<Unit>().finalPath = null;
                        GetComponent<Unit>().actionDone = true;
                    }
                } 
                else
                {
                    nodoInicio = grid.NodeFromWorldPosition(transform.position);
                    nodoFinal = MapGenerator.nodoCastilloEnemigo;

                    if (Vector3.Distance(nodoInicio.position, nodoFinal.position) > 1)
                        pathfinding.Pathfinding(nodoInicio, nodoFinal, ref GetComponent<Unit>().finalPath);
                    else
                    { // CODIGO DE ACCIÓN CUANDO EL TANQUE ESTA JUNTO AL CASTILLO
                        GetComponent<Unit>().finalPath = null;
                        GetComponent<Unit>().actionDone = true;
                    }
                }
                //else
                //{
                //    Debug.Log("wdawd");
                //    List<Nodo> adyacenteCastillo = grid.GetNeighbouringNodes(MapGenerator.nodoCastilloEnemigo);

                //    foreach (Nodo adyacente in adyacenteCastillo)
                //        if (adyacente != grid.NodeFromWorldPosition(transform.position))
                //        {
                //            Debug.Log("HOLAx3" + name);
                //            nodoFinal = MapGenerator.nodoCastilloEnemigo;
                //            pathfinding.Pathfinding(nodoInicio, nodoFinal, ref GetComponent<Unit>().finalPath);
                //        }
                    //pathfinding.Pathfinding(nodoInicio, nodoFinal, ref GetComponent<Unit>().finalPath);
                    //GetComponent<Unit>().finalPath = null;
                    //GetComponent<Unit>().actionDone = true;
                //}
            }
        }
        else if (ActualMode == Mode.Defensa)
        {
            Debug.Log("MODO DEFENSA");
            nodoInicio = grid.NodeFromWorldPosition(transform.position);
            nodoFinal = MapGenerator.nodoCastilloEnemigo;

            if (Vector3.Distance(nodoInicio.position, nodoFinal.position) > 1)
                pathfinding.Pathfinding(nodoInicio, nodoFinal, ref GetComponent<Unit>().finalPath);
            else { // CODIGO DE ACCIÓN CUANDO EL TANQUE ESTA JUNTO AL CASTILLO
                GetComponent<Unit>().finalPath = null;
                GetComponent<Unit>().actionDone = true;
            }
        }
        actionInitialized = true;
    }    
}

