using System.Runtime.InteropServices;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct ReadyPacket
{
    public int command; // 0 = READY
}