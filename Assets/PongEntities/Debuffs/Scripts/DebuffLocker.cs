using System.Collections.Generic;
using UnityEngine;

public enum DebuffType
{
    DebuffBurn,
    DebuffFreeze,
    DebuffSlow
}
[System.Serializable]
public class AllowedDebuffs
{
    public bool freeze = true;
    public bool burn = true;
    public bool slow = true;

    public bool GetAllowedDebuff(DebuffType debuffType)
    {
        switch (debuffType)
        {
            default:
            case DebuffType.DebuffBurn:
                return burn;
            case DebuffType.DebuffSlow:
                return slow;
            case DebuffType.DebuffFreeze:
                return freeze;

        }
    }
    public void SetAllowedDebuff(DebuffType debuffType, bool allowed)
    {
        switch (debuffType)
        {
            case DebuffType.DebuffBurn:
                burn = allowed;
                break;
            case DebuffType.DebuffSlow:
                slow = allowed;
                break;
            case DebuffType.DebuffFreeze:
                freeze = allowed;
                break;
        }
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
            debuffSlow.DestroyAllFragments();            
        }
        if (debuffBurn.gameObject.activeSelf)
        {
            debuffBurn.StopAllCoroutines();
            debuffFreeze.StopAllCoroutines();
            debuffBurn.gameObject.SetActive(false);
            debuffBurn.transform.position = debuffBurn.calculatedPath[0];
            debuffBurn.transform.rotation = Quaternion.Euler(Vector3.zero);
            debuffFreeze.gameObject.SetActive(false);
            debuffFreeze.transform.position = debuffFreeze.calculatedPath[0];
            debuffFreeze.transform.rotation = Quaternion.Euler(Vector3.zero);
            if (debuffBurn.fragments.Count + PongBehaviour.field.fragmentStore.icosahedronBurnFragments.Count == debuffBurn.fragmentCapacity)
            {
                foreach (Fragment fragment in PongBehaviour.field.fragmentStore.icosahedronBurnFragments)
                {
                    debuffBurn.fragments.Add(new DebuffFragment(fragment, false));
                }
                PongBehaviour.field.fragmentStore.icosahedronBurnFragments.Clear();
            }
            if (debuffBurn.hasAllFragments)
            {
                foreach (DebuffFragment debuffFragment in debuffBurn.fragments)
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
                        debuffFragment.fragment.transform.SetParent(debuffBurn.transform);
                        debuffFragment.fragment.gameObject.layer = LayerMask.NameToLayer("Debuff");
                        debuffFragment.fragment.transform.localPosition = Vector3.zero;
                        debuffFragment.fragment.transform.localRotation = Quaternion.identity;
                    }
                }
                debuffBurn.readyForStage = true;
            }
            else
            {
                debuffBurn.DestroyAllFragments();
                PongBehaviour.field.fragmentStore.DestroyBallFragmentsForMesh(BallMesh.Icosahedron);
                debuffBurn.readyForStage = false;
            }
        }
        if (debuffFreeze.gameObject.activeSelf)
        {
            if (debuffFreeze.fragments.Count + PongBehaviour.field.fragmentStore.icosahedronFreezeFragments.Count == debuffBurn.fragmentCapacity)
            {
                foreach (Fragment fragment in PongBehaviour.field.fragmentStore.icosahedronFreezeFragments)
                {
                    debuffFreeze.fragments.Add(new DebuffFragment(fragment, false));
                }
                PongBehaviour.field.fragmentStore.icosahedronFreezeFragments.Clear();
            }
            if (debuffFreeze.hasAllFragments)
            {
                foreach (DebuffFragment debuffFragment in debuffFreeze.fragments)
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
                        debuffFragment.fragment.transform.SetParent(debuffFreeze.transform);
                        debuffFragment.fragment.gameObject.layer = LayerMask.NameToLayer("Debuff");
                        debuffFragment.fragment.transform.localPosition = Vector3.zero;
                        debuffFragment.fragment.transform.localRotation = Quaternion.identity;
                    }
                }
                debuffFreeze.readyForStage = true;
            }
            else
            {
                debuffBurn.DestroyAllFragments();
                PongBehaviour.field.fragmentStore.DestroyBallFragmentsForMesh(BallMesh.Icosahedron);
                debuffFreeze.readyForStage = false;
            }
        }
    }
}
[System.Serializable]
public class DebuffFragment
{
    public Fragment fragment { get; private set; }
    public bool orbiting = false;
    public DebuffFragment(Fragment fragment, bool orbiting)
    {
        this.fragment = fragment;
        this.orbiting = orbiting;
    }
}