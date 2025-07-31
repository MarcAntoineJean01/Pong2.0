using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine.Events;
public class NewGameManager : PongManager
{
    public bool debugMoveCam = false;
    public UnityEvent spawnedSpikes;
    public UnityEvent roundStarted;
    public UnityEvent roundStopped;
    public UnityEvent gameOver;
    public bool debugNeverReachGoal;
    bool explainedSpikes = false;
    float timeScaleWhenPaused = 1;
    public float ellapsedGameTime = 0;
    bool scoreCoolingDown = false;

    float FieldMoveZ(Edge edge)
    {
        switch (currentStage)
        {

            default:
            case Stage.DD:
                return edge.transform.position.z + sizes.ballDiameter * 2;
            case Stage.DDD:
                return edge.transform.position.z + sizes.fieldDepth * 0.20f;
            case Stage.Universe:
                return edge.transform.position.z + sizes.fieldDepth * 0.20f;
            case Stage.GravityWell:
                return edge.transform.position.z + sizes.fieldDepth * 0.20f;
            case Stage.FreeMove:
                return edge.transform.position.z;
            case Stage.FireAndIce:
                return edge.transform.position.z + sizes.fieldDepth * 0.20f;
            case Stage.Neon:
                return edge.transform.position.z + sizes.fieldDepth * 0.20f - sizes.ballDiameter * 2;
            case Stage.Final:
                return edge.transform.position.z;
        }
    }
    void Update()
    {
        if (launchCalled)
        {
            ellapsedGameTime += Time.deltaTime;
        }
    }
    void OnEnable()
    {
        um.inputSystemUI.actionsAsset.Disable();
        gameOver.AddListener(() => TriggerGameOver());
    }
    public void Pause(Side pauseSide = Side.Left)
    {
        if (currentPhase != GamePhase.Pause && !um.menuOn)
        {
            previousPhase = currentPhase;
            timeScaleWhenPaused = Time.timeScale;
            Time.timeScale = 0;
            um.inputSystemUI.actionsAsset.Enable();
            field.DisablePadControls();
            um.OpenPauseMenu();
            currentPhase = GamePhase.Pause;
            roundStopped.Invoke();
            if (debugMoveCam)
            {
                StartCoroutine("DebugMoveCam");
            }
        }
    }
    public void UnPause()
    {
        if (debugMoveCam)
        {
            StopCoroutine("DebugMoveCam");
            CameraManager.activeVCam.transform.rotation = Quaternion.identity;
        }
        if (previousPhase != GamePhase.Pause)
        {
            currentPhase = previousPhase;
        }
        Time.timeScale = timeScaleWhenPaused;
        um.inputSystemUI.actionsAsset.Disable();
        field.EnablePadControls();
        if (currentStage != Stage.Neon && um.metaCube.transform.parent != menuCanvas.transform)
        {
            um.metaCube.transform.SetParent(menuCanvas.transform, false);
        }
        previousPhase = GamePhase.Pause;
        if (field.ball.st == State.Live)
        {
            roundStarted.Invoke();
        }
    }

    public void StartGame()
    {
        leftPlayer = new Player(mainSettings.leftPlayerController, options.startingHealth, um.hudBarLeft, um.hudBarLeftSplit);
        rightPlayer = new Player(mainSettings.rightPlayerController, options.startingHealth, um.hudBarRight, um.hudBarRightSplit);
        menuCanvas.planeDistance = 100;
        builder.Build();
        newStageManager.SetStage();
        cm.SetupVirtualCameras();
        if (mainSettings.tutorialsOn)
        {
            dm.MakePixySpeechBubble(Dialogs.controlsInstructions, false, true);
            dm.pixySpeechBubble.pixyBubbleDied.AddListener(() => { ListenForPad(Side.None); dm.pixySpeechBubble.pixyBubbleDied.RemoveAllListeners(); });
        }
        else
        {
            ListenForPad(Side.None);
        }
        field.rightWall.ballTouchedWall.AddListener(() => TryUpdateScore(Side.Right));
        field.leftWall.ballTouchedWall.AddListener(() => TryUpdateScore(Side.Left));
        goals = 0;
        currentPhase = GamePhase.Playing;
    }
    void TryUpdateScore(Side sd)
    {
        Vector3 intersectionDir;
        float intersectionDist;
        bool ballTouchingPad = Physics.ComputePenetration(
            sd == Side.Left ? field.leftPad.col : field.rightPad.col,
            sd == Side.Left ? field.leftPad.transform.position : field.rightPad.transform.position,
            sd == Side.Left ? field.leftPad.transform.rotation : field.rightPad.transform.rotation,
            field.ball.col,
            field.ball.transform.position,
            field.ball.transform.rotation,
            out intersectionDir,
            out intersectionDist
        );
        if (!ballTouchingPad && !scoreCoolingDown)
        {
            if (sd == Side.Left)
            {
                leftPlayer.UpdateHealth(5, true);
                if (leftPlayer.health <= 0)
                {
                    gameOver.Invoke();
                }
                else if (leftPlayer.health <= 5)
                {
                    am.PlayAudio(AudioType.WarningLowHealth, field.rightWall.transform.position);
                    field.leftWall.WarnLowHealth();
                }
                else
                {
                    am.PlayAudio(AudioType.LostHealth, field.leftWall.transform.position);
                }
            }
            else
            {
                rightPlayer.UpdateHealth(5, true);
                if (rightPlayer.health <= 0)
                {
                    gameOver.Invoke();
                }
                else if (rightPlayer.health <= 5)
                {
                    am.PlayAudio(AudioType.WarningLowHealth, field.rightWall.transform.position);
                    field.rightWall.WarnLowHealth();
                }
                else
                {
                    am.PlayAudio(AudioType.LostHealth, field.rightWall.transform.position);
                }
            }
            if (currentStage != Stage.DD)
            {
                vfx.FragmentWall(sd);
            }
            goals += 1;
            displayHud.Invoke(sd);
            newStageManager.CheckStage(sd);
            StopCoroutine("CycleScoreDelay");
            StartCoroutine("CycleScoreDelay");
        }
    }
    public void StopListenForPad(Side padSide)
    {
        switch (padSide)
        {
            case Side.Left:
                field.leftPad.padConfirm.RemoveAllListeners();
                field.leftPad.padPause.RemoveAllListeners();
                break;
            case Side.Right:
                field.rightPad.padConfirm.RemoveAllListeners();
                field.rightPad.padPause.RemoveAllListeners();
                break;
            case Side.None:
                field.leftPad.padConfirm.RemoveAllListeners();
                field.rightPad.padConfirm.RemoveAllListeners();
                field.leftPad.padPause.RemoveAllListeners();
                field.rightPad.padPause.RemoveAllListeners();
                break;
        }
    }
    public void ListenForPad(Side padSide)
    {
        StopListenForPad(padSide);
        switch (padSide)
        {
            case Side.Left:
                field.leftPad.padConfirm.AddListener(() => { LaunchBall(); });
                field.leftPad.padPause.AddListener(() => Pause(Side.Left));
                break;
            case Side.Right:
                field.rightPad.padConfirm.AddListener(() => { LaunchBall(); });
                field.rightPad.padPause.AddListener(() => Pause(Side.Right));
                break;
            case Side.None:
                field.leftPad.padConfirm.AddListener(() => { LaunchBall(); });
                field.rightPad.padConfirm.AddListener(() => { LaunchBall(); });
                field.leftPad.padPause.AddListener(() => Pause(Side.Left));
                field.rightPad.padPause.AddListener(() => Pause(Side.Right));
                break;
        }
    }
    void LaunchBall()
    {
        if (!um.menuOn && !launchCalled && !stillTransitioning && currentPhase != GamePhase.Startup)
        {
            roundStarted.Invoke();
            previousPhase = currentPhase;
            currentPhase = GamePhase.Playing;
            field.ball.SetBallState(State.Live);
            am.PlayAudio(AudioType.LaunchBall, Vector3.zero);
            launchCalled = true;
            if (mainSettings.gameMode == GameMode.Time)
            {
                FieldDoMoveZ();
            }
            if (currentStage >= Stage.FireAndIce)
            {
                field.debuffStore.debuffFreeze.Release();
                field.debuffStore.debuffBurn.Release();
            }
            if (currentStage == Stage.Neon)
            {
                foreach (Fragment fragment in fallenPadFragments.allFragments.Concat(field.leftPad.fragments).Concat(field.rightPad.fragments))
                {
                    ConstantForce cs = fragment.AddComponent<ConstantForce>();
                    cs.force = new Vector3(0, 0, 25);
                }
            }
        }

    }
    public void FieldDoMoveZ()
    {
        KillFieldDoMoveZ();
        field.background.transform.DOMoveZ(FieldMoveZ(field.background), options.timeThreshold).OnComplete(() => { newStageManager.stageTimedOut.Invoke(); roundStopped.Invoke(); });
        field.topFloor.transform.DOMoveZ(FieldMoveZ(field.topFloor), options.timeThreshold);
        field.bottomFloor.transform.DOMoveZ(FieldMoveZ(field.bottomFloor), options.timeThreshold);
        field.leftWall.transform.DOMoveZ(FieldMoveZ(field.leftWall), options.timeThreshold);
        field.rightWall.transform.DOMoveZ(FieldMoveZ(field.rightWall), options.timeThreshold);
        vfx.UpdateFieldNoiseIntensity();
    }
    public void RestartFieldDoMoveZ()
    {
        field.background.transform.DOPlay();
        field.topFloor.transform.DOPlay();
        field.bottomFloor.transform.DOPlay();
        field.leftWall.transform.DOPlay();
        field.rightWall.transform.DOPlay();
        vfx.UpdateFieldNoiseIntensity();
    }
    public void PauseFieldDoMoveZ()
    {
        field.background.transform.DOPause();
        field.topFloor.transform.DOPause();
        field.bottomFloor.transform.DOPause();
        field.leftWall.transform.DOPause();
        field.rightWall.transform.DOPause();
        vfx.StopUpdateFieldNoiseIntensity();
    }
    public void KillFieldDoMoveZ(bool complete = false)
    {
        field.background.transform.DOKill(complete);
        field.topFloor.transform.DOKill(complete);
        field.bottomFloor.transform.DOKill(complete);
        field.leftWall.transform.DOKill(complete);
        field.rightWall.transform.DOKill(complete);
        vfx.StopUpdateFieldNoiseIntensity();
    }
    public void StopGame()
    {

    }
    public void ResetGame()
    {

    }
    public void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
    public void PickASpike()
    {
        bool canAddPieces = field.leftPad.CanAddPiece() || field.rightPad.CanAddPiece();
        bool canAddBlocks = (field.leftPad.CanAddBlock(Side.Top) || field.leftPad.CanAddBlock(Side.Bottom) || field.rightPad.CanAddBlock(Side.Top) || field.rightPad.CanAddBlock(Side.Bottom)) && currentStage != Stage.DD;
        field.spikeStore.StoreSpikes();
        List<SpikeType> spikeList = new List<SpikeType>();
        if (options.allowedSpikes.addPadPiece && canAddPieces) { spikeList.Add(SpikeType.SpikePadPiece); }
        if (options.allowedSpikes.addPadBlock && canAddBlocks) { spikeList.Add(SpikeType.SpikePadBlock); }
        if (options.allowedSpikes.dissolve) { spikeList.Add(SpikeType.SpikeDissolve); }
        if (options.allowedSpikes.randomDirection) { spikeList.Add(SpikeType.SpikeRandomDirection); }
        if (options.allowedSpikes.wallAttractor) { spikeList.Add(SpikeType.SpikeWallAttractor); }
        if (options.allowedSpikes.magnet) { spikeList.Add(SpikeType.SpikeMagnet); }
        if (options.allowedSpikes.magnet) { spikeList.Add(SpikeType.HealthUp); }
        field.spikeStore.SetActiveSpikes(spikeList.ElementAt(UnityEngine.Random.Range(0, spikeList.Count)));
    }
    public void SpikeSpawn()
    {
        if (options.allowedSpikes.anyAllowedSpikes)
        {
            StopCoroutine("CycleSpikeSpawn");
            StartCoroutine("CycleSpikeSpawn");
        }
    }
    IEnumerator CycleSpikeSpawn()
    {
        while (true)
        {
            yield return new WaitForSeconds(pm.gameEffects.spikeInterval + ((int)currentStage - 1));
            if (currentPhase == GamePhase.Playing && !field.spikeStore.anyActiveSpike)
            {

                PickASpike();
                yield return null;
                field.spikeStore.activeSpikes.ActivateSpikes();
                spawnedSpikes.Invoke();
                if (mainSettings.cutScenesOn && mainSettings.tutorialsOn && currentPhase == GamePhase.Playing && !explainedSpikes)
                {
                    explainedSpikes = true;
                    if (currentStage == Stage.DD)
                    {
                        csm.PlayScene(CutScene.PixyExplainsSpikes);
                    }
                }
            }
        }
    }
    public void ResetEntityPositions()
    {

    }
    IEnumerator CycleScoreDelay()
    {
        scoreCoolingDown = true;
        yield return new WaitForSeconds(0.5f);
        scoreCoolingDown = false;
    }
    IEnumerator CycleResetEntityPositions()
    {
        Vector3 initialBallPos = field.ball.transform.position;
        Vector3 initialLeftPadPos = field.leftPad.transform.position;
        Vector3 initialRightPadPos = field.rightPad.transform.position;
        float t = 0f;

        float maxDistance = Mathf.Max(new float[] { Vector3.Distance(new Vector3(0, 0, initialBallPos.z), initialBallPos), Vector3.Distance(new Vector3(initialLeftPadPos.x, 0, initialLeftPadPos.z), initialLeftPadPos), Vector3.Distance(new Vector3(initialRightPadPos.x, 0, initialRightPadPos.z), initialRightPadPos) });
        float fieldDiagonal = Mathf.Sqrt(sizes.fieldWidth * sizes.fieldWidth + sizes.fieldHeight * sizes.fieldHeight) * 0.5f;
        if (maxDistance > fieldDiagonal) { maxDistance = fieldDiagonal; }
        float scaledValue = 0;
        if (maxDistance > 0) { scaledValue = maxDistance / fieldDiagonal * pm.speeds.transitionSpeeds.resetTransitionSpeed; }
        if (scaledValue > Time.deltaTime)
        {
            while (t < scaledValue)
            {
                t += Time.deltaTime;
                if (t > scaledValue) { t = scaledValue; }
                var normalizedProgress = t / scaledValue;
                var easing = newStageManager.resetXYCurve.Evaluate(normalizedProgress);
                field.ball.transform.position = new Vector3(Mathf.Lerp(initialBallPos.x, 0, easing), Mathf.Lerp(initialBallPos.y, 0, easing), initialBallPos.z);
                field.leftPad.transform.position = new Vector3(initialLeftPadPos.x, Mathf.Lerp(initialLeftPadPos.y, 0, easing), initialLeftPadPos.z);
                field.rightPad.transform.position = new Vector3(initialRightPadPos.x, Mathf.Lerp(initialRightPadPos.y, 0, easing), initialRightPadPos.z);
                yield return null;
            }
        }
        else
        {
            field.ball.transform.position = new Vector3(0, 0, initialBallPos.z);
            field.leftPad.transform.position = new Vector3(initialLeftPadPos.x, 0, initialLeftPadPos.z);
            field.rightPad.transform.position = new Vector3(initialRightPadPos.x, 0, initialRightPadPos.z);
        }
    }

    void TriggerGameOver()
    {
        roundStopped.Invoke();
        Time.timeScale = 0;
    }

    public void AddHealth(Side side)
    {
        switch (side)
        {
            case Side.Left:
                leftPlayer.UpdateHealth(5);
                break;
            case Side.Right:
                rightPlayer.UpdateHealth(5);
                break;
        }
    }
    IEnumerator DebugMoveCam()
    {
        while (true)
        {
            CameraManager.activeVCam.transform.Rotate(new Vector3(0, 0.2f, 0));
            yield return null;
        }
    }
}

