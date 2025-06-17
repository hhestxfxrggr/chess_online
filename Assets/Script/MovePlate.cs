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
        //    gameObject.GetComponent<SpriteRenderer>().color = new Color(1.0f, 1.0f, 0.0f, 1.0f); // 노란색
        //}
    }

    public void OnMouseUp()
    {
        controller = GameObject.FindGameObjectWithTag("GameController");

        if(attack)
        {
            GameObject cp = controller.GetComponent<Chess_Manager>().GetPosition(martixX, martixY);

            if (cp.name == "white_king") controller.GetComponent<Chess_Manager>().Winner("black");
            if (cp.name == "black_king") controller.GetComponent<Chess_Manager>().Winner("white");

            Destroy(cp);
        }

        //말 이동 위치 저장
        int fromX = reference.GetComponent<ChessMan>().GetXBoard();
        int fromY = reference.GetComponent<ChessMan>().GetYBoard();
        int toX = martixX;
        int toY = martixY;


        controller.GetComponent<Chess_Manager>().SetPositionEmpty(reference.GetComponent<ChessMan>().GetXBoard()
            ,reference.GetComponent<ChessMan>().GetYBoard());

        reference.GetComponent<ChessMan>().setXBoard(martixX);
        reference.GetComponent<ChessMan>().setYBoard(martixY);
        reference.GetComponent<ChessMan>().SetCoords();

        controller.GetComponent<Chess_Manager>().setPosition(reference);

        controller.GetComponent<Chess_Manager>().NextTurn();

        reference.GetComponent<ChessMan>().DestroyMovePlates();

        ChessMan cm = reference.GetComponent<ChessMan>();
        if ((cm.name == "white_pawn" && martixY == 7) || (cm.name == "black_pawn" && martixY == 0))
        {
            cm.name = (cm.name.StartsWith("white")) ? "white_queen" : "black_queen";
            cm.Activate(); // 스프라이트 갱신
        }

        if (ChessClient.Instance != null)
        {
            ChessClient.Instance.SendMoveStruct(fromX, fromY, toX, toY);
        }



        //if ((cm.name == "white_king" && cm.GetXBoard() == 4 && cm.GetYBoard() == 0) ||
        //    (cm.name == "black_king" && cm.GetXBoard() == 4 && cm.GetYBoard() == 7))
        //{
        //    // 오른쪽 캐슬링
        //    if (martixX == 6)
        //    {
        //        GameObject rook = controller.GetComponent<Chess_Manager>().GetPosition(7, martixY);
        //        if (rook != null && rook.GetComponent<ChessMan>().name == $"{cm.GetPlayer()}_rook")
        //        {
        //            controller.GetComponent<Chess_Manager>().SetPositionEmpty(7, martixY);
        //            rook.GetComponent<ChessMan>().setXBoard(5);
        //            rook.GetComponent<ChessMan>().setYBoard(martixY);
        //            rook.GetComponent<ChessMan>().SetCoords();
        //            controller.GetComponent<Chess_Manager>().setPosition(rook);
        //        }
        //    }

        //    // 왼쪽 캐슬링
        //    if (martixX == 2)
        //    {
        //        GameObject rook = controller.GetComponent<Chess_Manager>().GetPosition(0, martixY);
        //        if (rook != null && rook.GetComponent<ChessMan>().name == $"{cm.GetPlayer()}_rook")
        //        {
        //            controller.GetComponent<Chess_Manager>().SetPositionEmpty(0, martixY);
        //            rook.GetComponent<ChessMan>().setXBoard(3);
        //            rook.GetComponent<ChessMan>().setYBoard(martixY);
        //            rook.GetComponent<ChessMan>().SetCoords();
        //            controller.GetComponent<Chess_Manager>().setPosition(rook);
        //        }
        //    }
        //}
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
        // 킹이 이동하는 위치가 캐슬링 좌표인지 확인
        return (reference != null && reference.GetComponent<ChessMan>().name.Contains("king") &&
                (martixX == 2 || martixX == 6));
    }
}
