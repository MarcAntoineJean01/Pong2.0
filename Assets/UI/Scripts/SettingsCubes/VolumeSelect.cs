using System;
using System.Collections;
using UnityEngine.EventSystems;
using UnityEngine;
using DG.Tweening;
public class VolumeSelect : SettingSelectCube
{
    [Range(0, 1)]
    public float currentSelectedSetting = 0;
    [Range(0, 2)]
    public int volumeIndex;
    protected override string currentSelectedString => ((int)(currentSelectedSetting*10)).ToString();
    protected override string unselectedString
    {
        get
        {
            switch (volumeIndex)
            {
                default:
                case 0:
                    return "MAIN VOLUME" + "\n" + "<size=60%>" + ((int)(PongManager.options.soundVolume * 10)).ToString();
                case 1:
                    return "MUSIC VOLUME" + "\n" + "<size=60%>" + ((int)(PongManager.options.m_MusicVolume * 10)).ToString();
                case 2:
                    return "EFFECTS VOLUME" + "\n" + "<size=60%>" + ((int)(PongManager.options.m_EffectsVolume * 10)).ToString();
            }
        }
    }
    public override void OnPointerClick(PointerEventData eventData)
    {
        switch (volumeIndex)
        {
            case 0:
                currentSelectedSetting = PongManager.options.soundVolume;
                break;
            case 1:
                currentSelectedSetting = PongManager.options.m_MusicVolume;
                break;
            case 2:
                currentSelectedSetting = PongManager.options.m_EffectsVolume;
                break;
        }
        base.OnPointerClick(eventData);
    }
    public override void OnSubmit(BaseEventData eventData)
    {
        if (!usingCube && !release)
        {
            switch (volumeIndex)
            {
                case 0:
                    currentSelectedSetting = PongManager.options.soundVolume;
                    break;
                case 1:
                    currentSelectedSetting = PongManager.options.m_MusicVolume;
                    break;
                case 2:
                    currentSelectedSetting = PongManager.options.m_EffectsVolume;
                    break;
            }
        }
        base.OnSubmit(eventData);
    }
    protected IEnumerator CycleActivate()
    {
        while (usingCube)
        {
            yield return null;
        }
        switch (volumeIndex)
        {
            case 0:
                currentSelectedSetting = PongManager.options.soundVolume;
                break;
            case 1:
                currentSelectedSetting = PongManager.options.m_MusicVolume;
                break;
            case 2:
                currentSelectedSetting = PongManager.options.m_EffectsVolume;
                break;
        }
        DeActivate();  
    }
    protected override void SwitchSetting()
    {
        switch (volumeIndex)
        {
            case 0:
                PongManager.options.soundVolume = currentSelectedSetting;
                break;
            case 1:
                PongManager.options.m_MusicVolume = currentSelectedSetting;
                break;
            case 2:
                PongManager.options.m_EffectsVolume = currentSelectedSetting;
                break;
        }
        transform.DOComplete();
        PongBehaviour.am.UpdateVolume();
        usingCube = false;
        release = true;
    }
    protected override void SwitchSide(Side dir)
    {
        currentSelectedSetting += dir == Side.Top || dir == Side.Right ? -0.1f : 0.1f;
        if (currentSelectedSetting < 0) { currentSelectedSetting = 1; }
        if (currentSelectedSetting >= 1.1f) { currentSelectedSetting = 0; }
        transform.DOComplete();
        base.SwitchSide(dir);
        PongManager.am.PlayAudio(AudioType.UiSwitchFaces, transform.position);
    }
}