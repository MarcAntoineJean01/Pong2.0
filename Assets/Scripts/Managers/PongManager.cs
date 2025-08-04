using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class PongManager : PongBehaviour
{
    public static MainGameSettings mainSettings;
    public static GameOptions options;
    public static GamePhase currentPhase = GamePhase.Startup;
    public static GamePhase previousPhase = GamePhase.CutScene;
    public static Canvas menuCanvas;
    public static Canvas leftCanvas;
    public static Canvas rightCanvas;
    public static Player rightPlayer;
    public static Player leftPlayer;
    public static FreeMoveFieldManager fmfm;
    public static GameObject fieldParent;
    public static bool launchCalled = false;
    public static bool stillTransitioning => NewStageManager.transitioning || CameraManager.camBrain.IsBlending || CameraManager.blending || MeshManager.transitioning || CutSceneManager.cutSceneOn;
    public static Sizes sizes;
    public static Side lastToLoseHealth;
    public static int goals = 0;
    public static int totalGoalsThresholdForStage => options.goalsThreshold * (int)PongBehaviour.currentStage;
    public static int remainingGoalsThresholdForStage => totalGoalsThresholdForStage - PongManager.goals;
}
