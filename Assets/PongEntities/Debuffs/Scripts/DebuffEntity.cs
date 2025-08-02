using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using DG.Tweening;
using Unity.VisualScripting;
public class DebuffEntity : PongEntity
{
    public bool readyForStage = false;
    protected bool exploded = false;
    public bool hasAllFragments => fragments.Count == fragmentCapacity;
    public List<DebuffFragment> fragments = new List<DebuffFragment>();
    public bool orbitWheIdle = true;
    public bool orbitWhenActive => !orbitWheIdle;
    public int fragmentCapacity;
    public OrbitPath activeOrbitPaths;
    protected float suctionRange;
    protected float suctionThreshold;
    public float delayBetweenFragments;
    public bool inverseIdleOrbit = false;
    public bool orbiting = true;
    [Range(6, 360)]
    public int segments;
    public bool withinSpeedLimits => rbd != null ? rbd.velocity.sqrMagnitude >= sqrDebuffMinSpeed && rbd.velocity.sqrMagnitude <= sqrDebuffMaxSpeed : true;
    public float debuffMaxSpeed => pm.speeds.entitySpeeds.debuffMaxLinearVelocity * speedModifier;
    public float debuffMinSpeed => pm.speeds.entitySpeeds.debuffMinLinearVelocity * speedModifier;
    public float sqrDebuffMaxSpeed => debuffMaxSpeed * debuffMaxSpeed;
    public float sqrDebuffMinSpeed => debuffMinSpeed * debuffMinSpeed;
    Vector3 clampedVelocity
    {
        get
        {
            if (rbd.velocity.sqrMagnitude > sqrDebuffMaxSpeed) { return rbd.velocity.normalized * debuffMaxSpeed; }
            else if (rbd.velocity.sqrMagnitude < sqrDebuffMinSpeed) { return rbd.velocity.normalized * debuffMinSpeed; }
            return rbd.velocity;
        }
    }
    protected Vector3 initialDebuffVelocity
    {
        get
        {
            int[] directions = { -1, 1 };
            float x = UnityEngine.Random.Range(1f, 2f) * directions[UnityEngine.Random.Range(0, 2)] * debuffMaxSpeed * 0.75f;
            float y = UnityEngine.Random.Range(1f, 2f) * directions[UnityEngine.Random.Range(0, 2)] * debuffMaxSpeed * 0.75f;
            float z = 0;

            return new Vector3(x, y, z);
        }
    }
    public Vector3[] idleOrbitPath
    {
        get
        {
            Vector3[] points = new Vector3[segments + 1];
            for (int i = 0; i < segments; i++)
            {
                float angle = (float)i / (float)segments * 360 * Mathf.Deg2Rad;
                float x = Mathf.Cos(angle) * PongManager.sizes.fieldHeight * 0.25f;
                float y = Mathf.Sin(angle) * PongManager.sizes.fieldHeight * 0.25f;
                points[i] = new Vector3(x, y, stagePosZ);
            }
            points[segments] = points[0];
            return !inverseIdleOrbit ? points : points[(segments / 2)..(segments + 1)].Concat(points[0..((segments / 2) + 1)]).ToArray();
        }
    }
    protected Vector3[] activeOrbitPath
    {
        get
        {
            Vector3[] points = new Vector3[segments];
            Vector3 point = Vector3.zero;
            switch (activeOrbitPaths)
            {
                default:
                case OrbitPath.Ellipse:
                    for (int i = 0; i < (segments - 1); i++)
                    {
                        float angle = (float)i / (float)(segments - 1) * 360 * Mathf.Deg2Rad;
                        point.x = Mathf.Sin(angle) * fieldBounds.x * 0.9f;
                        point.y = Mathf.Cos(angle) * fieldBounds.y * 0.9f;
                        point.z = stagePosZ + PongManager.sizes.ballDiameter * 2.5f;
                        points[i] = point;
                    }
                    points[segments - 1] = points[0];
                    return points;
                case OrbitPath.Gerono:
                    for (int i = 0; i < (segments - 1); i++)
                    {
                        float angle = (float)i / (float)(segments - 1) * 360 * Mathf.Deg2Rad;
                        point.x = Mathf.Cos(angle) * fieldBounds.x * 0.9f;
                        point.y = Mathf.Sin(2 * angle) * fieldBounds.y * 0.9f;
                        point.z = stagePosZ + PongManager.sizes.ballDiameter * 2.5f;
                        points[i] = point;
                    }
                    points[segments - 1] = points[0];
                    // SHIFT START BY 1/4 TO START CENTER SCREEN (CROSS SECTION OF THE FIGURE8 SHAPE OF THE PATH)
                    return points.ToList().GetRange(segments / 4, (int)(segments * 0.75f)).Concat(points.ToList().GetRange(0, segments / 4)).ToArray();
                case OrbitPath.Bernoulli:
                    for (int i = 0; i < (segments - 1); i++)
                    {
                        float angle = (float)i / (float)(segments - 1) * 360 * Mathf.Deg2Rad;
                        point.x = Mathf.Cos(angle) * fieldBounds.x * 0.9f;
                        point.y = Mathf.Sin(2 * angle) / 1.5f * fieldBounds.y * 0.9f;
                        point.z = stagePosZ + PongManager.sizes.ballDiameter * 2.5f;
                        points[i] = point;
                    }
                    points[segments - 1] = points[0];
                    // SHIFT START BY 1/4 TO START CENTER SCREEN (CROSS SECTION OF THE FIGURE8 SHAPE OF THE PATH)
                    return points.ToList().GetRange(segments / 4, (int)(segments * 0.75f)).Concat(points.ToList().GetRange(0, segments / 4)).ToArray();
            }
        }
    }
    protected override void OnEnable()
    {
        base.OnEnable();
        orbiting = true;
        transform.position = orbitWheIdle ? idleOrbitPath[0] : activeOrbitPath[0];
        if (col != null)
        {
            col.enabled = false;
        }
        if (orbitWheIdle)
        {
            IdleOrbit();
        }
    }
    protected override void FixedUpdate()
    {
        base.FixedUpdate();
        if (!orbitWhenActive && !orbiting && !magnetized && !withinSpeedLimits)
        {
            rbd.velocity = clampedVelocity;
        }
        if (Mathf.Abs(transform.position.x) > Mathf.Abs(fieldBounds.x) || Mathf.Abs(transform.position.y) > Mathf.Abs(fieldBounds.y))
        {
            transform.position = new Vector3(0, 0, stagePosZ);
        }
    }
    protected void AddFragment(Fragment fragment)
    {
        fragments.Add(new DebuffFragment(fragment, true));
    }
    protected void OnAllBallFragmentsDropped()
    {
        StartCoroutine(CycleGobbleFragments());
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
    public void IdleOrbit()
    {
        // DOTween.defaultTimeScaleIndependent = true;
        transform.position = idleOrbitPath[0];
        rbd.isKinematic = false;
        rbd.velocity = Vector3.zero;
        rbd.angularVelocity = Vector3.zero;
        rbd.isKinematic = true;
        transform.rotation = Quaternion.identity;
        transform.DOKill();
        transform.DOPath(idleOrbitPath, pm.gameEffects.debuffOrbitTime, PathType.Linear).SetEase(Ease.Linear).SetLoops(-1).Play();
        meshR.enabled = false;
    }
    public virtual void OnGobbledAllFragments()
    {

    }
    protected IEnumerator CycleGobbleFragments()
    {
        foreach (DebuffFragment debuffFragment in fragments)
        {
            if (debuffFragment.fragment != null)
            {
                if (debuffFragment.fragment.col != null)
                {
                    debuffFragment.fragment.col.enabled = false;
                }
                if (debuffFragment.fragment.rbd != null)
                {
                    GameObject.Destroy(debuffFragment.fragment.rbd);
                    debuffFragment.fragment.rbd = null;
                }
                debuffFragment.fragment.transform.SetParent(PongManager.fieldParent.transform, true);
                debuffFragment.fragment.gameObject.layer = LayerMask.NameToLayer("Fragment");
                StartCoroutine(CycleGobbleFragment(debuffFragment));
                yield return new WaitForSecondsRealtime(delayBetweenFragments);
            }
        }
        while (fragments.Any(frg => frg.orbiting))
        {
            yield return null;
        }
        if (field.ball.st == State.Live)
        {
            OnGobbledAllFragments();
        }
        if (fragments.All(frg => !frg.orbiting))
        {
            readyForStage = true;
        }

    }
    protected IEnumerator CycleGobbleFragment(DebuffFragment debuffFragment)
    {
        int direction = UnityEngine.Random.Range(0, 2) > 0 ? -1 : 1;
        float angle = 1 * direction;
        float radius = 10;
        float angleSpeed = 50;
        float radialSpeed = 1;
        float maxDistanceDelta = PongManager.sizes.fieldWidth / 90; // testing this is +/- the distance the ball moves in one fixed update
        debuffFragment.fragment.meshR.material.SetFloat("_SuctionRange", suctionRange);
        debuffFragment.fragment.meshR.material.SetFloat("_SuctionThreshold", suctionThreshold);
        debuffFragment.orbiting = true;
        if (debuffFragment.fragment.rbd != null)
        {
            GameObject.Destroy(debuffFragment.fragment.rbd);
            debuffFragment.fragment.rbd = null;
        }
        while (orbiting && (Vector3.Distance(transform.position, debuffFragment.fragment.transform.position) > 1f || Quaternion.Angle(transform.rotation, Quaternion.identity) > 0.2f))
        {
            angle += Time.unscaledDeltaTime * angleSpeed;
            radius -= Time.unscaledDeltaTime * radialSpeed;
            radius = Mathf.Max(0, radius);

            float x = transform.position.x + radius * Mathf.Cos(Mathf.Deg2Rad * angle);
            float z = transform.position.z + radius * Mathf.Sin(Mathf.Deg2Rad * angle);
            float y = transform.position.y + radius * Mathf.Cos(Mathf.Deg2Rad * angle);

            debuffFragment.fragment.transform.position = Vector3.MoveTowards(debuffFragment.fragment.transform.position, new Vector3(x, y, z), maxDistanceDelta);
            debuffFragment.fragment.transform.rotation = Quaternion.RotateTowards(debuffFragment.fragment.transform.rotation, Quaternion.identity, 0.1f);
            yield return null;
        }
        debuffFragment.fragment.transform.SetParent(transform);
        debuffFragment.fragment.transform.localPosition = Vector3.zero;
        debuffFragment.fragment.transform.rotation = Quaternion.identity;
        debuffFragment.orbiting = false;
        debuffFragment.fragment.gameObject.layer = LayerMask.NameToLayer("Debuff");
    }
    protected void OnDestroy()
    {
        StopAllCoroutines();
        foreach (DebuffFragment debuffFragment in fragments)
        {
            GameObject.Destroy(debuffFragment.fragment.gameObject);
        }
    }
    public virtual void DestroyAllFragments()
    {
        fragments.ForEach(frg => GameObject.Destroy(frg.fragment.gameObject));
        fragments.Clear();
    }
}
