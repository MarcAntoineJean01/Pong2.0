using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.Events;
using Cinemachine;
using System.Linq;
using DG.Tweening;
using PongLocker;
using MeshLocker;
using AudioLocker;
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
            TerminateStage(currentStage == Stage.DD, nextStage == Stage.DD);
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
            if (mainSettings.gameMode == GameMode.NonStop)
            {
                field.leftPad.transform.position = new Vector3(field.leftPad.transform.position.x, field.leftPad.transform.position.y, stagePosZ);
                field.rightPad.transform.position = new Vector3(field.rightPad.transform.position.x, field.rightPad.transform.position.y, stagePosZ);
                field.ball.transform.position = new Vector3(field.ball.transform.position.x, field.ball.transform.position.y, stagePosZ);
            }
            else
            {
                field.leftPad.transform.position = new Vector3(field.leftPad.transform.position.x, 0, stagePosZ);
                field.rightPad.transform.position = new Vector3(field.rightPad.transform.position.x, 0, stagePosZ);
                field.ball.transform.position = new Vector3(0, 0, stagePosZ);                
            }

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
        if (currentStage == Stage.FireAndIce)
        {
            field.debuffStore.debuffBurn.gameObject.SetActive(true);
            field.debuffStore.debuffFreeze.gameObject.SetActive(true);
        }
        field.fragmentStore.SetFragmentsForStage(currentStage);
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

        SetStage(mainSettings.gameMode != GameMode.Goals);
        if (mainSettings.cutScenesOn)
        {
            csm.PlayScene(CutScene.StageIntro);
        }
        else if (currentStage == Stage.Universe)
        {
            StartCoroutine("CycleSetupUniverse");
        }
        if (currentStage >= Stage.FireAndIce)
        {
            field.debuffStore.debuffBurn.gameObject.SetActive(true);
            field.debuffStore.debuffFreeze.gameObject.SetActive(true);
            field.debuffStore.debuffBurn.orbiting = true;
            field.debuffStore.debuffFreeze.orbiting = true;
            field.debuffStore.debuffBurn.IdleOrbit();
            field.debuffStore.debuffFreeze.IdleOrbit();

        }
    }

    public void TerminateStage(bool lerpGhostsFromWall = false, bool lerpGhostsToWall = false, bool nonStopTransition = true)
    {
        field.debuffStore.StoreDebuffs();
        field.fragmentStore.allPadFragments.ForEach(frg => field.fragmentStore.RemoveFragmentConstantForce(frg));
        if (fmfm != null)
        {
            GameObject.Destroy(fmfm.gameObject);
            fmfm = null;
        }
        if (mainSettings.gameMode == GameMode.NonStop && nonStopTransition)
        {
            StartCoroutine(CycleImmediateStageTransition());
        }
        else
        {
            vfx.KillAllLiveEffects();
            field.spikeStore.StoreSpikes();
            field.leftPad.StopAllPadCoroutines();
            field.rightPad.StopAllPadCoroutines();
            field.ball.SetBallState(State.Idle);
            CinemachineBasicMultiChannelPerlin noise = CameraManager.activeVCam.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();
            noise.m_AmplitudeGain = 0;
            noise.m_FrequencyGain = 0;
            field.fragmentStore.allPadFragments.ForEach(frg => field.fragmentStore.RemoveFragmentConstantForce(frg));
            if (mainSettings.cutScenesOn)
            {
                csm.PlayScene(CutScene.StageOutro);
            }
            else
            {
                csm.TakeControl();
                csm.SimpleStageTransitionScene(lerpGhostsFromWall, lerpGhostsToWall);
            }
        }
    }
    public void SelectNewStage(int currentSelectedStage)
    {
        bool lerpGhostsFromWall = currentStage == Stage.DD;
        previousStage = currentSelectedStage - 2 >= 0 ? (Stage)(currentSelectedStage - 2) : 0;
        currentStage = (Stage)(currentSelectedStage - 1);
        nextStage = (Stage)currentSelectedStage;
        bool lerpGhostsToWall = nextStage == Stage.DD;
        newGameManager.StopListenForPads();
        if (mainSettings.gameMode == GameMode.Time || mainSettings.gameMode == GameMode.NonStop)
        {
            newGameManager.KillFieldDoMoveZ();
            field.background.transform.position = TimeModeFieldPos(field.background);
            field.leftWall.transform.position = TimeModeFieldPos(field.leftWall);
            field.rightWall.transform.position = TimeModeFieldPos(field.rightWall);
            field.topFloor.transform.position = TimeModeFieldPos(field.topFloor);
            field.bottomFloor.transform.position = TimeModeFieldPos(field.bottomFloor);
        }
        TerminateStage(lerpGhostsFromWall, lerpGhostsToWall, false);
    }
    public void ResetStage()
    {
        SelectNewStage((int)currentStage);
    }
    public void CheckStage(Side loserSide)
    {
        lastToLoseHealth = loserSide;
        if (mainSettings.gameMode == GameMode.Goals && reachedGoalForStage)
        {
            bool lerpGhostsFromWall = currentStage == Stage.DD;
            bool lerpGhostsToWall = nextStage == Stage.DD;
            newGameManager.roundStopped.Invoke();
            TerminateStage(lerpGhostsFromWall, lerpGhostsToWall);
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
                newBall = builder.MakeFullBall(BallMesh.IcosahedronRough);
                field.ReplaceEntity(Entity.Ball, newBall);
                field.fragmentStore.GatherBallFragments(newBall.ballType);
                break;
            case Stage.GravityWell:
                newBall = builder.MakeFullBall(BallMesh.Icosahedron);
                field.ReplaceEntity(Entity.Ball, newBall);
                break;
            case Stage.FreeMove:
                newBall = builder.MakeFullBall(BallMesh.Icosahedron);
                field.ReplaceEntity(Entity.Ball, newBall);
                break;
            case Stage.FireAndIce:
                newBall = builder.MakeFullBall(BallMesh.Octacontagon);
                field.ReplaceEntity(Entity.Ball, newBall);
                field.fragmentStore.GatherBallFragments(newBall.ballType);
                break;
            case Stage.Neon:
                newBall = builder.MakeFullBall(BallMesh.Octacontagon);
                field.ReplaceEntity(Entity.Ball, newBall);
                break;
            case Stage.Final:
                newBall = builder.MakeFullBall(BallMesh.Octacontagon);
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
        edges.ForEach(edge => { edge.meshR.material.SetFloat("_EmissionIntensity", 1); edge.meshR.material.SetFloat("_DissolveEdgeDepth", 0.01f); });
        while (t < pm.gameEffects.wallDissolveSpeed)
        {
            t += Time.unscaledDeltaTime;
            if (t > pm.gameEffects.wallDissolveSpeed) { t = pm.gameEffects.wallDissolveSpeed; }
            edges.ForEach(edge => edge.meshR.material.SetFloat("_DissolveProgress", Mathf.SmoothStep(0, 1, t / pm.gameEffects.wallDissolveSpeed)));
            yield return null;
        }
        field.debuffStore.debuffSlow.gameObject.SetActive(true);
        transitioning = false;
    }

    IEnumerator CycleImmediateStageTransition()
    {
        bool materializeEdges = field.edges.Any(edge => Mathf.Approximately(edge.meshR.material.GetFloat("_DissolveProgress"), 1));
        Vector3 initialLeftPadPos = field.leftPad.transform.position;
        Vector3 initialRightPadPos = field.rightPad.transform.position;
        Vector3 initialBallPos = field.ball.transform.position;
        float initialBlockScale = field.blocks.Count > 0 ? field.blocks[0].transform.localScale.x : 0;
        float t = 0f;
        if (nextStage == Stage.Neon || CameraManager.leftPadVCamEnd.gameObject.activeSelf || CameraManager.rightPadVCamEnd.gameObject.activeSelf)
        {
            cm.MoveToNextCamera();
        }
        if (nextStage == Stage.Neon || nextStage == Stage.Final)
        {
            am.PlayMusic(MusicType.BackgroundMusic);
        }
        while (t < pm.speeds.transitionSpeeds.entitiesTransitionSpeed)
        {
            t += Time.unscaledDeltaTime;
            if (t > pm.speeds.transitionSpeeds.entitiesTransitionSpeed) { t = pm.speeds.transitionSpeeds.entitiesTransitionSpeed; }
            if (materializeEdges)
            {
                field.edges.ForEach(edge => edge.meshR.material.SetFloat("_DissolveProgress", Mathf.SmoothStep(1, 0, t / pm.speeds.transitionSpeeds.entitiesTransitionSpeed)));
            }
            Pad.ScalePadBlocks(t, initialBlockScale, initialLeftPadPos, initialRightPadPos);
            field.leftPad.transform.position = new Vector3(field.leftPad.transform.position.x, field.leftPad.transform.position.y, Mathf.Lerp(initialLeftPadPos.z, stagePosZ, t / pm.speeds.transitionSpeeds.entitiesTransitionSpeed));
            field.rightPad.transform.position = new Vector3(field.rightPad.transform.position.x, field.rightPad.transform.position.y, Mathf.Lerp(initialRightPadPos.z, stagePosZ, t / pm.speeds.transitionSpeeds.entitiesTransitionSpeed));
            field.ball.transform.position = new Vector3(field.ball.transform.position.x, field.ball.transform.position.y, Mathf.Lerp(initialBallPos.z, stagePosZ, t / pm.speeds.transitionSpeeds.entitiesTransitionSpeed));
            yield return null;
        }
        if (currentStage == Stage.DD)
        {
            vfx.DestroyIntersectionGhosts(true);
        }
        if (nextStage == Stage.FreeMove)
        {
            if (field.leftPad.energyShield == null || field.rightPad.energyShield == null)
            {
                if (field.leftPad.padType != PadType.Slick)
                {
                    Pad newLeftpad = builder.MakePad(PadType.Slick, Side.Left);
                    field.ReplaceEntity(Entity.LeftPad, newLeftpad);
                }
                if (field.rightPad.padType != PadType.Slick)
                {
                    Pad newRightPad = builder.MakePad(PadType.Slick, Side.Right);
                    field.ReplaceEntity(Entity.RightPad, newRightPad);
                }
                field.fragmentStore.GatherPadFragments();
                vfx.SetFreeMovePad(Side.Left);
                vfx.SetFreeMovePad(Side.Right);
            }
            yield return null;
            while (field.fragmentStore.allPadFragments.Any(frg => DOTween.IsTweening(frg.transform))) { yield return null; }
            if (field.leftPad.energyShield == null)
            {
                builder.MakeEnergyShield(field.leftPad);
            }
            if (field.rightPad.energyShield == null)
            {
                builder.MakeEnergyShield(field.rightPad);
            }
            t = 0f;
            while (t < 1)
            {
                t += Time.unscaledDeltaTime;
                if (t > 1) { t = 1; }
                field.leftPad.energyShield.GetComponent<Fragment>().meshR.material.SetFloat("_Alpha", Mathf.Lerp(0, 0.7f, t / 1));
                field.rightPad.energyShield.GetComponent<Fragment>().meshR.material.SetFloat("_Alpha", Mathf.Lerp(0, 0.7f, t / 1));
                yield return null;
            }
        }
        else // nextStage != Stage.FreeMove
        {
            if (field.leftPad.energyShield != null) { GameObject.Destroy(field.leftPad.energyShield); }
            if (field.rightPad.energyShield != null) { GameObject.Destroy(field.rightPad.energyShield); }
        }
        if (nextStage < Stage.FreeMove)
        {
            if (field.leftPad.padType == PadType.Slick || field.rightPad.padType == PadType.Slick)
            {
                vfx.RebuildPads();
            }
            yield return null;
            while (field.fragmentStore.allPadFragments.Any(frg => DOTween.IsTweening(frg.transform))) { yield return null; }
            if (field.leftPad.padType == PadType.Slick)
            {
                Pad newLeftpad = builder.MakePad(PadType.Rough, Side.Left);
                field.ReplaceEntity(Entity.LeftPad, newLeftpad);
            }
            if (field.rightPad.padType == PadType.Slick)
            {
                Pad newRightPad = builder.MakePad(PadType.Rough, Side.Right);
                field.ReplaceEntity(Entity.RightPad, newRightPad);
            }
            field.fragmentStore.HidePadFragments();
        }
        else if (nextStage > Stage.FreeMove)
        {
            if (field.leftPad.padType != PadType.Slick || field.rightPad.padType != PadType.Slick)
            {
                Pad newLeftpad = builder.MakePad(PadType.Slick, Side.Left);
                Pad newRightPad = builder.MakePad(PadType.Slick, Side.Right);
                field.ReplaceEntity(Entity.LeftPad, newLeftpad);
                field.ReplaceEntity(Entity.RightPad, newRightPad);
                field.fragmentStore.GatherPadFragments();
            }
            field.fragmentStore.DropPadFragments();
        }
        StartStage();
        
        newGameManager.FieldDoMoveZ();
        if (mainSettings.gameMode == GameMode.NonStop && currentStage == Stage.DD)
        {
            vfx.MoveGhostsWhilePlaying();
        }
        if (currentStage > Stage.FireAndIce)
        {
            field.debuffStore.debuffFreeze.EnterStage();
            field.debuffStore.debuffBurn.EnterStage();
        }
        if (currentStage == Stage.Neon)
        {
            field.fragmentStore.allPadFragments.ForEach(frg => field.fragmentStore.AddFragmentConstantForce(frg, new Vector3(0, 0, 25)));
        }
    }

}
