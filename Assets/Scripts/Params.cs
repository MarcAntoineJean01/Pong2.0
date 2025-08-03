using UnityEngine;

[System.Serializable]
public class InitialSizes
{
    [Range(1, 100)]
    public int ballSize;
    [Range(1, 100)]
    public int spikeSize;
    [Range(1, 100)]
    public int padSize;
    [Range(1, 100)]
    public int debuffSize;
    [Range(1, 10)]
    public float ballSizeMultiplier;
}
[System.Serializable]
public class Speeds
{
    public EntitySpeeds entitySpeeds;
    public TransitionSpeeds transitionSpeeds;
}
[System.Serializable]
public class EntitySpeeds
{
    [Range(1, 100)]
    public int ballMinLinearVelocity;
    [Range(1, 100)]
    public int ballMaxLinearVelocity;
    [Range(1, 100)]
    public int ballMaxAngularVelocity;
    [Range(1, 100)]
    public int ballMinAngularVelocity;
    [Range(1, 100)]
    public int spikeLinearVelocity;
    [Range(1, 1000)]
    public int PadLinearVelocity;
    [Range(1, 100)]
    public int debuffMinLinearVelocity;
    [Range(1, 100)]
    public int debuffMaxLinearVelocity;
}
[System.Serializable]
public class TransitionSpeeds
{
    [Range(1, 100)]
    public int cameraTransitionSpeed;
    [Range(1, 100)]
    public int entitiesTransitionSpeed;
    [Range(1, 100)]
    public int resetTransitionSpeed;
    [Range(1, 10)]
    public int roundResetTransitionSpeed;
    [Range(1, 100)]
    public int meshSwappingSpeed;
}
[System.Serializable]
public class GameEffects
{
    [Range(1, 30)]
    public int spikeInterval;
    [Range(1, 100)]
    public int explosionStrength;
    [Range(1, 100)]
    public int explosionSize;
    [Range(0.01f, 2f)]
    public float explosionDuration;
    [Range(10, 500)]
    public int magnetStrength;
    [Range(1, 100)]
    public int magnetSize;
    [Range(1, 100)]
    public int magnetDuration;
    [Range(1, 20)]
    public int magnetCooldown;
    [Range(1, 10)]
    public int iceProjectileSpeed;
    [Range(1, 10)]
    public int fireProjectileSpeed;
    [Range(1, 10)]
    public int iceProjectileSize;
    [Range(1, 10)]
    public int fireProjectileSize;
    [Range(1, 20)]
    public int projectileCooldown;
    [Range(1, 10)]
    public int frostDuration;
    [Range(0.01f, 2f)]
    public float dissolveSpeed;
    [Range(1, 10)]
    public int wallDissolveSpeed;
    [Range(0.01f, 1f)]
    public float dissolveStrength;
    [Range(1, 100)]
    public int wallAttractorStrength;
    [Range(1, 30)]
    public int wallAttractorDuration;
    [Range(1, 100)]
    public int floorAttractorStrength;
    [Range(0.1f, 2f)]
    public float cameraNoiseDuration;
    [Range(0.1f, 10f)]
    public float debuffOrbitTime;
}