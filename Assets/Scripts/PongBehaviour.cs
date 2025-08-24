using UnityEngine;
using System;
using UnityEngine.Events;
using PongLocker;
using FieldLocker;
public class PongBehaviour : MonoBehaviour
{
    [Serializable]
    public class DisplayHud : UnityEvent<Side> { }
    [SerializeField]
    protected static DisplayHud m_DisplayHud = new DisplayHud();
    public static DisplayHud displayHud
    {
        get { return m_DisplayHud; }
        set { m_DisplayHud = value; }
    }
    public static NewGameManager newGameManager;
    public static NewStageManager newStageManager;
    public static UiManager um;
    public static Builder builder;
    public static CameraManager cm;
    public static MeshManager mm;
    public static ParamsManager pm;
    public static DialogManager dm;
    public static CutSceneManager csm;
    public static LightsManager lm;
    public static VFX vfx;
    public static PVE pve;
    public static AudioManager am;
    public static Field field;
    public static Stage currentStage;
    public static Stage previousStage;
    public static Stage nextStage;
    public static float stagePosZ
    {
        get
        {
            if (PongManager.mainSettings.gameMode == GameMode.Time || PongManager.mainSettings.gameMode == GameMode.NonStop)
            {
                if (currentStage <= Stage.DD)
                {
                    return PongManager.sizes.planeDistance + PongManager.sizes.fieldDepth * 0.5f - PongManager.sizes.ballDiameter * 1.5f;
                }
                else
                {
                    return PongManager.sizes.fieldDepth * 0.5f + PongManager.sizes.planeDistance + PongManager.sizes.ballDiameter * 0.5f;
                }
            }
            else
            {
                switch (currentStage)
                {
                    case Stage.StartMenu:
                    case Stage.DD:
                        return PongManager.sizes.planeDistance + PongManager.sizes.fieldDepth * 0.5f - PongManager.sizes.ballDiameter * 1.5f;
                    case Stage.FreeMove:
                        // *0.20f === /5
                        return PongManager.sizes.planeDistance + PongManager.sizes.fieldDepth * 0.20f - PongManager.sizes.ballDiameter * 1.5f;
                    default:
                        return cm.CameraPosition(currentStage).z + PongManager.sizes.planeDistance + PongManager.sizes.ballDiameter * 0.5f;
                }
            }
        }
    }
    public static float nextStagePosZ
    {
        get
        {
            if (PongManager.mainSettings.gameMode == GameMode.Time || PongManager.mainSettings.gameMode == GameMode.NonStop)
            {
                if (nextStage <= Stage.DD)
                {
                    return PongManager.sizes.planeDistance + PongManager.sizes.fieldDepth * 0.5f - PongManager.sizes.ballDiameter * 1.5f;
                }
                else
                {
                    return PongManager.sizes.fieldDepth * 0.5f + PongManager.sizes.planeDistance + PongManager.sizes.ballDiameter * 0.5f;
                }
            }
            else
            {
                switch (nextStage)
                {
                    case Stage.StartMenu:
                    case Stage.DD:
                        return PongManager.sizes.planeDistance + PongManager.sizes.fieldDepth * 0.5f - PongManager.sizes.ballDiameter * 1.5f;
                    case Stage.FreeMove:
                        // *0.20f === /5
                        return PongManager.sizes.planeDistance + PongManager.sizes.fieldDepth * 0.20f - PongManager.sizes.ballDiameter * 1.5f;
                    default:
                        return cm.CameraPosition(nextStage).z + PongManager.sizes.planeDistance + PongManager.sizes.ballDiameter * 0.5f;
                }
            }
        }
    }
    public static bool pauseCalled = false;
}
