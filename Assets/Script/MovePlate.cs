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
        //else if (IsCastlingMove())
        //{
        //    gameObject.GetComponent<SpriteRenderer>().color = new Color(1.0f, 1.0f, 0.0f, 1.0f); // �����
        //}
    }

    public void OnMouseUp()
    {

        controller = GameObject.FindGameObjectWithTag("GameController");

        //if (!ChessClient.Instance.isGameActive)
        //{
        //    Debug.Log("[����] ���� ���� ��� ���̰ų� �� ���� �ƴ�");
        //    return;
        //}

        if (!ChessSyncManager.Instance.ReadyFlag)
        {
            Debug.Log("[����] ���� ���� ��� ���̰ų� �� ���� �ƴ�");
            return;
        }

        if (ChessClient.Instance.myTeam != controller.GetComponent<Chess_Manager>().GetCurrentPlayer())
        {
            Debug.Log($"[����] �� ���� �ƴմϴ�. ���� ��: {controller.GetComponent<Chess_Manager>().GetCurrentPlayer()}");
            return;
        }
        Debug.Log($"[����] �� Ȯ�ο�. ��: {ChessClient.Instance.myTeam}, {controller.GetComponent<Chess_Manager>().GetCurrentPlayer()}");
        
        if (attack)
        {
            GameObject cp = controller.GetComponent<Chess_Manager>().GetPosition(martixX, martixY);

            if (cp.name == "white_king" || cp.name == "black_king")
            {
                int winner = (cp.name == "black_king") ? 0 : 1; // white wins if black king dies
                ChessClient.Instance.SendVictory(winner);       // ������ �¸� ��Ŷ ����
            }

            Destroy(cp);
        }

        //�� �̵� ��ġ ����
        int fromX = reference.GetComponent<ChessMan>().GetXBoard();
        int fromY = reference.GetComponent<ChessMan>().GetYBoard();
        int toX = martixX;
        int toY = martixY;


        controller.GetComponent<Chess_Manager>().SetPositionEmpty(reference.GetComponent<ChessMan>().GetXBoard()
            ,reference.GetComponent<ChessMan>().GetYBoard());

        reference.GetComponent<ChessMan>().SetXBoard(martixX);
        reference.GetComponent<ChessMan>().SetYBoard(martixY);
        reference.GetComponent<ChessMan>().SetCoords();

        controller.GetComponent<Chess_Manager>().setPosition(reference);
        controller.GetComponent<Chess_Manager>().NextTurn();
        reference.GetComponent<ChessMan>().DestroyMovePlates();

        ChessMan cm = reference.GetComponent<ChessMan>();
        if ((cm.name == "white_pawn" && martixY == 7) || (cm.name == "black_pawn" && martixY == 0))
        {
            cm.name = (cm.name.StartsWith("white")) ? "white_queen" : "black_queen";
            cm.Activate(); // ��������Ʈ ����
        }

        if (ChessClient.Instance != null)
        {
            //��Ŷ ����
            ChessClient.Instance.SendMoveStruct(fromX, fromY, toX, toY,attack,controller.GetComponent<Chess_Manager>().GetCurrentPlayer());
        }
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

    private bool IsCastlingMove()
    {
        // ŷ�� �̵��ϴ� ��ġ�� ĳ���� ��ǥ���� Ȯ��
        return (reference != null && reference.GetComponent<ChessMan>().name.Contains("king") &&
                (martixX == 2 || martixX == 6));
    }
}
