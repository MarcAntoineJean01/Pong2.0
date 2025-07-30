using System.Collections;
using UnityEngine.EventSystems;
using DG.Tweening;
public class GameModeSelect : SettingSelectCube
{
    public static int currentSelectedSetting = (int)PongManager.mainSettings.gameMode;
    protected override string currentSelectedString => ((GameMode)currentSelectedSetting).ToString();
    protected override string unselectedString => "GAME MODE" + "\n" + "<size=60%>" + PongManager.mainSettings.gameMode.ToString();
    public override void OnPointerClick(PointerEventData eventData)
    {
        currentSelectedSetting = (int)PongManager.mainSettings.gameMode;
        base.OnPointerClick(eventData);
    }
    public override void OnSubmit(BaseEventData eventData)
    {
        if (!usingCube && !release)
        {
            currentSelectedSetting = (int)PongManager.mainSettings.gameMode;
        }
        base.OnSubmit(eventData);
    }
    protected IEnumerator CycleActivate()
    {
        while (usingCube)
        {
            yield return null;
        }
        currentSelectedSetting = (int)PongManager.mainSettings.gameMode;
        DeActivate();  
    }
    protected override void SwitchSetting()
    {
        base.SwitchSetting();
        PongManager.mainSettings.gameMode = (GameMode)currentSelectedSetting;
        transform.DOComplete();
        usingCube = false;
        release = true;
    }
    protected override void SwitchSide(Side dir)
    {
        currentSelectedSetting = (int)currentSelectedSetting > 0 ? 0 : 1;
        transform.DOComplete();
        base.SwitchSide(dir);
        PongManager.am.PlayAudio(AudioType.UiSwitchFaces, transform.position);
    }
}