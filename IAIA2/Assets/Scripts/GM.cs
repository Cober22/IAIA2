using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GM : MonoBehaviour
{
    public Unit selectedUnit;

    public int playerTurn = 2;

    public Transform selectedUnitSquare;

    private Animator camAnim;
    public Image playerIcon; 
    public Sprite playerOneIcon;
    public Sprite playerTwoIcon;

    public GameObject unitInfoPanel;
    public Vector2 unitInfoPanelShift;
    Unit currentInfoUnit;
    public Text heathInfo;
    public Text attackDamageInfo;
    public Text armorInfo;
    public Text defenseDamageInfo;

    public static int player1Gold;
    public static int player2Gold;

    public Text player1GoldText;
    public Text player2GoldText;

    public Unit createdUnit;
    public Village createdVillage;

    public List<GameObject> unitsIAonScene;
    public List<GameObject> unitsAliadeOnScene;

    public GameObject blueVictory;
    public GameObject darkVictory;

	private AudioSource source;

    private int unitElement;
    int numUnits;

    private void Awake()
    {
        unitElement = 0;
        source = GetComponent<AudioSource>();
        camAnim = Camera.main.GetComponent<Animator>();
        GetGoldIncome(1);

        player1Gold = 100;
        player2Gold = 100;

        UpdateGoldText();

    }

    private void Update()
    {
        numUnits = GameObject.Find("/Units").transform.childCount;
        for (int i = 0; i < numUnits; i++)
        {
            GameObject unitToAdd = GameObject.Find("/Units").transform.GetChild(i).gameObject;
            if (unitToAdd.layer == 7 && !unitsIAonScene.Contains(unitToAdd))
                unitsIAonScene.Add(unitToAdd);
            else if (unitToAdd.layer != 7 && !unitsAliadeOnScene.Contains(unitToAdd))
                unitsAliadeOnScene.Add(unitToAdd);
        }
        numUnits = GameObject.Find("/Units").transform.childCount;

        //Debug.Log("Turn: " + playerTurn + " Unit: " + unitElement + " Total: " + numUnits);
        if(playerTurn == 1 && unitElement >= numUnits)
        {
            EndTurn(); 
            player2Gold -= GetComponent<EconomyManager>().FeedUnits(MapGenerator.unitsPlayer, player2Gold); 
            UpdateGoldText();
        }

        IAActions();

        if (Input.GetKeyDown(KeyCode.Space) /*|| Input.GetKeyDown("b")*/)
            if (playerTurn == 2)
            {
                EndTurn();
                player1Gold -= GetComponent<EconomyManager>().FeedUnits(MapGenerator.unitsEnemy, player1Gold);
                UpdateGoldText(); 
            }

        if (selectedUnit != null) // moves the white square to the selected unit!
        {
            selectedUnitSquare.gameObject.SetActive(true);
            selectedUnitSquare.position = selectedUnit.transform.position;
        }
        else
        {
            selectedUnitSquare.gameObject.SetActive(false);
        }
    }

    public void IAActions()
    {
        if (playerTurn == 1 && unitElement < numUnits)
        {
            // UNA ACCION UNO POR UNO
            GameObject unit = GameObject.Find("/Units").transform.GetChild(unitElement).gameObject;
            if (unit.layer == 7)
            {
                if (!unit.GetComponent<BTCharacter>().actionInitialized)
                {
                    unit.GetComponent<BTCharacter>().Action();
                    unit.GetComponent<BTCharacter>().actionInitialized = true;
                }
                else if (unit.GetComponent<Unit>().actionDone)
                    unitElement++;
            } 
            else if (unit.layer != 7)
                unitElement++;
        } 
    }

    // Sets panel active/inactive and moves it to the correct place
    public void UpdateInfoPanel(Unit unit) {

        if (unit.Equals(currentInfoUnit) == false)
        {
            unitInfoPanel.transform.position = (Vector2)unit.transform.position + unitInfoPanelShift;
            unitInfoPanel.SetActive(true);

            currentInfoUnit = unit;

            UpdateInfoStats();

        } else {
            unitInfoPanel.SetActive(false);
            currentInfoUnit = null;
        }

    }

    // Updates the stats of the infoPanel
    public void UpdateInfoStats() {
        if (currentInfoUnit != null)
        {
            attackDamageInfo.text = currentInfoUnit.attackDamage.ToString();
            defenseDamageInfo.text = currentInfoUnit.defenseDamage.ToString();
            armorInfo.text = currentInfoUnit.armor.ToString();
            heathInfo.text = currentInfoUnit.health.ToString();
        }
    }

    // Moves the udpate panel (if the panel is actived on a unit and that unit moves)
    public void MoveInfoPanel(Unit unit) {
        if (unit.Equals(currentInfoUnit))
        {
            unitInfoPanel.transform.position = (Vector2)unit.transform.position + unitInfoPanelShift;
        }
    }

    // Deactivate info panel (when a unit dies)
    public void RemoveInfoPanel(Unit unit) {
        if (unit.Equals(currentInfoUnit))
        {
            unitInfoPanel.SetActive(false);
			currentInfoUnit = null;
        }
    }

    public void ResetTiles() {
        Tile[] tiles = FindObjectsOfType<Tile>();
        foreach (Tile tile in tiles)
        {
            tile.Reset();
        }
    }

    void EndTurn() {
		source.Play();
        camAnim.SetTrigger("shake");

        // deselects the selected unit when the turn ends
        if (selectedUnit != null) {
            selectedUnit.ResetWeaponIcon();
            selectedUnit.isSelected = false;
            selectedUnit = null;
        }

        ResetTiles();

        Unit[] units = FindObjectsOfType<Unit>();
        foreach (Unit unit in units) {
            unit.hasAttacked = false;
            unit.hasMoved = false;
            unit.ResetWeaponIcon();
        }

        if (playerTurn == 1) {
            playerIcon.sprite = playerTwoIcon;
            playerTurn = 2;
        } else if (playerTurn == 2) {
            playerIcon.sprite = playerOneIcon;
            // Reset units
            foreach (GameObject unit in unitsIAonScene)
            {
                Nodo nodo = GameObject.Find("Map Generator").GetComponent<Grid>().NodeFromWorldPosition(unit.transform.position);
                nodo.IsWall = false;
                unit.GetComponent<Unit>().actionDone = false;
                unit.GetComponent<BTCharacter>().actionInitialized = false;
                unit.GetComponent<Unit>().pathfindingDoneThisTurn = false;
                unit.GetComponent<Unit>().stepsTaken = 0;
                unit.GetComponent<Unit>().count = 0;
                unit.GetComponent<Unit>().finalPath = null;
                unitElement = 0;
            }
            playerTurn = 1;
        }

        GetGoldIncome(playerTurn);
        GetComponent<CharacterCreation>().CloseCharacterCreationMenus();
        createdUnit = null;

        // Actualizar mapa de influencia y actualizar valor de influencia de los nodos
        MapGenerator._influenceMap.DeletePropagators();
        foreach (Unit unit in units)
        {
            unit.GetComponent<Unit>().RebootPropagators();
        }
        MapGenerator._influenceMap.Propagate();
        float[,] influenceMap = MapGenerator._influenceMap.GetInfluences();

        for (int x = 0; x < influenceMap.GetLength(0); x++)
            for (int y = influenceMap.GetLength(1) - 1; y >= 0; y--)
                GameObject.Find("Map Generator").GetComponent<Grid>().grid[x, /*influenceMap.GetLength(1) - 1 -*/ y].influencia = influenceMap[x, y];

        Nodo[,] grid = GameObject.Find("Map Generator").GetComponent<Grid>().grid;

        foreach (Unit unit in units)
            unit.GetComponent<Unit>().hasAttacked = false;
    }

    void GetGoldIncome(int playerTurn) {
        foreach (Village village in FindObjectsOfType<Village>())
        {
            if (village.playerNumber == playerTurn)
            {
                if (playerTurn == 1)
                {
                    player1Gold += village.goldPerTurn;
                }
                else
                {
                    player2Gold += village.goldPerTurn;
                }
            }
        }
        UpdateGoldText();
    }

    public void UpdateGoldText()
    {
        player1GoldText.text = player1Gold.ToString();
        player2GoldText.text = player2Gold.ToString();
    }

    // Victory UI

    public void ShowVictoryPanel(int playerNumber) {

        if (playerNumber == 1)
        {
            blueVictory.SetActive(true);
        } else if (playerNumber == 2) {
            darkVictory.SetActive(true);
        }
    }

    public void RestartGame() {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }


}
