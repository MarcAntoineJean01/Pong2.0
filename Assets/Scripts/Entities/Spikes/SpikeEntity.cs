using UnityEngine;
using UnityEngine.Rendering;
public class SpikeEntity : PongEntity
{
    protected int bounceLimit = 10; //THIS IS TO KILL THE SPIKE AFTER AN N AMOUNT OF BOUNCES
    public int bounces = 0;
    public Side sd;
    public float sqrSpikeSpeed => pm.speeds.entitySpeeds.spikeLinearVelocity * pm.speeds.entitySpeeds.spikeLinearVelocity * speedModifier;
    bool withinSpeedLimits => rbd.velocity.sqrMagnitude == sqrSpikeSpeed;
    public SpikeMesh spikeMesh;
    Vector3 clampedVelocity
    {
        get
        {
            if (rbd.velocity.sqrMagnitude != sqrSpikeSpeed) { return rbd.velocity.normalized * pm.speeds.entitySpeeds.spikeLinearVelocity * speedModifier; }
            return rbd.velocity;
        }
    }
    Vector3 initialSpikeVelocity
    {
        get
        {
            int directions = sd == Side.Left ? -1 : 1;
            float x = UnityEngine.Random.Range(1f, 2f) * directions * pm.speeds.entitySpeeds.spikeLinearVelocity * speedModifier;
            float y = UnityEngine.Random.Range(1f, 2f) * directions * pm.speeds.entitySpeeds.spikeLinearVelocity * speedModifier;
            float z = 0;
            return new Vector3(x, y, z);
        }
    }
    protected override void FixedUpdate()
    {
        base.FixedUpdate();
        if (!magnetized && !withinSpeedLimits)
        {
            rbd.velocity = clampedVelocity;
        }
        if (Mathf.Abs(transform.position.x) > Mathf.Abs(fieldBounds.x) || Mathf.Abs(transform.position.y) > Mathf.Abs(fieldBounds.y))
        {
            transform.position = new Vector3(0, 0, stagePosZ);
        }
    }
    public void StartSpike()
    {
        bounces = 0;
        rbd.AddForce(initialSpikeVelocity, ForceMode.VelocityChange);
    }
    public void SetSpikeForStage()
    {
        switch (currentStage)
        {
            case Stage.StartMenu:
            case Stage.DD:
                rbd.constraints = rbd.constraints | RigidbodyConstraints.FreezePositionZ | RigidbodyConstraints.FreezeRotation;
                meshR.shadowCastingMode = ShadowCastingMode.ShadowsOnly;
                break;
            default:
                rbd.constraints = rbd.constraints | RigidbodyConstraints.FreezePositionZ | RigidbodyConstraints.FreezeRotation;
                meshR.shadowCastingMode = ShadowCastingMode.On;
                break;
        }
    }
    public void KillSpike()
    {
        if (this is SpikePadBlock || this is SpikePadPiece || this is SpikeMagnet || this is SpikeHealthUp)
        {
            if (this == field.spikeStore.activeSpikes.spikeLeft)
            {
                field.spikeStore.activeSpikes.spikeLeft = null;
            }
            else if (this == field.spikeStore.activeSpikes.spikeRight)
            {
                field.spikeStore.activeSpikes.spikeRight = null;
            }
            gameObject.SetActive(false);
        }
        else
        {
            field.spikeStore.StoreSpikes();
        }
    }
    public void ActivateSpike()
    {
        SetSpikeForStage();
        gameObject.SetActive(true);
        transform.position = new Vector3(sd == Side.Left ? -col.bounds.size.x : col.bounds.size.x, 0, stagePosZ);
        if (currentStage == Stage.Neon)
        {
            transform.rotation = Quaternion.Euler(new Vector3(transform.rotation.x, sd == Side.Left ? 90 : 270, this is SpikeDissolve ? 270 : 0));
        }
        StartSpike();
    }
    protected void PostMortem(bool proc = false)
    {
        if (proc)
        {
            am.PlayAudio(AudioType.SpikeProc, transform.position);
        }
        newGameManager.SpikeSpawn();
        KillSpike();
    }
}
