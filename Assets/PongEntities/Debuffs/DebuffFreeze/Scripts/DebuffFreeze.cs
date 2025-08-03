using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using Unity.VisualScripting;
using UnityEngine;

public class DebuffFreeze : DebuffEntity
{
    public Vector3[] calculatedPath
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
            return points;
        }
    }
    protected override void OnEnable()
    {
        base.OnEnable();
        field.fragmentStore.droppedAllIcosahedronFragments.RemoveListener(OnAllBallFragmentsDropped);
        if (field.fragmentStore.droppedIcosahedronFragmentsIndex == 0 && currentStage == Stage.FireAndIce)
        {
            field.fragmentStore.droppedAllIcosahedronFragments.AddListener(OnAllBallFragmentsDropped);
        }
    }
    public override void OnGobbledAllFragments()
    {
        EnterStage();
    }
    public void EnterStage()
    {
        transform.DOKill();
        StopAllCoroutines();
        orbiting = false;
        rbd.isKinematic = false;
        transform.position = new Vector3(transform.position.x, transform.position.y, stagePosZ);
        rbd.AddForce(initialDebuffVelocity, ForceMode.VelocityChange);
        col.enabled = true;
    }
    public void TriggerExplosion()
    {
        if (!exploded)
        {
            exploded = true;
            orbiting = true;
            fragmentListForDebuff.ForEach(frg => field.fragmentStore.DropFragment(frg));
            StartCoroutine("CyclePostExplosion");
            rbd.angularVelocity = Vector3.zero;
            rbd.velocity = Vector3.zero;
            rbd.isKinematic = true;
            col.enabled = false;
        }
    }
    IEnumerator CyclePostExplosion()
    {
        yield return new WaitForSeconds(2);
        exploded = false;
        IdleOrbit();
        StartCoroutine(CycleGobbleFragments());
    }
}