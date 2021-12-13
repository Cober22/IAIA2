using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Unit : MonoBehaviour
{
    public string unitType;
    public int feedingCost;

    public bool isSelected;
    public bool hasMoved;

    public int tileSpeed;
    public float moveSpeed;

    private GM gm;
    private PathfindingAStar pathfinding;
    public bool pathfindingDoneThisTurn = false;
    private Nodo targetNode;

    public int attackRadius;
    public bool hasAttacked;
    public List<Unit> enemiesInRange = new List<Unit>();
    public List<Nodo> finalPath = new List<Nodo>();

    public int playerNumber;

    public GameObject weaponIcon;

    // Attack Stats
    public int health;
    public int healthTotal;
    public int attackDamage;
    public int defenseDamage;
    public int armor;

    public DamageIcon damageIcon;

    public int cost;

	public GameObject deathEffect;

	private Animator camAnim;

    public bool isKing;

	private AudioSource source;

    public Text displayedText;

    public float influenceValue;

    public int count = 0;
    public int stepsTaken = 0;
    private int maxSteps;

    public bool actionDone;


    [HideInInspector]
    public Vector3 position;

    private void Awake()
    {
        bool aliado = this.name.Contains("Aliado");

        if (this.name.Contains("Guerrero"))
        {
            if (aliado)
            {
                influenceValue = 20f;
                maxSteps = 4;
            } else 
                GetComponent<BTCharacter>().mode = BTCharacter.Mode.Ataque;
        }
        else if (this.name.Contains("Tanque"))
        {
            if (aliado)
            {
                influenceValue = 15f;
                maxSteps = 3;
            } 
            else
                GetComponent<BTCharacter>().mode = BTCharacter.Mode.Defensa;
        }
        else if (this.name.Contains("Volador"))
        {
            if (aliado)
            {
                influenceValue = 10f;
                maxSteps = 5;
            }
            else
                GetComponent<BTCharacter>().mode = BTCharacter.Mode.Ataque;
        }

        healthTotal = health;

        MapGenerator._influenceMap.RegisterPropagator(this);

        if (!aliado)
            maxSteps = tileSpeed;


        position = this.transform.position;
    }

    private void Start()
    {
		source = GetComponent<AudioSource>();
		camAnim = Camera.main.GetComponent<Animator>();
        gm = FindObjectOfType<GM>();
        UpdateHealthDisplay();
        pathfinding = GameObject.FindObjectOfType<PathfindingAStar>();
        actionDone = false;
        //maxSteps = tileSpeed;
    }
    public void RebootPropagators()
    {
        if (this.name.Contains("Aliado"))
        {
            MapGenerator._influenceMap.RegisterPropagator(this);
        }
    }

    private void Update()
    {        
        // UNA ACCION POR TURNO
        if (gameObject.layer == 7)
        {
            if (GameObject.FindObjectOfType<GM>().playerTurn == 1 && !actionDone)
            {
                //if (finalPath != null)
                //    Debug.Log(name + " " + finalPath.Count);
                //else
                //    Debug.Log("FFFF");

                if (finalPath != null && finalPath.Count > 0)
                {
                    MoveThroughNodes(finalPath);    
                    if (stepsTaken >= maxSteps)
                    {
                        actionDone = true;
                        Nodo nodo = GameObject.Find("/Map Generator").GetComponent<Grid>().NodeFromWorldPosition(transform.position);
                        nodo.IsWall = true;
                        finalPath = null;
                    }
                    //nodo.tile.GetComponent<Tile>().unitInTile = transform.gameObject;
                }
                //Debug.Log(name + " " + actionDone);
            }

            Grid grid = GameObject.Find("Map Generator").GetComponent<Grid>();
            List<Nodo> vecinos = grid.GetNeighbouringNodes(grid.NodeFromWorldPosition(this.transform.position));
            Unit[] units = FindObjectsOfType<Unit>();
            GM gm = FindObjectOfType<GM>();

            foreach (Unit unit in units)
            {
                foreach (Nodo vecino in vecinos)
                {
                    if (grid.NodeFromWorldPosition(unit.position) == vecino && unit.gameObject.layer != 7 && Input.GetKeyDown(KeyCode.A) && !hasAttacked && gm.playerTurn != 1)
                    {
                        AttackAliade(unit);
                    }
                }
            }

            if (this.gameObject.GetComponent<BTCharacter>().atacar)
            {
                foreach (Unit unit in units)
                {
                    foreach (Nodo vecino in vecinos)
                    {
                        //Debug.Log(grid.NodeFromWorldPosition(unit.position) == vecino);
                        if (grid.NodeFromWorldPosition(unit.position) == vecino && unit.gameObject.layer != 7 && !unit.hasAttacked && gm.playerTurn == 1)
                        {
                            AttackEnemie(unit);
                        }
                    }
                }
            }
        }

        if(gameObject.layer != 7 && name.Contains("Volador"))
        {
//            Debug.Log(count);
        }

        if (gameObject.layer != 7 && finalPath != null && count < finalPath.Count)
            MoveThroughNodes(finalPath);
        else if (gameObject.layer != 7 && finalPath != null)
        {
            finalPath = null;
            count = 0;
        }

        position = this.transform.position;
    }

    private void UpdateHealthDisplay ()
    {
        if (isKing)
            displayedText.text = health.ToString();
    }

    private void OnMouseDown() // select character or deselect if already selected
    {
        ResetWeaponIcon();
        if(gameObject.layer != 7)
        {
            if (isSelected == true)
            {   
                isSelected = false;
                gm.selectedUnit = null;
                gm.ResetTiles();

            }
            else {
                if (playerNumber == gm.playerTurn) { // select unit only if it's his turn
                    if (gm.selectedUnit != null)
                    { // deselect the unit that is currently selected, so there's only one isSelected unit at a time
                        gm.selectedUnit.isSelected = false;
                    }
                    gm.ResetTiles();

                    gm.selectedUnit = this;

                    isSelected = true;
			        if(source != null){
				        source.Play();
			        }
				
                    GetWalkableTiles();
                    GetEnemies();
                }
            }

            //Collider2D col = Physics2D.OverlapCircle(Camera.main.ScreenToWorldPoint(Input.mousePosition), 0.15f);
            //if (col != null)
            //{
            //    Unit unit = col.GetComponent<Unit>(); // double check that what we clicked on is a unit
            //    if (unit != null && gm.selectedUnit != null)
            //    {
            //        if (gm.selectedUnit.enemiesInRange.Contains(unit) && !gm.selectedUnit.hasAttacked)
            //        { // does the currently selected unit have in his list the enemy we just clicked on
            //            gm.selectedUnit.Attack(unit);

            //        }
            //    }
            //}
        }
    }

    private void OnMouseOver()
    {
        if (Input.GetMouseButtonDown(1))
            gm.UpdateInfoPanel(this);
    }

    void GetWalkableTiles() { // Looks for the tiles the unit can walk on
        if (hasMoved == true) {
            return;
        }

        Tile[] tiles = FindObjectsOfType<Tile>();
        foreach (Tile tile in tiles) {
            if (Mathf.Abs(transform.position.x - tile.transform.position.x) + Mathf.Abs(transform.position.y - tile.transform.position.y) <= tileSpeed)
            { // how far he can move
                if (tile.isClear() == true && tile.gameObject.layer != 9)
                { // is the tile clear from any obstacles
                    tile.Highlight();
                }
            }          
        }
    }

    public List<Tile> GetTilesInRange()
    {
        Tile[] tiles = FindObjectsOfType<Tile>();
        List<Tile> tilesInRange = new List<Tile>();
        foreach (Tile tile in tiles)
        {
            if (Mathf.Abs(transform.position.x - tile.transform.position.x) + Mathf.Abs(transform.position.y - tile.transform.position.y) <= tileSpeed)
            { // how far he can move
                if (tile.isClear() == true && tile.gameObject.layer != 9)
                { // is the tile clear from any obstacles
                    //tile.Highlight();
                    tilesInRange.Add(tile);
                }
            }
        }
        return tilesInRange;
    }

    void GetEnemies() {
    
        enemiesInRange.Clear();

        Unit[] enemies = FindObjectsOfType<Unit>();
        foreach (Unit enemy in enemies)
        {
            if (Mathf.Abs(transform.position.x - enemy.transform.position.x) + Mathf.Abs(transform.position.y - enemy.transform.position.y) <= attackRadius) // check is the enemy is near enough to attack
            {
                if (enemy.playerNumber != gm.playerTurn && !hasAttacked) { // make sure you don't attack your allies
                    enemiesInRange.Add(enemy);
                    enemy.weaponIcon.SetActive(true);
                }
            }
        }
    }

    public void Move(Transform movePos)
    {
        gm.ResetTiles();

        Nodo nodoInicial = GameObject.FindObjectOfType<Grid>().NodeFromWorldPosition(transform.position);
        Nodo nodoFinal = GameObject.FindObjectOfType<Grid>().NodeFromWorldPosition(movePos.position);

        pathfinding.PathfindingPlayer(nodoInicial, nodoFinal, ref finalPath);

        //hasMoved = true;
        //ResetWeaponIcon();
        //GetEnemies();
        //gm.MoveInfoPanel(this);
    }

    void Attack(Unit enemy) {
        hasAttacked = true;

        int enemyDamege = attackDamage - enemy.armor;
        int unitDamage = enemy.defenseDamage - armor;

        if (enemyDamege >= 1)
        {
            enemy.health -= enemyDamege;
            enemy.UpdateHealthDisplay();
            DamageIcon d = Instantiate(damageIcon, enemy.transform.position, Quaternion.identity);
            d.Setup(enemyDamege);
        }

        if (transform.tag == "Archer" && enemy.tag != "Archer")
        {
            if (Mathf.Abs(transform.position.x - enemy.transform.position.x) + Mathf.Abs(transform.position.y - enemy.transform.position.y) <= 1) // check is the enemy is near enough to attack
            {
                if (unitDamage >= 1)
                {
                    health -= unitDamage;
                    UpdateHealthDisplay();
                    DamageIcon d = Instantiate(damageIcon, transform.position, Quaternion.identity);
                    d.Setup(unitDamage);
                }
            }
        } else {
            if (unitDamage >= 1)
            {
                health -= unitDamage;
                UpdateHealthDisplay();
                DamageIcon d = Instantiate(damageIcon, transform.position, Quaternion.identity);
                d.Setup(unitDamage);
            }
        }

        if (enemy.health <= 0)
        {
         
            if (deathEffect != null){
				Instantiate(deathEffect, enemy.transform.position, Quaternion.identity);
				camAnim.SetTrigger("shake");
			}

            if (enemy.isKing)
            {
                gm.ShowVictoryPanel(enemy.playerNumber);
            }

            GetWalkableTiles(); // check for new walkable tiles (if enemy has died we can now walk on his tile)
            gm.RemoveInfoPanel(enemy);
            Destroy(enemy.gameObject);
        }

        if (health <= 0)
        {

            if (deathEffect != null)
			{
				Instantiate(deathEffect, enemy.transform.position, Quaternion.identity);
				camAnim.SetTrigger("shake");
			}

			if (isKing)
            {
                gm.ShowVictoryPanel(playerNumber);
            }

            gm.ResetTiles(); // reset tiles when we die
            gm.RemoveInfoPanel(this);
            Destroy(gameObject);
        }

        gm.UpdateInfoStats();
    }

    public void ResetWeaponIcon() {
        Unit[] enemies = FindObjectsOfType<Unit>();
        foreach (Unit enemy in enemies)
        {
            enemy.weaponIcon.SetActive(false);
        }
    }
    
    public void AttackEnemie(Unit Aliade)
    {
        Aliade.hasAttacked = true;

        //DDebug.Log("atacando");

        int unitDamage = attackDamage - Aliade.armor;

        if (unitDamage >= 1)
        {
            Aliade.health -= unitDamage;
            UpdateHealthDisplay();
            DamageIcon d = Instantiate(damageIcon, transform.position, Quaternion.identity);
            d.Setup(unitDamage);
        }

        if (Aliade.health <= 0)
        {

            if (deathEffect != null)
            {
                Instantiate(deathEffect, Aliade.transform.position, Quaternion.identity);
                camAnim.SetTrigger("shake");
            }

            gm.ResetTiles(); // reset tiles when we die
            gm.RemoveInfoPanel(this);
            Destroy(Aliade.gameObject);
        }

        gm.UpdateInfoStats();
        ResetWeaponIcon();
    }

    public void AttackAliade(Unit enemy)
    {
        hasAttacked = true;

        //Debug.Log("atacando");

        //GetEnemies();

        //foreach (Unit enemy in enemiesInRange)
        //{
        //int enemyDamege = attackDamage - enemy.armor;
        int unitDamage = enemy.attackDamage - armor;

        /*if (enemyDamege >= 1)
        {
            enemy.health -= enemyDamege;
            enemy.UpdateHealthDisplay();
            DamageIcon d = Instantiate(damageIcon, enemy.transform.position, Quaternion.identity);
            d.Setup(enemyDamege);
        }*/
        if (unitDamage >= 1)
        {
            health -= unitDamage;
            UpdateHealthDisplay();
            DamageIcon d = Instantiate(damageIcon, transform.position, Quaternion.identity);
            d.Setup(unitDamage);
        }

        /*if (enemy.health <= 0)
        {
         
            if (deathEffect != null){
				Instantiate(deathEffect, enemy.transform.position, Quaternion.identity);
				camAnim.SetTrigger("shake");
			}

            if (enemy.isKing)
            {
                gm.ShowVictoryPanel(enemy.playerNumber);
            }

            GetWalkableTiles(); // check for new walkable tiles (if enemy has died we can now walk on his tile)
            gm.RemoveInfoPanel(enemy);
            Destroy(enemy.gameObject);
        }*/

        if (health <= 0)
        {

            if (deathEffect != null)
            {
                Instantiate(deathEffect, enemy.transform.position, Quaternion.identity);
                camAnim.SetTrigger("shake");
            }

            gm.ResetTiles(); // reset tiles when we die
            gm.RemoveInfoPanel(this);
            Destroy(gameObject);
        }

        gm.UpdateInfoStats();
        ResetWeaponIcon();
        //}
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawWireCube(GameObject.FindObjectOfType<Grid>().transform.position, new Vector3(20, 15, 1));
        Color color = Color.white;
        if (finalPath != null && finalPath.Count > 0)
        {
            if (this.name.Contains("Guerrero"))
            {
                if (this.name.Contains("Aliado"))
                    color = Color.red;
                else
                    color = Color.blue;
            }
            else if (this.name.Contains("Tanque"))
                if (this.name.Contains("Aliado"))
                    color = Color.gray;
                else
                    color = Color.yellow;
            else if (this.name.Contains("Volador"))
            {
                if (this.name.Contains("Aliado"))
                    color = Color.cyan;
                else
                    color = Color.magenta;
            }

            Gizmos.color = color;
            foreach (Nodo nodo in finalPath)
                Gizmos.DrawCube(nodo.position, Vector3.one * 0.35f);
        }
    }

    private void MoveThroughNodes(List<Nodo> path)
    {
        //si en el camino el último nodo es el castillo o algun aliado (muro), lo quitamos de las listas del camino
        //int i = finalPath.Count - 1;
        //Debug.Log(i);
        //Debug.Log(finalPath.Count);
        //Debug.Log(finalPath[i].IsWall);
        //while (i > 0 && finalPath.Count > 0 && finalPath[i].IsWall)
        //{
        //    Debug.Log(finalPath[finalPath.Count - 1]);
        //    finalPath.Remove(finalPath[i]);
        //    Debug.Log(finalPath[finalPath.Count - 1]);
        //    i--;
        //}

        if (finalPath[finalPath.Count-1] == MapGenerator.nodoCastilloEnemigo)
            finalPath.Remove(MapGenerator.nodoCastilloEnemigo);

        //if (finalPath[finalPath.Count - 1] == MapGenerator.nodoCastilloEnemigo)
        //    finalPath.Remove(MapGenerator.nodoCastilloEnemigo);

        // El NPC recorrera todos los nodos hasta su penúltimo, para no quedarse sin nodos que perseguir y evitar posibles errores
        float distanceToNextNode = Vector3.Distance(transform.position, path[count].position);

        // El NPC recorrera todos los nodos hasta su penúltimo, para no quedarse sin nodos que perseguir y evitar posibles errores
        transform.position = Vector3.MoveTowards(transform.position, path[count].position, Time.deltaTime*6f);

        if (distanceToNextNode < 0.01f && count < finalPath.Count)
        {
            stepsTaken++;
            count++;
        }

        if (count >= finalPath.Count)
        {
            Nodo nodo = GameObject.Find("Map Generator").GetComponent<Grid>().NodeFromWorldPosition(transform.position);
            nodo.IsWall = true;
            actionDone = true;
        }
    }
}
