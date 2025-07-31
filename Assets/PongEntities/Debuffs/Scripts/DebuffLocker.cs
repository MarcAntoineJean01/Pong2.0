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
[System.Serializable]
public struct DebuffStore
{
    public DebuffBurn debuffBurn;
    public DebuffFreeze debuffFreeze;
    public DebuffSlow debuffSlow;

    public List<DebuffEntity> allDebuffs => new List<DebuffEntity> { debuffBurn, debuffFreeze, debuffSlow };

    public void StoreDebuffs(bool forceStoreSlow = false)
    {
        if (PongBehaviour.currentStage != Stage.Universe || forceStoreSlow)
        {
            debuffSlow.gameObject.SetActive(false);
            debuffSlow.transform.position = Vector3.zero;
            debuffSlow.transform.rotation = Quaternion.Euler(Vector3.zero);
        }
        if (PongBehaviour.currentStage < Stage.FireAndIce)
        {
            debuffBurn.gameObject.SetActive(false);
            debuffBurn.transform.position = Vector3.zero;
            debuffBurn.transform.rotation = Quaternion.Euler(Vector3.zero);
            debuffFreeze.gameObject.SetActive(false);
            debuffFreeze.transform.position = Vector3.zero;
            debuffFreeze.transform.rotation = Quaternion.Euler(Vector3.zero);
        }
    }
}