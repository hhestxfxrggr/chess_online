using System.Runtime.InteropServices;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct ControlPacket
{
    public int command;  // 0 = WIN, 1 = RESET, 2 = SURRENDER µî
    public int winner;   // 0 = white, 1 = black
}
