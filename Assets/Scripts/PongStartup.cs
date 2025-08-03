using System.Collections;
using Cinemachine;
using UnityEngine;
using DG.Tweening;
using System.Linq;

public class PongStartup : MonoBehaviour
{
    public CanvasGroup startupPanel;
    public CanvasGroup startupTitle;
    public NewGameManager newGameManager;
    public NewStageManager newStageManager;
    public UiManager uiManager;
    public GameObject managerBox;
    public Builder builder;
    public CameraManager cameraManager;
    public MeshManager meshManager;
    public ParamsManager paramsManager;
    public DialogManager dialogManager;
    public CutSceneManager cutSceneManager;
    public AudioManager audioManager;
    public LightsManager lightsManager;
    public Canvas menuCanvas;
    public Canvas leftCanvas;
    public Canvas rightCanvas;
    public GameObject fieldParent;
    public CinemachineBrain camBrain;
    public VFX vfx;
    public PVE pve;
    bool titleSkipCalled = false;
    void Awake()
    {
        Startup();
    }
    void FillMetaCube()
    {
        foreach (PongUiMenu menu in uiManager.metaCubeSides)
        {
            int initialChildCount = menu.transform.childCount;
            if (initialChildCount < 16)
            {
                for (int i = 0; i < 16 - initialChildCount; i++)
                {
                    GameObject.Instantiate(uiManager.debugFakeCubePrefab, menu.grid.transform).GetComponent<FakeUiCube>().transform.SetAsFirstSibling();
                }
            }
        }
    }
    public void Startup()
    {
        DOTween.SetTweensCapacity(500,125);
        uiManager.fontMaterial.SetColor("_OutlineColor", Color.white);
        startupPanel.gameObject.SetActive(true);
        PongBehaviour.um = uiManager;
        PongBehaviour.builder = builder;
        PongBehaviour.cm = cameraManager;
        PongBehaviour.mm = meshManager;
        PongBehaviour.pm = paramsManager;
        PongBehaviour.vfx = vfx;
        PongBehaviour.pve = pve;
        PongBehaviour.dm = dialogManager;
        PongBehaviour.csm = cutSceneManager;
        PongBehaviour.am = audioManager;
        PongBehaviour.lm = lightsManager;
        PongBehaviour.newGameManager = newGameManager;
        PongBehaviour.newStageManager = newStageManager;

        PongManager.menuCanvas = menuCanvas;
        PongManager.leftCanvas = leftCanvas;
        PongManager.rightCanvas = rightCanvas;
        PongManager.fieldParent = fieldParent;
        CameraManager.camBrain = camBrain;
        PongBehaviour.currentStage = Stage.StartMenu;
        PongBehaviour.previousStage = Stage.StartMenu;
        PongBehaviour.nextStage = Stage.DD;
        uiManager.menuOn = true;
        managerBox.SetActive(true);
        SetInitialSettings();
        FillMetaCube();
        StartCoroutine("CycleStartup");
    }
    public void SetInitialSettings()
    {
        PongManager.mainSettings = new MainGameSettings();
        PongManager.options = new GameOptions();
        PongManager.mainSettings.gameMode = GameMode.Time;
        PongManager.mainSettings.cutScenesOn = false;
        PongManager.mainSettings.inGameDialogsOn = false;
        PongManager.mainSettings.tutorialsOn = true;
        PongManager.mainSettings.leftPlayerController = PlayerController.Player;
        PongManager.mainSettings.rightPlayerController = PlayerController.Player;
        PongManager.options.soundVolume = 0.1f;
        PongManager.options.m_MusicVolume = 0.1f;
        PongManager.options.m_EffectsVolume = 0.1f;
        PongManager.options.allowedSpikes = new AllowedSpikes();
        PongManager.options.allowedDebuffs = new AllowedDebuffs();
        PongManager.options.timeThreshold = 45;
        PongManager.options.goalsThreshold = 5;
        PongManager.options.padMaxMagnetCharges = 2;
        PongManager.options.startingHealth = 100;
        SettingSelectCube.options = PongManager.options;
        SettingSelectCube.settings = PongManager.mainSettings;
    }
    IEnumerator CycleStartup()
    {
        float t = 0;
        PongPlayerControls startupControls = new PongPlayerControls();
        startupControls.MenuControls.Confirm.performed += ctx => titleSkipCalled = true;
        startupControls.MenuControls.Confirm.Enable();
        uiManager.inputSystemUI.actionsAsset.Disable();
        while (t < 2 && !titleSkipCalled)
        {
            t += Time.unscaledDeltaTime;
            if (t > 2) { t = 2; }
            yield return null;
        }
        audioManager.PlayAudio(AudioType.StartupSound, Vector3.zero, 5);
        t = 0;
        while (t < 3 && !titleSkipCalled)
        {
            t += Time.unscaledDeltaTime;
            if (t > 3) { t = 3; }
            startupTitle.alpha = Mathf.Lerp(0, 1, t / 3);
            yield return null;
        }
        t = 0;
        while (t < 1 && !titleSkipCalled)
        {
            t += Time.unscaledDeltaTime;
            if (t > 1) { t = 1; }
            yield return null;
        }
        if (!titleSkipCalled)
        {
            uiManager.OpenStartMenu();
        }
        t = 0;
        while (t < 1 && !titleSkipCalled)
        {
            t += Time.unscaledDeltaTime;
            if (t > 1) { t = 1; }
            startupPanel.alpha = Mathf.Lerp(1, 0, t);
            yield return null;
        }
        startupControls.MenuControls.Confirm.Disable();
        startupControls.Dispose();
        if (titleSkipCalled)
        {
            float currentAlpha = startupPanel.alpha;
            t = 0;
            uiManager.OpenStartMenu();
            if (currentAlpha > 0.5f)
            {
                while (t < 0.4f)
                {
                    t += Time.unscaledDeltaTime;
                    if (t > 0.4f) { t = 0.4f; }
                    startupPanel.alpha = Mathf.Lerp(currentAlpha, 0, t / 0.4f);
                    yield return null;
                }
            }
            else if (currentAlpha > 0)
            {
                while (t < 0.2f)
                {
                    t += Time.unscaledDeltaTime;
                    if (t > 0.2f) { t = 0.2f; }
                    startupPanel.alpha = Mathf.Lerp(currentAlpha, 0, t / 0.2f);
                    yield return null;
                }
            }
            else
            {
                startupPanel.alpha = 0;
            }

        }
        startupPanel.gameObject.SetActive(false);
        startupPanel.alpha = 1;
        audioManager.PlayMusic(MusicType.BackgroundMusic);
        vfx.LerpStartMenuStyle();
        uiManager.currentActiveMenu.MenuInteractionOn();
        uiManager.inputSystemUI.actionsAsset.Enable();
    }
}