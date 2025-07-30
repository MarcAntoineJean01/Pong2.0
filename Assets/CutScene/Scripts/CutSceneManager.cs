using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using DG.Tweening;
using System.Linq;
using System;
using UnityEngine.Events;
public class CutSceneManager : PongManager
{
    public static bool cutSceneOn = false;
    public void TakeControl()
    {
        Time.timeScale = 0;
        CameraManager.camBrain.m_IgnoreTimeScale = true;
        DOTween.defaultTimeScaleIndependent = true;
        previousPhase = currentPhase;
        currentPhase = GamePhase.CutScene;
        newGameManager.StopListenForPad(Side.None);
        cutSceneOn = true;
    }
    public void ReleaseControl(bool newStage = false)
    {
        Time.timeScale = 1;
        CameraManager.camBrain.m_IgnoreTimeScale = false;
        DOTween.defaultTimeScaleIndependent = false;
        if (previousPhase != GamePhase.Pause)
        {
            currentPhase = previousPhase;
        }
        else
        {
            currentPhase = GamePhase.Playing;
        }
        previousPhase = GamePhase.CutScene;
        if (newStage && mainSettings.tutorialsOn)
        {
            dm.MakePixySpeechBubble(Dialogs.spikesAndDebuffsForStage, false, true);
            dm.pixySpeechBubble.pixyBubbleDied.AddListener(() => { newGameManager.ListenForPad(Side.None); dm.pixySpeechBubble.pixyBubbleDied.RemoveAllListeners(); });
        }
        else
        {
            newGameManager.ListenForPad(Side.None);
        }
        cutSceneOn = false;
    }
    public void PlayScene(CutScene scene)
    {
        TakeControl();
        switch (scene)
        {
            case CutScene.SimpleStageTransition:
                SimpleStageTransitionScene();
                break;
            case CutScene.Greetings:
                GreetingsFromPoly();
                break;
            case CutScene.StageIntro:
                if (currentStage == Stage.Universe)
                {
                    PolyWantsToGetOut();
                }
                else if (currentStage == Stage.DD)
                {
                    GreetingsFromPoly();
                }
                else
                {
                    SimpleDialogScene(DialogType.StageIntro);
                }
                break;
            case CutScene.StageOutro:
                if (currentStage == Stage.Universe)
                {
                    PolyEatsSingularity();
                }
                else
                {
                    SimpleDialogScene(DialogType.StageOutro);
                }

                break;
            case CutScene.PixyExplainsSpikes:
                SimpleDialogScene(DialogType.ExplainSpikes);
                break;
            case CutScene.PolyTakesPity:
                PolyTakesPity();
                break;
            case CutScene.PolyWantsToGetOut:
                PolyWantsToGetOut();
                break;
            case CutScene.PolyFeelsLighter:
                SimpleDialogScene(DialogType.FirstFeelLighter);
                break;
            case CutScene.PolyFeelsEvenLighter:
                SimpleDialogScene(DialogType.LastFeelLighter);
                break;
            case CutScene.ScoldingFromPixy:
                ScoldingFromPixy();
                break;
            case CutScene.PixyExplainsDebuffs:
                PixyExplainsDebuffs();
                break;

        }
    }
    void GreetingsFromPoly()
    {
        StartCoroutine("CycleGreetingsFromPoly");
    }
    void SimpleDialogScene(DialogType dialogType)
    {
        StartCoroutine(CycleSimpleDialog(dialogType));
    }
    public void SimpleStageTransitionScene()
    {
        StartCoroutine("CycleSimpleStageTransitionScene");
    }
    void PolyTakesPity()
    {

    }
    void PolyWantsToGetOut()
    {
        StartCoroutine("CyclePolyWantsToGetOut");
    }
    void ScoldingFromPixy()
    {
        StartCoroutine("CycleScoldingFromPixy");
    }
    void PolyEatsSingularity()
    {
        StartCoroutine("CyclePolyEatsSingularity");
    }
    void PixyExplainsDebuffs()
    {

    }
    IEnumerator CycleSimpleDialog(DialogType dialogType)
    {
        if (dialogType == DialogType.StageIntro && currentStage >= Stage.FireAndIce)
        {
            if (!field.debuffStore.debuffFreeze.gotFragments || !field.debuffStore.debuffBurn.gotFragments)
            {
                BallEntity newBall = builder.MakeFragmentedBall(BallMesh.Icosahedron, 1.2f);
                newBall.SetBallForStage();
                field.ReplaceEntity(Entity.Ball, newBall);
            }
            field.debuffStore.debuffBurn.gameObject.SetActive(true);
            field.debuffStore.debuffFreeze.gameObject.SetActive(true);

            field.debuffStore.debuffFreeze.Orbit();
            field.debuffStore.debuffBurn.Orbit();
            field.ball.ballType = BallMesh.Octacontagon;
            field.ball.fragmented = false;
        }
        vfx.StartPolyIdleAnimation();
        dm.MakeSPeechBubble(dialogType);
        while (dm.activePixyBubble || dm.activePolyBubble)
        {
            yield return null;
        }
        if (dialogType == DialogType.StageOutro)
        {
            SimpleStageTransitionScene();
        }
        else
        {
            vfx.StopPolyIdleAnimation();
            ReleaseControl();
        }

    }
    IEnumerator CycleGreetingsFromPoly()
    {
        vfx.StartPolyIdleAnimation();
        dm.MakeSPeechBubble(DialogType.GreetingsFromPoly);
        while (dm.activePixyBubble || dm.activePolyBubble)
        {
            yield return null;
        }

        DOTween.defaultTimeScaleIndependent = true;
        Sequence polyMoveAnimation = DOTween.Sequence();
        polyMoveAnimation.Append(field.ball.transform.DOMoveX(sizes.fieldWidth * 0.4f, 2));
        polyMoveAnimation.SetEase(Ease.Linear);
        polyMoveAnimation.OnComplete(() => polyMoveAnimation.Kill(true));
        while (polyMoveAnimation.IsActive()) { yield return null; }
        dm.MakeSPeechBubble(DialogType.CustomPoly, "Good Rectangle, could you let me through? I'm tryi", true);
        while (dm.activePixyBubble || dm.activePolyBubble)
        {
            yield return null;
        }

        dm.MakeSPeechBubble(DialogType.CustomPixy, "DO NOT LET IT THROUGH!");
        while (dm.activePixyBubble || dm.activePolyBubble)
        {
            yield return null;
        }
        GameObject.Destroy(dm.polyBubbleGhost);
        dm.MakeSPeechBubble(DialogType.CustomPoly, "Rude! i was talking to this nice rectangle and you interrupted me!");
        while (dm.activePixyBubble || dm.activePolyBubble)
        {
            yield return null;
        }
        DOTween.defaultTimeScaleIndependent = true;
        polyMoveAnimation = DOTween.Sequence();
        polyMoveAnimation.Append(field.ball.transform.DOMoveX(0, 2));
        polyMoveAnimation.SetEase(Ease.Linear);
        polyMoveAnimation.OnComplete(() => polyMoveAnimation.Kill(true));
        while (polyMoveAnimation.IsActive()) { yield return null; }
        dm.MakeSPeechBubble(DialogType.GreetingsFromPixy);
        while (dm.activePixyBubble || dm.activePolyBubble)
        {
            yield return null;
        }
        dm.MakeSPeechBubble(DialogType.ChallengeFromPoly);
        while (dm.activePixyBubble || dm.activePolyBubble)
        {
            yield return null;
        }
        vfx.StopPolyIdleAnimation();
        ReleaseControl();

    }
    void LerpVfxGhostsToWall(float t, float ghostZend, Vector3 initialBallPos, Vector3 initialLeftPadPos, Vector3 initialRightPadPos)
    {
        var normalizedProgress = t / pm.speeds.transitionSpeeds.entitiesTransitionSpeed;
        var easing = newStageManager.moveEntitiesCurve.Evaluate(normalizedProgress);
        vfx.ballghost.transform.position = new Vector3(Mathf.Lerp(initialBallPos.x, 0, easing), Mathf.Lerp(initialBallPos.y, 0, easing), Mathf.Lerp(initialBallPos.z, ghostZend, easing));
        vfx.leftPadGhost.transform.position = new Vector3(initialLeftPadPos.x, Mathf.Lerp(initialLeftPadPos.y, 0, easing), Mathf.Lerp(initialLeftPadPos.z, ghostZend, easing));
        vfx.rightPadGhost.transform.position = new Vector3(initialRightPadPos.x, Mathf.Lerp(initialRightPadPos.y, 0, easing), Mathf.Lerp(initialRightPadPos.z, ghostZend, easing));
    }
    void LerpVfxGhostsFromWall(float t, float ghostZstart, Vector3 initialBallPos, Vector3 initialLeftPadPos, Vector3 initialRightPadPos)
    {
        var normalizedProgress = t / pm.speeds.transitionSpeeds.entitiesTransitionSpeed;
        var easing = newStageManager.moveEntitiesCurve.Evaluate(normalizedProgress);
        float targetPosZ = mainSettings.gameMode == GameMode.Time ? PongManager.sizes.fieldDepth * 0.5f + PongManager.sizes.planeDistance + PongManager.sizes.ballDiameter * 0.5f : stagePosZ;
        vfx.ballghost.transform.position = new Vector3(Mathf.Lerp(initialBallPos.x, 0, easing), Mathf.Lerp(initialBallPos.y, 0, easing), Mathf.Lerp(ghostZstart, targetPosZ, easing));
        vfx.leftPadGhost.transform.position = new Vector3(initialLeftPadPos.x, Mathf.Lerp(initialLeftPadPos.y, 0, easing), Mathf.Lerp(ghostZstart, targetPosZ, easing));
        vfx.rightPadGhost.transform.position = new Vector3(initialRightPadPos.x, Mathf.Lerp(initialRightPadPos.y, 0, easing), Mathf.Lerp(ghostZstart, targetPosZ, easing));
    }
    void LerpEntitiesPositions(float t, Vector3 initialBallPos, Vector3 initialLeftPadPos, Vector3 initialRightPadPos)
    {
        var normalizedProgress = t / pm.speeds.transitionSpeeds.entitiesTransitionSpeed;
        var easing = newStageManager.moveEntitiesCurve.Evaluate(normalizedProgress);
        field.ball.transform.position = new Vector3(Mathf.Lerp(initialBallPos.x, 0, easing), Mathf.Lerp(initialBallPos.y, 0, easing), Mathf.Lerp(initialBallPos.z, nextStagePosZ, easing));
        field.leftPad.transform.position = new Vector3(initialLeftPadPos.x, Mathf.Lerp(initialLeftPadPos.y, 0, easing), Mathf.Lerp(initialLeftPadPos.z, nextStagePosZ, easing));
        field.rightPad.transform.position = new Vector3(initialRightPadPos.x, Mathf.Lerp(initialRightPadPos.y, 0, easing), Mathf.Lerp(initialRightPadPos.z, nextStagePosZ, easing));
        foreach (Block block in field.blocks)
        {
            block.transform.position = new Vector3(block.transform.position.x, block.transform.position.y, Mathf.Lerp(block.sd == Side.Left ? initialLeftPadPos.z : initialRightPadPos.z, nextStagePosZ, easing));
        }
    }
    void ScalePadBlocks(float t, float initialBlockScale, Vector3 initialLeftPadPos, Vector3 initialRightPadPos)
    {
        var normalizedProgress = t / pm.speeds.transitionSpeeds.entitiesTransitionSpeed;
        var easing = newStageManager.moveEntitiesCurve.Evaluate(normalizedProgress);
        foreach (Block block in field.blocks)
        {
            if (nextStage == Stage.FreeMove)
            {
                block.transform.localScale = new Vector3(
                    block.transform.localScale.x,
                    block.transform.localScale.y,
                    Mathf.Lerp(initialBlockScale, initialBlockScale * sizes.fieldDepth, easing));
            }
            else if (block.transform.localScale.z > initialBlockScale)
            {
                block.transform.localScale = new Vector3(
                    block.transform.localScale.x,
                    block.transform.localScale.y,
                    Mathf.Lerp(initialBlockScale * sizes.fieldDepth, initialBlockScale, easing));
            }
        }
    }
    IEnumerator CycleSimpleStageTransitionScene()
    {
        List<Edge> edges = new List<Edge>()
        {
            field.leftWall,
            field.rightWall,
            field.topFloor,
            field.bottomFloor,
            field.background
        };
        bool materializeEdges;

        vfx.StartPolyIdleAnimation();
        if (mainSettings.gameMode == GameMode.Goals || currentStage == Stage.Neon || nextStage == Stage.Neon)
        {
            cm.MoveToNextCamera();
        }
        if (nextStage == Stage.Neon || nextStage == Stage.Final)
        {
            am.PlayMusic(MusicType.BackgroundMusic);
        }
        Vector3 initialBallPos = field.ball.transform.position;
        Vector3 initialLeftPadPos = field.leftPad.transform.position;
        Vector3 initialRightPadPos = field.rightPad.transform.position;
        float initialBlockScale = field.blocks.Count > 0 ? field.blocks[0].transform.localScale.x : 0;
        float t = 0f;
        if (currentStage == Stage.DD || nextStage == Stage.DD)
        {
            vfx.MakeIntersectionGhosts();
        }
        if (nextStage == Stage.DD)
        {
            float ghostZend = field.background.transform.position.z + field.background.col.bounds.size.z / 2 + sizes.ballDiameter;
            while (t < pm.speeds.transitionSpeeds.entitiesTransitionSpeed)
            {
                t += Time.unscaledDeltaTime;
                if (t > pm.speeds.transitionSpeeds.entitiesTransitionSpeed) { t = pm.speeds.transitionSpeeds.entitiesTransitionSpeed; }
                LerpVfxGhostsToWall(t, ghostZend, initialBallPos, initialLeftPadPos, initialRightPadPos);
                LerpEntitiesPositions(t, initialBallPos, initialLeftPadPos, initialRightPadPos);
                yield return null;
            }
            vfx.DestroyIntersectionGhosts();
        }
        switch (currentStage)
        {
            case Stage.DD:
                float ghostZstart = field.background.transform.position.z + field.background.col.bounds.size.z / 2 + sizes.ballDiameter;
                materializeEdges = edges.Any(edge => Mathf.Approximately(edge.meshR.material.GetFloat("_DissolveProgress"), 1));
                while (t < pm.speeds.transitionSpeeds.entitiesTransitionSpeed)
                {
                    t += Time.unscaledDeltaTime;
                    if (t > pm.speeds.transitionSpeeds.entitiesTransitionSpeed) { t = pm.speeds.transitionSpeeds.entitiesTransitionSpeed; }
                    LerpVfxGhostsFromWall(t, ghostZstart, initialBallPos, initialLeftPadPos, initialRightPadPos);
                    LerpEntitiesPositions(t, initialBallPos, initialLeftPadPos, initialRightPadPos);
                    if (materializeEdges)
                    {
                        edges.ForEach(edge => edge.meshR.material.SetFloat("_DissolveProgress", Mathf.SmoothStep(1, 0, t / pm.speeds.transitionSpeeds.entitiesTransitionSpeed)));
                    }
                    yield return null;
                }
                vfx.DestroyIntersectionGhosts();
                break;
            default:
                materializeEdges = currentStage == Stage.Universe && edges.Any(edge => Mathf.Approximately(edge.meshR.material.GetFloat("_DissolveProgress"), 1));
                while (t < pm.speeds.transitionSpeeds.entitiesTransitionSpeed)
                {
                    t += Time.unscaledDeltaTime;
                    if (t > pm.speeds.transitionSpeeds.entitiesTransitionSpeed) { t = pm.speeds.transitionSpeeds.entitiesTransitionSpeed; }
                    if (currentStage == Stage.Universe)
                    {
                        if (mainSettings.cutScenesOn)
                        {
                            field.ball.meshR.material.SetColor("_DissolveEdgeColor", Color.Lerp(mm.materials.darknessPolyGlowColor, mm.materials.ballDissolveGlowColor, t / pm.speeds.transitionSpeeds.entitiesTransitionSpeed));
                            field.ball.meshR.material.SetFloat("_DissolveEdgeDepth", Mathf.Lerp(1, 0, t / pm.speeds.transitionSpeeds.entitiesTransitionSpeed));
                        }
                    }
                    if (materializeEdges)
                    {
                        edges.ForEach(edge => edge.meshR.material.SetFloat("_DissolveProgress", Mathf.SmoothStep(1, 0, t / pm.speeds.transitionSpeeds.entitiesTransitionSpeed)));
                    }
                    LerpEntitiesPositions(t, initialBallPos, initialLeftPadPos, initialRightPadPos);
                    ScalePadBlocks(t, initialBlockScale, initialLeftPadPos, initialRightPadPos);
                    yield return null;
                }
                if (currentStage == Stage.Universe && mainSettings.cutScenesOn)
                {
                    field.ball.meshR.material.SetColor("_DissolveEdgeColor", mm.materials.ballDissolveGlowColor);
                    field.ball.meshR.material.SetFloat("_DissolveEdgeDepth", 0);
                }
                break;
        }
        if (currentStage == Stage.DD || nextStage == Stage.DD)
        {
            field.ball.transform.position = new Vector3(0, 0, stagePosZ);
            field.leftPad.transform.position = new Vector3(initialLeftPadPos.x, 0, stagePosZ);
            field.rightPad.transform.position = new Vector3(initialRightPadPos.x, 0, stagePosZ);
        }
        vfx.StopPolyIdleAnimation();
        if (nextStage != Stage.FreeMove && field.leftPad.energyShield != null)
        {
            GameObject.Destroy(field.leftPad.energyShield);
        }
        if (nextStage != Stage.FreeMove && field.rightPad.energyShield != null)
        {
            GameObject.Destroy(field.rightPad.energyShield);
        }

        if (nextStage == Stage.FreeMove)
        {
            Pad newLeftpad = builder.MakePad("PadSlick", Side.Left);
            Pad newRightPad = builder.MakePad("PadSlick", Side.Right);
            field.ReplaceEntity(Entity.LeftPad, newLeftpad);
            field.ReplaceEntity(Entity.RightPad, newRightPad);
            newGameManager.StopListenForPad(Side.None);
            vfx.SetFreeMovePads();
            yield return null;
            while (field.leftPad.fragments.Concat(field.rightPad.fragments).Any(frg => DOTween.IsTweening(frg.transform))) { yield return null; }
            builder.MakeEnergyShield(field.leftPad);
            builder.MakeEnergyShield(field.rightPad);
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
        else if ((field.leftPad.fragmented || field.rightPad.fragmented || !fallenPadFragments.empty) && nextStage < Stage.FreeMove)
        {
            vfx.RebuildPads();
            yield return null;
            while (field.leftPad.fragments.Concat(field.rightPad.fragments).Any(frg => DOTween.IsTweening(frg.transform))) { yield return null; }
            Pad newLeftpad = builder.MakePad("PadRough", Side.Left);
            Pad newRightPad = builder.MakePad("PadRough", Side.Right);
            field.ReplaceEntity(Entity.LeftPad, newLeftpad);
            field.ReplaceEntity(Entity.RightPad, newRightPad);
            newGameManager.StopListenForPad(Side.None);
        }
        else if (nextStage > Stage.FreeMove)
        {
            if (!field.leftPad.slickPad)
            {
                Pad newLeftpad = builder.MakePad("PadSlick", Side.Left);
                field.ReplaceEntity(Entity.LeftPad, newLeftpad);
                newGameManager.StopListenForPad(Side.Left);
            }
            if (!field.rightPad.slickPad)
            {
                Pad newRightPad = builder.MakePad("PadSlick", Side.Right);
                field.ReplaceEntity(Entity.RightPad, newRightPad);
                newGameManager.StopListenForPad(Side.Right);
            }
            fallenPadFragments.GatherFragments(field.leftPad);
            fallenPadFragments.GatherFragments(field.rightPad);
        }
        newStageManager.StartStage();
        ReleaseControl(true);
    }
    IEnumerator CyclePolyWantsToGetOut()
    {
        float t = 0f;
        vfx.StartPolyIdleAnimation();

        dm.MakeSPeechBubble(DialogType.WillToGetOut);
        yield return null;
        while (dm.activePixyBubble || dm.activePolyBubble)
        {
            yield return null;
        }

        //camera shake
        CinemachineBasicMultiChannelPerlin noise = CameraManager.activeVCam.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();
        noise.m_AmplitudeGain = 0.25f;
        noise.m_FrequencyGain = 5;
        //

        //edge dissolve
        List<Edge> edges = new List<Edge>()
        {
            field.leftWall,
            field.rightWall,
            field.topFloor,
            field.bottomFloor,
            field.background
        };
        edges.ForEach(edge => { edge.meshR.material.SetFloat("_EmissionIntensity", 1); edge.meshR.material.SetFloat("_DissolveEdgeDepth", 0.01f); });
        //

        //dissolve walls and shake camera
        while (t < pm.gameEffects.wallDissolveSpeed)
        {
            t += Time.unscaledDeltaTime;
            if (t > pm.gameEffects.wallDissolveSpeed) { t = pm.gameEffects.wallDissolveSpeed; }
            edges.ForEach(edge => edge.meshR.material.SetFloat("_DissolveProgress", Mathf.SmoothStep(0, 1, t / pm.gameEffects.wallDissolveSpeed)));
            noise.m_AmplitudeGain = Mathf.SmoothStep(0.25f, 0, t / pm.gameEffects.wallDissolveSpeed);
            noise.m_FrequencyGain = Mathf.SmoothStep(5, 0, t / pm.gameEffects.wallDissolveSpeed);
            yield return null;
        }
        //

        dm.MakeSPeechBubble(DialogType.AweForCreation);
        yield return null;
        while (dm.activePixyBubble || dm.activePolyBubble)
        {
            yield return null;
        }

        // try to get out
        bool punching = true;
        field.ball.transform.DOPunchPosition(new Vector3(-15, PongEntity.fieldBounds.y, 0), 2, 3, 1).SetEase(Ease.InExpo).OnComplete(() => field.ball.transform.DOMove(new Vector3(0, 0, stagePosZ), 0.5f).OnComplete(() => punching = false));
        while (punching)
        {
            yield return null;
        }
        dm.MakeSPeechBubble(DialogType.CustomPoly, "What the...");
        yield return null;
        while (dm.activePixyBubble || dm.activePolyBubble)
        {
            yield return null;
        }
        punching = true;
        field.ball.transform.DOPunchPosition(new Vector3(10, -PongEntity.fieldBounds.y, 0), 2, 3, 1).SetEase(Ease.InExpo).OnComplete(() => field.ball.transform.DOMove(new Vector3(0, 0, stagePosZ), 0.5f).OnComplete(() => punching = false));
        while (punching)
        {
            yield return null;
        }
        dm.MakeSPeechBubble(DialogType.CustomPoly, "OUCH!");
        yield return null;
        while (dm.activePixyBubble || dm.activePolyBubble)
        {
            yield return null;
        }
        //

        dm.MakeSPeechBubble(DialogType.ScreenFrustration);
        yield return null;
        while (dm.activePixyBubble || dm.activePolyBubble)
        {
            yield return null;
        }
        field.debuffStore.debuffSlow.gameObject.SetActive(true);
        field.debuffStore.debuffSlow.StartBlackhole();
        vfx.StopPolyIdleAnimation();
        ReleaseControl();
    }
    public IEnumerator CycleScoldingFromPixy()
    {
        float t = 0f;
        vfx.StartPolyIdleAnimation();

        dm.MakeSPeechBubble(DialogType.Scolding);
        yield return null;
        while (dm.activePixyBubble || dm.activePolyBubble)
        {
            yield return null;
        }

        //wall materialize
        List<Edge> edges = new List<Edge>()
        {
            field.leftWall,
            field.rightWall,
            field.topFloor,
            field.bottomFloor,
            field.background
        };
        //

        //camera shake
        CinemachineBasicMultiChannelPerlin noise = CameraManager.activeVCam.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();
        noise.m_AmplitudeGain = 0.25f;
        noise.m_FrequencyGain = 5;
        //
        while (t < pm.gameEffects.wallDissolveSpeed)
        {
            t += Time.unscaledDeltaTime;
            if (t > pm.gameEffects.wallDissolveSpeed) { t = pm.gameEffects.wallDissolveSpeed; }
            edges.ForEach(edge => edge.meshR.material.SetFloat("_DissolveProgress", Mathf.SmoothStep(1, 0, t / pm.gameEffects.wallDissolveSpeed)));
            noise.m_AmplitudeGain = Mathf.SmoothStep(0.25f, 0, t / pm.gameEffects.wallDissolveSpeed);
            noise.m_FrequencyGain = Mathf.SmoothStep(5, 0, t / pm.gameEffects.wallDissolveSpeed);
            yield return null;
        }
        edges.ForEach(edge => { edge.meshR.material.SetFloat("_EmissionIntensity", 0); edge.meshR.material.SetFloat("_DissolveEdgeDepth", 0); });

        dm.MakeSPeechBubble(DialogType.Scolding);
        yield return null;
        while (dm.activePixyBubble || dm.activePolyBubble)
        {
            yield return null;
        }

        vfx.StopPolyIdleAnimation();
        ReleaseControl();
    }
    IEnumerator CyclePolyEatsSingularity()
    {
        float t = 0f;
        vfx.StartPolyIdleAnimation();

        bool ballMoving = true;
        bool singularityMoving = true;
        field.debuffStore.debuffSlow.sqnc.OnComplete(null);
        field.debuffStore.debuffSlow.sqnc.SetAutoKill(true);
        field.debuffStore.debuffSlow.sqnc.Kill();
        field.debuffStore.debuffSlow.transform.DOMove(new Vector3(-sizes.fieldWidth / 3, 0, stagePosZ), 2).OnComplete(() => singularityMoving = false);
        field.ball.transform.DOMove(new Vector3(sizes.fieldWidth / 3, 0, stagePosZ), 2).OnComplete(() => ballMoving = false);
        while (ballMoving || singularityMoving)
        {
            yield return null;
        }

        dm.MakeSPeechBubble(DialogType.ChatSingularity);
        yield return null;
        while (dm.activePixyBubble || dm.activePolyBubble)
        {
            yield return null;
        }

        // poly eats singularity
        Vector3 starsTargetPosition;
        Vector3 coreTargetPosition;
        Vector3 debuffInitialPosition = field.debuffStore.debuffSlow.transform.position;
        starsTargetPosition = coreTargetPosition = field.ball.transform.position;
        starsTargetPosition += field.debuffStore.debuffSlow.visualEffect.GetVector3("StarsAttractorTarget");
        coreTargetPosition += field.debuffStore.debuffSlow.visualEffect.GetVector3("CoreAttractorTarget");
        field.debuffStore.debuffSlow.visualEffect.SetVector3("SuctionTarget", field.ball.transform.position);
        field.debuffStore.debuffSlow.visualEffect.SetVector3("CoreAttractorTarget", field.debuffStore.debuffSlow.visualEffect.transform.InverseTransformPoint(coreTargetPosition));
        field.debuffStore.debuffSlow.visualEffect.SetVector3("StarsAttractorTarget", field.debuffStore.debuffSlow.visualEffect.transform.InverseTransformPoint(starsTargetPosition));
        float range = Vector3.Distance(field.debuffStore.debuffSlow.transform.position, field.ball.transform.position);
        field.debuffStore.debuffSlow.visualEffect.SetFloat("SuctionRange", range * 0.9f);
        field.debuffStore.debuffSlow.visualEffect.SetFloat("SuctionThreshold", range * 0.1f);
        mm.TransitionBallMeshes(BallMesh.Icosahedron, 1.2f);
        while (t < pm.speeds.transitionSpeeds.meshSwappingSpeed)
        {
            t += Time.unscaledDeltaTime;
            if (t > pm.speeds.transitionSpeeds.meshSwappingSpeed) { t = pm.speeds.transitionSpeeds.meshSwappingSpeed; }

            field.debuffStore.debuffSlow.visualEffect.SetFloat("StarsAttractorForce", Mathf.Lerp(0.3f, 10, t / pm.speeds.transitionSpeeds.meshSwappingSpeed));
            field.debuffStore.debuffSlow.visualEffect.SetFloat("StarsAttractorSpeed", Mathf.Lerp(1, 10, t / pm.speeds.transitionSpeeds.meshSwappingSpeed));

            field.debuffStore.debuffSlow.transform.position = Vector3.Lerp(debuffInitialPosition, field.ball.transform.position, t / pm.speeds.transitionSpeeds.meshSwappingSpeed);

            Vector3 xOffset = new Vector3(range - Vector3.Distance(field.debuffStore.debuffSlow.transform.position, field.ball.transform.position), 0, 0);
            starsTargetPosition -= xOffset;
            coreTargetPosition -= xOffset;
            field.debuffStore.debuffSlow.visualEffect.SetVector3("StarsAttractorTarget", field.debuffStore.debuffSlow.visualEffect.transform.InverseTransformPoint(starsTargetPosition));
            field.debuffStore.debuffSlow.visualEffect.SetVector3("CoreAttractorTarget", field.debuffStore.debuffSlow.visualEffect.transform.InverseTransformPoint(coreTargetPosition));

            field.ball.meshR.material.SetColor("_DissolveEdgeColor", Color.Lerp(mm.materials.ballDissolveGlowColor, mm.materials.darknessPolyGlowColor, t / pm.speeds.transitionSpeeds.meshSwappingSpeed));
            field.ball.meshR.material.SetFloat("_DissolveEdgeDepth", Mathf.Lerp(0, 1, t / pm.speeds.transitionSpeeds.meshSwappingSpeed));
            yield return null;
        }
        mm.TransitionBallMeshes(BallMesh.Icosahedron, 1.2f);
        field.ball.meshR.material = mm.materials.ballMaterials.MaterialForCurrentMesh(BallMesh.Icosahedron);
        field.ball.meshR.material.SetColor("_DissolveEdgeColor", mm.materials.darknessPolyGlowColor);
        field.ball.meshR.material.SetFloat("_DissolveEdgeDepth", 1);

        yield return new WaitForSecondsRealtime(pm.speeds.transitionSpeeds.meshSwappingSpeed-2);
        t = 0;
        field.debuffStore.debuffSlow.transform.position = field.ball.transform.position;
        while (t < 2)
        {
            t += Time.unscaledDeltaTime;
            if (t > 2) { t = 2; }
            field.debuffStore.debuffSlow.transform.localScale = Vector3.Lerp(Vector3.one, Vector3.zero, t / 2);
            field.debuffStore.debuffSlow.visualEffect.SetFloat("AttractorsScale", Mathf.Lerp(1, 0.1f, t / 2));
            yield return null;
        }
        field.debuffStore.debuffSlow.transform.DOKill();
        field.debuffStore.debuffSlow.visualEffect.transform.DOKill();
        field.debuffStore.StoreDebuffs();
        //

        dm.MakeSPeechBubble(DialogType.ChatSingularity);
        yield return null;
        while (dm.activePixyBubble || dm.activePolyBubble)
        {
            yield return null;
        }
        SimpleStageTransitionScene();
    }
}
