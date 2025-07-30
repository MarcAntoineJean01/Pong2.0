using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class DebuffFreeze : DebuffEntity
{
    float ballFragmentSuctionThreshold = 0.1f;
    float ballFragmentSuctionRange => PongManager.sizes.ballDiameter * transform.localScale.x;
    List<Fragment> orbitingFragments = new List<Fragment>();
    List<Fragment> caughtFragments = new List<Fragment>();
    public bool inTriggerZone => field.ball.transform.position.x < trigerZone.x && field.ball.transform.position.x > -trigerZone.x && field.ball.transform.position.y < trigerZone.y && field.ball.transform.position.y > -trigerZone.y;
    public Vector2 trigerZone => fieldBounds * 0.25f;
    public bool gotFragments = false;

    public Vector3[] calculatedPath
    {
        get
        {
            Vector3[] points = new Vector3[segments + 1];
            for (int i = 0; i < segments; i++)
            {
                float angle = (float)i / (float)segments * 360 * Mathf.Deg2Rad;
                float x = Mathf.Cos(angle) * pm.gameEffects.debuffOrbitingRadius;
                float y = Mathf.Sin(angle) * pm.gameEffects.debuffOrbitingRadius;
                points[i] = new Vector3(x, y, stagePosZ);
            }
            points[segments] = points[0];
            return points;
        }
    }
    public void Orbit()
    {
        col = !gotFragments ? freezeFragments[0].col : caughtFragments[0].col;
        DOTween.defaultTimeScaleIndependent = true;
        transform.position = calculatedPath[0];
        rbd.angularVelocity = Vector3.zero;
        transform.rotation = Quaternion.identity;
        transform.DOPath(calculatedPath, pm.gameEffects.debuffOrbitTime, PathType.Linear).SetEase(Ease.Linear).SetLoops(-1).Play();
        meshR.enabled = false;
        if (!gotFragments)
        {
            foreach (Fragment fragment in freezeFragments)
            {
                fragment.col.enabled = false;
                fragment.transform.SetParent(PongManager.fieldParent.transform, true);
                StartCoroutine(CycleGobbleFragments());
            }
        }
    }
    public void Release()
    {
        transform.DOKill();
        StopAllCoroutines();
        orbiting = false;
        transform.SetParent(PongManager.fieldParent.transform);
        transform.position = new Vector3(transform.position.x, transform.position.y, stagePosZ);
        rbd.AddForce(initialDebuffVelocity, ForceMode.VelocityChange);
        if (!gotFragments)
        {
            foreach (Fragment fragment in orbitingFragments)
            {
                fragment.transform.SetParent(transform);
                fragment.transform.localPosition = Vector3.zero;
                fragment.transform.rotation = Quaternion.identity;
                field.ball.fragments.Remove(fragment);
            }
            caughtFragments.AddRange(orbitingFragments);
            caughtFragments.AddRange(freezeFragments);
            orbitingFragments = new List<Fragment>();
            field.ball.fragmented = false;
            foreach (Fragment fragment in caughtFragments)
            {
                fragment.col.enabled = true;
                fragment.gameObject.layer = LayerMask.NameToLayer("Debuff");
                fragment.transform.SetParent(transform);
                fragment.transform.localPosition = Vector3.zero;
                fragment.transform.rotation = Quaternion.identity;
                field.ball.fragments.Remove(fragment);
            }
            gotFragments = true;
        }
    }
    IEnumerator CycleGobbleFragments()
    {
        foreach (Fragment fragment in freezeFragments)
        {
            if (fragment != null)
            {
                fragment.col.enabled = false;
                fragment.transform.SetParent(PongManager.fieldParent.transform, true);
                StartCoroutine(CycleGobbleFragment(fragment));
                yield return new WaitForSecondsRealtime(0.2f);                
            }
        }
    }
    IEnumerator CycleGobbleFragment(Fragment fragment)
    {
        int direction = UnityEngine.Random.Range(0, 2) > 0 ? -1 : 1;
        float angle = 1 * direction;
        float radius = 10;
        float angleSpeed = 50;
        float radialSpeed = 1;
        fragment.meshR.material.SetFloat("_SuctionRange", ballFragmentSuctionRange);
        fragment.meshR.material.SetFloat("_SuctionThreshold", ballFragmentSuctionThreshold);
        orbitingFragments.Add(fragment);
        while (orbiting && (Vector3.Distance(transform.position, fragment.transform.position) > ballFragmentSuctionThreshold || Quaternion.Angle(transform.rotation, Quaternion.identity) > 0.2f))
        {
            fragment.transform.LookAt(transform.position);

            angle += Time.unscaledDeltaTime * angleSpeed;
            radius -= Time.unscaledDeltaTime * radialSpeed;
            radius = Mathf.Max(0, radius);

            float x = transform.position.x + radius * Mathf.Cos(Mathf.Deg2Rad * angle);
            float z = transform.position.z + radius * Mathf.Sin(Mathf.Deg2Rad * angle);
            float y = transform.position.y + radius * Mathf.Cos(Mathf.Deg2Rad * angle);

            fragment.transform.position = Vector3.MoveTowards(fragment.transform.position, new Vector3(x, y, z), 0.1f * field.ball.transform.localScale.x);
            fragment.transform.rotation = Quaternion.RotateTowards(fragment.transform.rotation, Quaternion.identity, 0.1f);
            yield return null;
        }
        fragment.transform.SetParent(transform);
        fragment.transform.localPosition = Vector3.zero;
        fragment.transform.rotation = Quaternion.identity;
        field.ball.fragments.Remove(fragment);
        orbitingFragments.Remove(fragment);
        caughtFragments.Add(fragment);
    }
}