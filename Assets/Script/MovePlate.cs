using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovePlate : MonoBehaviour
{
    public GameObject controller;

    GameObject reference = null;

    int martixX;
    int martixY;

    public bool attack = false;

    public void Start()
    {
        if(attack)
        {
            gameObject.GetComponent<SpriteRenderer>().color = new Color(1.0f, 0.0f, 0.0f, 1.0f);
        }
    }

    public void OnMouseUp()
    {
        controller = GameObject.FindGameObjectWithTag("GameController");

        if(attack)
        {
            GameObject cp = controller.GetComponent<Chess_Manager>().GetPosition(martixX, martixY);

            Destroy(cp);
        }

        controller.GetComponent<Chess_Manager>().SetPositionEmpty(reference.GetComponent<ChessMan>().GetXBoard()
            ,reference.GetComponent<ChessMan>().GetYBoard());

        reference.GetComponent<ChessMan>().setXBoard(martixX);
        reference.GetComponent<ChessMan>().setYBoard(martixY);
        reference.GetComponent<ChessMan>().SetCoords();

        controller.GetComponent<Chess_Manager>().setPosition(reference);

        reference.GetComponent<ChessMan>().DestroyMovePlates();

    }

    public void SetCoords(int x, int y)
    {
        martixX=x; martixY=y;
    }    

    public void SetReference(GameObject obj)
    {
        reference = obj;
    }

    public GameObject GetReference() { return reference; }


}
