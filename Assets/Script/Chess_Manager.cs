using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;


public class Chess_Manager : MonoBehaviour
{


    public GameObject chesspiece;

    private GameObject[,] positions = new GameObject[8, 8];

    private GameObject[] playerBlack = new GameObject[16];
    private GameObject[] playerWhite = new GameObject[16];

    private string currentPlayer = "white";
    private string player = "";
    private bool gameOver = false;

    // Start is called before the first frame update
    void Start()
    {
        player = ChessClient.Instance.myTeam;
        playerWhite = new GameObject[]
        {
            Create("white_rook", 0, 0), Create("white_knight", 1, 0), Create("white_bishop", 2, 0), Create("white_queen", 3, 0),
            Create("white_king", 4, 0), Create("white_bishop", 5, 0), Create("white_knight", 6, 0), Create("white_rook", 7, 0),
            Create("white_pawn", 0, 1), Create("white_pawn", 1, 1), Create("white_pawn", 2, 1), Create("white_pawn", 3, 1),
            Create("white_pawn", 4, 1), Create("white_pawn", 5, 1), Create("white_pawn", 6, 1), Create("white_pawn", 7, 1)
        };

        playerBlack = new GameObject[]
        {
            Create("black_rook", 0, 7), Create("black_knight", 1, 7), Create("black_bishop", 2, 7), Create("black_queen", 3, 7),
            Create("black_king", 4, 7), Create("black_bishop", 5, 7), Create("black_knight", 6, 7), Create("black_rook", 7, 7),
            Create("black_pawn", 0, 6), Create("black_pawn", 1, 6), Create("black_pawn", 2, 6), Create("black_pawn", 3, 6),
            Create("black_pawn", 4, 6), Create("black_pawn", 5, 6), Create("black_pawn", 6, 6), Create("black_pawn", 7, 6)
        };

        for (int i = 0; i < playerWhite.Length; i++)
        {
            setPosition(playerWhite[i]);
            setPosition(playerBlack[i]);
        }
    }

    private void InitializePieces()
    {
        playerWhite = new GameObject[]
       {
            Create("white_rook", 0, 0), Create("white_knight", 1, 0), Create("white_bishop", 2, 0), Create("white_queen", 3, 0),
            Create("white_king", 4, 0), Create("white_bishop", 5, 0), Create("white_knight", 6, 0), Create("white_rook", 7, 0),
            Create("white_pawn", 0, 1), Create("white_pawn", 1, 1), Create("white_pawn", 2, 1), Create("white_pawn", 3, 1),
            Create("white_pawn", 4, 1), Create("white_pawn", 5, 1), Create("white_pawn", 6, 1), Create("white_pawn", 7, 1)
       };

        playerBlack = new GameObject[]
        {
            Create("black_rook", 0, 7), Create("black_knight", 1, 7), Create("black_bishop", 2, 7), Create("black_queen", 3, 7),
            Create("black_king", 4, 7), Create("black_bishop", 5, 7), Create("black_knight", 6, 7), Create("black_rook", 7, 7),
            Create("black_pawn", 0, 6), Create("black_pawn", 1, 6), Create("black_pawn", 2, 6), Create("black_pawn", 3, 6),
            Create("black_pawn", 4, 6), Create("black_pawn", 5, 6), Create("black_pawn", 6, 6), Create("black_pawn", 7, 6)
        };

        for (int i = 0; i < playerWhite.Length; i++)
        {
            setPosition(playerWhite[i]);
            setPosition(playerBlack[i]);
        }
    }


    public GameObject Create(string name, int x, int y)
    {
        GameObject obj = Instantiate(chesspiece, new Vector3(0,0,-1),Quaternion.identity);
        ChessMan cm = obj.GetComponent<ChessMan>();
        cm.name = name;
        cm.SetXBoard(x);
        cm.SetYBoard(y);
        cm.Activate();
        return obj;
    }

    public void setPosition(GameObject obj)
    {
        ChessMan cm = obj.GetComponent<ChessMan> ();
        positions[cm.GetXBoard(), cm.GetYBoard()] = obj;
    }

    public GameObject GetPosition(int x, int y)
    {
        return positions[x, y];
    }

    public void SetPositionEmpty(int x, int y)
    {
        positions[x, y] = null;
    }

    public bool PositionOnBoard(int x, int y)
    {
        if(x<0||y<0||x>= positions.GetLength(0) || y>=positions.GetLength(1)) return false;
        return true;
    }

    public string GetCurrentPlayer()
    {
        return currentPlayer;
    }

    public void SetCurrentPlayer(string player)
    {
        currentPlayer = player;
        Debug.Log($"[SetCurrentPlayer] 현재 턴: {currentPlayer}");
    }

    public bool IsGameOver()
    {
        return gameOver;
    }

    public void NextTurn(int nextPlayer)
    {
        currentPlayer = (nextPlayer == 0) ? "white" : "black";
    }

    public void NextTurn()
    {
        if (currentPlayer == "white")
            currentPlayer = "black";
        else if (currentPlayer == "black")
            currentPlayer = "white";
    }

    public void Update()
    {
        Debug.Log($"{gameOver}");

        if (gameOver == true && Input.GetMouseButtonDown(0))
        {
            gameOver = false;

            ////Destroy(gameObject);
            //SceneManager.LoadScene("Game");

            ChessClient.Instance.SendRestartRequest(); // 서버로 재시작 요청 전송
        }
    }

    public void Winner(string playerWinner)
    {
        gameOver = true;

        GameObject.FindGameObjectWithTag("WinnerText").GetComponent<TextMeshProUGUI>().enabled = true;
        GameObject.FindGameObjectWithTag("WinnerText").GetComponent<TextMeshProUGUI>().text = playerWinner + " is the winner";

        GameObject.FindGameObjectWithTag("RestartText").GetComponent<TextMeshProUGUI>().enabled = true;
    }

    public void ApplyMove(MovePacket move)
    {
        var controller = GameObject.FindGameObjectWithTag("GameController").GetComponent<Chess_Manager>();

        // 1. 공격이면 말 삭제
        if (move.isAttack == 1)
        {
            GameObject target = controller.GetPosition(move.toX, move.toY);
            if (target != null)
            {
                GameObject.Destroy(target);
            }
        }

        // 2. 이동 처리
        GameObject moving = controller.GetPosition(move.fromX, move.fromY);
        if (moving != null)
        {
            controller.SetPositionEmpty(move.fromX, move.fromY);

            ChessMan cm = moving.GetComponent<ChessMan>();
            cm.SetXBoard(move.toX);
            cm.SetYBoard(move.toY);
            cm.SetCoords();

            controller.setPosition(moving);
            controller.NextTurn(move.nextPlayer);
            Debug.Log($"{move.nextPlayer}");
        }
        
    }

    public void ResetGame()
    {
        Debug.Log("[체스매니저] ResetGame() 호출됨");

        // 말 전부 제거
        ChessMan[] allPieces = FindObjectsOfType<ChessMan>();
        foreach (ChessMan cm in allPieces)
        {
            Destroy(cm.gameObject);
        }

        positions = new GameObject[8, 8];
        currentPlayer = "white";
        gameOver = false;

        // 새 말 배치
        InitializePieces();

        // UI 초기화
        GameObject.FindGameObjectWithTag("WinnerText").GetComponent<TextMeshProUGUI>().enabled = false;
        GameObject.FindGameObjectWithTag("RestartText").GetComponent<TextMeshProUGUI>().enabled = false;

        ChessClient.Instance.SendReady();

        Debug.Log("[체스매니저] 보드 리셋 완료");
    }
}
