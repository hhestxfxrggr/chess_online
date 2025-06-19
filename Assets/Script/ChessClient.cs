using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using UnityEngine;


public class ChessClient : MonoBehaviour
{
    private TcpClient client;
    private NetworkStream stream;
    private Thread recvThread;
    public static ChessClient Instance;
    public string serverIP = "127.0.0.1";
    public int serverPort = 9000;
    public string myTeam = "";
    public bool isGameActive = false;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
        DontDestroyOnLoad(gameObject);
    }

    void Start()
    {
        ConnectToServer();
    }

    public void ConnectToServer()
    {
        try
        {
            client = new TcpClient();
            client.Connect(serverIP, serverPort);
            stream = client.GetStream();
            Debug.Log("서버 연결 성공");

            recvThread = new Thread(new ThreadStart(Receive));
            recvThread.IsBackground = true;
            recvThread.Start();

            Send("LOGIN:unity_client");
            SendReady();
        }
        catch (Exception e)
        {
            Debug.LogError("서버 연결 실패: " + e.Message);
        }
    }

    public void Send(string msg)
    {
        if (client == null || !client.Connected) return;

        byte[] body = Encoding.UTF8.GetBytes(msg);
        byte[] packet = new byte[1 + body.Length];
        packet[0] = 0x01;
        Buffer.BlockCopy(body, 0, packet, 1, body.Length);
        stream.Write(packet, 0, packet.Length);
    }

    void Receive()
    {
        try
        {
            byte[] typeBuffer = new byte[1];
            while (true)
            {
                int typeRead = stream.Read(typeBuffer, 0, 1);
                if (typeRead <= 0) break;

                byte packetType = typeBuffer[0];
                switch (packetType)
                {
                    case 0x01: // 문자열
                        byte[] stringBuf = new byte[1024];
                        int len = stream.Read(stringBuf, 0, stringBuf.Length);
                        if (len <= 0) break;

                        string msg = Encoding.UTF8.GetString(stringBuf, 0, len).Trim('\0');

                        if (msg.StartsWith("TEAM:"))
                        {
                            myTeam = msg.Substring(5);
                            Debug.Log($"[클라이언트] 내 팀은 {myTeam}");
                        }
                        //else if (msg == "READY")
                        //{
                        //    ChessSyncManager.Instance.ReadyFlag = true;
                        //}
                        break;

                    case 0x02: // MovePacket
                        byte[] moveBuf = new byte[Marshal.SizeOf(typeof(MovePacket))];
                        int total = 0;
                        while (total < moveBuf.Length)
                        {
                            int r = stream.Read(moveBuf, total, moveBuf.Length - total);
                            if (r <= 0) return;
                            total += r;
                        }

                        GCHandle handle = GCHandle.Alloc(moveBuf, GCHandleType.Pinned);
                        MovePacket move = (MovePacket)Marshal.PtrToStructure(handle.AddrOfPinnedObject(), typeof(MovePacket));
                        handle.Free();

                        ChessSyncManager.Instance.EnqueueMove(move);
                        break;

                    case 0x03:
                        {
                            byte[] ctrlBuf = new byte[Marshal.SizeOf(typeof(ControlPacket))];
                            total = 0;
                            while (total < ctrlBuf.Length)
                            {
                                int r = stream.Read(ctrlBuf, total, ctrlBuf.Length - total);
                                if (r <= 0) return;
                                total += r;
                            }

                            handle = GCHandle.Alloc(ctrlBuf, GCHandleType.Pinned);
                            ControlPacket ctrl = (ControlPacket)Marshal.PtrToStructure(handle.AddrOfPinnedObject(), typeof(ControlPacket));
                            handle.Free();

                            if (ctrl.command == 0) // 승리
                            {
                                Debug.Log($"[게임 종료] {(ctrl.winner == 0 ? "White" : "Black")} 승리");

                                                 // ❗메인 스레드에서 실행되도록 큐잉
                                ChessSyncManager.Instance.EnqueueAction(() =>
                                {
                                    ChessSyncManager.Instance.ShowVictory(ctrl.winner);
                                    ChessClient.Instance.isGameActive = false;
                                });
                            }
                            else if (ctrl.command == 1) // RESET
                            {
                                Debug.Log("[클라이언트] RESET 패킷 수신 → 게임 리셋");

                                ChessSyncManager.Instance.EnqueueAction(() =>
                                {
                                    ChessClient.Instance.isGameActive = false;

                                                       // 말만 다시 배치
                                    var mgr = GameObject.FindGameObjectWithTag("GameController").GetComponent<Chess_Manager>();
                                    mgr.ResetGame();
                                });
                            }

                            break;
                        }
                    case 0x04: // ReadyPacket
                        {
                            byte[] buf = new byte[Marshal.SizeOf(typeof(ReadyPacket))];
                            total = 0;
                            while (total < buf.Length)
                            {
                                int r = stream.Read(buf, total, buf.Length - total);
                                if (r <= 0) return;
                                total += r;
                            }

                            handle = GCHandle.Alloc(buf, GCHandleType.Pinned);
                            ReadyPacket rp = (ReadyPacket)Marshal.PtrToStructure(handle.AddrOfPinnedObject(), typeof(ReadyPacket));
                            handle.Free();

                            if (rp.command == 1)
                            {
                                ChessSyncManager.Instance.ReadyFlag = true;
                                Debug.Log("[클라이언트] 두 명 모두 준비됨 → 게임 시작!");
                            }
                            break;
                        }

                }
            }
        }
        catch (Exception e)
        {
            Debug.LogError("수신 오류: " + e.Message);
        }
    }
   
    public void SendMoveStruct(int fromX, int fromY, int toX, int toY, bool isAttack, string currentPlayer)
    {
        if (client == null || !client.Connected) return;

        MovePacket packet = new MovePacket
        {
            fromX = fromX,
            fromY = fromY,
            toX = toX,
            toY = toY,
            isAttack = isAttack ? 1 : 0,
            nextPlayer = (currentPlayer == "white") ? 1 : 0
        };

        byte[] structBytes = StructToBytes(packet);
        byte[] dataToSend = new byte[1 + structBytes.Length];
        dataToSend[0] = 0x02;
        Buffer.BlockCopy(structBytes, 0, dataToSend, 1, structBytes.Length);

        stream.Write(dataToSend, 0, dataToSend.Length);
        stream.Flush();
    }

    public static byte[] StructToBytes<T>(T str) where T : struct
    {
        int size = Marshal.SizeOf(str);
        byte[] arr = new byte[size];
        IntPtr ptr = Marshal.AllocHGlobal(size);
        Marshal.StructureToPtr(str, ptr, true);
        Marshal.Copy(ptr, arr, 0, size);
        Marshal.FreeHGlobal(ptr);
        return arr;
    }
    public void SendVictory(int winner)
    {
        if (client == null || !client.Connected) return;

        ControlPacket pkt = new ControlPacket
        {
            command = 0, // WIN
            winner = winner
        };

        byte[] bytes = StructToBytes(pkt);
        byte[] data = new byte[1 + bytes.Length];
        data[0] = 0x03;
        Buffer.BlockCopy(bytes, 0, data, 1, bytes.Length);

        stream.Write(data, 0, data.Length);
        stream.Flush();

        isGameActive = false;
    }

    public void SendReady()
    {
        if (client == null || !client.Connected) return;

        ReadyPacket pkt = new ReadyPacket { command = 0 };
        byte[] bytes = StructToBytes(pkt);
        byte[] data = new byte[1 + bytes.Length];
        data[0] = 0x04; // ReadyPacket 타입
        Buffer.BlockCopy(bytes, 0, data, 1, bytes.Length);

        stream.Write(data, 0, data.Length);
        stream.Flush();

        Debug.Log("[클라이언트] ReadyPacket(command=0) 전송됨");
    }
    public void SendRestartRequest()
    {
        if (client == null || !client.Connected) return;

        ControlPacket pkt = new ControlPacket
        {
            command = 1, // RESET 요청
            winner = -1  // 의미 없음
        };

        byte[] bytes = StructToBytes(pkt);
        byte[] data = new byte[1 + bytes.Length];
        data[0] = 0x03;
        Buffer.BlockCopy(bytes, 0, data, 1, bytes.Length);

        stream.Write(data, 0, data.Length);
        stream.Flush();

        Debug.Log("[클라이언트] 재시작 요청 전송 완료");
    }
    public void ResetAndReconnect()
    {
        // 연결 끊기
        if (recvThread != null && recvThread.IsAlive)
            recvThread.Abort();
        stream?.Close();
        client?.Close();

        // 재연결
        ConnectToServer();
    }

    void OnApplicationQuit()
    {
        if (recvThread != null && recvThread.IsAlive)
            recvThread.Abort();
        stream?.Close();
        client?.Close();
    }

    
}
