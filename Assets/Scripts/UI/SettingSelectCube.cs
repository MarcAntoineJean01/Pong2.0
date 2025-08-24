using System.Collections;
using UnityEngine.EventSystems;
using UnityEngine;
using TMPro;
using DG.Tweening;
using System.Linq;
using PongGame.UiLocker;
using PongGame.PongLocker;
using System.Collections.Generic;
using System;
using PongGame.SpikeLocker;
using PongGame.AudioLocker;
namespace PongGame
{
    public class SettingSelectCube : PongUiCube
    {
        CubeFaceIndexAndDirection currentIndexAndDirection = new(Side.None, 5);
        [SerializeField]
        Side padSide;
        [SerializeField]
        SpikeType spikeType;
        [SerializeField]
        DebuffType debuffType;
        public static MainGameSettings settings = new();
        public static GameOptions options = new();
        public static Stage stage;
        public Setting cubeSetting;
        string unselectedString => cubeText + "\n" + "<size=60%>" + currentRealSettingString;
        bool usingCube = false;
        bool release = false;
        PongPlayerControls cubeControls;
        bool invokeOnClick = false;
        string currentRealSettingString
        {
            get
            {
                switch (cubeSetting)
                {
                    default:
                        return "null";
                    case Setting.GameMode:
                        return PongManager.mainSettings.gameMode.ToString();
                    case Setting.CutScenesOn:
                        return PongManager.mainSettings.cutScenesOn ? "ON" : "OFF";
                    case Setting.InGameDialogsOn:
                        return PongManager.mainSettings.inGameDialogsOn ? "ON" : "OFF";
                    case Setting.TutorialsOn:
                        return PongManager.mainSettings.tutorialsOn ? "ON" : "OFF";
                    case Setting.LeftPlayerController:
                        return PongManager.mainSettings.leftPlayerController.ToString();
                    case Setting.RightPlayerController:
                        return PongManager.mainSettings.rightPlayerController.ToString();
                    case Setting.SoundVolume:
                        return ((int)(PongManager.options.soundVolume * 10)).ToString();
                    case Setting.MusicVolume:
                        return ((int)(PongManager.options.m_MusicVolume * 10)).ToString();
                    case Setting.EffectsVolume:
                        return ((int)(PongManager.options.m_EffectsVolume * 10)).ToString();
                    case Setting.AllowedSpikes:
                        return PongManager.options.allowedSpikes.GetAllowedSpike(spikeType) ? "ON" : "OFF";
                    case Setting.AllowedDebuffs:
                        return PongManager.options.allowedDebuffs.GetAllowedDebuff(debuffType) ? "ON" : "OFF";
                    case Setting.TimeThreshold:
                        return PongManager.options.timeThreshold.ToString();
                    case Setting.GoalsThreshold:
                        return PongManager.options.goalsThreshold.ToString();
                    case Setting.PadMaxMagnetCharges:
                        return PongManager.options.padMaxMagnetCharges.ToString();
                    case Setting.StartingHealth:
                        return PongManager.options.startingHealth.ToString();
                    case Setting.Stage:
                        return PongBehaviour.currentStage.ToString();
                }
            }
        }
        string currentSelectedString
        {
            get
            {
                switch (cubeSetting)
                {
                    default:
                        return "null";
                    case Setting.GameMode:
                        return settings.gameMode.ToString();
                    case Setting.CutScenesOn:
                        return settings.cutScenesOn ? "ON" : "OFF";
                    case Setting.InGameDialogsOn:
                        return settings.inGameDialogsOn ? "ON" : "OFF";
                    case Setting.TutorialsOn:
                        return settings.tutorialsOn ? "ON" : "OFF";
                    case Setting.LeftPlayerController:
                        return settings.leftPlayerController.ToString();
                    case Setting.RightPlayerController:
                        return settings.rightPlayerController.ToString();
                    case Setting.SoundVolume:
                        return ((int)(options.soundVolume * 10)).ToString();
                    case Setting.MusicVolume:
                        return ((int)(options.m_MusicVolume * 10)).ToString();
                    case Setting.EffectsVolume:
                        return ((int)(options.m_EffectsVolume * 10)).ToString();
                    case Setting.AllowedSpikes:
                        return options.allowedSpikes.GetAllowedSpike(spikeType) ? "ON" : "OFF";
                    case Setting.AllowedDebuffs:
                        return options.allowedDebuffs.GetAllowedDebuff(debuffType) ? "ON" : "OFF";
                    case Setting.TimeThreshold:
                        return options.timeThreshold.ToString();
                    case Setting.GoalsThreshold:
                        return options.goalsThreshold.ToString();
                    case Setting.PadMaxMagnetCharges:
                        return options.padMaxMagnetCharges.ToString();
                    case Setting.StartingHealth:
                        return options.startingHealth.ToString();
                    case Setting.Stage:
                        return stage.ToString();
                }
            }
        }
        Vector3 RotationForDirection(Side side)
        {
            switch (side)
            {
                case Side.Left:
                    return new Vector3(0, -90, 0);
                case Side.Right:
                    return new Vector3(0, 90, 0);
                case Side.Top:
                    return new Vector3(90, 0, 0);
                case Side.Bottom:
                    return new Vector3(-90, 0, 0);
                default:
                    return Vector3.zero;
            }
        }
        GameObject NextSide(Side side)
        {
            switch (side)
            {
                case Side.Left:
                    return sides.First(side => side.transform.position.x <= sides.Min(sd => sd.transform.position.x));
                case Side.Right:
                    return sides.First(side => side.transform.position.x >= sides.Max(sd => sd.transform.position.x));
                case Side.Top:
                    return sides.First(side => side.transform.position.y <= sides.Min(sd => sd.transform.position.y));
                case Side.Bottom:
                    return sides.First(side => side.transform.position.y >= sides.Max(sd => sd.transform.position.y));
                default:
                    return null;
            }
        }
        protected override void OnEnable()
        {
            base.OnEnable();
            cubeControls = new PongPlayerControls();
            cubeControls.Disable();
            usingCube = false;
            if (!PongBehaviour.um.useMeshForUiCubes)
            {
                foreach (GameObject side in sides)
                {
                    side.GetComponentInChildren<TMP_Text>().text = unselectedString;
                }
            }

        }
        public override void OnSubmit(BaseEventData eventData)
        {
            if (!usingCube && !release)
            {
                NavOff();
                if (PongUiMenu.menuControls != null)
                {
                    PongUiMenu.menuControls.UiCubeControls.Cancel.Disable();
                }
                usingCube = true;
                cubeControls.UiCubeControls.Enable();
                cubeControls.UiCubeControls.Up.performed += ctx => SwitchSide(Side.Bottom);
                cubeControls.UiCubeControls.Down.performed += ctx => SwitchSide(Side.Top);
                cubeControls.UiCubeControls.Left.performed += ctx => SwitchSide(Side.Left);
                cubeControls.UiCubeControls.Right.performed += ctx => SwitchSide(Side.Right);
                cubeControls.UiCubeControls.Cancel.performed += ctx => { usingCube = false; release = true; };
                cubeControls.UiCubeControls.Confirm.performed += ctx => SwitchSetting();
                StartCoroutine("CycleActiveGlow");
                stage = PongBehaviour.currentStage;
                if (!PongBehaviour.um.useMeshForUiCubes)
                {
                    sides[0].GetComponentInChildren<TMP_Text>().text = currentSelectedString;
                }
                else
                {
                    PongBehaviour.um.cubeTextureText[5].text = PongBehaviour.um.cubeTextureText[4].text = currentSelectedString;

                    PongBehaviour.um.cubeTextureText[0].text = NextCubeSettingString();
                    PongBehaviour.um.cubeTextureText[1].text = NextCubeSettingString();

                    PongBehaviour.um.cubeTextureText[2].text = NextCubeSettingString(false);
                    PongBehaviour.um.cubeTextureText[3].text = NextCubeSettingString(false);
                    // PongBehaviour.um.cubeTextureText[0].text = "0.1";
                    // PongBehaviour.um.cubeTextureText[1].text = "1";
                    // PongBehaviour.um.cubeTextureText[2].text = "2";
                    // PongBehaviour.um.cubeTextureText[3].text = "3";
                    // PongBehaviour.um.cubeTextureText[4].text = "4";
                    // PongBehaviour.um.cubeTextureText[5].text = "5";
                    currentIndexAndDirection.faceIndex = 5;
                    currentIndexAndDirection.dir = Side.None;

                }
                sqnc.OnComplete(() => StartCoroutine("CycleActivate"));
                sqnc.SetAutoKill(true);
                sqnc.Complete();
            }
            else if (!usingCube)
            {
                release = false;
            }
        }
        void ResetText(int faceOrientation)
        {
            int[] nextFaces = CubeDirectory.NextFaces(Tuple.Create(faceOrientation, currentIndexAndDirection.faceIndex));
            int[] previousFaces = CubeDirectory.PreviousFaces(Tuple.Create(faceOrientation, currentIndexAndDirection.faceIndex));
            PongBehaviour.um.cubeTextureText[nextFaces[0]].text = NextCubeSettingString();
            PongBehaviour.um.cubeTextureText[nextFaces[1]].text = NextCubeSettingString();

            PongBehaviour.um.cubeTextureText[previousFaces[0]].text = NextCubeSettingString(false);
            PongBehaviour.um.cubeTextureText[previousFaces[1]].text = NextCubeSettingString(false);
        }
        void SwitchSide(Side dir)
        {
            NextCubeSettingOption(dir == Side.Bottom || dir == Side.Right);
            transform.DOComplete();
            PongManager.am.PlayAudio(PongAudioType.UiSwitchFaces, transform.position);
            if (!PongBehaviour.um.useMeshForUiCubes)
            {
                NextSide(dir).transform.rotation = Quaternion.Euler(RotationForDirection(dir) * -1);
                if (PongBehaviour.currentStage == Stage.Neon)
                {
                    NextSide(dir).transform.rotation *= PongBehaviour.um.metaCube.transform.rotation;
                }
                NextSide(dir).GetComponentInChildren<TMP_Text>().text = currentSelectedString;
            }
            else
            {
                currentIndexAndDirection.dir = dir;
                CubeNextFaceIndexAndTextRotation nextIndexAndRot = CubeDirectory.NextFaceIndexAndRotation(currentIndexAndDirection, transform.rotation);
                currentIndexAndDirection.faceIndex = nextIndexAndRot.nextFaceIndex;
                PongBehaviour.um.cubeTextureText[nextIndexAndRot.nextFaceIndex].transform.DORotate(nextIndexAndRot.textRotation, 0.25f, RotateMode.Fast).SetAutoKill(true);
                ResetText((int)nextIndexAndRot.textRotation.z);
            }
            transform.DOLocalRotate(RotationForDirection(dir), 0.5f, RotateMode.WorldAxisAdd).SetAutoKill(true);
        }
        string NextCubeSettingString(bool add = true)
        {
            switch (cubeSetting)
            {
                case Setting.GameMode:
                    if ((int)settings.gameMode + (add ? 1 : -1) < 1) { return ((GameMode)System.Enum.GetValues(typeof(GameMode)).Length - 1).ToString(); }
                    if ((int)settings.gameMode + (add ? 1 : -1) >= System.Enum.GetValues(typeof(GameMode)).Length) { return ((GameMode)1).ToString(); }
                    return (settings.gameMode + (add ? 1 : -1)).ToString();
                case Setting.CutScenesOn:
                    return settings.cutScenesOn ? "OFF" : "ON";
                case Setting.InGameDialogsOn:
                    return settings.inGameDialogsOn ? "OFF" : "ON";
                case Setting.TutorialsOn:
                    return settings.tutorialsOn ? "OFF" : "ON";
                case Setting.LeftPlayerController:
                    if (settings.leftPlayerController + (add ? 1 : -1) < 0) { return ((PlayerController)System.Enum.GetValues(typeof(PlayerController)).Length - 1).ToString(); }
                    if (settings.leftPlayerController + (add ? 1 : -1) >= (PlayerController)System.Enum.GetValues(typeof(PlayerController)).Length) { return ((PlayerController)1).ToString(); }
                    return (settings.leftPlayerController + (add ? 1 : -1)).ToString();
                case Setting.RightPlayerController:
                    if (settings.rightPlayerController + (add ? 1 : -1) < 0) { return ((PlayerController)System.Enum.GetValues(typeof(PlayerController)).Length - 1).ToString(); }
                    if (settings.rightPlayerController + (add ? 1 : -1) >= (PlayerController)System.Enum.GetValues(typeof(PlayerController)).Length) { return ((PlayerController)1).ToString(); }
                    return (settings.rightPlayerController + (add ? 1 : -1)).ToString();
                case Setting.SoundVolume:
                    if (options.soundVolume + (add ? 0.1f : -0.1f) < 0) { return "10"; }
                    if (options.soundVolume + (add ? 0.1f : -0.1f) >= 1.1f) { return "0"; }
                    return ((options.soundVolume + (add ? 0.1f : -0.1f)) * 10).ToString();
                case Setting.MusicVolume:
                    if (options.m_MusicVolume + (add ? 0.1f : -0.1f) < 0) { return "10"; }
                    if (options.m_MusicVolume + (add ? 0.1f : -0.1f) >= 1.1f) { return "0"; }
                    return ((options.m_MusicVolume + (add ? 0.1f : -0.1f)) * 10).ToString();
                case Setting.EffectsVolume:
                    if (options.m_EffectsVolume + (add ? 0.1f : -0.1f) < 0) { return "10"; }
                    if (options.m_EffectsVolume + (add ? 0.1f : -0.1f) >= 1.1f) { return "0"; }
                    return ((options.m_EffectsVolume + (add ? 0.1f : -0.1f)) * 10).ToString();
                case Setting.AllowedSpikes:
                    return options.allowedSpikes.GetAllowedSpike(spikeType) ? "OFF" : "ON";
                case Setting.AllowedDebuffs:
                    return options.allowedDebuffs.GetAllowedDebuff(debuffType) ? "OFF" : "ON";
                case Setting.TimeThreshold:
                    if (options.timeThreshold + (add ? 1 : -1) < 10) { return "60"; }
                    if (options.timeThreshold + (add ? 1 : -1) >= 61) { return "10"; }
                    return (options.timeThreshold + (add ? 1 : -1)).ToString();
                case Setting.GoalsThreshold:
                    if (options.goalsThreshold + (add ? 1 : -1) < 3) { return "20"; }
                    if (options.goalsThreshold + (add ? 1 : -1) >= 21) { return "3"; }
                    return (options.goalsThreshold + (add ? 1 : -1)).ToString();
                case Setting.PadMaxMagnetCharges:
                    if (options.padMaxMagnetCharges + (add ? 1 : -1) < 1) { return "20"; }
                    if (options.padMaxMagnetCharges + (add ? 1 : -1) >= 21) { return "1"; }
                    return (options.padMaxMagnetCharges + (add ? 1 : -1)).ToString();
                case Setting.StartingHealth:
                    if (options.startingHealth + (add ? 5 : -5) < 5) { return "200"; }
                    if (options.startingHealth + (add ? 5 : -5) >= 201) { return "5"; }
                    return (options.startingHealth + (add ? 5 : -5)).ToString();
                case Setting.Stage:
                    if ((int)(stage + (add ? 1 : -1)) < 1) { return ((Stage)System.Enum.GetValues(typeof(Stage)).Length - 1).ToString(); }
                    if ((int)(stage + (add ? 1 : -1)) >= System.Enum.GetValues(typeof(Stage)).Length) { return ((Stage)1).ToString(); }
                    return (stage + (add ? 1 : -1)).ToString();
                default:
                    return "X";
            }
        }
        void NextCubeSettingOption(bool add = true)
        {
            switch (cubeSetting)
            {
                case Setting.GameMode:
                    settings.gameMode += add ? 1 : -1;
                    if ((int)settings.gameMode < 1) { settings.gameMode = (GameMode)System.Enum.GetValues(typeof(GameMode)).Length - 1; }
                    if ((int)settings.gameMode >= System.Enum.GetValues(typeof(GameMode)).Length) { settings.gameMode = (GameMode)1; }
                    break;
                case Setting.CutScenesOn:
                    settings.cutScenesOn = !settings.cutScenesOn;
                    break;
                case Setting.InGameDialogsOn:
                    settings.inGameDialogsOn = !settings.inGameDialogsOn;
                    break;
                case Setting.TutorialsOn:
                    settings.tutorialsOn = !settings.tutorialsOn;
                    break;
                case Setting.LeftPlayerController:
                    settings.leftPlayerController += add ? 1 : -1;
                    if (settings.leftPlayerController < 0) { settings.leftPlayerController = (PlayerController)System.Enum.GetValues(typeof(PlayerController)).Length - 1; }
                    if (settings.leftPlayerController >= (PlayerController)System.Enum.GetValues(typeof(PlayerController)).Length) { settings.leftPlayerController = 0; }
                    break;
                case Setting.RightPlayerController:
                    settings.rightPlayerController += add ? 1 : -1;
                    if (settings.rightPlayerController < 0) { settings.rightPlayerController = (PlayerController)System.Enum.GetValues(typeof(PlayerController)).Length - 1; }
                    if (settings.rightPlayerController >= (PlayerController)System.Enum.GetValues(typeof(PlayerController)).Length) { settings.rightPlayerController = 0; }
                    break;
                case Setting.SoundVolume:
                    options.soundVolume += add ? 0.1f : -0.1f;
                    if (options.soundVolume < 0) { options.soundVolume = 1; }
                    if (options.soundVolume >= 1.1f) { options.soundVolume = 0; }
                    break;
                case Setting.MusicVolume:
                    options.m_MusicVolume += add ? 0.1f : -0.1f;
                    if (options.m_MusicVolume < 0) { options.m_MusicVolume = 1; }
                    if (options.m_MusicVolume >= 1.1f) { options.m_MusicVolume = 0; }
                    break;
                case Setting.EffectsVolume:
                    options.m_EffectsVolume += add ? 0.1f : -0.1f;
                    if (options.m_EffectsVolume < 0) { options.m_EffectsVolume = 1; }
                    if (options.m_EffectsVolume >= 1.1f) { options.m_EffectsVolume = 0; }
                    break;
                case Setting.AllowedSpikes:
                    options.allowedSpikes.SetAllowedSpike(spikeType, !options.allowedSpikes.GetAllowedSpike(spikeType));
                    break;
                case Setting.AllowedDebuffs:
                    options.allowedDebuffs.SetAllowedDebuff(debuffType, !options.allowedDebuffs.GetAllowedDebuff(debuffType));
                    break;
                case Setting.TimeThreshold:
                    options.timeThreshold += add ? 1 : -1;
                    if (options.timeThreshold < 10) { options.timeThreshold = 60; }
                    if (options.timeThreshold >= 61) { options.timeThreshold = 10; }
                    break;
                case Setting.GoalsThreshold:
                    options.goalsThreshold += add ? 1 : -1;
                    if (options.goalsThreshold < 3) { options.goalsThreshold = 20; }
                    if (options.goalsThreshold >= 21) { options.goalsThreshold = 3; }
                    break;
                case Setting.PadMaxMagnetCharges:
                    options.padMaxMagnetCharges += add ? 1 : -1;
                    if (options.padMaxMagnetCharges < 1) { options.padMaxMagnetCharges = 20; }
                    if (options.padMaxMagnetCharges >= 21) { options.padMaxMagnetCharges = 1; }
                    break;
                case Setting.StartingHealth:
                    options.startingHealth += add ? 5 : -5;
                    if (options.startingHealth < 5) { options.startingHealth = 200; }
                    if (options.startingHealth >= 201) { options.startingHealth = 5; }
                    break;
                case Setting.Stage:
                    stage += add ? 1 : -1;
                    if ((int)stage < 1) { stage = (Stage)System.Enum.GetValues(typeof(Stage)).Length - 1; }
                    if ((int)stage >= System.Enum.GetValues(typeof(Stage)).Length) { stage = (Stage)1; }
                    break;
            }
        }
        void SwitchSetting()
        {
            switch (cubeSetting)
            {
                case Setting.GameMode:
                    PongManager.mainSettings.gameMode = settings.gameMode;
                    break;
                case Setting.CutScenesOn:
                    PongManager.mainSettings.cutScenesOn = settings.cutScenesOn;
                    break;
                case Setting.InGameDialogsOn:
                    PongManager.mainSettings.inGameDialogsOn = settings.inGameDialogsOn;
                    break;
                case Setting.TutorialsOn:
                    PongManager.mainSettings.tutorialsOn = settings.tutorialsOn;
                    break;
                case Setting.LeftPlayerController:
                    PongManager.mainSettings.leftPlayerController = settings.leftPlayerController;
                    break;
                case Setting.RightPlayerController:
                    PongManager.mainSettings.rightPlayerController = settings.rightPlayerController;
                    break;
                case Setting.SoundVolume:
                    invokeOnClick = true;
                    PongManager.options.soundVolume = options.soundVolume;
                    break;
                case Setting.MusicVolume:
                    invokeOnClick = true;
                    PongManager.options.m_MusicVolume = options.m_MusicVolume;
                    break;
                case Setting.EffectsVolume:
                    invokeOnClick = true;
                    PongManager.options.m_EffectsVolume = options.m_EffectsVolume;
                    break;
                case Setting.AllowedSpikes:
                    PongManager.options.allowedSpikes.SetAllowedSpike(spikeType, options.allowedSpikes.GetAllowedSpike(spikeType));
                    break;
                case Setting.AllowedDebuffs:
                    PongManager.options.allowedDebuffs.SetAllowedDebuff(debuffType, options.allowedDebuffs.GetAllowedDebuff(debuffType));
                    break;
                case Setting.TimeThreshold:
                    PongManager.options.timeThreshold = options.timeThreshold;
                    break;
                case Setting.GoalsThreshold:
                    PongManager.options.goalsThreshold = options.goalsThreshold;
                    break;
                case Setting.PadMaxMagnetCharges:
                    PongManager.options.padMaxMagnetCharges = options.padMaxMagnetCharges;
                    break;
                case Setting.StartingHealth:
                    PongManager.options.startingHealth = options.startingHealth;
                    break;
                case Setting.Stage:
                    invokeOnClick = true;
                    PongBehaviour.newStageManager.SelectNewStage((int)stage);
                    break;
            }
            cubeControls.UiCubeControls.Disable();
            transform.DOComplete();
            usingCube = false;
            release = true;
        }
        // public override void OnPointerClick(PointerEventData eventData)
        // {
        //     StartCoroutine("CycleActiveGlow");
        //     if (!PongBehaviour.um.useMeshForUiCubes)
        //     {
        //         sides[0].GetComponentInChildren<TMP_Text>().text = currentSelectedString;
        //     }
        //     else
        //     {
        //         PongBehaviour.um.cubeTextureText[5].text = PongBehaviour.um.cubeTextureText[4].text = currentSelectedString;

        //         PongBehaviour.um.cubeTextureText[1].text = NextCubeSettingString();
        //         PongBehaviour.um.cubeTextureText[2].text = NextCubeSettingString();

        //         PongBehaviour.um.cubeTextureText[0].text = NextCubeSettingString(false);
        //         PongBehaviour.um.cubeTextureText[3].text = NextCubeSettingString(false);
        //     }
        //     sqnc.OnComplete(() => StartCoroutine("CycleActivate"));
        //     sqnc.SetAutoKill(true);
        // }
        IEnumerator CycleActiveGlow()
        {
            float t = 0f;
            while (t < fadeDuration)
            {
                t += Time.unscaledDeltaTime;
                if (t > fadeDuration) { t = fadeDuration; }
                mat.SetColor("_BaseColor", Color.Lerp(PongBehaviour.um.cubeNormalColor, PongBehaviour.um.cubePressedColor, t / fadeDuration));
                yield return null;
            }
        }
        void DeActivate()
        {
            cubeControls.Disable();
            if (PongUiMenu.menuControls != null)
            {
                PongUiMenu.menuControls.UiCubeControls.Cancel.Enable();
            }
            if (!PongBehaviour.um.useMeshForUiCubes)
            {
                sides[0].transform.localRotation = Quaternion.Euler(Vector3.zero);
                sides[0].GetComponentInChildren<TMP_Text>().text = unselectedString;
            }
            transform.localRotation = Quaternion.Euler(Vector3.zero);
            PongBehaviour.um.StopRenderCubeTexture(this);
            NavOn();
            StartCoroutine("CycleGlow");
            if (invokeOnClick && !release) { onClick.Invoke(); }
            release = false;
        }
        void ResetFromRealSettings()
        {
            switch (cubeSetting)
            {
                case Setting.GameMode:
                    settings.gameMode = PongManager.mainSettings.gameMode;
                    break;
                case Setting.CutScenesOn:
                    settings.cutScenesOn = PongManager.mainSettings.cutScenesOn;
                    break;
                case Setting.InGameDialogsOn:
                    settings.inGameDialogsOn = PongManager.mainSettings.inGameDialogsOn;
                    break;
                case Setting.TutorialsOn:
                    settings.tutorialsOn = PongManager.mainSettings.tutorialsOn;
                    break;
                case Setting.LeftPlayerController:
                    settings.leftPlayerController = PongManager.mainSettings.leftPlayerController;
                    break;
                case Setting.RightPlayerController:
                    settings.rightPlayerController = PongManager.mainSettings.rightPlayerController;
                    break;
                case Setting.SoundVolume:
                    options.soundVolume = PongManager.options.soundVolume;
                    break;
                case Setting.MusicVolume:
                    options.m_MusicVolume = PongManager.options.m_MusicVolume;
                    break;
                case Setting.EffectsVolume:
                    options.m_EffectsVolume = PongManager.options.m_EffectsVolume;
                    break;
                case Setting.AllowedSpikes:
                    options.allowedSpikes.SetAllowedSpike(spikeType, PongManager.options.allowedSpikes.GetAllowedSpike(spikeType));
                    break;
                case Setting.AllowedDebuffs:
                    options.allowedDebuffs.SetAllowedDebuff(debuffType, PongManager.options.allowedDebuffs.GetAllowedDebuff(debuffType));
                    break;
                case Setting.TimeThreshold:
                    options.timeThreshold = PongManager.options.timeThreshold;
                    break;
                case Setting.GoalsThreshold:
                    options.goalsThreshold = PongManager.options.goalsThreshold;
                    break;
                case Setting.PadMaxMagnetCharges:
                    options.padMaxMagnetCharges = PongManager.options.padMaxMagnetCharges;
                    break;
                case Setting.StartingHealth:
                    options.startingHealth = PongManager.options.startingHealth;
                    break;
                case Setting.Stage:
                    invokeOnClick = true;
                    stage = PongBehaviour.currentStage;
                    break;
            }
            cubeControls.UiCubeControls.Disable();
            transform.DOComplete();
        }
        IEnumerator CycleActivate()
        {
            if (PongBehaviour.um.useMeshForUiCubes)
            {
                PongBehaviour.um.RenderCubeTexture(mat.GetTexture("_CubeTexture") as RenderTexture);
            }
            while (usingCube)
            {
                yield return null;
            }
            cubeControls.Disable();
            ResetFromRealSettings();
            DeActivate();
        }
    }
}

