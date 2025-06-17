using System.Runtime.InteropServices;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct MovePacket
{
    public int fromX;
    public int fromY;
    public int toX;
    public int toY;
}
