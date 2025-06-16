using System;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;

public class ChessClient : MonoBehaviour
{
    private TcpClient client;
    private NetworkStream stream;
    private Thread recvThread;
    public string serverIP = "127.0.0.1";
    public int serverPort = 9000;

    void Start()
    {
        ConnectToServer();
    }

    void OnApplicationQuit()
    {
        Disconnect();
    }

    public void ConnectToServer()
    {
        try
        {
            client = new TcpClient();
            client.Connect(serverIP, serverPort);
            stream = client.GetStream();
            Debug.Log("서버 연결 성공");

            // 수신 스레드 시작
            recvThread = new Thread(new ThreadStart(Receive));
            recvThread.IsBackground = true;
            recvThread.Start();

            // 테스트용: 로그인 전송
            Send("LOGIN:unity_client");
        }
        catch (Exception e)
        {
            Debug.LogError("서버 연결 실패: " + e.Message);
        }
    }

    public void Send(string msg)
    {
        if (client == null || !client.Connected) return;

        byte[] data = Encoding.UTF8.GetBytes(msg + "\n");
        stream.Write(data, 0, data.Length);
        stream.Flush();
    }

    void Receive()
    {
        try
        {
            byte[] buffer = new byte[1024];
            while (true)
            {
                int bytes = stream.Read(buffer, 0, buffer.Length);
                if (bytes <= 0) break;

                string msg = Encoding.UTF8.GetString(buffer, 0, bytes);
                Debug.Log("서버 응답: " + msg);
            }
        }
        catch (Exception e)
        {
            Debug.LogError("수신 오류: " + e.Message);
        }
    }

    public void Disconnect()
    {
        if (recvThread != null && recvThread.IsAlive)
            recvThread.Abort();

        stream?.Close();
        client?.Close();
    }
}
