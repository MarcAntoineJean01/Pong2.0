using System.Collections.Generic;
using UnityEngine;

public enum SpikeType
{
    SpikePadPiece,
    SpikePadBlock,
    SpikeDissolve,
    SpikeRandomDirection,
    SpikeWallAttractor,
    SpikeMagnet,
    HealthUp
}
[System.Serializable]
public class AllowedSpikes
{
    public bool m_AddPadPiece = true;
    public bool m_AddPadBlock = true;
    public bool m_Dissolve = true;
    public bool m_RandomDirection = true;
    public bool m_WallAttractor = true;
    public bool m_Magnet = true;
    public bool m_HealthUp = true;
    public bool addPadPiece => m_AddPadPiece;
    public bool addPadBlock => m_AddPadBlock && PongBehaviour.currentStage > Stage.DD;
    public bool dissolve => m_Dissolve && PongBehaviour.currentStage > Stage.FreeMove;
    public bool randomDirection => m_RandomDirection && PongBehaviour.currentStage == Stage.Final;
    public bool wallAttractor => m_WallAttractor && PongBehaviour.currentStage > Stage.Universe;
    public bool magnet => m_Magnet && PongBehaviour.currentStage > Stage.DDD;
    public bool healthUp => m_HealthUp && PongBehaviour.currentStage > Stage.DDD;
    public bool anyAllowedSpikes => addPadPiece || addPadBlock || dissolve || randomDirection || wallAttractor || magnet || healthUp;
    public List<SpikeType> allowedSpikeTypes
    {
        get
        {
            List<SpikeType> newList = new List<SpikeType>();
            if (addPadPiece) { newList.Add(SpikeType.SpikePadPiece); }
            if (addPadBlock) { newList.Add(SpikeType.SpikePadBlock); }
            if (dissolve) { newList.Add(SpikeType.SpikeDissolve); }
            if (randomDirection) { newList.Add(SpikeType.SpikeRandomDirection); }
            if (wallAttractor) { newList.Add(SpikeType.SpikeWallAttractor); }
            if (magnet) { newList.Add(SpikeType.SpikeMagnet); }
            if (healthUp) { newList.Add(SpikeType.HealthUp); }
            return newList;
        }
    }
    public bool GetAllowedSpike(SpikeType spikeType)
    {
        switch (spikeType)
        {
            default:
            case SpikeType.SpikePadPiece:
                return m_AddPadPiece;
            case SpikeType.SpikePadBlock:
                return m_AddPadBlock;
            case SpikeType.SpikeDissolve:
                return m_Dissolve;
            case SpikeType.SpikeRandomDirection:
                return m_RandomDirection;
            case SpikeType.SpikeWallAttractor:
                return m_WallAttractor;
            case SpikeType.SpikeMagnet:
                return m_Magnet;
            case SpikeType.HealthUp:
                return m_HealthUp;

        }
    }
    public void SetAllowedSpike(SpikeType spikeType, bool allowed)
    {
        switch (spikeType)
        {
            case SpikeType.SpikePadPiece:
                m_AddPadPiece = allowed;
                break;
            case SpikeType.SpikePadBlock:
                m_AddPadBlock = allowed;
                break;
            case SpikeType.SpikeDissolve:
                m_Dissolve = allowed;
                break;
            case SpikeType.SpikeRandomDirection:
                m_RandomDirection = allowed;
                break;
            case SpikeType.SpikeWallAttractor:
                m_WallAttractor = allowed;
                break;
            case SpikeType.SpikeMagnet:
                m_Magnet = allowed;
                break;
            case SpikeType.HealthUp:
                m_HealthUp = allowed;
                break;

        }
    }
}
[System.Serializable]
public class SpikeStore
{
    public SpikePair activeSpikes;
    public SpikePair lastActiveSpikes;
    public SpikePair spikePadPiecePair;
    public SpikePair spikePadBlockPair;
    public SpikePair spikeDissolvePair;
    public SpikePair spikeRandomDirectionPair;
    public SpikePair spikeWallAttractorPair;
    public SpikePair spikeMagnetPair;
    public SpikePair spikeHealthUpPair;
    public bool anyActiveSpike => activeSpikes.spikeLeft != null || activeSpikes.spikeRight != null;
    public List<SpikeEntity> allSpikes => new List<SpikeEntity>()
    {
        spikePadPiecePair.spikeLeft,
        spikePadPiecePair.spikeRight,
        spikePadBlockPair.spikeLeft,
        spikePadBlockPair.spikeRight,
        spikeDissolvePair.spikeLeft,
        spikeDissolvePair.spikeRight,
        spikeRandomDirectionPair.spikeLeft,
        spikeRandomDirectionPair.spikeRight,
        spikeWallAttractorPair.spikeLeft,
        spikeWallAttractorPair.spikeRight,
        spikeMagnetPair.spikeLeft,
        spikeMagnetPair.spikeRight,
        spikeHealthUpPair.spikeLeft,
        spikeHealthUpPair.spikeRight
    };
    public void StoreSpikes()
    {
        foreach (SpikeEntity spike in allSpikes)
        {
            spike.gameObject.SetActive(false);
            spike.transform.position = Vector3.zero;
            spike.transform.rotation = Quaternion.Euler(Vector3.zero);
            spike.rbd.velocity = Vector3.zero;
            spike.rbd.angularVelocity = Vector3.zero;
        }
        activeSpikes = new SpikePair(null, "activeSpikes");
    }
    public void SetActiveSpikes(SpikeType spikeType)
    {
        switch (spikeType)
        {
            case SpikeType.SpikePadPiece:
                activeSpikes.SwapSpikes(spikePadPiecePair);
                break;
            case SpikeType.SpikePadBlock:
                activeSpikes.SwapSpikes(spikePadBlockPair);
                break;
            case SpikeType.SpikeDissolve:
                activeSpikes.SwapSpikes(spikeDissolvePair);
                break;
            case SpikeType.SpikeRandomDirection:
                activeSpikes.SwapSpikes(spikeRandomDirectionPair);
                break;
            case SpikeType.SpikeWallAttractor:
                activeSpikes.SwapSpikes(spikeWallAttractorPair);
                break;
            case SpikeType.SpikeMagnet:
                activeSpikes.SwapSpikes(spikeMagnetPair);
                break;
            case SpikeType.HealthUp:
                activeSpikes.SwapSpikes(spikeHealthUpPair);
                break;
        }
        lastActiveSpikes.SwapSpikes(activeSpikes);
    }
}
[System.Serializable]
public class SpikePair
{
    public string name;
    public SpikeEntity spikeLeft;
    public SpikeEntity spikeRight;

    public SpikePair(SpikeEntity spike, string name)
    {
        if (spike != null)
        {
            SpikeEntity spikeClone = GameObject.Instantiate(spike, spike.transform.parent);
            spikeLeft = spike;
            spikeLeft.sd = Side.Left;
            spikeRight = spikeClone;
            spikeRight.sd = Side.Right;
        }
        else
        {
            spikeLeft = null;
            spikeRight = null;
        }
        this.name = name;

    }
    public void ActivateSpikes()
    {
        spikeLeft.ActivateSpike();
        spikeRight.ActivateSpike();
    }

    public void SwapSpikes(SpikePair spikePair)
    {
        spikeLeft = spikePair.spikeLeft;
        spikeRight = spikePair.spikeRight;
    }
}
