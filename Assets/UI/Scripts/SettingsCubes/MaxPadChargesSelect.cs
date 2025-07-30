using System;
using System.Collections;
using UnityEngine.EventSystems;
using UnityEngine;
using DG.Tweening;
public class MaxPadChargesSelect : SettingSelectCube
{
    [Range(1, 20)]
    public static int currentSelectedSetting = PongManager.options.padMaxMagnetCharges;
    protected override string currentSelectedString => currentSelectedSetting.ToString();
    protected override string unselectedString => "MAX MAGNET CHARGES" + "\n" + "<size=60%>" + PongManager.options.padMaxMagnetCharges.ToString();
    public override void OnPointerClick(PointerEventData eventData)
    {
        currentSelectedSetting = PongManager.options.padMaxMagnetCharges;
        base.OnPointerClick(eventData);
    }
    public override void OnSubmit(BaseEventData eventData)
    {
        if (!usingCube && !release)
        {
            currentSelectedSetting = PongManager.options.padMaxMagnetCharges;
        }
        base.OnSubmit(eventData);
    }
    protected IEnumerator CycleActivate()
    {
        while (usingCube)
        {
            yield return null;
        }
        currentSelectedSetting = PongManager.options.padMaxMagnetCharges;
        DeActivate();  
    }
    protected override void SwitchSetting()
    {
        base.SwitchSetting();
        PongManager.options.padMaxMagnetCharges = currentSelectedSetting;
        transform.DOComplete();
        usingCube = false;
        release = true;
    }
    protected override void SwitchSide(Side dir)
    {
        currentSelectedSetting += dir == Side.Top || dir == Side.Right ? -1 : 1;
        if (currentSelectedSetting < 1) { currentSelectedSetting = 20; }
        if (currentSelectedSetting >= 21) { currentSelectedSetting = 1; }
        transform.DOComplete();
        base.SwitchSide(dir);
        PongManager.am.PlayAudio(AudioType.UiSwitchFaces, transform.position);
    }
}