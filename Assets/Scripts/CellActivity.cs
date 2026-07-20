using System;

[Flags]
public enum CellActivity
{
    None = 0,

    Swapping = 1 << 0,
    Falling = 1 << 1,
    Destroying = 1 << 2,
    Spawning = 1 << 3,
}