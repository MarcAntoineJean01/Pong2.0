using System;
using UnityEngine;
using System.Collections;
using UnityEngine.Events;

public class PongEntity : PongBehaviour
{
    [Serializable]
    public class Bounced : UnityEvent { }
    [SerializeField]
    protected Bounced m_Bounced = new Bounced();
    public Bounced bounced
    {
        get { return m_Bounced; }
        set { m_Bounced = value; }
    }
    public int id = 0;
    //last collision time
    public float lct;
    //last trigger time
    public float ltt;
    // collider
    public Collider col;
    // mesh filter
    public MeshFilter meshF;
    // mesh renderer
    public MeshRenderer meshR;
    public Rigidbody rbd;
    public bool frozen = false;
    public static Vector2 fieldBounds;
    public float speedModifier = 1;
    public bool magnetized = false;
    public bool slowed = false;
    protected virtual void OnEnable()
    {

    }
    protected virtual void Start()
    {
        id = GetInstanceID();
    }
    protected virtual void Update()
    {

    }
    protected virtual void FixedUpdate()
    {

    }
    protected virtual void OnTriggerEnter(Collider collider)
    {
        float radius = Vector3.Distance(transform.position, collider.transform.position);
        switch (collider.name)
        {
            case "Explosion":
                if (this is not Pad && this is not Edge && this is not DebuffSlow && this is not Block)
                {
                    rbd.velocity = Vector3.zero;
                    rbd.AddExplosionForce(pm.gameEffects.explosionStrength, collider.transform.position, radius, 0, ForceMode.VelocityChange);
                    break;
                }
                break;
            case "MagnetAttract":
                if (this is not Pad && this is not Edge && this is not DebuffSlow && this is not Block)
                {
                    rbd.velocity = Vector3.zero;
                    magnetized = true;
                }
                break;
            case "MagnetRepulse":
                if (this is not Pad && this is not Edge && this is not DebuffSlow && this is not Block)
                {
                    rbd.velocity = Vector3.zero;
                    magnetized = true;                    
                }
                break;
            case "DebuffSlow":
                // if (this is not DebuffSlow && this is not Edge && !slowed)
                if (this is Pad && !slowed)
                {
                    speedModifier = 0.3f;
                    rbd.maxAngularVelocity = rbd.maxAngularVelocity*speedModifier;
                    slowed = true;
                }
                break;
        }
    }
    protected virtual void OnTriggerStay(Collider collider)
    {
        float radius = Vector3.Distance(transform.position, collider.transform.position);
        switch (collider.name)
        {
            case "MagnetRepulse":
                if (this is not Pad && this is not Edge && magnetized)
                {
                    rbd.AddExplosionForce(pm.gameEffects.magnetStrength, collider.transform.position, radius, 0, ForceMode.Acceleration);
                }
                break;
            case "MagnetAttract":
                if (this is not Pad && this is not Edge && magnetized)
                {
                    rbd.AddExplosionForce(-pm.gameEffects.magnetStrength, collider.transform.position, radius, 0, ForceMode.Acceleration);
                }
                break;
        }
    }
    protected virtual void OnTriggerExit(Collider collider)
    {
        switch (collider.name)
        {
            case "MagnetAttract":
                if (this is not Pad && this is not Edge && magnetized)
                {
                    rbd.AddExplosionForce(-pm.gameEffects.magnetStrength, collider.transform.position, 0, 0, ForceMode.VelocityChange);
                    magnetized = false;
                }
                break;
            case "MagnetRepulse":
                if (this is not Pad && this is not Edge && magnetized)
                {
                    rbd.AddExplosionForce(pm.gameEffects.magnetStrength, collider.transform.position, 0, 0, ForceMode.VelocityChange);
                    magnetized = false;
                }
                break;
            case "DebuffSlow":
                // if (this is not DebuffSlow && this is not Edge)
                if (this is Pad || this is BallEntity)
                {
                    speedModifier = 1;
                    rbd.maxAngularVelocity = rbd.maxAngularVelocity*speedModifier;
                    slowed = false;
                }
                break;
        }
    }
    protected virtual void OnParticleCollision(GameObject other)
    {
        switch (other.name)
        {
            case "FireProjectile":
                ParticleSystem.Particle[] a = new ParticleSystem.Particle[1];
                other.GetComponent<ParticleSystem>().GetParticles(a);
                vfx.TriggerExplosion(a[0].position);
                other.GetComponent<ParticleSystem>().Clear();
                other.GetComponent<ParticleSystem>().Stop();
                break; 
        }
    }
    public void Frozen()
    {
        if (!frozen)
        {
            StartCoroutine("CycleFrozen");
        }
    }
    protected virtual IEnumerator CycleFrozen()
    {
        frozen = true;
        float t = 0f;
        while (t < pm.gameEffects.frostDuration)
        {
            t += Time.deltaTime;
            if (t > pm.gameEffects.frostDuration) {t = pm.gameEffects.frostDuration;}
            meshR.material.SetFloat("_FrostAmmount", Mathf.SmoothStep(meshR.material.GetFloat("_FrostAmmount"), 1, t / pm.gameEffects.frostDuration));
            yield return null;
        }
        meshR.material.SetFloat("_FrostAmmount", 1);
        yield return new WaitForSeconds(1);
        t = 0f;
        frozen = false;
        while (t < pm.gameEffects.frostDuration * 0.25f)
        {
            t += Time.deltaTime;
            if (t > pm.gameEffects.frostDuration * 0.25f) {t = pm.gameEffects.frostDuration * 0.25f;}
            meshR.material.SetFloat("_FrostAmmount", Mathf.SmoothStep(meshR.material.GetFloat("_FrostAmmount"), 0, t / (pm.gameEffects.frostDuration * 0.25f)));
            yield return null;
        }
        meshR.material.SetFloat("_FrostAmmount", 0);
    }
}
