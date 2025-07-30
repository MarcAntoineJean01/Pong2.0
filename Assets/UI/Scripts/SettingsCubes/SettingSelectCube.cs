using System.Collections;
using UnityEngine.EventSystems;
using UnityEngine;
using TMPro;
using DG.Tweening;
using System.Linq;

public class SettingSelectCube : PongUiCube
{
    protected virtual string currentSelectedString => "EMPTY";
    protected virtual string unselectedString => "EMPTY" + "\n" + "<size=40%>null";
    protected bool usingCube = false;
    protected bool release = false;
    PongPlayerControls cubeControls;
    protected Vector3 RotationForDirection(Side side)
    {
        switch (side)
        {
            case Side.Left:
                return new Vector3(0, -90, 0);
            case Side.Right:
                return new Vector3(0, 90, 0);
            case Side.Top:
                return new Vector3(90, 0, 0);
            case Side.Bottom:
                return new Vector3(-90, 0, 0);
            default:
                return Vector3.zero;
        }
    }
    protected GameObject NextSide(Side side)
    {
        switch (side)
        {
            case Side.Left:
                return sides.First(side => side.transform.position.x <= sides.Min(sd => sd.transform.position.x));
            case Side.Right:
                return sides.First(side => side.transform.position.x >= sides.Max(sd => sd.transform.position.x));
            case Side.Top:
                return sides.First(side => side.transform.position.y <= sides.Min(sd => sd.transform.position.y));
            case Side.Bottom:
                return sides.First(side => side.transform.position.y >= sides.Max(sd => sd.transform.position.y));
            default:
                return null;
        } 
    }
    protected override void OnEnable()
    {
        base.OnEnable();
        cubeControls = new PongPlayerControls();
        cubeControls.Disable();
        usingCube = false;
        foreach (GameObject side in sides)
        {
            side.GetComponentInChildren<TMP_Text>().text = unselectedString;
        }
    }
    public override void OnSubmit(BaseEventData eventData)
    {
        if (!usingCube && !release)
        {
            NavOff();
            usingCube = true;
            cubeControls.UiCubeControls.Enable();
            cubeControls.UiCubeControls.Up.performed += ctx => SwitchSide(Side.Bottom);
            cubeControls.UiCubeControls.Down.performed += ctx => SwitchSide(Side.Top);
            cubeControls.UiCubeControls.Left.performed += ctx => SwitchSide(Side.Right);
            cubeControls.UiCubeControls.Right.performed += ctx => SwitchSide(Side.Left);
            cubeControls.UiCubeControls.Cancel.performed += ctx => usingCube = false;
            cubeControls.UiCubeControls.Confirm.performed += ctx => SwitchSetting();
            StartCoroutine("CycleActiveGlow");
            sides[0].GetComponentInChildren<TMP_Text>().text = currentSelectedString;
            sqnc.OnComplete(() => StartCoroutine("CycleActivate"));
            sqnc.SetAutoKill(true);
            sqnc.Complete();
        }
        else if (!usingCube)
        {
            release = false;
        }
    }
    // protected Vector3 RotationForSplitScreen(Side side)
    // {
    //     switch (side)
    //     {
    //         case Side.Left:
    //             return  new Vector3(180, 0, 270);
    //         case Side.Right:
    //             return new Vector3(0, 0, 270);
    //         case Side.Top:
    //             return new Vector3(180, 0, 180);
    //         case Side.Bottom:
    //             return  new Vector3(270, 0, 90);
    //         default:
    //             return Vector3.zero;
    //     }
    // }
    protected virtual void SwitchSide(Side dir)
    {
        transform.DOComplete();
        PongManager.am.PlayAudio(AudioType.UiSwitchFaces, transform.position);
        NextSide(dir).transform.rotation = Quaternion.Euler(RotationForDirection(dir) * -1);
        transform.DORotate(RotationForDirection(dir), 0.5f, RotateMode.WorldAxisAdd);
        NextSide(dir).GetComponentInChildren<TMP_Text>().text = currentSelectedString;
    }
    // protected virtual void SwitchSide(Side dir)
    // {
    //     transform.DOComplete();
    //     PongManager.am.PlayAudio(AudioType.UiSwitchFaces, transform.position);
    //     NextSide(dir).transform.rotation = PongBehaviour.currentStage == Stage.Neon ? Quaternion.LookRotation(PongBehaviour.cm.leftPadCam.transform.position - PongBehaviour.um.metaCube.transform.position) * Quaternion.Euler(RotationForSplitScreen(dir) * -1) : Quaternion.Euler(RotationForDirection(dir) * -1);
    //     transform.DOLocalRotate(RotationForDirection(dir), 0.5f, RotateMode.WorldAxisAdd);
    //     NextSide(dir).GetComponentInChildren<TMP_Text>().text = currentSelectedString;
    // }
    protected virtual void SwitchSetting()
    {
        cubeControls.UiCubeControls.Disable();
    }
    public override void OnPointerClick(PointerEventData eventData)
    {
        StartCoroutine("CycleActiveGlow");
        sides[0].GetComponentInChildren<TMP_Text>().text = currentSelectedString;
        sqnc.OnComplete(() => StartCoroutine("CycleActivate"));
        sqnc.SetAutoKill(true);
    }
    IEnumerator CycleActiveGlow()
    {
        float t = 0f;
        while (t < fadeDuration)
        {
            t += Time.unscaledDeltaTime;
            if (t > fadeDuration) { t = fadeDuration; }
            mat.SetColor("_BaseColor", Color.Lerp(normalColor, pressedColor, t / fadeDuration));
            yield return null;
        }
    }
    protected virtual void DeActivate()
    {
        cubeControls.Disable();
        sides[0].transform.localRotation = Quaternion.Euler(Vector3.zero);
        sides[0].GetComponentInChildren<TMP_Text>().text = unselectedString;
        transform.localRotation = Quaternion.Euler(Vector3.zero);
        NavOn();
        StartCoroutine("CycleGlow");
    }
}
