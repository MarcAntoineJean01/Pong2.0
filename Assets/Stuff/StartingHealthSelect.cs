// using System;
// using System.Collections;
// using UnityEngine.EventSystems;
// using UnityEngine;
// using DG.Tweening;
// public class StartingHealthSelect : SettingSelectCube
// {
//     [Range(5, 200)]
//     public static int currentSelectedSetting = 0;
//     protected override string currentSelectedString => currentSelectedSetting.ToString();
//     protected override string unselectedString => "STARTING HEALTH" + "\n" + "<size=60%>" + PongManager.options.startingHealth.ToString();
//     public override void OnPointerClick(PointerEventData eventData)
//     {
//         currentSelectedSetting = PongManager.options.startingHealth;
//         base.OnPointerClick(eventData);
//     }
//     public override void OnSubmit(BaseEventData eventData)
//     {
//         if (!usingCube && !release)
//         {
//             currentSelectedSetting = PongManager.options.startingHealth;
//         }
//         base.OnSubmit(eventData);
//     }
//     protected IEnumerator CycleActivate()
//     {
//         while (usingCube)
//         {
//             yield return null;
//         }
//         currentSelectedSetting = PongManager.options.startingHealth;
//         DeActivate();  
//     }
//     protected override void SwitchSetting()
//     {
//         base.SwitchSetting();
//         PongManager.options.startingHealth = currentSelectedSetting;
//         transform.DOComplete();
//         usingCube = false;
//         release = true;
//     }
//     protected override void SwitchSide(Side dir)
//     {
//         currentSelectedSetting += dir == Side.Top || dir == Side.Right ? -5 : 5;
//         if (currentSelectedSetting < 5) { currentSelectedSetting = 200; }
//         if (currentSelectedSetting >= 201) { currentSelectedSetting = 5; }
//         transform.DOComplete();
//         base.SwitchSide(dir);
//         PongManager.am.PlayAudio(AudioType.UiSwitchFaces, transform.position);
//     }
// }