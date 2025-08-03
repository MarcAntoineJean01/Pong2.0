using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;


public class UiManager : PongManager
{
    [SerializeField]
    [ColorUsage(true, true)]
    public Color cubeNormalColor;
    [SerializeField]
    [ColorUsage(true, true)]
    public Color cubeHighlightedColor;
    [SerializeField]
    [ColorUsage(true, true)]
    public Color cubePressedColor;
    [SerializeField]
    public Color cubeTextColor;
    [ColorUsage(true, true)]
    public Color fontGlowColor;
    [SerializeField]
    public Material cubeMaterial;
    [SerializeField]
    public GameObject debugFakeCubePrefab;
    public RectTransform metaCube;
    public List<PongUiMenu> metaCubeSides = new List<PongUiMenu>(6);
    public float uiCubePadding => menuCanvas.GetComponent<RectTransform>().sizeDelta.y / 25;
    float uiCubeSize => (menuCanvas.GetComponent<RectTransform>().sizeDelta.y - (uiCubePadding * 5)) / 4;
    float metaCubeSize => uiCubeSize * 4 + uiCubePadding * 3;
    public InputSystemUIInputModule inputSystemUI;
    public Material menuBackgroundMaterial;
    public Material fontMaterial;// this way to grab fonts is temporary (and crap).
    public GridLayoutGroup hudCanvasGrid;
    public CanvasGroup leftHudFull;
    public CanvasGroup rightHudFull;
    public CanvasGroup leftHudSplit;
    public CanvasGroup rightHudSplit;
    public CanvasGroup leftHud => currentStage == Stage.Neon ? leftHudSplit : leftHudFull;
    public CanvasGroup rightHud => currentStage == Stage.Neon ? rightHudSplit : rightHudFull;
    public TMP_Text leftAttractorCount;
    public TMP_Text leftRepulsorCount;
    public TMP_Text rightAttractorCount;
    public TMP_Text rightRepulsorCount;
    public GameObject startMenu;
    public GameObject settingsMenu;
    public GameObject pauseMenu;
    public GameObject gameOverPanel;
    public ProgressBar hudBarLeft;
    public ProgressBar hudBarRight;
    public ProgressBar hudBarLeftSplit;
    public ProgressBar hudBarRightSplit;
    public bool menuOn = false;
    bool displayingLeftHud = false;
    bool displayingRightHud = false;
    bool leftHudExtention = false;
    bool rightHudExtention = false;
    public PongUiMenu currentActiveMenu;
    public static int currentActiveMenuIndex = 0;

    Dictionary<int, CubesToHideFromFace[]> cubesToHideForMenu = new Dictionary<int, CubesToHideFromFace[]>()
    {
        {0, new CubesToHideFromFace[4]{
                new CubesToHideFromFace(2, new int[4] {0,1,2,3}),
                new CubesToHideFromFace(3, new int[4] {12,13,14,15}),
                new CubesToHideFromFace(4, new int[4] {3,7,11,15}) ,
                new CubesToHideFromFace(5, new int[4] {0,4,8,12})
            }
        },
        {1, new CubesToHideFromFace[4]{
                new CubesToHideFromFace(2, new int[4] {12,13,14,15}),
                new CubesToHideFromFace(3, new int[4] {0,1,2,3}),
                new CubesToHideFromFace(4, new int[4] {0,4,8,12}) ,
                new CubesToHideFromFace(5, new int[4] {3,7,11,15})
            }
        },
        {2, new CubesToHideFromFace[4]{
                new CubesToHideFromFace(0, new int[4] {12,13,14,15}),
                new CubesToHideFromFace(1, new int[4] {0,1,2,3}),
                new CubesToHideFromFace(4, new int[4] {12,13,14,15}) ,
                new CubesToHideFromFace(5, new int[4] {12,13,14,15})
            }
        },
        {3, new CubesToHideFromFace[4]{
                new CubesToHideFromFace(0, new int[4] {0,1,2,3}),
                new CubesToHideFromFace(1, new int[4] {12,13,14,15}),
                new CubesToHideFromFace(4, new int[4] {0,1,2,3}),
                new CubesToHideFromFace(5, new int[4] {0,1,2,3})
            }
        },
        {4, new CubesToHideFromFace[4]{
                new CubesToHideFromFace(0, new int[4] {0,4,8,12}),
                new CubesToHideFromFace(1, new int[4] {0,4,8,12}),
                new CubesToHideFromFace(2, new int[4] {0,4,8,12}),
                new CubesToHideFromFace(3, new int[4] {0,4,8,12})
            }
        },
        {5, new CubesToHideFromFace[4]{
                new CubesToHideFromFace(0, new int[4] {3,7,11,15}),
                new CubesToHideFromFace(1, new int[4] {3,7,11,15}),
                new CubesToHideFromFace(2, new int[4] {3,7,11,15}),
                new CubesToHideFromFace(3, new int[4] {3,7,11,15})
            }
        }
    };
    void OnEnable()
    {
        SetMetaCube();
        foreach (PongUiMenu menu in metaCubeSides)
        {
            menu.metaCubeVanished.AddListener(() => TurnOffMetaCube());
        }
        hudCanvasGrid.cellSize = new Vector2(menuCanvas.GetComponent<RectTransform>().rect.width * 0.5f, menuCanvas.GetComponent<RectTransform>().rect.height * 0.25f);
        leftAttractorCount.text = "0";
        leftRepulsorCount.text = "0";
        rightAttractorCount.text = "0";
        rightRepulsorCount.text = "0";
        leftHud.alpha = 0;
        rightHud.alpha = 0;
        displayHud.AddListener((sd) => { DisplayHudForSeconds(sd); });
        newGameManager.gameOver.AddListener(() => DisplayGameOverPanel());
    }
    void SetMetaCube()
    {
        metaCube.sizeDelta = new Vector2(metaCubeSize, metaCubeSize);
        metaCube.transform.localPosition = new Vector3(0, 0, metaCubeSize * 0.5f);
        foreach (PongUiMenu menu in metaCubeSides)
        {
            menu.grid.cellSize = new Vector2(uiCubeSize, uiCubeSize);
            menu.grid.spacing = new Vector2(uiCubePadding, uiCubePadding);
        }
        metaCubeSides[0].transform.localPosition = new Vector3(0, 0, -metaCubeSize * 0.5f + uiCubeSize);
        metaCubeSides[0].transform.localRotation = Quaternion.Euler(0, 0, 0);

        metaCubeSides[1].transform.localPosition = new Vector3(0, 0, metaCubeSize * 0.5f - uiCubeSize);
        metaCubeSides[1].transform.localRotation = Quaternion.Euler(0, 180, 180);

        metaCubeSides[2].transform.localPosition = new Vector3(0, -metaCubeSize * 0.5f + uiCubeSize, 0);
        metaCubeSides[2].transform.localRotation = Quaternion.Euler(-90, 0, 0);

        metaCubeSides[3].transform.localPosition = new Vector3(0, metaCubeSize * 0.5f - uiCubeSize, 0);
        metaCubeSides[3].transform.localRotation = Quaternion.Euler(90, 0, 0);

        metaCubeSides[4].transform.localPosition = new Vector3(-metaCubeSize * 0.5f + uiCubeSize, 0, 0);
        metaCubeSides[4].transform.localRotation = Quaternion.Euler(0, 90, 0);

        metaCubeSides[5].transform.localPosition = new Vector3(metaCubeSize * 0.5f - uiCubeSize, 0, 0);
        metaCubeSides[5].transform.localRotation = Quaternion.Euler(0, 270, 0);
    }
    void DisplayGameOverPanel()
    {
        gameOverPanel.SetActive(true);
        StartCoroutine("CycleDisplayGameOverPanel");
    }
    IEnumerator CycleDisplayGameOverPanel()
    {
        am.PlayAudio(AudioType.GameOverVoice, Vector3.zero);
        yield return new WaitForSecondsRealtime(2);
        am.PlayMusic(MusicType.GameOverMusic);
        float t = 0;
        while (t < 4)
        {
            t += Time.unscaledDeltaTime;
            if (t > 4) { t = 4; }
            gameOverPanel.GetComponent<CanvasGroup>().alpha = Mathf.Lerp(0, 1, t / 4);
            yield return null;
        }
        t = 0;
        while (t < 10)
        {
            t += Time.unscaledDeltaTime;
            if (t > 10) { t = 10; }
            Color c = gameOverPanel.GetComponent<Image>().color;
            c.a = Mathf.Lerp(0, 1, t / 10);
            gameOverPanel.GetComponent<Image>().color = c;
            yield return null;
        }
        yield return new WaitForSecondsRealtime(5);
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
    public void OpenStartMenu()
    {
        TurnOnMetaCube(0);
    }
    public void OpenSettingsMenu()
    {
        TurnOnMetaCube(currentPhase == GamePhase.Startup ? 1 : 2);
    }
    public void OpenPauseMenu()
    {
        TurnOnMetaCube(2);
        currentActiveMenu.MenuInteractionOn();
    }
    public void OpenAllowedSpikesMenu()
    {
        TurnOnMetaCube(3);
    }
    public void OpenSoundMenu()
    {
        TurnOnMetaCube(4);
    }
    public void OpenGameplayMenu()
    {
        TurnOnMetaCube(5);
    }
    public void DisplayHudForSeconds(Side sd)
    {
        if ((!displayingLeftHud && sd == Side.Left) || (!displayingRightHud && sd == Side.Right))
        {
            if (sd == Side.Left)
            {
                displayingLeftHud = true;
            }
            else
            {
                displayingRightHud = true;
            }
            UpdateHud();
            StartCoroutine(CycleDisplayHud(sd));
        }
        else if (sd == Side.Left)
        {
            leftHudExtention = true;
            UpdateHud();
        }
        else if (sd == Side.Right)
        {
            rightHudExtention = true;
            UpdateHud();
        }
    }
    public void UpdateHud()
    {
        leftPlayer.healthBar.SetCurrentFill();;
        rightPlayer.healthBar.SetCurrentFill();;
        leftAttractorCount.text = PongBehaviour.field.leftPad.attractorCharges.ToString();
        leftRepulsorCount.text = PongBehaviour.field.leftPad.repulsorCharges.ToString();
        rightAttractorCount.text = PongBehaviour.field.rightPad.attractorCharges.ToString();
        rightRepulsorCount.text = PongBehaviour.field.rightPad.repulsorCharges.ToString();
    }
    IEnumerator CycleDisplayHud(Side sd)
    {
        float t = 0f;
        CanvasGroup cg = sd == Side.Left ? leftHud : rightHud;
        while (t < 0.5f)
        {
            t += Time.unscaledDeltaTime;
            if (t > 0.5f) { t = 0.5f; }
            cg.alpha = Mathf.Lerp(0, 1, t / 0.5f);
            yield return null;
        }
        cg.alpha = 1;
        yield return new WaitForSecondsRealtime(3);
        while (sd == Side.Left && leftHudExtention)
        {
            leftHudExtention = false;
            yield return new WaitForSecondsRealtime(2);
            yield return null;
        }
        while (sd == Side.Right && rightHudExtention)
        {
            rightHudExtention = false;
            yield return new WaitForSecondsRealtime(2);
            yield return null;
        }
        t = 0f;
        while (t < 0.5f)
        {
            t += Time.unscaledDeltaTime;
            if (t > 0.5f) { t = 0.5f; }
            cg.alpha = Mathf.Lerp(1, 0, t / 0.5f);
            yield return null;
        }
        cg.alpha = 0;
        if (sd == Side.Left)
        {
            displayingLeftHud = false;
        }
        else
        {
            displayingRightHud = false;
        }
    }
    public void TurnOnMetaCube(int side)
    {
        ShowOverlappingCubes();
        if (!metaCube.gameObject.activeSelf)
        {
            currentActiveMenu = metaCubeSides[side];
            if (currentStage == Stage.Neon)
            {
                cm.overlayCam.transform.position = cm.leftPadCam.transform.position;
                cm.overlayCam.transform.rotation = cm.leftPadCam.transform.rotation;
                metaCube.transform.position = new Vector3(metaCube.transform.position.x, metaCube.transform.position.y, stagePosZ);
                metaCube.transform.LookAt(cm.leftPadCam.transform.position);
                metaCube.transform.rotation = Quaternion.LookRotation(cm.leftPadCam.transform.position - metaCube.transform.position) * Quaternion.Euler(RotationForMenu(side));
                metaCube.transform.localScale = Vector3.one * 0.5f;
                menuCanvas.renderMode = RenderMode.WorldSpace;
            }
            else
            {
                cm.overlayCam.transform.position = cm.mainCam.transform.position;
                cm.overlayCam.transform.rotation = cm.mainCam.transform.rotation;
                metaCube.transform.localPosition = new Vector3(0, 0, metaCubeSize * 0.5f);
                metaCube.transform.rotation = Quaternion.Euler(RotationForMenu(side));
            }
            metaCube.gameObject.SetActive(true);
        }
        else
        {
            currentActiveMenu.MenuInteractionOff();
            if (currentStage == Stage.Neon)
            {
                metaCube.transform.DORotateQuaternion(Quaternion.LookRotation(cm.leftPadCam.transform.position - metaCube.transform.position) * Quaternion.Euler(RotationForMenu(side)), 1).OnComplete(() =>
                {
                    currentActiveMenu = metaCubeSides[side];
                    metaCube.gameObject.transform.rotation = Quaternion.LookRotation(cm.leftPadCam.transform.position - metaCube.transform.position) * Quaternion.Euler(RotationForMenu(side));
                    currentActiveMenu.MenuInteractionOn();
                });
            }
            else
            {
                metaCube.transform.DORotate(RotationForMenu(side), 1).OnComplete(() =>
                {
                    currentActiveMenu = metaCubeSides[side];
                    metaCube.gameObject.transform.rotation = Quaternion.Euler(RotationForMenu(side));
                    currentActiveMenu.MenuInteractionOn();
                });
            }

        }
        HideOverlappingCubes(side);
        currentActiveMenuIndex = side;
    }
    public void TurnOffMetaCube()
    {
        currentActiveMenu.MenuInteractionOff();
        metaCube.gameObject.SetActive(false);
        cm.overlayCam.transform.position = cm.mainCam.transform.position;
        cm.overlayCam.transform.rotation = cm.mainCam.transform.rotation;
        metaCube.transform.localScale = Vector3.one;
        menuCanvas.renderMode = RenderMode.ScreenSpaceCamera;
        metaCube.transform.rotation = Quaternion.identity;
        metaCube.transform.localScale = Vector3.one;
    }
    Vector3 RotationForMenu(int side)
    {
        switch (side)
        {
            default:
            case 0:
                return Vector3.zero;
            case 1:
                return new Vector3(180, 0, 0);
            case 2:
                return currentStage == Stage.Neon ? new Vector3(0,90,-90) : new Vector3(90, 0, 0);
            case 3:
                return currentStage == Stage.Neon ? new Vector3(180,90,90) : new Vector3(-90, 0, 0);
            case 4:
                return currentStage == Stage.Neon ? new Vector3(-90,90,0) : new Vector3(0, -90, 0);
            case 5:
                return new Vector3(0, 90, 0);
        }
    }
    void ShowOverlappingCubes()
    {
        foreach (PongUiMenu menu in metaCubeSides)
        {
            for (int i = 0; i < 16; i++)
            {
                menu.transform.GetChild(i).GetComponent<CanvasGroup>().alpha = 1;
            }
        }
    }
    public void HideOverlappingCubes(int side)
    {
        foreach (CubesToHideFromFace cubesToHideFromFace in cubesToHideForMenu[side])
        {
            foreach (int cube in cubesToHideFromFace.cubesToHide)
            {
                metaCubeSides[cubesToHideFromFace.face].transform.GetChild(cube).GetComponent<CanvasGroup>().alpha = 0;
            }
        }
        List<int> metaCubeFaces = new List<int>(){0,1,2,3,4,5};
        metaCubeFaces.Remove(side);
        foreach (int metaCubeFace in metaCubeFaces)
        {
            metaCubeSides[metaCubeFace].MenuInteractionOff();
        }
    }
}
