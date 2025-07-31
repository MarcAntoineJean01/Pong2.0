using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.Events;
using Cinemachine;
public class NewStageManager : PongManager
{
    public GameObject freeMoveManagerPrefab;
    [Serializable]
    public class StageTimedOut : UnityEvent { }
    [SerializeField]
    protected StageTimedOut m_StageTimedOut = new StageTimedOut();
    public StageTimedOut stageTimedOut
    {
        get { return m_StageTimedOut; }
        set { m_StageTimedOut = value; }
    }
    public static bool transitioning = false;
    [SerializeField]
    public AnimationCurve resetXYCurve;
    [SerializeField]
    public AnimationCurve moveEntitiesCurve;
    public Vector3 TimeModeFieldPos(Edge edge)
    {
        switch (nextStage)
        {

            default:
            case Stage.DD:
                return new Vector3(edge.transform.position.x, edge.transform.position.y, field.InitialEdgePostion(edge).z);
            case Stage.DDD:
                return new Vector3(edge.transform.position.x, edge.transform.position.y, field.InitialEdgePostion(edge).z + sizes.ballDiameter * 2);
            case Stage.Universe:
                return new Vector3(edge.transform.position.x, edge.transform.position.y, field.InitialEdgePostion(edge).z + sizes.fieldDepth * 0.20f - sizes.ballDiameter * 2);
            case Stage.FreeMove:
            case Stage.GravityWell:
                return new Vector3(edge.transform.position.x, edge.transform.position.y, field.InitialEdgePostion(edge).z + sizes.fieldDepth * 0.40f - sizes.ballDiameter * 2);
            case Stage.FireAndIce:
                return new Vector3(edge.transform.position.x, edge.transform.position.y, field.InitialEdgePostion(edge).z + sizes.fieldDepth * 0.60f - sizes.ballDiameter * 2);
            case Stage.Neon:
                return new Vector3(edge.transform.position.x, edge.transform.position.y, field.InitialEdgePostion(edge).z + sizes.fieldDepth * 0.80f - sizes.ballDiameter * 2);
            case Stage.Final:
                return new Vector3(edge.transform.position.x, edge.transform.position.y, field.InitialEdgePostion(edge).z + sizes.fieldDepth - sizes.ballDiameter * 2);
        }
    }
    public bool reachedGoalForStage
    {
        get
        {
            if (newGameManager.debugNeverReachGoal)
            {
                return false;
            }
            else
            {
                switch (currentStage)
                {
                    case Stage.DD:
                        return PongManager.remainingGoalsThresholdForStage <= 0 || (!field.leftPad.CanAddPiece() && !field.rightPad.CanAddPiece());
                    case Stage.DDD:
                        return PongManager.remainingGoalsThresholdForStage <= 0 || (!field.leftPad.CanAddBlock(Side.Top) && !field.leftPad.CanAddBlock(Side.Bottom) && !field.rightPad.CanAddBlock(Side.Top) && !field.rightPad.CanAddBlock(Side.Bottom));
                    default:
                        return PongManager.remainingGoalsThresholdForStage <= 0;
                }
            }
        }
    }
    void OnEnable()
    {
        stageTimedOut.AddListener(() =>
        {
            TerminateStage();
        });
    }
    public void SetStage(bool immediate = true)
    {
        SetEntitiesForStage();
        if (pve.pveActive)
        {
            pve.SetVirutalFieldForStage();
        }
        if (immediate)
            {
                field.leftPad.transform.position = new Vector3(field.leftPad.transform.position.x, 0, stagePosZ);
                field.rightPad.transform.position = new Vector3(field.rightPad.transform.position.x, 0, stagePosZ);
                field.ball.transform.position = new Vector3(0, 0, stagePosZ);
            }
            else
            {
                StopCoroutine("CyclePositionEntitiess");
                StartCoroutine("CyclePositionEntitiess");
            }
    }
    public void SetEntitiesForStage()
    {
        ReplaceEntitiesForStage();
        foreach (SpikeEntity spike in field.spikeStore.allSpikes)
        {
            spike.SetSpikeForStage();
        }
        foreach (DebuffEntity debuff in field.debuffStore.allDebuffs)
        {
            debuff.SetDebuffForStage();
        }
        if (currentStage == Stage.FreeMove)
        {
            if (fmfm != null)
            {
                GameObject.Destroy(fmfm.gameObject);
            }
            fmfm = GameObject.Instantiate(freeMoveManagerPrefab, fieldParent.transform).GetComponent<FreeMoveFieldManager>();
            fmfm.gameObject.SetActive(true);
        }
    }
    public void StartStage()
    {
        previousStage = currentStage;
        currentStage = nextStage;
        if (currentStage != Stage.Final)
        {
            nextStage = currentStage + 1;
        }
        else
        {
            nextStage = Stage.Final;
        }

        SetStage(mainSettings.gameMode == GameMode.Time);
        if (mainSettings.cutScenesOn)
        {
            csm.PlayScene(CutScene.StageIntro);
        }
        else if (currentStage == Stage.Universe)
        {
            StartCoroutine("CycleSetupUniverse");
        }
    }

    public void TerminateStage()
    {
        ResetStage();
        if (fmfm != null)
        {
            GameObject.Destroy(fmfm.gameObject);
            fmfm = null;
        }
        if (mainSettings.cutScenesOn)
        {
            csm.PlayScene(CutScene.StageOutro);
        }
        else
        {
            if (currentStage == Stage.Universe)
            {
                field.debuffStore.StoreDebuffs(true);
            }
            csm.TakeControl();
            csm.SimpleStageTransitionScene();
        }
    }
    public void SelectNewStage(int currentSelectedStage)
    {
        previousStage = currentSelectedStage - 2 >= 0 ? (Stage)(currentSelectedStage - 2) : 0;
        currentStage = (Stage)(currentSelectedStage - 1);
        nextStage = (Stage)currentSelectedStage;
        if (mainSettings.gameMode == GameMode.Time)
        {
            SetStage();
            newGameManager.KillFieldDoMoveZ();
            field.background.transform.position = TimeModeFieldPos(field.background);

            field.leftWall.transform.position = TimeModeFieldPos(field.leftWall);
            field.rightWall.transform.position = TimeModeFieldPos(field.rightWall);

            field.topFloor.transform.position = TimeModeFieldPos(field.topFloor);
            field.bottomFloor.transform.position = TimeModeFieldPos(field.bottomFloor);
        }
        else
        {
            SetStage();
        }
        TerminateStage();
    }
    public void ResetStage()
    {
        vfx.KillAllLiveEffects();
        field.spikeStore.StoreSpikes();
        field.debuffStore.StoreDebuffs();
        field.leftPad.StopAllPadCoroutines();
        field.rightPad.StopAllPadCoroutines();
        field.ball.SetBallState(State.Idle);
        CinemachineBasicMultiChannelPerlin noise = CameraManager.activeVCam.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();
        noise.m_AmplitudeGain = 0;
        noise.m_FrequencyGain = 0;
        foreach (Fragment fragment in fallenPadFragments.allFragments)
        {
            ConstantForce cs = fragment.GetComponent<ConstantForce>();
            if (cs != null)
            {
                GameObject.Destroy(cs);
            }
        }
    }
    public void CheckStage(Side loserSide)
    {
        lastToLoseHealth = loserSide;
        if (mainSettings.gameMode == GameMode.Goals && reachedGoalForStage)
        {
            newGameManager.roundStopped.Invoke();
            TerminateStage();
        }
    }
    public void ReplaceEntitiesForStage()
    {
        BallEntity newBall;
        switch (currentStage)
        {
            case Stage.DD:
                newBall = builder.MakeFullBall(BallMesh.Cube);
                field.ReplaceEntity(Entity.Ball, newBall);
                break;
            case Stage.DDD:
                newBall = builder.MakeFullBall(BallMesh.Cube);
                field.ReplaceEntity(Entity.Ball, newBall);
                break;
            case Stage.Universe:
                newBall = builder.MakeFragmentedBall(BallMesh.Cube);
                field.ReplaceEntity(Entity.Ball, newBall);
                break;
            case Stage.GravityWell:
                newBall = builder.MakeFullBall(BallMesh.Icosahedron, 1.2f);
                field.ReplaceEntity(Entity.Ball, newBall);
                break;
            case Stage.FreeMove:
                newBall = builder.MakeFullBall(BallMesh.Icosahedron, 1.2f);
                field.ReplaceEntity(Entity.Ball, newBall);
                break;
            case Stage.FireAndIce:
                if (!field.debuffStore.debuffFreeze.gotFragments || !field.debuffStore.debuffBurn.gotFragments)
                {
                    newBall = builder.MakeFragmentedBall(BallMesh.Icosahedron, 1.2f);
                    newBall.SetBallForStage();
                    field.ReplaceEntity(Entity.Ball, newBall);
                }
                field.debuffStore.debuffBurn.gameObject.SetActive(true);
                field.debuffStore.debuffFreeze.gameObject.SetActive(true);
                field.debuffStore.debuffFreeze.Orbit();
                field.debuffStore.debuffBurn.Orbit();
                field.ball.ballType = BallMesh.Octacontagon;
                field.ball.fragmented = false;
                break;
            case Stage.Neon:
                newBall = builder.MakeFullBall(BallMesh.Octacontagon, 1.2f);
                field.ReplaceEntity(Entity.Ball, newBall);
                break;
            case Stage.Final:
                newBall = builder.MakeFullBall(BallMesh.Octacontagon, 1.2f);
                field.ReplaceEntity(Entity.Ball, newBall);
                break;

        }
        field.ball.SetBallForStage();
        field.leftPad.SetPadForStage();
        field.rightPad.SetPadForStage();
    }
    IEnumerator CyclePositionEntitiess()
    {
        transitioning = true;
        float t = 0f;
        float transitionTime = pm.speeds.transitionSpeeds.roundResetTransitionSpeed / 2;
        field.ball.dissolved = true;
        float leftPadInitialPosY = field.leftPad.transform.position.y;
        float rightPadInitialPosY = field.rightPad.transform.position.y;
        while (t < transitionTime)
        {
            t += Time.deltaTime;
            if (t > transitionTime) { t = transitionTime; }
            field.leftPad.transform.position = new Vector3(field.leftPad.transform.position.x, Mathf.Lerp(leftPadInitialPosY, 0, t / transitionTime), stagePosZ);
            field.rightPad.transform.position = new Vector3(field.rightPad.transform.position.x, Mathf.Lerp(rightPadInitialPosY, 0, t / transitionTime), stagePosZ);
            yield return null;
        }
        field.leftPad.transform.position = new Vector3(field.leftPad.transform.position.x, 0, stagePosZ);
        field.rightPad.transform.position = new Vector3(field.rightPad.transform.position.x, 0, stagePosZ);
        field.ball.transform.position = new Vector3(0, 0, stagePosZ);
        transitioning = false;
    }

    IEnumerator CycleSetupUniverse()
    {
        transitioning = true;
        float t = 0f;   
        List<Edge> edges = new List<Edge>()
        {
            field.leftWall,
            field.rightWall,
            field.topFloor,
            field.bottomFloor,
            field.background
        };
        edges.ForEach(edge => {edge.meshR.material.SetFloat("_EmissionIntensity", 1); edge.meshR.material.SetFloat("_DissolveEdgeDepth", 0.01f); });
        while (t < pm.gameEffects.wallDissolveSpeed)
        {
            t += Time.unscaledDeltaTime;
            if (t > pm.gameEffects.wallDissolveSpeed) { t = pm.gameEffects.wallDissolveSpeed; }
            edges.ForEach(edge => edge.meshR.material.SetFloat("_DissolveProgress", Mathf.SmoothStep(0, 1, t / pm.gameEffects.wallDissolveSpeed)));
            yield return null;
        }
        field.debuffStore.debuffSlow.gameObject.SetActive(true);
        field.debuffStore.debuffSlow.StartBlackhole();
        transitioning = false;
    }

}
