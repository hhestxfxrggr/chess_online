using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ChessSyncManager : MonoBehaviour
{
    public static ChessSyncManager Instance;
    public bool ReadyFlag = false;
    private readonly Queue<MovePacket> moveQueue = new Queue<MovePacket>();
    private readonly Queue<System.Action> mainThreadActions = new Queue<System.Action>();
    private Chess_Manager controller;

    void Awake()
    {
        Instance = this;
        controller = GameObject.FindGameObjectWithTag("GameController").GetComponent<Chess_Manager>();
    }

    void Update()
    {
        if (ReadyFlag && !ChessClient.Instance.isGameActive)
        {
            ChessClient.Instance.isGameActive = true;
            Debug.Log("[게임 시작] READY 수신 완료 → 말 배치됨");
        }

        while (moveQueue.Count > 0)
        {
            MovePacket move = moveQueue.Dequeue();
            controller.ApplyMove(move);
            //controller.SetCurrentPlayer(move.nextPlayer == 0 ? "white" : "black");
            ChessClient.Instance.isGameActive = true;
            Debug.Log($"턴바꾸기: {move.nextPlayer}");
        }

        while (mainThreadActions.Count > 0)
        {
            var action = mainThreadActions.Dequeue();
            action();
        }
    }

    public void EnqueueMove(MovePacket move)
    {
        lock (moveQueue)
        {
            moveQueue.Enqueue(move);
        }
    }
    public void EnqueueAction(System.Action action)
    {
        lock (mainThreadActions)
        {
            mainThreadActions.Enqueue(action);
        }
    }

    public void ShowVictory(int winner)
    {
       
        string text = (winner == 0 ? "White" : "Black");
        controller.Winner(text);
        Debug.Log($"[UI] 승리 텍스트 출력됨: {text}");
    }
}
