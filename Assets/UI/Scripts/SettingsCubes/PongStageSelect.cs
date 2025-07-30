using System.Collections;
using UnityEngine.EventSystems;
using DG.Tweening;
public class PongStageSelect : SettingSelectCube
{
    bool changedCurrentStage = false;
    public static int currentSelectedSetting = 1;
    protected override string currentSelectedString => ((Stage)currentSelectedSetting).ToString();
    protected override string unselectedString => "CURRENT STAGE" + "\n" + "<size=60%>" + PongBehaviour.currentStage.ToString();
    public override void OnPointerClick(PointerEventData eventData)
    {
        currentSelectedSetting = (int)PongBehaviour.currentStage;
        base.OnPointerClick(eventData);
    }
    public override void OnSubmit(BaseEventData eventData)
    {
        if (!usingCube && !release)
        {
            currentSelectedSetting = (int)PongBehaviour.currentStage;
        }
        base.OnSubmit(eventData);
    }
    protected IEnumerator CycleActivate()
    {
        changedCurrentStage = false;
        while (usingCube)
        {
            yield return null;
        }
        currentSelectedSetting = (int)PongBehaviour.currentStage;
        DeActivate();
    }
    protected override void SwitchSetting()
    {
        PongBehaviour.newStageManager.SelectNewStage(currentSelectedSetting);
        transform.DOComplete();
        usingCube = false;
        release = true;
        changedCurrentStage = true;
    }
    protected override void SwitchSide(Side dir)
    {
        currentSelectedSetting += dir == Side.Top || dir == Side.Right ? -1 : 1;
        if (currentSelectedSetting < 1) { currentSelectedSetting = System.Enum.GetValues(typeof(Stage)).Length - 1; }
        if (currentSelectedSetting >= System.Enum.GetValues(typeof(Stage)).Length) { currentSelectedSetting = 1; }
        transform.DOComplete();
        base.SwitchSide(dir);
        PongManager.am.PlayAudio(AudioType.UiSwitchFaces, transform.position);
    }
    protected override void DeActivate()
    {
        base.DeActivate();
        if (changedCurrentStage) {onClick.Invoke(); }
    }
}