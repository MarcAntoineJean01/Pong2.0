// using System.Collections;
// using UnityEngine.EventSystems;
// using DG.Tweening;
// public class TutorialsSelect : SettingSelectCube
// {
//     public static bool currentSelectedSetting = PongManager.mainSettings.tutorialsOn;
//     protected override string currentSelectedString => currentSelectedSetting ? "ON" : "OFF";
//     protected override string unselectedString => "TUTORIALS" + "\n" + (currentSelectedSetting ? "<size=60%>ON" : "<size=60%>OFF");
//     public override void OnPointerClick(PointerEventData eventData)
//     {
//         currentSelectedSetting = PongManager.mainSettings.tutorialsOn;
//         base.OnPointerClick(eventData);
//     }
//     public override void OnSubmit(BaseEventData eventData)
//     {
//         if (!usingCube && !release)
//         {
//             currentSelectedSetting = PongManager.mainSettings.tutorialsOn;
//         }
//         base.OnSubmit(eventData);
//     }
//     protected IEnumerator CycleActivate()
//     {
//         while (usingCube)
//         {
//             yield return null;
//         }
//         currentSelectedSetting = PongManager.mainSettings.tutorialsOn;
//         DeActivate();  
//     }
//     protected override void SwitchSetting()
//     {
//         PongManager.mainSettings.tutorialsOn = currentSelectedSetting;
//         transform.DOComplete();
//         usingCube = false;
//         release = true;
//     }
//     protected override void SwitchSide(Side dir)
//     {
//         base.SwitchSetting();
//         currentSelectedSetting = !currentSelectedSetting;
//         transform.DOComplete();
//         base.SwitchSide(dir);
//         PongManager.am.PlayAudio(AudioType.UiSwitchFaces, transform.position);
//     }
// }