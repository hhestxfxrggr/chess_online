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

        //if (!ChessClient.Instance.isGameActive)
        //{
        //    Debug.Log("[차단] 서버 응답 대기 중이거나 내 턴이 아님");
        //    return;
        //}

        if (!ChessSyncManager.Instance.ReadyFlag)
        {
            Debug.Log("[차단] 서버 응답 대기 중이거나 내 턴이 아님");
            return;
        }

        if (ChessClient.Instance.myTeam != controller.GetComponent<Chess_Manager>().GetCurrentPlayer())
        {
            Debug.Log($"[차단] 내 턴이 아닙니다. 현재 턴: {controller.GetComponent<Chess_Manager>().GetCurrentPlayer()}");
            return;
        }
        Debug.Log($"[차단] 턴 확인용. 팀: {ChessClient.Instance.myTeam}, {controller.GetComponent<Chess_Manager>().GetCurrentPlayer()}");
        
        if (attack)
        {
            GameObject cp = controller.GetComponent<Chess_Manager>().GetPosition(martixX, martixY);

            if (cp.name == "white_king" || cp.name == "black_king")
            {
                int winner = (cp.name == "black_king") ? 0 : 1; // white wins if black king dies
                ChessClient.Instance.SendVictory(winner);       // 서버에 승리 패킷 전송
            }

            Destroy(cp);
        }

        //말 이동 위치 저장
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
            cm.Activate(); // 스프라이트 갱신
        }

        if (ChessClient.Instance != null)
        {
            //패킷 전송
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
        // 킹이 이동하는 위치가 캐슬링 좌표인지 확인
        return (reference != null && reference.GetComponent<ChessMan>().name.Contains("king") &&
                (martixX == 2 || martixX == 6));
    }
}
