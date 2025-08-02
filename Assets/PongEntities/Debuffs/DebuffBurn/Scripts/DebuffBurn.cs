using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using Unity.VisualScripting;
using UnityEngine;

public class DebuffBurn : DebuffEntity
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
            return points[(segments / 2)..(segments + 1)].Concat(points[0..((segments / 2) + 1)]).ToArray();
        }
    }
    protected override void OnEnable()
    {
        base.OnEnable();
        field.fragmentStore.removedAllIcosahedronBurnFragments.RemoveAllListeners();
        field.fragmentStore.droppedIcosahedronBurnFragment.RemoveAllListeners();
        if (fragments.Count == 0 && currentStage == Stage.FireAndIce)
        {
            field.fragmentStore.removedAllIcosahedronBurnFragments.AddListener(() => OnAllBallFragmentsDropped());
            field.fragmentStore.droppedIcosahedronBurnFragment.AddListener(frg => AddFragment(frg));
        }
        else
        {
            fragments = builder.MakeFreshBurnFragments(this, false, 1.2f);
            readyForStage = true;
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
        // foreach (DebuffFragment debuffFragment in fragments)
        // {
        //     debuffFragment.fragment.col.enabled = true;
        // }
    }
    // void OnDisable()
    // {

    //     foreach (DebuffFragment debuffFragment in fragments)
    //     {
    //         GameObject.Destroy(debuffFragment.fragment.gameObject);
    //     }
    //     fragments.Clear();
    // }
    public void TriggerExplosion()
    {
        if (!exploded)
        {
            readyForStage = false;
            exploded = true;
            orbiting = true;
            foreach (DebuffFragment debuffFragment in fragments)
            {
                if (debuffFragment.fragment.rbd == null)
                {
                    debuffFragment.fragment.rbd = debuffFragment.fragment.AddComponent<Rigidbody>();
                    debuffFragment.fragment.rbd.mass = 1;
                    debuffFragment.fragment.rbd.angularDrag = 0;
                    debuffFragment.fragment.rbd.drag = 0;
                    debuffFragment.fragment.col.sharedMaterial = null;
                }
                if (debuffFragment.fragment.col != null)
                {
                    debuffFragment.fragment.col.enabled = true;
                }
                debuffFragment.fragment.rbd.AddExplosionForce(20, transform.position, 0, 0, ForceMode.Acceleration);
                debuffFragment.fragment.transform.SetParent(PongManager.fieldParent.transform, true);
                debuffFragment.fragment.gameObject.layer = LayerMask.NameToLayer("Fragment");
            }
            StartCoroutine("CyclePostExplosion");
            rbd.angularVelocity = Vector3.zero;
            rbd.velocity = Vector3.zero;
        }
    }
    public override void DestroyAllFragments()
    {
        field.fragmentStore.icosahedronBurnFragments.ForEach(frg => GameObject.Destroy(frg.gameObject));
        base.DestroyAllFragments();
    }
    IEnumerator CyclePostExplosion()
    {
        yield return new WaitForSeconds(2);
        exploded = false;
        IdleOrbit();
        StartCoroutine(CycleGobbleFragments());
    }
}