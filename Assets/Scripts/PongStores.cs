using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.Events;
using PongLocker;
using MeshLocker;
using SpikeLocker;

[System.Serializable]
public class FragmentStore
{
    public UnityEvent droppedAllCubeFragments = new UnityEvent();
    public UnityEvent droppedAllIcosahedronFragments = new UnityEvent();
    public UnityEvent droppedAllOctacontagonFragments = new UnityEvent();

    public List<Fragment> leftPadFragments;
    public List<Fragment> rightPadFragments;

    public List<Fragment> cubeFragments;
    public List<Fragment> icosahedronFreezeFragments;
    public List<Fragment> icosahedronBurnFragments;
    public List<Fragment> icosahedronFragments => icosahedronFreezeFragments.Concat(icosahedronBurnFragments).ToList();
    public List<Fragment> octacontagonFragments;

    public List<Fragment> allPadFragments => leftPadFragments.Concat(rightPadFragments).ToList();
    public List<Fragment> allBallFragments => cubeFragments.Concat(icosahedronFragments).Concat(octacontagonFragments).ToList();

    public int droppedCubeFragmentsIndex;
    public int droppedIcosahedronFragmentsIndex;
    public int droppedOctacontagonFragmentsIndex;

    public FragmentStore(List<Fragment> leftPadFragments, List<Fragment> rightPadFragments, List<Fragment> cubeFragments, List<Fragment> icosahedronFreezeFragments, List<Fragment> icosahedronBurnFragments, List<Fragment> octacontagonFragments)
    {
        this.leftPadFragments = leftPadFragments;
        this.rightPadFragments = rightPadFragments;
        this.cubeFragments = cubeFragments;
        this.icosahedronFreezeFragments = icosahedronFreezeFragments;
        this.icosahedronBurnFragments = icosahedronBurnFragments;
        this.octacontagonFragments = octacontagonFragments;
        droppedCubeFragmentsIndex = 0;
        droppedIcosahedronFragmentsIndex = 0;
        droppedOctacontagonFragmentsIndex = 0;
    }
    public void SetFragmentsForStage(Stage currentStage)
    {
        switch (currentStage)
        {
            case Stage.Universe:
                icosahedronFragments.ForEach(frg => frg.gameObject.SetActive(false));
                break;
            case Stage.DD:
            case Stage.DDD:
            case Stage.GravityWell:
            case Stage.FreeMove:
                cubeFragments.ForEach(frg => frg.gameObject.SetActive(false));
                icosahedronFragments.ForEach(frg => frg.gameObject.SetActive(false));
                break;
            case Stage.FireAndIce:
            case Stage.Neon:
            case Stage.Final:
                cubeFragments.ForEach(frg => frg.gameObject.SetActive(false));
                break;
        }
    }
    void ResetFragmentIndexForBall(BallMesh ballMesh)
    {
        switch (ballMesh)
        {
            case BallMesh.IcosahedronRough:
                droppedCubeFragmentsIndex = 0;
                break;
            case BallMesh.Octacontagon:
                droppedIcosahedronFragmentsIndex = 0;
                break;
            case BallMesh.Icosikaihenagon:
                droppedOctacontagonFragmentsIndex = 0;
                break;
        }
    }
    public List<Fragment> BallFragmentsForMesh(BallMesh ballMesh)
    {
        switch (ballMesh)
        {
            case BallMesh.IcosahedronRough:
                return cubeFragments;
            case BallMesh.Octacontagon:
                return icosahedronFragments;
            case BallMesh.Icosikaihenagon:
                return octacontagonFragments;
        }
        return new List<Fragment>();
    }
    public void ResetBallFragmentsMaterials()
    {
        for (int i = 0; i < allBallFragments.Count; i++)
        {
            allBallFragments[i].meshR.material.SetFloat("_SuctionRange", 0);
            allBallFragments[i].meshR.material.SetFloat("_SuctionThreshold", 0);
            allBallFragments[i].meshR.material.SetFloat("_DissolveProgress", 0);
            allBallFragments[i].meshR.material.SetFloat("_FrostAmmount", 0);
            allBallFragments[i].meshR.material.SetFloat("_DissolveEdgeDepth", 0);
            allBallFragments[i].meshR.material.SetFloat("_EmissionIntensity", 0);
        }
    }
    public bool NoMoreFragmentsForBall(BallMesh ballMesh)
    {
        switch (ballMesh)
        {
            case BallMesh.IcosahedronRough:
                return droppedCubeFragmentsIndex >= cubeFragments.Count;
            case BallMesh.Octacontagon:
                return droppedIcosahedronFragmentsIndex >= icosahedronFragments.Count;
            case BallMesh.Icosikaihenagon:
                return droppedOctacontagonFragmentsIndex >= octacontagonFragments.Count;

        }
        return true;
    }
    public void GatherBallFragments(BallMesh ballMesh, bool immediate = true)
    {
        BallFragmentsForMesh(ballMesh).ForEach(frg =>
        {
            frg.gameObject.SetActive(true);
            frg.col.isTrigger = false;
            RemoveFragmentRigidBody(frg, "Ball");
            frg.transform.SetParent(PongBehaviour.field.ball.transform);
            frg.transform.localScale = Vector3.one;
            if (immediate)
            {
                frg.transform.localPosition = Vector3.zero;
                frg.transform.localRotation = Quaternion.identity;
            }
            else
            {
                //CYCLERESETFRAGMENTPOSROT
            }
        });
        if (immediate)
        {
            ResetFragmentIndexForBall(ballMesh);
        }
    }
    public void GatherPadFragments(bool immediate = true)
    {
        leftPadFragments.ForEach(frg =>
        {
            frg.gameObject.SetActive(true);
            RemoveFragmentConstantForce(frg);
            RemoveFragmentRigidBody(frg, "Pad");
            frg.transform.SetParent(PongBehaviour.field.leftPad.transform);
            frg.transform.localScale = Vector3.one;
            if (immediate)
            {
                frg.transform.localPosition = Vector3.zero;
                frg.transform.localRotation = Quaternion.identity;
            }
            else
            {
                //CYCLERESETFRAGMENTPOSROT
            }
        });
        rightPadFragments.ForEach(frg =>
        {
            frg.gameObject.SetActive(true);
            RemoveFragmentConstantForce(frg);
            RemoveFragmentRigidBody(frg, "Pad");
            frg.transform.SetParent(PongBehaviour.field.rightPad.transform);
            frg.transform.localScale = Vector3.one;
            if (immediate)
            {
                frg.transform.localPosition = Vector3.zero;
                frg.transform.localRotation = Quaternion.identity;
            }
            else
            {
                //CYCLERESETFRAGMENTPOSROT
            }
        });
    }
    public void GatherDebuffFragments(bool immediate = true)
    {
        icosahedronBurnFragments.ForEach(frg =>
        {
            frg.gameObject.SetActive(true);
            frg.col.isTrigger = false;
            RemoveFragmentRigidBody(frg, "Debuff");
            frg.transform.SetParent(PongBehaviour.field.debuffStore.debuffBurn.transform);
            frg.transform.localScale = Vector3.one;
            if (immediate)
            {
                frg.transform.localPosition = Vector3.zero;
                frg.transform.localRotation = Quaternion.identity;
            }
            else
            {
                //CYCLERESETFRAGMENTPOSROT
            }
        });
        icosahedronFreezeFragments.ForEach(frg =>
        {
            frg.gameObject.SetActive(true);
            frg.col.isTrigger = false;
            RemoveFragmentRigidBody(frg, "Debuff");
            frg.transform.SetParent(PongBehaviour.field.debuffStore.debuffFreeze.transform);
            frg.transform.localScale = Vector3.one;
            if (immediate)
            {
                frg.transform.localPosition = Vector3.zero;
                frg.transform.localRotation = Quaternion.identity;
            }
            else
            {
                //CYCLERESETFRAGMENTPOSROT
            }
        });
        droppedIcosahedronFragmentsIndex = 20;
    }
    public void AddFragmentRigidBody(Fragment fragment)
    {
        if (fragment.rbd == null)
        {
            fragment.rbd = fragment.gameObject.AddComponent<Rigidbody>();
        }
        fragment.rbd.mass = 1;
        fragment.rbd.angularDrag = 0;
        fragment.rbd.drag = 0;
        if (fragment.col != null)
        {
            fragment.col.sharedMaterial = PongManager.builder.notSoBouncyMaterial;
            fragment.col.enabled = true;
        }
        fragment.gameObject.layer = LayerMask.NameToLayer("Fragment");
    }
    public void RemoveFragmentRigidBody(Fragment fragment, string layer)
    {
        if (fragment.rbd != null)
        {
            GameObject.Destroy(fragment.rbd);
        }
        fragment.rbd = null;
        if (fragment.col != null)
        {
            fragment.col.sharedMaterial = null;
            fragment.col.enabled = false;
        }
        fragment.gameObject.layer = LayerMask.NameToLayer(layer);
    }
    public void AddFragmentConstantForce(Fragment fragment, Vector3 force)
    {
        if (fragment.cf == null)
        {
            fragment.cf = fragment.gameObject.AddComponent<ConstantForce>();
        }
        fragment.cf.force = force;
    }
    public void RemoveFragmentConstantForce(Fragment fragment)
    {
        if (fragment.cf != null)
        {
            GameObject.Destroy(fragment.cf);
        }
    }
    public void DropPadFragments()
    {
        allPadFragments.ForEach(frg => DropFragment(frg));
    }
    public void DropBallFragments(BallMesh ballMesh)
    {
        switch (ballMesh)
        {
            case BallMesh.IcosahedronRough:
                if (droppedCubeFragmentsIndex < cubeFragments.Count)
                {
                    droppedCubeFragmentsIndex += 1;
                    DropFragment(cubeFragments[droppedCubeFragmentsIndex - 1]);
                    if (droppedCubeFragmentsIndex >= cubeFragments.Count) { droppedAllCubeFragments.Invoke(); }
                }
                break;
            case BallMesh.Octacontagon:
                if (droppedIcosahedronFragmentsIndex < icosahedronFragments.Count)
                {
                    droppedIcosahedronFragmentsIndex += 2;
                    DropFragment(icosahedronFragments[droppedIcosahedronFragmentsIndex - 2]);
                    DropFragment(icosahedronFragments[droppedIcosahedronFragmentsIndex - 1]);
                    if (droppedIcosahedronFragmentsIndex >= icosahedronFragments.Count) { droppedAllIcosahedronFragments.Invoke(); }
                }
                break;
            case BallMesh.Icosikaihenagon:
                if (droppedOctacontagonFragmentsIndex < octacontagonFragments.Count)
                {
                    DropFragment(octacontagonFragments[droppedOctacontagonFragmentsIndex - 1]);
                    if (droppedOctacontagonFragmentsIndex >= octacontagonFragments.Count) { droppedAllOctacontagonFragments.Invoke(); }
                }
                break;
        }
    }
    public void UnParentBallFragments(BallMesh ballMesh)
    {
        BallFragmentsForMesh(ballMesh).ForEach(frg => { if (frg.transform.parent == PongBehaviour.field.ball.transform) { frg.transform.SetParent(PongManager.fieldParent.transform); } });
    }
    public void DropFragment(Fragment fragment)
    {
        AddFragmentRigidBody(fragment);
        fragment.transform.SetParent(PongManager.fieldParent.transform);
        fragment.rbd.AddExplosionForce(20, fragment.transform.position, 0, 0, ForceMode.Acceleration);
    }
    public void HidePadFragments()
    {
        allPadFragments.ForEach(frg => { frg.transform.SetParent(PongManager.fieldParent.transform); frg.gameObject.SetActive(false); });
    }
}
[System.Serializable]
public struct DebuffStore
{
    public DebuffBurn debuffBurn;
    public DebuffFreeze debuffFreeze;
    public DebuffSlow debuffSlow;

    public List<DebuffEntity> allDebuffs => new List<DebuffEntity> { debuffBurn, debuffFreeze, debuffSlow };

    public void StoreDebuffs()
    {
        if (debuffSlow.gameObject.activeSelf)
        {
            debuffSlow.gameObject.SetActive(false);
            debuffSlow.transform.position = Vector3.zero;
            debuffSlow.transform.rotation = Quaternion.Euler(Vector3.zero);
        }
        debuffBurn.StopAllCoroutines();
        debuffFreeze.StopAllCoroutines();
        debuffBurn.gameObject.SetActive(false);
        debuffBurn.transform.position = debuffBurn.calculatedPath[0];
        debuffBurn.transform.rotation = Quaternion.Euler(Vector3.zero);
        debuffFreeze.gameObject.SetActive(false);
        debuffFreeze.transform.position = debuffFreeze.calculatedPath[0];
        debuffFreeze.transform.rotation = Quaternion.Euler(Vector3.zero);
        debuffBurn.exploded = false;
        debuffFreeze.exploded = false;
        debuffBurn.orbiting = true;
        debuffFreeze.orbiting = true;
        if (PongBehaviour.nextStage > Stage.FireAndIce)
        {
            PongBehaviour.field.fragmentStore.GatherDebuffFragments();
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