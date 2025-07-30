using Cinemachine;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
public class CameraManager : PongManager
{
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
    public void SetupVirtualCameras()
    {
        if (mainSettings.gameMode == GameMode.Time)
        {
            SetupVirtualCamerasTimeGameMode();
        }
        else
        {
            SetupVirtualCamerasGoalsGameMode();
        }
    }
    public void SetupVirtualCamerasTimeGameMode()
    {
        virtualCams.Add(Stage.StartMenu, GameObject.Instantiate(virtualCamPrefab, transform).GetComponent<CinemachineVirtualCamera>());
        virtualCams[Stage.StartMenu].transform.position = CameraPosition(Stage.StartMenu);
        virtualCams[Stage.StartMenu].gameObject.name = "VirtualCam-StartMenu";
        virtualCams[Stage.StartMenu].gameObject.layer = LayerMask.NameToLayer("MainCamera");
        virtualCams[Stage.StartMenu].LookAt = field.background.transform;
        virtualCams[Stage.StartMenu].m_Lens.ModeOverride = LensSettings.OverrideModes.Perspective;
        virtualCams[Stage.StartMenu].m_Lens.FieldOfView = 25;
        virtualCams[Stage.StartMenu].gameObject.SetActive(false);

        virtualCams.Add(Stage.DD, GameObject.Instantiate(virtualCamPrefab, transform).GetComponent<CinemachineVirtualCamera>());
        virtualCams[Stage.DD].transform.position = CameraPosition(Stage.DD);
        virtualCams[Stage.DD].gameObject.name = "VirtualCam-DD";
        virtualCams[Stage.DD].gameObject.layer = LayerMask.NameToLayer("MainCamera");
        virtualCams[Stage.DD].LookAt = field.background.transform;
        virtualCams[Stage.DD].m_Lens.ModeOverride = LensSettings.OverrideModes.Perspective;
        virtualCams[Stage.DD].m_Lens.FieldOfView = 25;
        virtualCams[Stage.DD].gameObject.SetActive(true);
        activeVCam = virtualCams[Stage.DD];

        mainCam.cullingMask = mainCam.cullingMask & ~(1 << 7);
        mainCam.cullingMask = mainCam.cullingMask & ~(1 << 6);
        // mainCam.cullingMask = mainCam.cullingMask & ~(1 << 5);

        leftPadVCamStart = GameObject.Instantiate(virtualCamPrefab, transform).GetComponent<CinemachineVirtualCamera>();
        rightPadVCamStart = GameObject.Instantiate(virtualCamPrefab, transform).GetComponent<CinemachineVirtualCamera>();

        leftPadVCamStart.transform.position = rightPadVCamStart.transform.position = activeVCam.transform.position;
        leftPadVCamStart.LookAt = rightPadVCamStart.LookAt = field.background.transform;
        leftPadVCamStart.m_Lens.ModeOverride = rightPadVCamStart.m_Lens.ModeOverride = LensSettings.OverrideModes.Perspective;
        leftPadVCamStart.m_Lens.FieldOfView = rightPadVCamStart.m_Lens.FieldOfView = 12.73f;

        leftPadVCamStart.transform.rotation = Quaternion.Euler(-6.37f, 0, 0);
        rightPadVCamStart.transform.rotation = Quaternion.Euler(6.37f, 0, 0);

        leftPadVCamStart.gameObject.name = "LeftPadVirtualCamStart";
        rightPadVCamStart.gameObject.name = "RightPadVirtualCamStart";

        leftPadVCamStart.gameObject.layer = LayerMask.NameToLayer("PadCamLeft");
        rightPadVCamStart.gameObject.layer = LayerMask.NameToLayer("PadCamRight");
        leftPadCam.cullingMask = rightPadCam.cullingMask = leftPadCam.cullingMask & ~(1 << 8);
        // leftPadCam.cullingMask = rightPadCam.cullingMask = leftPadCam.cullingMask & ~(1 << 5);
        leftPadCam.cullingMask = leftPadCam.cullingMask & ~(1 << 7);
        rightPadCam.cullingMask = rightPadCam.cullingMask & ~(1 << 6);

        leftPadCam.rect = new Rect(0.0f, 0.5f, 1.0f - 0.0f, 1.0f);
        rightPadCam.rect = new Rect(0.0f, -0.5f, 1.0f - 0.0f, 1.0f);

        leftPadVCamStart.gameObject.SetActive(false);
        rightPadVCamStart.gameObject.SetActive(false);

        leftPadVCamEnd = GameObject.Instantiate(virtualCamPrefab, transform).GetComponent<CinemachineVirtualCamera>();
        rightPadVCamEnd = GameObject.Instantiate(virtualCamPrefab, transform).GetComponent<CinemachineVirtualCamera>();

        leftPadVCamEnd.m_Lens.Dutch = rightPadVCamEnd.m_Lens.Dutch = -90;
        leftPadVCamEnd.m_Lens.ModeOverride = rightPadVCamEnd.m_Lens.ModeOverride = LensSettings.OverrideModes.Perspective;
        leftPadVCamEnd.m_Lens.FieldOfView = rightPadVCamEnd.m_Lens.FieldOfView = 25;

        leftPadVCamEnd.gameObject.name = "LeftPadVirtualCamEnd";
        rightPadVCamEnd.gameObject.name = "RightPadVirtualCamEnd";

        leftPadVCamEnd.gameObject.SetActive(false);
        rightPadVCamEnd.gameObject.SetActive(false);

        leftPadVCamEnd.gameObject.layer = LayerMask.NameToLayer("PadCamLeft");
        rightPadVCamEnd.gameObject.layer = LayerMask.NameToLayer("PadCamRight");

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
    public void SetupVirtualCamerasGoalsGameMode()
    {
        for (int i = 0; i < System.Enum.GetValues(typeof(Stage)).Length; i++)
        {
            virtualCams.Add((Stage)i, GameObject.Instantiate(virtualCamPrefab, transform).GetComponent<CinemachineVirtualCamera>());
            virtualCams[(Stage)i].transform.position = CameraPosition((Stage)i);
            virtualCams[(Stage)i].gameObject.name = "VirtualCam-" + (Stage)i;
            virtualCams[(Stage)i].gameObject.layer = LayerMask.NameToLayer("MainCamera");
            switch ((Stage)i)
            {
                case Stage.StartMenu:
                    virtualCams[Stage.StartMenu].LookAt = field.background.transform;
                    virtualCams[Stage.StartMenu].m_Lens.ModeOverride = LensSettings.OverrideModes.Perspective;
                    virtualCams[Stage.StartMenu].m_Lens.FieldOfView = 25;
                    virtualCams[Stage.StartMenu].gameObject.SetActive(false);
                    break;
                case Stage.DD:
                    virtualCams[Stage.DD].LookAt = field.background.transform;
                    virtualCams[Stage.DD].m_Lens.ModeOverride = LensSettings.OverrideModes.Perspective;
                    virtualCams[Stage.DD].m_Lens.FieldOfView = 25;
                    virtualCams[Stage.DD].gameObject.SetActive(true);
                    activeVCam = virtualCams[Stage.DD];
                    break;
                case Stage.DDD:
                    virtualCams[Stage.DDD].LookAt = field.background.transform;
                    virtualCams[Stage.DDD].m_Lens.ModeOverride = LensSettings.OverrideModes.Perspective;
                    virtualCams[Stage.DDD].m_Lens.FieldOfView = 25;
                    virtualCams[Stage.DDD].gameObject.SetActive(false);
                    break;
                case Stage.Universe:
                    virtualCams[Stage.Universe].LookAt = field.background.transform;
                    virtualCams[Stage.Universe].m_Lens.ModeOverride = LensSettings.OverrideModes.Perspective;
                    virtualCams[Stage.Universe].m_Lens.FieldOfView = 25;
                    virtualCams[Stage.Universe].gameObject.SetActive(false);
                    break;
                case Stage.GravityWell:
                    virtualCams[Stage.GravityWell].LookAt = field.background.transform;
                    virtualCams[Stage.GravityWell].m_Lens.ModeOverride = LensSettings.OverrideModes.Perspective;
                    virtualCams[Stage.GravityWell].m_Lens.FieldOfView = 25;
                    virtualCams[Stage.GravityWell].gameObject.SetActive(false);
                    break;
                case Stage.FreeMove:
                    virtualCams[Stage.FreeMove].LookAt = field.background.transform;
                    virtualCams[Stage.FreeMove].m_Lens.ModeOverride = LensSettings.OverrideModes.Perspective;
                    virtualCams[Stage.FreeMove].m_Lens.FieldOfView = 25;
                    virtualCams[Stage.FreeMove].gameObject.SetActive(false);
                    break;
                case Stage.FireAndIce:
                    virtualCams[Stage.FireAndIce].LookAt = field.background.transform;
                    virtualCams[Stage.FireAndIce].m_Lens.ModeOverride = LensSettings.OverrideModes.Perspective;
                    virtualCams[Stage.FireAndIce].m_Lens.FieldOfView = 25;
                    virtualCams[Stage.FireAndIce].gameObject.SetActive(false);
                    break;
                case Stage.Neon:
                    virtualCams[Stage.Neon].LookAt = field.background.transform;
                    virtualCams[Stage.Neon].m_Lens.ModeOverride = LensSettings.OverrideModes.Perspective;
                    virtualCams[Stage.Neon].m_Lens.FieldOfView = 25;
                    virtualCams[Stage.Neon].gameObject.SetActive(false);
                    mainCam.cullingMask = mainCam.cullingMask & ~(1 << 7);
                    mainCam.cullingMask = mainCam.cullingMask & ~(1 << 6);

                    leftPadVCamStart = GameObject.Instantiate(virtualCamPrefab, transform).GetComponent<CinemachineVirtualCamera>();
                    rightPadVCamStart = GameObject.Instantiate(virtualCamPrefab, transform).GetComponent<CinemachineVirtualCamera>();

                    leftPadVCamStart.transform.position = rightPadVCamStart.transform.position = CameraPosition((Stage)i);
                    leftPadVCamStart.LookAt = rightPadVCamStart.LookAt = field.background.transform;
                    leftPadVCamStart.m_Lens.ModeOverride = rightPadVCamStart.m_Lens.ModeOverride = LensSettings.OverrideModes.Perspective;
                    leftPadVCamStart.m_Lens.FieldOfView = rightPadVCamStart.m_Lens.FieldOfView = 12.5f;

                    leftPadVCamStart.transform.rotation = Quaternion.Euler(-6.25f, 0, 0);
                    rightPadVCamStart.transform.rotation = Quaternion.Euler(6.25f, 0, 0);

                    leftPadVCamStart.gameObject.name = "LeftPadVirtualCamStart";
                    rightPadVCamStart.gameObject.name = "RightPadVirtualCamStart";

                    leftPadVCamStart.gameObject.layer = LayerMask.NameToLayer("PadCamLeft");
                    rightPadVCamStart.gameObject.layer = LayerMask.NameToLayer("PadCamRight");
                    leftPadCam.cullingMask = rightPadCam.cullingMask = leftPadCam.cullingMask & ~(1 << 8);
                    leftPadCam.cullingMask = leftPadCam.cullingMask & ~(1 << 7);
                    rightPadCam.cullingMask = rightPadCam.cullingMask & ~(1 << 6);

                    leftPadCam.rect = new Rect(0.0f, 0.5f, 1.0f - 0.0f, 1.0f);
                    rightPadCam.rect = new Rect(0.0f, -0.5f, 1.0f - 0.0f, 1.0f);

                    leftPadVCamStart.gameObject.SetActive(false);
                    rightPadVCamStart.gameObject.SetActive(false);

                    leftPadVCamEnd = GameObject.Instantiate(virtualCamPrefab, transform).GetComponent<CinemachineVirtualCamera>();
                    rightPadVCamEnd = GameObject.Instantiate(virtualCamPrefab, transform).GetComponent<CinemachineVirtualCamera>();

                    leftPadVCamEnd.m_Lens.Dutch = rightPadVCamEnd.m_Lens.Dutch = -90;
                    leftPadVCamEnd.m_Lens.ModeOverride = rightPadVCamEnd.m_Lens.ModeOverride = LensSettings.OverrideModes.Perspective;
                    leftPadVCamEnd.m_Lens.FieldOfView = rightPadVCamEnd.m_Lens.FieldOfView = 25;

                    leftPadVCamEnd.gameObject.name = "LeftPadVirtualCamEnd";
                    rightPadVCamEnd.gameObject.name = "RightPadVirtualCamEnd";

                    leftPadVCamEnd.gameObject.SetActive(false);
                    rightPadVCamEnd.gameObject.SetActive(false);

                    leftPadVCamEnd.gameObject.layer = LayerMask.NameToLayer("PadCamLeft");
                    rightPadVCamEnd.gameObject.layer = LayerMask.NameToLayer("PadCamRight");

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
                    break;
                case Stage.Final:
                    virtualCams[Stage.Final].LookAt = field.background.transform;
                    virtualCams[Stage.Final].m_Lens.ModeOverride = LensSettings.OverrideModes.Perspective;
                    virtualCams[Stage.Final].m_Lens.FieldOfView = 25;
                    virtualCams[Stage.Final].gameObject.SetActive(false);
                    break;
            }
        }
    }
    public void TransitionCamera()
    {
        activeVCam.gameObject.SetActive(false);
        activeVCam = virtualCams[nextStage];
        activeVCam.gameObject.SetActive(true);
    }
    public void TurnOnPadCams()
    {
        activeVCam.gameObject.SetActive(false);
        MovePadCamsToStart();
        rightPadCam.gameObject.SetActive(true);
        leftPadCam.gameObject.SetActive(true);
        mainCam.gameObject.SetActive(false);
    }
    public void MovePadCamsToStart()
    {
        rightPadVCamStart.gameObject.SetActive(true);
        leftPadVCamStart.gameObject.SetActive(true);
        rightPadVCamEnd.gameObject.SetActive(false);
        leftPadVCamEnd.gameObject.SetActive(false);
    }
    public void MovePadCamsToEnd()
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
    public void TurnOffPadCams()
    {
        rightPadVCamEnd.gameObject.SetActive(false);
        leftPadVCamEnd.gameObject.SetActive(false);
        rightPadVCamStart.gameObject.SetActive(false);
        leftPadVCamStart.gameObject.SetActive(false);
        rightPadCam.gameObject.SetActive(false);
        leftPadCam.gameObject.SetActive(false);
        activeVCam.gameObject.SetActive(true);
        mainCam.gameObject.SetActive(true);
    }
    public void PadCameraNoise(CinemachineVirtualCamera vcam)
    {
        StartCoroutine(CyclePadCameraNoise(vcam));
    }
    public void MoveToNextCamera()
    {
        StopCoroutine("CycleMoveToNextCamera");
        StartCoroutine("CycleMoveToNextCamera");
    }
    IEnumerator CyclePadCameraNoise(CinemachineVirtualCamera vcam)
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
    IEnumerator CycleMainCameraNoise(CinemachineVirtualCamera vcam, float duration)
    {
        CinemachineBasicMultiChannelPerlin noise = vcam.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();
        noise.m_AmplitudeGain = 1;
        noise.m_FrequencyGain = 1;
        float t = 0f;
        while (t < duration)
        {
            t += Time.deltaTime;
            if (t > pm.gameEffects.cameraNoiseDuration) { t = duration; }
            noise.m_AmplitudeGain = Mathf.SmoothStep(1, 0, t / duration);
            noise.m_FrequencyGain = Mathf.SmoothStep(1, 0, t / duration);
            yield return null;
        }
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
            TurnOnPadCams();
            yield return null;
            MovePadCamsToEnd();
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
        else if (currentStage == Stage.Neon)
        {
            CinemachineBrain leftBrain = leftPadCam.GetComponent<CinemachineBrain>();
            CinemachineBrain rightBrain = rightPadCam.GetComponent<CinemachineBrain>();
            float camThresholdX = sizes.fieldWidth * 0.5f;// *0.5f === /2
            MovePadCamsToStart();
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
            TurnOffPadCams();
        }
        if (mainSettings.gameMode == GameMode.Goals)
        {
            TransitionCamera();            
        }
        yield return null;
        while (CameraManager.camBrain.IsBlending) { yield return null; }
        blending = false;
    }
}
