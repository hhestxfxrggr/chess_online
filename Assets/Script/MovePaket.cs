using System.Runtime.InteropServices;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct MovePacket
{
    public int fromX;
    public int fromY;
    public int toX;
    public int toY;
    public int isAttack; // 0: 일반 이동, 1: 공격
    public int nextPlayer;//0: white 1:black
}

