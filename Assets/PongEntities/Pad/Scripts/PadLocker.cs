using System.Collections.Generic;
using UnityEngine;
using System.Linq;
public enum PadController
{
    GamePad,
    KeyBoard,
    Environment
}
public enum PlayerController
{
    Player,
    Environment
}
public enum ProjectileType
{
    Fire,
    Ice
}
public struct PadPowers
{
    public bool magnet;
    public bool projectiles;

    public PadPowers(bool magnet, bool projectiles)
    {
        this.magnet = magnet;
        this.projectiles = projectiles;
    }
}
[System.Serializable]
public class PadFragmentsGroup
{
    public PadFragmentsGroup(Fragment[] leftFragments, Fragment[] rightFragments)
    {
        this.leftFragments = leftFragments;
        this.rightFragments = rightFragments;
    }
    public bool empty => leftFragments.Length <= 0 && rightFragments.Length <= 0;
    public bool full => leftFragments.Length == 4 && rightFragments.Length == 4;
    public Fragment[] leftFragments;
    public Fragment[] rightFragments;
    public Fragment[] allFragments => leftFragments.Concat(rightFragments).ToArray();
    public void TransferFragments(Pad pad)
    {
        if (pad.sd == Side.Left)
        {
            pad.fragments = leftFragments.ToList();
            leftFragments = new Fragment[0];
        }
        else
        {
            pad.fragments = rightFragments.ToList();
            rightFragments = new Fragment[0];
        }
        foreach (Fragment fragment in pad.fragments)
        {
            ConstantForce cs = fragment.GetComponent<ConstantForce>();
            if (cs != null)
            {
                GameObject.Destroy(cs);
            }
            GameObject.Destroy(fragment.GetComponent<Rigidbody>());
            fragment.col.sharedMaterial = PongManager.builder.bouncyMaterial;
            fragment.transform.SetParent(pad.transform);
        }
        pad.fragmented = pad.fragments.Count > 0;
    }
    public void GatherFragments(Pad pad)
    {
        if (pad.sd == Side.Left)
        {
            leftFragments = leftFragments.Concat(pad.fragments).ToArray();
        }
        else
        {
            rightFragments = rightFragments.Concat(pad.fragments).ToArray();
        }
        foreach (Fragment fragment in pad.fragments)
        {
            fragment.rbd = fragment.gameObject.AddComponent<Rigidbody>();
            fragment.rbd.mass = 1;
            fragment.rbd.angularDrag = 0;
            fragment.rbd.drag = 0;
            fragment.col.sharedMaterial = null;
            fragment.transform.SetParent(PongManager.fieldParent.transform);
        }
        pad.fragments = new List<Fragment>();
        pad.fragmented = false;
    }
}