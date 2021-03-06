﻿namespace GameOffsets.Controller
{
    using System;
    using System.Runtime.InteropServices;
    using GameOffsets.Natives;

    [StructLayout(LayoutKind.Explicit, Pack = 1)]
    public struct GameStateStaticOffset
    {
        [FieldOffset(0x00)] public IntPtr GameState;
    }

    [StructLayout(LayoutKind.Explicit, Pack = 1)]
    public struct GameStateOffset
    {
        [FieldOffset(0x08)] public StdVector CurrentStateOffset1; // Used in RemoteObject -> CurrentState
        //[FieldOffset(0x20)] public StdVector CurrentStateOffset2; // same as CurrentStateOffset1.
        [FieldOffset(0x48)] public StdMap States;
    }
}