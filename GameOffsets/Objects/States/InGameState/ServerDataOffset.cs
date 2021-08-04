namespace GameOffsets.Objects.States.InGameState
{
    using System;
    using Natives;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Explicit, Pack = 1)]
    public struct ServerDataStructure
    {
        [FieldOffset(0x8758)] public StdVector PlayerInventories; // InventoryArrayStruct
    }

    [StructLayout(LayoutKind.Explicit, Pack =1)]
    public struct InventoryArrayStruct
    {
        [FieldOffset(0x00)] public int InventoryId;
        [FieldOffset(0x04)] public int PAD_0;
        [FieldOffset(0x08)] public IntPtr InventoryPtr0; // InventoryStruct
        [FieldOffset(0x10)] public IntPtr InventoryPtr1; // this points to 0x10 bytes before InventoryPtr0
        [FieldOffset(0x18)] public long PAD_1;
    }
}