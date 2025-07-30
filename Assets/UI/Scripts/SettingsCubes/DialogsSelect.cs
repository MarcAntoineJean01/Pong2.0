using System.Collections;
using UnityEngine.EventSystems;
using DG.Tweening;
public class DialogsSelect : SettingSelectCube
{
    public static bool currentSelectedSetting = PongManager.mainSettings.inGameDialogsOn;
    protected override string currentSelectedString => currentSelectedSetting ? "ON" : "OFF";
    protected override string unselectedString => "DIALOGS" + "\n" + (currentSelectedSetting ? "<size=60%>ON" : "<size=60%>OFF");
    public override void OnPointerClick(PointerEventData eventData)
    {
        currentSelectedSetting = PongManager.mainSettings.inGameDialogsOn;
        base.OnPointerClick(eventData);
    }
    public override void OnSubmit(BaseEventData eventData)
    {
        if (!usingCube && !release)
        {
            currentSelectedSetting = PongManager.mainSettings.inGameDialogsOn;
        }
        base.OnSubmit(eventData);
    }
    protected IEnumerator CycleActivate()
    {
        while (usingCube)
        {
            yield return null;
        }
        currentSelectedSetting = PongManager.mainSettings.inGameDialogsOn;
        DeActivate();  
    }
    protected override void SwitchSetting()
    {
        base.SwitchSetting();
        PongManager.mainSettings.inGameDialogsOn = currentSelectedSetting;
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