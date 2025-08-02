using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
public enum EdgeType
{
    Floor,
    Wall,
    Background
}
public struct Field
{
    public GameObject parent { get; private set; }
    public Floor topFloor { get; private set; }
    public Floor bottomFloor { get; private set; }
    public Wall leftWall { get; private set; }
    public Wall rightWall { get; private set; }
    public Background background { get; private set; }
    public List<Edge> edges => new List<Edge>()
    {
        this.leftWall,
        this.rightWall,
        this.topFloor,
        this.bottomFloor,
        this.background
    };
    public Pad leftPad { get; private set; }
    public Pad rightPad { get; private set; }
    public BallEntity ball { get; private set; }
    public SpikeStore spikeStore { get; private set; }
    public DebuffStore debuffStore { get; private set; }
    public FragmentStore fragmentStore { get; private set; }
    public Vector3 initialBackgroundPosition { get; private set; }
    public Vector3 initialLeftWallPosition { get; private set; }
    public Vector3 initialRightWallPosition { get; private set; }
    public Vector3 initialTopFloorPosition { get; private set; }
    public Vector3 initialBottomFloorPosition { get; private set; }
    public List<Block> blocks => new List<Block>() { leftPad.topBlock, leftPad.bottomBlock, rightPad.topBlock, rightPad.bottomBlock }.Where(b => b != null).ToList();
    public Field(GameObject parent, Floor topFloor, Floor bottomFloor, Wall leftWall, Wall rightWall, Background background, Pad leftPad, Pad rightPad, BallEntity ball, SpikeStore spikeStore, DebuffStore debuffStore, FragmentStore fragmentStore)
    {
        this.parent = parent;
        this.topFloor = topFloor;
        this.bottomFloor = bottomFloor;
        this.leftWall = leftWall;
        this.rightWall = rightWall;
        this.background = background;
        this.leftPad = leftPad;
        this.rightPad = rightPad;
        this.ball = ball;
        this.spikeStore = spikeStore;
        this.debuffStore = debuffStore;
        this.fragmentStore = fragmentStore;

        this.initialBackgroundPosition = background.transform.position;
        this.initialLeftWallPosition = leftWall.transform.position;
        this.initialRightWallPosition = rightWall.transform.position;
        this.initialTopFloorPosition = topFloor.transform.position;
        this.initialBottomFloorPosition = bottomFloor.transform.position;
    }
    public void ReplaceEntity(Entity ent, PongEntity newEnt)
    {
        switch (ent)
        {
            case Entity.Ball:
                newEnt.transform.position = this.ball.transform.position;
                newEnt.transform.rotation = this.ball.transform.rotation;
                newEnt.meshR.shadowCastingMode = this.ball.meshR.shadowCastingMode;
                (newEnt as BallEntity).SetBallState(this.ball.st);
                (newEnt as BallEntity).attracted = this.ball.attracted;
                (newEnt as BallEntity).speedOverTimeModifier = this.ball.speedOverTimeModifier;
                (newEnt as BallEntity).speedModifier = this.ball.speedModifier;
                (newEnt as BallEntity).slowed = this.ball.slowed;
                (newEnt as BallEntity).magnetized = this.ball.magnetized;
                (newEnt as BallEntity).frozen = this.ball.frozen;
                // Debug.LogWarning("FIX HERE" + this.ball.ballType);
                // Debug.LogWarning(fragmentStore.BallFragmentsForMesh(this.ball.ballType));
                // Debug.LogWarning(fragmentStore.BallFragmentsForMesh(this.ball.ballType).Any(frg => frg.transform.parent == PongBehaviour.field.ball.transform));
                //FIX THIS FIX THIS FIX THIS
                if (fragmentStore.BallFragmentsForMesh(this.ball.ballType).Any(frg => frg.transform.parent == PongBehaviour.field.ball.transform))
                {
                    fragmentStore.DestroyBallFragmentsForMesh(this.ball.ballType);
                }
                GameObject.Destroy(this.ball.gameObject);
                this.ball = newEnt as BallEntity;
                break;
            case Entity.LeftPad:
                newEnt.transform.position = this.leftPad.transform.position;
                newEnt.transform.rotation = this.leftPad.transform.rotation;
                (newEnt as Pad).topBlock = this.leftPad.topBlock;
                (newEnt as Pad).bottomBlock = this.leftPad.bottomBlock;
                (newEnt as Pad).cntrl = this.leftPad.cntrl;
                (newEnt as Pad).attractorCharges = this.leftPad.attractorCharges;
                (newEnt as Pad).repulsorCharges = this.leftPad.repulsorCharges;
                (newEnt as Pad).playerControlsEnabled = this.leftPad.playerControlsEnabled;
                newEnt.meshR.shadowCastingMode = this.leftPad.meshR.shadowCastingMode;
                GameObject.Destroy(this.leftPad.gameObject);
                this.leftPad = newEnt as Pad;
                PongBehaviour.newGameManager.ListenForPad(Side.Left);
                if (CameraManager.leftPadVCamEnd.gameObject.activeSelf)
                {
                    CameraManager.leftPadVCamEnd.Follow = this.leftPad.transform;
                    CameraManager.leftPadVCamEnd.LookAt = this.leftPad.transform;
                }
                break;
            case Entity.RightPad:
                newEnt.transform.position = this.rightPad.transform.position;
                newEnt.transform.rotation = this.rightPad.transform.rotation;
                (newEnt as Pad).topBlock = this.rightPad.topBlock;
                (newEnt as Pad).bottomBlock = this.rightPad.bottomBlock;
                (newEnt as Pad).cntrl = this.rightPad.cntrl;
                (newEnt as Pad).attractorCharges = this.rightPad.attractorCharges;
                (newEnt as Pad).repulsorCharges = this.rightPad.repulsorCharges;
                (newEnt as Pad).playerControlsEnabled = this.rightPad.playerControlsEnabled;
                newEnt.meshR.shadowCastingMode = this.rightPad.meshR.shadowCastingMode;
                GameObject.Destroy(this.rightPad.gameObject);
                this.rightPad = newEnt as Pad;
                PongBehaviour.newGameManager.ListenForPad(Side.Right);
                if (CameraManager.rightPadVCamEnd.gameObject.activeSelf)
                {
                    CameraManager.rightPadVCamEnd.Follow = this.rightPad.transform;
                    CameraManager.rightPadVCamEnd.LookAt = this.rightPad.transform;
                }
                break;
        }
    }
    public void DisablePadControls()
    {
        if (leftPad.playerControlsEnabled)
        {
            leftPad.playerControls.PadControls.Disable();
        }
        if (rightPad.playerControlsEnabled)
        {
            rightPad.playerControls.PadControls.Disable();            
        }
    }
    public void EnablePadControls()
    {
        if (leftPad.playerControlsEnabled)
        {
            leftPad.playerControls.PadControls.Enable();
        }
        if (rightPad.playerControlsEnabled)
        {
            rightPad.playerControls.PadControls.Enable();            
        }
    }

    public Vector3 InitialEdgePostion(Edge edge)
    {
        if (edge == background)
        {
            return initialBackgroundPosition;
        }
        else if (edge == leftWall)
        {
            return initialLeftWallPosition;
        }
        else if (edge == rightWall)
        {
            return initialRightWallPosition;
        }
        else if (edge == topFloor)
        {
            return initialTopFloorPosition;
        }
        else if (edge == bottomFloor)
        {
            return initialBottomFloorPosition;
        }   
        return Vector3.zero;
    }
}
[System.Serializable]
public class FragmentStore
{
    public UnityEvent<Fragment> droppedCubeFragment = new UnityEvent<Fragment>();
    public UnityEvent<Fragment> droppedIcosahedronBurnFragment = new UnityEvent<Fragment>();
    public UnityEvent<Fragment> droppedIcosahedronFreezeFragment = new UnityEvent<Fragment>();
    public UnityEvent<Fragment> droppedOctacontagonFragment = new UnityEvent<Fragment>();
    public UnityEvent removedAllCubeFragments = new UnityEvent();
    public UnityEvent removedAllIcosahedronBurnFragments = new UnityEvent();
    public UnityEvent removedAllIcosahedronFreezeFragments = new UnityEvent();
    public UnityEvent removedAllOctacontagonFragments = new UnityEvent();
    public List<Fragment> leftPadFragments = new List<Fragment>();
    public List<Fragment> rightPadFragments = new List<Fragment>();

    public bool leftPadFragmentsEmpty => leftPadFragments.Count == 0;
    public bool rightPadFragmentsEmpty => rightPadFragments.Count == 0;

    public List<Fragment> cubeFragments = new List<Fragment>();
    public List<Fragment> icosahedronFreezeFragments = new List<Fragment>();
    public List<Fragment> icosahedronBurnFragments = new List<Fragment>();
    public List<Fragment> octacontagonFragments = new List<Fragment>();

    public bool cubeFragmentsEmpty => cubeFragments.Count == 0;
    bool icosahedronFreezeFragmentsEmpty => icosahedronFreezeFragments.Count == 0;
    bool icosahedronBurnFragmentsEmpty => icosahedronBurnFragments.Count == 0;
    public bool icosahedronFragmentsEmpty => icosahedronFreezeFragmentsEmpty && icosahedronBurnFragmentsEmpty;
    bool octacontagonFragmentsEmpty => octacontagonFragments.Count == 0;
    public bool ballFragmentsEmpty => octacontagonFragmentsEmpty && icosahedronBurnFragmentsEmpty && icosahedronFreezeFragmentsEmpty && cubeFragmentsEmpty;

    public List<Fragment> allPadFragments => leftPadFragments.Concat(rightPadFragments).ToList();
    public List<Fragment> allBallFragments => cubeFragments.Concat(icosahedronFreezeFragments).Concat(icosahedronBurnFragments).Concat(octacontagonFragments).ToList();
    public List<Fragment> allFragments => allPadFragments.Concat(cubeFragments).Concat(icosahedronFreezeFragments).Concat(icosahedronBurnFragments).ToList();

    
    public List<Fragment> BallFragmentsForMesh(BallMesh ballMesh)
    {
        switch (ballMesh)
        {
            case BallMesh.Cube:
                return cubeFragments;
            case BallMesh.Icosahedron:
                return icosahedronFreezeFragments.Concat(icosahedronBurnFragments).ToList();
            case BallMesh.Octacontagon:
                return octacontagonFragments;
        }
        return new List<Fragment>();
    }
    public void DropBallFragmentsForMesh(BallMesh ballMesh)
    {
        switch (ballMesh)
        {
            case BallMesh.Cube:
                if (!cubeFragmentsEmpty)
                {
                    DropFragment(cubeFragments[0]);
                    droppedCubeFragment.Invoke(cubeFragments[0]);
                    cubeFragments.Remove(cubeFragments[0]);
                    if (cubeFragmentsEmpty) { removedAllCubeFragments.Invoke(); }
                }
                break;
            case BallMesh.Icosahedron:
                if (!icosahedronBurnFragmentsEmpty)
                {
                    DropFragment(icosahedronBurnFragments[0]);
                    droppedIcosahedronBurnFragment.Invoke(icosahedronBurnFragments[0]);
                    icosahedronBurnFragments.Remove(icosahedronBurnFragments[0]);
                    if (icosahedronBurnFragmentsEmpty) { removedAllIcosahedronBurnFragments.Invoke(); }               
                }
                if (!icosahedronFreezeFragmentsEmpty)
                {
                    DropFragment(icosahedronFreezeFragments[0]);
                    droppedIcosahedronFreezeFragment.Invoke(icosahedronFreezeFragments[0]);
                    icosahedronFreezeFragments.Remove(icosahedronFreezeFragments[0]);
                    if (icosahedronFreezeFragmentsEmpty) { removedAllIcosahedronFreezeFragments.Invoke(); }               
                }
                break;
            case BallMesh.Octacontagon:
                if (!octacontagonFragmentsEmpty)
                {
                    DropFragment(octacontagonFragments[0]);
                    droppedOctacontagonFragment.Invoke(octacontagonFragments[0]);
                    octacontagonFragments.Remove(octacontagonFragments[0]);
                    if (octacontagonFragmentsEmpty) { removedAllOctacontagonFragments.Invoke(); }                   
                }
                break;
        }
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
    public bool NoFragmentsForBall(BallMesh ballMesh)
    {
        switch (ballMesh)
        {
            case BallMesh.Cube:
                return cubeFragmentsEmpty;
            case BallMesh.Icosahedron:
                return icosahedronFragmentsEmpty;
            case BallMesh.Octacontagon:
                return octacontagonFragmentsEmpty;

        }
        return true;
    }
    public void DestroyBallFragmentsForMesh(BallMesh ballMesh)
    {
        switch (ballMesh)
        {
            case BallMesh.Cube:
                foreach (Fragment fragment in cubeFragments)
                {
                    GameObject.Destroy(fragment.gameObject);
                }
                cubeFragments.Clear();
                break;
            case BallMesh.Icosahedron:
                foreach (Fragment fragment in icosahedronFreezeFragments)
                {
                    GameObject.Destroy(fragment.gameObject);
                }
                foreach (Fragment fragment in icosahedronBurnFragments)
                {
                    GameObject.Destroy(fragment.gameObject);
                }
                icosahedronFreezeFragments.Clear();
                icosahedronBurnFragments.Clear();
                break;
            case BallMesh.Octacontagon:
                foreach (Fragment fragment in octacontagonFragments)
                {
                    GameObject.Destroy(fragment.gameObject);
                }
                octacontagonFragments.Clear();
                break;
        }
    }
    public void TransferBallFragments(BallMesh ballMesh)
    {
        List<Fragment> fragments = BallFragmentsForMesh(ballMesh);
        for (int i = 0; i < fragments.Count; i++)
        {
            Fragment frg = fragments[i];
            frg.col.isTrigger = false;
            frg.transform.SetParent(PongBehaviour.field.ball.transform);
        }
    }
    public void DropFragment(Fragment fragment)
    {
        fragment.rbd = fragment.rbd == null ? fragment.gameObject.AddComponent<Rigidbody>() : fragment.rbd;
        fragment.rbd.mass = 1;
        fragment.rbd.angularDrag = 0;
        fragment.rbd.drag = 0;
        fragment.col.sharedMaterial = null;
        fragment.transform.SetParent(PongManager.fieldParent.transform);
        fragment.gameObject.layer = LayerMask.NameToLayer("Fragment");
        fragment.rbd.AddExplosionForce(20, fragment.transform.position, 0, 0, ForceMode.Acceleration);
    }
    public void GatherBallFragments(BallMesh ballMesh)
    {
        switch (ballMesh)
        {
            case BallMesh.Cube:
                break;
            case BallMesh.Icosahedron:
                break;

        }
    }
    public void TransferPadFragments(Pad pad)
    {
        foreach (Fragment fragment in pad.sd == Side.Left ? leftPadFragments : rightPadFragments)
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
    }
    public void GatherPadFragments(Pad pad)
    {
        foreach (Fragment fragment in (pad.sd == Side.Left ? leftPadFragments : rightPadFragments).Where(frg => frg.transform.parent == pad.transform))
        {
            fragment.rbd = fragment.rbd == null ? fragment.gameObject.AddComponent<Rigidbody>() : fragment.rbd;
            fragment.rbd.mass = 1;
            fragment.rbd.angularDrag = 0;
            fragment.rbd.drag = 0;
            fragment.col.sharedMaterial = null;
            fragment.transform.SetParent(PongManager.fieldParent.transform);
            fragment.gameObject.layer = LayerMask.NameToLayer("Fragment");
            fragment.rbd.AddExplosionForce(20, fragment.transform.position, 0, 0, ForceMode.Acceleration);
        }
        // pad.fragmented = false;
    }
}