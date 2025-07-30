using System.Collections;
using UnityEngine.EventSystems;
using DG.Tweening;
public class CutScenesSelect : SettingSelectCube
{
    public static bool currentSelectedSetting = PongManager.mainSettings.cutScenesOn;
    protected override string currentSelectedString => currentSelectedSetting ? "ON" : "OFF";
    protected override string unselectedString => "CUT SCENES" + "\n" + (currentSelectedSetting ? "<size=40%>ON" : "<size=40%>OFF");
    public override void OnPointerClick(PointerEventData eventData)
    {
        currentSelectedSetting = PongManager.mainSettings.cutScenesOn;
        base.OnPointerClick(eventData);
    }
    public override void OnSubmit(BaseEventData eventData)
    {
        if (!usingCube && !release)
        {
            currentSelectedSetting = PongManager.mainSettings.cutScenesOn;
        }
        base.OnSubmit(eventData);
    }
    protected IEnumerator CycleActivate()
    {
        while (usingCube)
        {
            yield return null;
        }
        currentSelectedSetting = PongManager.mainSettings.cutScenesOn;
        DeActivate();  
    }
    protected override void SwitchSetting()
    {
        PongManager.mainSettings.cutScenesOn = currentSelectedSetting;
        transform.DOComplete();
        usingCube = false;
        release = true;
    }
    protected override void SwitchSide(Side dir)
    {
        currentSelectedSetting = !currentSelectedSetting;
        transform.DOComplete();
        base.SwitchSide(dir);
        PongManager.am.PlayAudio(AudioType.UiSwitchFaces, transform.position);
    }
}