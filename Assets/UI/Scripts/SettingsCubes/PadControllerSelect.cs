using System.Collections;
using UnityEngine.EventSystems;
using UnityEngine;
using DG.Tweening;
public class PadControllerSelect : SettingSelectCube
{
    [SerializeField]
    Side padSide;
    public static int currentSelectedSetting = 0;
    protected override string currentSelectedString => ((PadController)currentSelectedSetting).ToString();
    protected override string unselectedString => padSide == Side.Left ? "LEFT PAD CONTROLLER" + "\n" + "<size=60%>" + PongManager.mainSettings.leftPlayerController.ToString() : "RIGHT PAD CONTROLLER" + "\n" + "<size=60%>" + PongManager.mainSettings.rightPlayerController.ToString();
    public override void OnPointerClick(PointerEventData eventData)
    {
        currentSelectedSetting = padSide == Side.Left ? (int)PongManager.mainSettings.leftPlayerController : (int)PongManager.mainSettings.rightPlayerController;
        base.OnPointerClick(eventData);
    }
    public override void OnSubmit(BaseEventData eventData)
    {
        if (!usingCube && !release)
        {
            currentSelectedSetting = padSide == Side.Left ? (int)PongManager.mainSettings.leftPlayerController : (int)PongManager.mainSettings.rightPlayerController;
        }
        base.OnSubmit(eventData);
    }
    protected IEnumerator CycleActivate()
    {
        while (usingCube)
        {
            yield return null;
        }
        currentSelectedSetting = padSide == Side.Left ? (int)PongManager.mainSettings.leftPlayerController : (int)PongManager.mainSettings.rightPlayerController;
        DeActivate();  
    }
    protected override void SwitchSetting()
    {
        if (padSide == Side.Left)
        {
            PongManager.mainSettings.leftPlayerController = (PlayerController)currentSelectedSetting;
        }
        else
        {
            PongManager.mainSettings.rightPlayerController = (PlayerController)currentSelectedSetting;
        }
        transform.DOComplete();
        usingCube = false;
        release = true;
    }
    protected override void SwitchSide(Side dir)
    {
        currentSelectedSetting += dir == Side.Top || dir == Side.Right ? -1 : 1;
        if (currentSelectedSetting < 0) { currentSelectedSetting = System.Enum.GetValues(typeof(PadController)).Length - 1; }
        if (currentSelectedSetting >= System.Enum.GetValues(typeof(PadController)).Length) { currentSelectedSetting = 0; }
        transform.DOComplete();
        base.SwitchSide(dir);
        PongManager.am.PlayAudio(AudioType.UiSwitchFaces, transform.position);
    }
}