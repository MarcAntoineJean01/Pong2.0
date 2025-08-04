using Cinemachine;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
public class CameraManager : PongManager
{
    [SerializeField] public CameraMasks layerMasks;
    public static bool blending = false;
    public Camera mainCam;
    public Camera leftPadCam;
    public Camera rightPadCam;
    public Camera overlayCam;
    public static CinemachineBrain camBrain;
    public GameObject virtualCamPrefab;
    public Dictionary<Stage, CinemachineVirtualCamera> virtualCams = new Dictionary<Stage, CinemachineVirtualCamera>();
    public static CinemachineVirtualCamera activeVCam;
    public static CinemachineVirtualCamera leftPadVCamStart;
    public static CinemachineVirtualCamera rightPadVCamStart;
    public static CinemachineVirtualCamera leftPadVCamEnd;
    public static CinemachineVirtualCamera rightPadVCamEnd;
    private Coroutine mainCameraNoiseCoroutine;
    public void SwapMainCameraMask(bool freeMove)
    {
        mainCam.cullingMask = freeMove ? layerMasks.mainFreeMove : layerMasks.mainDefault;
    }
    public void MainCameraSetup(bool turnOnFirstStageCamera = true)
    {
        SetUpCameras();
        SetUpSplitScreenVirtualCameras();
        if (mainSettings.gameMode == GameMode.Time)
        {
            SetupVirtualCamera(Stage.StartMenu);
            SetupVirtualCamera(Stage.DD);
        }
        else
        {
            for (int i = 0; i < System.Enum.GetValues(typeof(Stage)).Length; i++)
            {
                switch ((Stage)i)
                {
                    case Stage.Neon:
                        break;
                    default:
                        SetupVirtualCamera((Stage)i);
                        break;
                }
            }
        }
        if (turnOnFirstStageCamera)
        {
            virtualCams[Stage.DD].gameObject.SetActive(true);
            activeVCam = virtualCams[Stage.DD];
        }
    }
    public Vector3 CameraPosition(Stage stage)
    {
        switch (stage)
        {
            default:
            case Stage.DD:
                return new Vector3(0, 0, sizes.fieldDepth * 0.5f); // *0.5f === /2
            case Stage.DDD:
                return new Vector3(0, 0, sizes.fieldDepth * 0.5f - sizes.ballDiameter * 2);
            case Stage.Universe:
                return new Vector3(0, 0, sizes.fieldDepth * 0.25f); // *0.25f === /4
            case Stage.GravityWell:
                return new Vector3(0, 0, sizes.fieldDepth * 0.20f); // *0.20f === /5
            case Stage.FreeMove:
                return new Vector3(0, 0, -(sizes.fieldDepth * 0.20f)); // *0.20f === /5
            case Stage.FireAndIce:
                return new Vector3(0, 0, -(sizes.fieldDepth * 0.25f)); // *0.25f === /4
            case Stage.Neon:
                return new Vector3(0, 0, -(sizes.fieldDepth * 0.3333f)); // *0.3333f === /3
            case Stage.Final:
                return new Vector3(0, 0, -(sizes.fieldDepth * 0.5f)); // *0.5f === /2
        }
    }
    void SetUpCameras()
    {
        mainCam.cullingMask = layerMasks.mainDefault;
        overlayCam.cullingMask = layerMasks.overlay;
        leftPadCam.cullingMask = layerMasks.leftPad;
        rightPadCam.cullingMask = layerMasks.rightPad;
        leftPadCam.rect = new Rect(0.0f, 0.5f, 1.0f - 0.0f, 1.0f);
        rightPadCam.rect = new Rect(0.0f, -0.5f, 1.0f - 0.0f, 1.0f);
    }
    void SetUpSplitScreenVirtualCameras()
    {
        leftPadVCamStart = GameObject.Instantiate(virtualCamPrefab, transform).GetComponent<CinemachineVirtualCamera>();
        rightPadVCamStart = GameObject.Instantiate(virtualCamPrefab, transform).GetComponent<CinemachineVirtualCamera>();
        leftPadVCamEnd = GameObject.Instantiate(virtualCamPrefab, transform).GetComponent<CinemachineVirtualCamera>();
        rightPadVCamEnd = GameObject.Instantiate(virtualCamPrefab, transform).GetComponent<CinemachineVirtualCamera>();

        leftPadVCamStart.LookAt = rightPadVCamStart.LookAt = field.background.transform;
        leftPadVCamStart.m_Lens.ModeOverride = rightPadVCamStart.m_Lens.ModeOverride = LensSettings.OverrideModes.Perspective;
        leftPadVCamStart.m_Lens.FieldOfView = rightPadVCamStart.m_Lens.FieldOfView = 12.73f;

        leftPadVCamStart.transform.rotation = Quaternion.Euler(-6.37f, 0, 0);
        rightPadVCamStart.transform.rotation = Quaternion.Euler(6.37f, 0, 0);

        leftPadVCamStart.gameObject.name = "LeftPadVirtualCamStart";
        rightPadVCamStart.gameObject.name = "RightPadVirtualCamStart";
        leftPadVCamEnd.gameObject.name = "LeftPadVirtualCamEnd";
        rightPadVCamEnd.gameObject.name = "RightPadVirtualCamEnd";

        leftPadVCamStart.gameObject.layer = leftPadVCamEnd.gameObject.layer = LayerMask.NameToLayer("PadCamLeft");
        rightPadVCamStart.gameObject.layer = rightPadVCamEnd.gameObject.layer = LayerMask.NameToLayer("PadCamRight");

        leftPadVCamStart.gameObject.SetActive(false);
        rightPadVCamStart.gameObject.SetActive(false);
        leftPadVCamEnd.gameObject.SetActive(false);
        rightPadVCamEnd.gameObject.SetActive(false);

        leftPadVCamEnd.m_Lens.Dutch = rightPadVCamEnd.m_Lens.Dutch = -90;
        leftPadVCamEnd.m_Lens.ModeOverride = rightPadVCamEnd.m_Lens.ModeOverride = LensSettings.OverrideModes.Perspective;
        leftPadVCamEnd.m_Lens.FieldOfView = rightPadVCamEnd.m_Lens.FieldOfView = 25;

        CinemachinePOV povL = leftPadVCamEnd.AddCinemachineComponent<CinemachinePOV>();
        CinemachinePOV povR = rightPadVCamEnd.AddCinemachineComponent<CinemachinePOV>();

        povL.m_HorizontalAxis.m_Wrap = povR.m_HorizontalAxis.m_Wrap = false;
        povL.m_HorizontalAxis.m_MaxSpeed = povR.m_HorizontalAxis.m_MaxSpeed = 50;
        povL.m_HorizontalAxis.m_AccelTime = povR.m_HorizontalAxis.m_AccelTime = 0.5f;
        povL.m_HorizontalAxis.m_DecelTime = povR.m_HorizontalAxis.m_DecelTime = 0.1f;
        povL.m_VerticalAxis.m_MinValue = povR.m_VerticalAxis.m_MinValue = -1;
        povL.m_VerticalAxis.m_MaxValue = povR.m_VerticalAxis.m_MaxValue = 1;
        povL.m_VerticalAxis.m_Wrap = povR.m_VerticalAxis.m_Wrap = false;
        povL.m_VerticalAxis.m_MaxSpeed = povR.m_VerticalAxis.m_MaxSpeed = 50;
        povL.m_VerticalAxis.m_AccelTime = povR.m_VerticalAxis.m_AccelTime = 0.5f;
        povL.m_VerticalAxis.m_DecelTime = povR.m_VerticalAxis.m_DecelTime = 0.1f;

        povL.m_HorizontalAxis.m_MinValue = povL.m_HorizontalAxis.Value = 77f;
        povL.m_HorizontalAxis.m_MaxValue = 78f;
        povL.m_HorizontalAxis.m_InputAxisName = "PovVertical";
        povL.m_VerticalAxis.m_InputAxisName = "PovHorizontal";

        povR.m_HorizontalAxis.m_MinValue = povR.m_HorizontalAxis.Value = -103f;
        povR.m_HorizontalAxis.m_MaxValue = -102f;
        povR.m_HorizontalAxis.m_InputAxisName = "PovVertical1";
        povR.m_VerticalAxis.m_InputAxisName = "PovHorizontal1";

        Cinemachine3rdPersonFollow thirdPL = leftPadVCamEnd.AddCinemachineComponent<Cinemachine3rdPersonFollow>();
        Cinemachine3rdPersonFollow thirdPR = rightPadVCamEnd.AddCinemachineComponent<Cinemachine3rdPersonFollow>();

        thirdPL.CameraDistance = thirdPR.CameraDistance = sizes.planeDistance * 0.3333f;// *0.3333f === /3
        thirdPL.ShoulderOffset = thirdPR.ShoulderOffset = new Vector3(-15, 0, 0);
    }
    void SetupVirtualCamera(Stage stage)
    {
        virtualCams.Add(stage, GameObject.Instantiate(virtualCamPrefab, transform).GetComponent<CinemachineVirtualCamera>());
        virtualCams[stage].transform.position = CameraPosition(stage);
        virtualCams[stage].gameObject.name = "VirtualCam-" + stage;
        virtualCams[stage].gameObject.layer = LayerMask.NameToLayer("MainCamera");
        virtualCams[stage].LookAt = field.background.transform;
        virtualCams[stage].m_Lens.ModeOverride = LensSettings.OverrideModes.Perspective;
        virtualCams[stage].m_Lens.FieldOfView = 25;
        virtualCams[stage].gameObject.SetActive(false);
    }
    public void MoveSplitScreenVirtualCamerasToStartPosition()
    {
        rightPadVCamStart.gameObject.SetActive(true);
        leftPadVCamStart.gameObject.SetActive(true);
        rightPadVCamEnd.gameObject.SetActive(false);
        leftPadVCamEnd.gameObject.SetActive(false);
    }
    public void MoveSplitScreenVirtualCamerasToEndPosition()
    {
        rightPadVCamStart.gameObject.SetActive(false);
        leftPadVCamStart.gameObject.SetActive(false);

        leftPadVCamEnd.Follow = field.leftPad.transform;
        leftPadVCamEnd.LookAt = field.leftPad.transform;
        rightPadVCamEnd.Follow = field.rightPad.transform;
        rightPadVCamEnd.LookAt = field.rightPad.transform;

        rightPadVCamEnd.gameObject.SetActive(true);
        leftPadVCamEnd.gameObject.SetActive(true);

    }
    public void SplitScreenCameraNoise(CinemachineVirtualCamera vcam)
    {
        StartCoroutine(CycleSplitScreenCameraNoise(vcam));
    }
    public void MoveToNextCamera()
    {
        StopCoroutine("CycleMoveToNextCamera");
        StartCoroutine("CycleMoveToNextCamera");
    }
    public void MainCameraNoise(CinemachineVirtualCamera vcam, float amplitudeGain, float frequencyGain, float duration)
    {
        KillCameraNoise(vcam);
        mainCameraNoiseCoroutine = StartCoroutine(CycleMainCameraNoise(vcam, amplitudeGain, frequencyGain, duration));
    }
    IEnumerator CycleSplitScreenCameraNoise(CinemachineVirtualCamera vcam)
    {
        CinemachineBasicMultiChannelPerlin noise = vcam.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();
        noise.m_AmplitudeGain = 1;
        noise.m_FrequencyGain = 1;
        float t = 0f;
        yield return new WaitForSecondsRealtime(pm.gameEffects.cameraNoiseDuration * 0.5f);
        t = 0f;
        while (t < pm.gameEffects.cameraNoiseDuration * 0.3333f)
        {
            t += Time.deltaTime;
            if (t > pm.gameEffects.cameraNoiseDuration) { t = pm.gameEffects.cameraNoiseDuration * 0.5f; }
            noise.m_AmplitudeGain = Mathf.SmoothStep(1, 0, t / (pm.gameEffects.cameraNoiseDuration * 0.5f));
            noise.m_FrequencyGain = Mathf.SmoothStep(1, 0, t / (pm.gameEffects.cameraNoiseDuration * 0.5f));
            yield return null;
        }
        noise.m_AmplitudeGain = 0;
        noise.m_FrequencyGain = 0;
    }
    IEnumerator CycleMainCameraNoise(CinemachineVirtualCamera vcam, float amplitudeGain, float frequencyGain, float duration)
    {
        CinemachineBasicMultiChannelPerlin noise = vcam.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();
        noise.m_AmplitudeGain = amplitudeGain;
        noise.m_FrequencyGain = frequencyGain;
        float t = 0f;
        while (t < duration)
        {
            t += Time.deltaTime;
            if (t > pm.gameEffects.cameraNoiseDuration) { t = duration; }
            noise.m_AmplitudeGain = Mathf.SmoothStep(amplitudeGain, 0, t / duration);
            noise.m_FrequencyGain = Mathf.SmoothStep(frequencyGain, 0, t / duration);
            yield return null;
        }
        noise.m_AmplitudeGain = 0;
        noise.m_FrequencyGain = 0;
        mainCameraNoiseCoroutine = null;
    }
    public void KillCameraNoise(CinemachineVirtualCamera vcam)
    {
        if (mainCameraNoiseCoroutine != null)
        {
            StopCoroutine(mainCameraNoiseCoroutine);
            mainCameraNoiseCoroutine = null;
        }
        CinemachineBasicMultiChannelPerlin noise = vcam.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();
        noise.m_AmplitudeGain = 0;
        noise.m_FrequencyGain = 0;
    }
    IEnumerator CycleMoveToNextCamera()
    {
        blending = true;
        if (nextStage == Stage.Neon)
        {
            mm.TransitionFieldMeshes();
#if !UNITY_WEBGL
            field.leftWall.SwitchNeonStage();
            field.rightWall.SwitchNeonStage();
            field.topFloor.SwitchNeonStage();
            field.bottomFloor.SwitchNeonStage();
#endif
            activeVCam.gameObject.SetActive(false);
            leftPadVCamStart.transform.position = rightPadVCamStart.transform.position = activeVCam.transform.position;
            MoveSplitScreenVirtualCamerasToStartPosition();
            rightPadCam.gameObject.SetActive(true);
            leftPadCam.gameObject.SetActive(true);
            mainCam.gameObject.SetActive(false);
            yield return null;

            MoveSplitScreenVirtualCamerasToEndPosition();
            yield return null;
            CinemachineBrain leftBrain = leftPadCam.GetComponent<CinemachineBrain>();
            CinemachineBrain rightBrain = rightPadCam.GetComponent<CinemachineBrain>();
            float camThresholdX = sizes.fieldWidth * 0.5f;// *0.5f === /2
            while (leftBrain.IsBlending || rightBrain.IsBlending)
            {
                if (leftPadCam.transform.position.x < -camThresholdX && field.leftWall.gameObject.layer != LayerMask.NameToLayer("PadCamRight"))
                {
                    field.leftWall.gameObject.layer = LayerMask.NameToLayer("PadCamRight");
                }
                if (rightPadCam.transform.position.x > camThresholdX && field.rightWall.gameObject.layer != LayerMask.NameToLayer("PadCamLeft"))
                {
                    field.rightWall.gameObject.layer = LayerMask.NameToLayer("PadCamLeft");
                }
                yield return null;
            }
        }
        else if (leftPadCam.gameObject.activeSelf || leftPadCam.gameObject.activeSelf)
        {
            CinemachineBrain leftBrain = leftPadCam.GetComponent<CinemachineBrain>();
            CinemachineBrain rightBrain = rightPadCam.GetComponent<CinemachineBrain>();
            float camThresholdX = sizes.fieldWidth * 0.5f;// *0.5f === /2
            MoveSplitScreenVirtualCamerasToStartPosition();
            yield return null;
            while (leftBrain.IsBlending || rightBrain.IsBlending)
            {
                if (leftPadCam.transform.position.x > -camThresholdX && field.leftWall.gameObject.layer != LayerMask.NameToLayer("Default"))
                {
                    field.leftWall.gameObject.layer = LayerMask.NameToLayer("Default");
                }
                if (rightPadCam.transform.position.x < camThresholdX && field.rightWall.gameObject.layer != LayerMask.NameToLayer("Default"))
                {
                    field.rightWall.gameObject.layer = LayerMask.NameToLayer("Default");
                }
                yield return null;
            }
            mm.TransitionFieldMeshes(false);
#if !UNITY_WEBGL
            field.leftWall.SwitchNeonStage(false);
            field.rightWall.SwitchNeonStage(false);
            field.topFloor.SwitchNeonStage(false);
            field.bottomFloor.SwitchNeonStage(false);
#endif
            rightPadVCamEnd.gameObject.SetActive(false);
            leftPadVCamEnd.gameObject.SetActive(false);
            rightPadVCamStart.gameObject.SetActive(false);
            leftPadVCamStart.gameObject.SetActive(false);
            rightPadCam.gameObject.SetActive(false);
            leftPadCam.gameObject.SetActive(false);

            activeVCam.gameObject.SetActive(true);
            mainCam.gameObject.SetActive(true);
        }
        if (mainSettings.gameMode == GameMode.Goals)
        {
            activeVCam.gameObject.SetActive(false);
            activeVCam = virtualCams[nextStage];
            activeVCam.gameObject.SetActive(true);        
        }
        yield return null;
        while (CameraManager.camBrain.IsBlending) { yield return null; }
        blending = false;
    }
}
