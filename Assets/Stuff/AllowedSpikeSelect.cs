using System.Collections;
using UnityEngine.EventSystems;
using UnityEngine;
using TMPro;
using DG.Tweening;
using System.Linq;
public class AllowedSpikeSelect : PongUiCube
{
    public SpikeType spikeType;
    public static bool currentSelectedSetting = true;
    bool usingCube = false;
    bool release = false;
    PongPlayerControls cubeControls;
    protected override void OnEnable()
    {
        base.OnEnable();
        cubeControls = new PongPlayerControls();
        cubeControls.Disable();
        usingCube = false;
        foreach (GameObject side in sides)
        {
            side.GetComponentInChildren<TMP_Text>().text = spikeType.ToString().Replace("Spike", "") + "\n" + (PongManager.options.allowedSpikes.GetAllowedSpike(spikeType) ? "<size=60%>ON" : "<size=60%>OFF");
        }
    }
    public override void OnPointerClick(PointerEventData eventData)
    {
        StartCoroutine("CycleActiveGlow");
        currentSelectedSetting = PongManager.options.allowedSpikes.GetAllowedSpike(spikeType);
        sides[0].GetComponentInChildren<TMP_Text>().text = currentSelectedSetting ? "ON" : "OFF";
        sqnc.OnComplete(() => StartCoroutine("CycleActivate"));
        sqnc.SetAutoKill(true);
    }
    public override void OnSubmit(BaseEventData eventData)
    {
        if (!usingCube && !release)
        {
            NavOff();
            usingCube = true;
            cubeControls.UiCubeControls.Enable();
            cubeControls.UiCubeControls.Up.performed += ctx => SwitchSide("Down");
            cubeControls.UiCubeControls.Down.performed += ctx => SwitchSide("Up");
            cubeControls.UiCubeControls.Left.performed += ctx => SwitchSide("Right");
            cubeControls.UiCubeControls.Right.performed += ctx => SwitchSide("Left");
            cubeControls.UiCubeControls.Cancel.performed += ctx => usingCube = false;
            cubeControls.UiCubeControls.Confirm.performed += ctx => SwitchSetting();
            StartCoroutine("CycleActiveGlow");
            currentSelectedSetting = PongManager.options.allowedSpikes.GetAllowedSpike(spikeType);
            sides[0].GetComponentInChildren<TMP_Text>().text = currentSelectedSetting ? "ON" : "OFF";
            sqnc.OnComplete(() => StartCoroutine("CycleActivate"));
            sqnc.SetAutoKill(true);
            sqnc.Complete();
        }
        else if (!usingCube)
        {
            release = false;
        }
    }
    IEnumerator CycleActiveGlow()
    {
        float t = 0f;
        while (t < fadeDuration)
        {
            t += Time.unscaledDeltaTime;
            if (t > fadeDuration) { t = fadeDuration; }
            mat.SetColor("_BaseColor", Color.Lerp(PongBehaviour.um.cubeNormalColor, PongBehaviour.um.cubePressedColor, t / fadeDuration));
            yield return null;
        }
    }
    protected IEnumerator CycleActivate()
    {
        while (usingCube)
        {
            yield return null;
        }
        cubeControls.Disable();
        sides[0].transform.localRotation = Quaternion.Euler(Vector3.zero);
        currentSelectedSetting = PongManager.options.allowedSpikes.GetAllowedSpike(spikeType);
        sides[0].GetComponentInChildren<TMP_Text>().text = spikeType.ToString().Replace("Spike", "") + "\n" + (currentSelectedSetting ? "<size=60%>ON" : "<size=60%>OFF");
        transform.localRotation = Quaternion.Euler(Vector3.zero);
        NavOn();
        StartCoroutine("CycleGlow");  
    }
    void SwitchSetting()
    {
        PongManager.options.allowedSpikes.SetAllowedSpike(spikeType, currentSelectedSetting);
        transform.DOComplete();
        usingCube = false;
        release = true;
    }
    void SwitchSide(string dir)
    {
        transform.DOComplete();
        PongManager.am.PlayAudio(AudioType.UiSwitchFaces, transform.position);
        currentSelectedSetting = !currentSelectedSetting;
        GameObject nextSide;
        switch (dir)
        {
            default:
            case "Up":
                nextSide = sides.First(side => side.transform.position.y <= sides.Min(sd => sd.transform.position.y));
                nextSide.transform.rotation = Quaternion.Euler(new Vector3(-90, 0, 0));
                transform.DORotate(new Vector3(90, 0, 0), 0.5f, RotateMode.WorldAxisAdd);
                break;
            case "Down":
                nextSide = sides.First(side => side.transform.position.y >= sides.Max(sd => sd.transform.position.y));
                nextSide.transform.rotation = Quaternion.Euler(new Vector3(90, 0, 0));
                transform.DORotate(new Vector3(-90, 0, 0), 0.5f, RotateMode.WorldAxisAdd);
                break;
            case "Left":
                nextSide = sides.First(side => side.transform.position.x <= sides.Min(sd => sd.transform.position.x));
                nextSide.transform.rotation = Quaternion.Euler(new Vector3(0, 90, 0));
                transform.DORotate(new Vector3(0, -90, 0), 0.5f, RotateMode.WorldAxisAdd);
                break;
            case "Right":
                nextSide = sides.First(side => side.transform.position.x >= sides.Max(sd => sd.transform.position.x));
                nextSide.transform.rotation = Quaternion.Euler(new Vector3(0, -90, 0));
                transform.DORotate(new Vector3(0, 90, 0), 0.5f, RotateMode.WorldAxisAdd);
                break;
        }
        nextSide.GetComponentInChildren<TMP_Text>().text = currentSelectedSetting ? "ON" : "OFF";
    }
}