using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
public class DebuffEntity : PongEntity
{
    protected static List<Fragment> freezeFragments => field.ball.fragments.FindAll( frg => Int32.Parse(frg.name[frg.name.Length - 1].ToString())%2!=0 );   
    protected static List<Fragment> burnFragments => field.ball.fragments.FindAll( frg => Int32.Parse(frg.name[frg.name.Length - 1].ToString())%2==0 );
    public Side sd;
    public bool orbiting = true;
    [Range(6, 360)]
    public int segments;
    public bool withinSpeedLimits =>rbd != null ? rbd.velocity.sqrMagnitude >= sqrDebuffMinSpeed && rbd.velocity.sqrMagnitude <= sqrDebuffMaxSpeed : true;
    public float debuffMaxSpeed => pm.speeds.entitySpeeds.debuffMaxLinearVelocity * speedModifier;
    public float debuffMinSpeed => pm.speeds.entitySpeeds.debuffMinLinearVelocity * speedModifier;
    public float sqrDebuffMaxSpeed => debuffMaxSpeed*debuffMaxSpeed;
    public float sqrDebuffMinSpeed => debuffMinSpeed*debuffMinSpeed;
    Vector3 clampedVelocity
    {
        get
        {
            if(rbd.velocity.sqrMagnitude > sqrDebuffMaxSpeed) {return rbd.velocity.normalized * debuffMaxSpeed;}
            else if(rbd.velocity.sqrMagnitude < sqrDebuffMinSpeed) {return rbd.velocity.normalized * debuffMinSpeed;}
            return rbd.velocity;            
        }
    }
    protected Vector3 initialDebuffVelocity
    {
        get
        {
            int[] directions = {-1, 1};
            float x = UnityEngine.Random.Range(1f, 2f)*directions[UnityEngine.Random.Range(0,2)]*debuffMaxSpeed*0.75f;
            float y = UnityEngine.Random.Range(1f, 2f)*directions[UnityEngine.Random.Range(0,2)]*debuffMaxSpeed*0.75f;
            float z = 0;

            return new Vector3(x,y,z);            
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
    public virtual void StartDebuff()
    {
        rbd.AddForce(initialDebuffVelocity, ForceMode.VelocityChange);
    }
    public void SetDebuffForStage()
    {
        switch (currentStage)
        {
            case Stage.StartMenu:
            case Stage.DD:
                if (meshR != null)
                {
                    meshR.shadowCastingMode = ShadowCastingMode.ShadowsOnly;
                }
                if (rbd != null)
                {
                    rbd.constraints = RigidbodyConstraints.FreezePositionZ | RigidbodyConstraints.FreezeRotation;
                }
                break;
            case Stage.FreeMove:
                if (rbd != null)
                {
                    rbd.constraints = RigidbodyConstraints.None;
                }
                if (meshR != null)
                {
                    meshR.shadowCastingMode = ShadowCastingMode.On;
                }
                break;
            default:
                if (rbd != null)
                {
                    rbd.constraints = RigidbodyConstraints.FreezePositionZ;
                }
                if (meshR != null)
                {
                    meshR.shadowCastingMode = ShadowCastingMode.On;
                }
                break;
        }
    }
}
