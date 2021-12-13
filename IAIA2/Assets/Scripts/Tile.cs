using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tile : MonoBehaviour
{
    public int posX;
    public int posY;

    private SpriteRenderer rend;
    public Color highlightedColor;
    public Color creatableColor;

    public LayerMask obstacles;

    public bool isWalkable;
    public bool isCreatable;

    private GM gm;

    public float amount;
    private bool sizeIncrease;

	private AudioSource source;

    //public GameObject unitInTile;
    //public bool tileReset = true;

    private void Start()
    {
		source = GetComponent<AudioSource>();
        gm = FindObjectOfType<GM>();
        rend = GetComponent<SpriteRenderer>();

    }

    /*private void Update()
    {
        //Debug.Log("Tile " + new Vector2(transform.position.x, transform.position.y));
        //Debug.Log("Mouse " + new Vector2(Input.mousePosition.x, Input.mousePosition.y));

        Nodo mouseNode = GameObject.Find("Map Generator").GetComponent<Grid>().NodeFromWorldPosition(new Vector2(Input.mousePosition.x, Input.mousePosition.y));
        Debug.Log(Vector2.Distance(new Vector2(mouseNode.position.x, mouseNode.position.y),
                            new Vector2(transform.position.x, transform.position.y)));
        if (Vector2.Distance(new Vector2(mouseNode.position.x, mouseNode.position.y), 
                            new Vector2(transform.position.x, transform.position.y)) < 0.5f && tileReset) 
        {
            if (isClear() == true)
            {
                source.Play();
                sizeIncrease = true;
                transform.localScale += new Vector3(amount, amount, amount);

                if(unitInTile != null)
                    unitInTile.transform.localScale += new Vector3(amount, amount, amount);

                tileReset = false;      
            }
        }
        else if (!tileReset && Vector2.Distance(new Vector2(mouseNode.position.x, mouseNode.position.y),
                            new Vector2(transform.position.x, transform.position.y)) >= 0.5f)
        {
            Debug.Log(tileReset);
            tileReset = true;
            if (isClear() == true)
            {
                sizeIncrease = false;
                transform.localScale -= new Vector3(amount, amount, amount);
            }

            if (isClear() == false && sizeIncrease == true)
            {
                sizeIncrease = false;
                transform.localScale -= new Vector3(amount, amount, amount);
            }
        }

        if (Input.GetMouseButtonDown(0) && !tileReset)
            if (isWalkable == true)
            {
                gm.selectedUnit.Move(this.transform);
            }
            else if (isCreatable == true && gm.createdUnit != null)
            {
                Unit unit = Instantiate(gm.createdUnit, new Vector3(transform.position.x, transform.position.y, 0), Quaternion.identity);
                unit.hasMoved = true;
                unit.hasAttacked = true;
                gm.ResetTiles();
                gm.createdUnit = null;
            }
            else if (isCreatable == true && gm.createdVillage != null)
            {
                Instantiate(gm.createdVillage, new Vector3(transform.position.x, transform.position.y, 0), Quaternion.identity);
                gm.ResetTiles();
                gm.createdVillage = null;
            }
    }*/


    public bool isClear() // does this tile have an obstacle on it. Yes or No?
    {
        Collider2D col = Physics2D.OverlapCircle(transform.position, 0.1f, obstacles);
        if (col == null)
        {
            return true;
        }
        else {
            return false;
        }
    }

    public void Highlight() {
		
        rend.color = highlightedColor;
        isWalkable = true;
    }

    public void Reset()
    {
        rend.color = Color.white;
        isWalkable = false;
        isCreatable = false;
    }

    public void SetCreatable() {
        rend.color = creatableColor;
        isCreatable = true;
    }

    private void OnMouseDown()
    {
        if (isWalkable == true)
        {
            gm.selectedUnit.Move(this.transform);
        }
        else if (isCreatable == true && gm.createdUnit != null)
        {
            Unit unit = Instantiate(gm.createdUnit, new Vector3(transform.position.x, transform.position.y, 0), Quaternion.identity);
            unit.hasMoved = true;
            unit.hasAttacked = true;
            gm.ResetTiles();
            gm.createdUnit = null;
        }
        else if (isCreatable == true && gm.createdVillage != null)
        {
            Instantiate(gm.createdVillage, new Vector3(transform.position.x, transform.position.y, 0), Quaternion.identity);
            gm.ResetTiles();
            gm.createdVillage = null;
        }
    }


    private void OnMouseEnter()
    {
        if (isClear() == true)
        {
            source.Play();
            sizeIncrease = true;
            transform.localScale += new Vector3(amount, amount, amount);
        }
    }

    private void OnMouseExit()
    {
        if (isClear() == true)
        {
            sizeIncrease = false;
            transform.localScale -= new Vector3(amount, amount, amount);
        }

        if (isClear() == false && sizeIncrease == true)
        {
            sizeIncrease = false;
            transform.localScale -= new Vector3(amount, amount, amount);
        }
    }
}
