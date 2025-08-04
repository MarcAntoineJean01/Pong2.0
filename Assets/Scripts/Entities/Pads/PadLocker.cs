using System.Collections.Generic;
using UnityEngine;
using System.Linq;
public enum PadController
{
    GamePad,
    KeyBoard,
    Environment
}
public enum PlayerController
{
    Player,
    Environment
}
public enum ProjectileType
{
    Fire,
    Ice
}
public struct PadPowers
{
    public bool magnet;
    public bool projectiles;

    public PadPowers(bool magnet, bool projectiles)
    {
        this.magnet = magnet;
        this.projectiles = projectiles;
    }
}
public enum PadType
{
    Rough,
    Slick
}