using System.Collections.Generic;
using UnityEngine;

public enum DebuffType
{
    DebuffBurn,
    DebuffFreeze,
    DebuffSlow
}
[System.Serializable]
public class AllowedDebuffs
{
    public bool freeze = true;
    public bool burn = true;
    public bool slow = true;

    public bool GetAllowedDebuff(DebuffType debuffType)
    {
        switch (debuffType)
        {
            default:
            case DebuffType.DebuffBurn:
                return burn;
            case DebuffType.DebuffSlow:
                return slow;
            case DebuffType.DebuffFreeze:
                return freeze;

        }
    }
    public void SetAllowedDebuff(DebuffType debuffType, bool allowed)
    {
        switch (debuffType)
        {
            case DebuffType.DebuffBurn:
                burn = allowed;
                break;
            case DebuffType.DebuffSlow:
                slow = allowed;
                break;
            case DebuffType.DebuffFreeze:
                freeze = allowed;
                break;
        }
    }
}