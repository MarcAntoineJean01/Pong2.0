// using System;
// using System.Collections;
// using UnityEngine.EventSystems;
// using UnityEngine;
// using DG.Tweening;
// public class GameModeThresholdSelect : SettingSelectCube
// {
//     [Range(10, 60)]
//     public int currentSelectedTimeThreshold = PongManager.options.timeThreshold;
//     [Range(3, 20)]
//     public int currentSelectedGoalsThreshold = PongManager.options.goalsThreshold;
//     public int currentSelectedSetting => PongManager.mainSettings.gameMode == GameMode.Time ? currentSelectedTimeThreshold : currentSelectedGoalsThreshold;
//     protected override string currentSelectedString => currentSelectedSetting.ToString();
//     protected override string unselectedString
//     {
//         get
//         {
//             switch (PongManager.mainSettings.gameMode)
//             {
//                 default:
//                 case GameMode.Time:
//                     return "TIME LIMIT" + "\n" + "<size=60%>" + PongManager.options.timeThreshold.ToString();
//                 case GameMode.Goals:
//                     return "GOALS THRESHOLD" + "\n" + "<size=60%>" + PongManager.options.goalsThreshold.ToString();
//             }
//         }
//     }
//     public override void OnPointerClick(PointerEventData eventData)
//     {
//         switch (PongManager.mainSettings.gameMode)
//         {
//             case GameMode.Time:
//                 currentSelectedTimeThreshold = PongManager.options.timeThreshold;
//                 break;
//             case GameMode.Goals:
//                 currentSelectedGoalsThreshold = PongManager.options.goalsThreshold;
//                 break;
//         }
//         base.OnPointerClick(eventData);
//     }
//     public override void OnSubmit(BaseEventData eventData)
//     {
//         if (!usingCube && !release)
//         {
//             switch (PongManager.mainSettings.gameMode)
//             {
//                 case GameMode.Time:
//                     currentSelectedTimeThreshold = PongManager.options.timeThreshold;
//                     break;
//                 case GameMode.Goals:
//                     currentSelectedGoalsThreshold = PongManager.options.goalsThreshold;
//                     break;
//             }
//         }
//         base.OnSubmit(eventData);
//     }
//     protected IEnumerator CycleActivate()
//     {
//         while (usingCube)
//         {
//             yield return null;
//         }
//         switch (PongManager.mainSettings.gameMode)
//         {
//             case GameMode.Time:
//                 currentSelectedTimeThreshold = PongManager.options.timeThreshold;
//                 break;
//             case GameMode.Goals:
//                 currentSelectedGoalsThreshold = PongManager.options.goalsThreshold;
//                 break;
//         }
//         DeActivate();  
//     }
//     protected override void SwitchSetting()
//     {
//         base.SwitchSetting();
//         switch (PongManager.mainSettings.gameMode)
//         {
//             case GameMode.Time:
//                 PongManager.options.timeThreshold = currentSelectedTimeThreshold;
//                 break;
//             case GameMode.Goals:
//                 PongManager.options.goalsThreshold = currentSelectedGoalsThreshold;
//                 break;
//         }
//         transform.DOComplete();
//         usingCube = false;
//         release = true;
//     }
//     protected override void SwitchSide(Side dir)
//     {
//         switch (PongManager.mainSettings.gameMode)
//         {
//             case GameMode.Time:
//                 currentSelectedTimeThreshold += dir == Side.Top || dir == Side.Right ? -1 : 1;
//                 if (currentSelectedTimeThreshold < 10) { currentSelectedTimeThreshold = 60; }
//                 if (currentSelectedTimeThreshold >= 61) { currentSelectedTimeThreshold = 10; }
//                 break;
//             case GameMode.Goals:
//                 currentSelectedGoalsThreshold += dir == Side.Top || dir == Side.Right ? -1 : 1;
//                 if (currentSelectedGoalsThreshold < 3) { currentSelectedGoalsThreshold = 20; }
//                 if (currentSelectedGoalsThreshold >= 21) { currentSelectedGoalsThreshold = 3; }
//                 break;
//         }
//         transform.DOComplete();
//         base.SwitchSide(dir);
//         PongManager.am.PlayAudio(AudioType.UiSwitchFaces, transform.position);
//     }
// }