using System.Collections.Generic;
using System.Linq;
using UnityEngine;
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
                this.fragmentStore.UnParentBallFragments(ball.ballType);
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
                PongBehaviour.newGameManager.ListenForPads();
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
                PongBehaviour.newGameManager.ListenForPads();
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