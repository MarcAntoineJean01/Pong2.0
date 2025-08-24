using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using System.Linq;
using TMPro;
using System;
using UnityEngine.Events;
using PongGame.PongLocker;
namespace PongGame
{
    public class PongUiMenu : MonoBehaviour
    {
        public GridLayoutGroup grid;
        public TMP_Text title;
        public Color titleColor;
        public List<PongUiCube> pongUiCubes
        {
            get
            {
                List<PongUiCube> tempList = new List<PongUiCube>();
                foreach (Transform child in transform)
                {
                    if (child.GetComponent<PongUiCube>() != null)
                    {
                        tempList.Add(child.GetComponent<PongUiCube>());
                    }
                }
                return tempList;
            }
        }
        public List<FakeUiCube> fakeUiCubes
        {
            get
            {
                List<FakeUiCube> tempList = new List<FakeUiCube>();
                foreach (Transform child in transform)
                {
                    if (child.GetComponent<FakeUiCube>() != null)
                    {
                        tempList.Add(child.GetComponent<FakeUiCube>());
                    }
                }
                return tempList;
            }
        }
        public List<FakeUiCube> fakeUiCubesWithText = new List<FakeUiCube>();
        [Serializable]
        public class MetaCubeVanishedEvent : UnityEvent { }
        [SerializeField]
        protected MetaCubeVanishedEvent m_MetaCubeVanished = new();
        public MetaCubeVanishedEvent metaCubeVanished
        {
            get { return m_MetaCubeVanished; }
            set { m_MetaCubeVanished = value; }
        }
        public static PongPlayerControls menuControls;
        void Start()
        {
            foreach (PongUiCube cube in pongUiCubes)
            {
                cube.onClick.AddListener(() => CubeClickInvoke(cube));
            }
            if (PongBehaviour.um.useMeshForUiCubes)
            {
                title.transform.localPosition = new Vector3(title.transform.localPosition.x, title.transform.localPosition.y, -(PongBehaviour.um.uiCubeSize * 0.5f));
                title.transform.localRotation = Quaternion.Euler((Vector3.right * 60) + (Vector3.back * 120));            
            }
        }

        void OnEnable()
        {
            PongBehaviour.um.menuOn = true;
            title.color = titleColor;
            if (PongBehaviour.am.beatsOn)
            {
                grid.spacing = Vector2.one * PongBehaviour.um.uiCubePadding;
            }
            else
            {
                grid.spacing = Vector2.Lerp(Vector2.zero, Vector2.one * PongBehaviour.um.uiCubePadding, PongBehaviour.vfx.styleLerpValue);
            }

            if (menuControls == null)
            {
                menuControls = new PongPlayerControls();
                menuControls.Disable();
                menuControls.UiCubeControls.Disable();
                menuControls.UiCubeControls.Cancel.performed += ctx => Back();
                menuControls.UiCubeControls.Cancel.Enable();
            }
        }
        void Back()
        {
            switch (UiManager.currentActiveMenuIndex)
            {

                case 1:
                    PongBehaviour.um.OpenStartMenu();
                    break;
                case 3:
                case 4:
                case 5:
                    if (PongManager.currentPhase != GamePhase.Startup)
                    {
                        PongBehaviour.um.OpenPauseMenu();
                    }
                    else
                    {
                        PongBehaviour.um.OpenSettingsMenu();
                    }
                    break;
                case 2:
                    StopAllCoroutines();
                    MenuInteractionOff();
                    StartCoroutine(CycleVanishMetaCube(null));
                    menuControls.Dispose();
                    menuControls = null;
                    break;
            }
        }
        public void MenuInteractionOn()
        {
            DOTween.defaultTimeScaleIndependent = true;
            title.alpha = 1;
            pongUiCubes.ForEach(cube => cube.CubeInteractionOn());
            fakeUiCubesWithText.ForEach(cube => cube.text.alpha = 1);
            pongUiCubes.First().Select();
            pongUiCubes.First().StopTransitions();
            pongUiCubes.First().StartCoroutine("CycleGlow");
            title.transform.DOLocalRotate(Vector3.back * 360, 12, RotateMode.LocalAxisAdd).SetEase(Ease.Linear).SetLoops(-1);
        }
        public void MenuInteractionOff()
        {
            pongUiCubes.ForEach(cube => cube.CubeInteractionOff());
            title.alpha = 0;
            title.transform.DOKill();
        }
        void OnDisable()
        {
            DOTween.defaultTimeScaleIndependent = false;
        }

        void CubeClickInvoke(PongUiCube cube)
        {
            StopAllCoroutines();
            if (cube.vanishMetaCubeOnClick)
            {
                MenuInteractionOff();
                StartCoroutine(CycleVanishMetaCube(cube));
            }
            else
            {
                cube.submitted = false;
            }
            foreach (PongUiCube cb in pongUiCubes)
            {
                cb.sqnc.Complete();
            }
        }
        private IEnumerator CycleVanishMetaCube(PongUiCube cube)
        {
            PongBehaviour.um.metaCube.transform.DORotate(new Vector3(45, 360, 90), 1, RotateMode.FastBeyond360).SetEase(Ease.Linear).SetLoops(-1);
            PongBehaviour.um.metaCube.transform.DOScale(new Vector3(0.01f, 0.01f, 0.01f), 1).SetEase(Ease.InSine).OnComplete(() => PongBehaviour.um.metaCube.transform.DOKill());
            yield return null;
            while (DOTween.IsTweening(PongBehaviour.um.metaCube.transform))
            {
                yield return null;
            }
            PongBehaviour.um.menuOn = false;
            if (cube != null)
            {
                cube.onMetaCubeVanished.Invoke();
            }
            else
            {
                PongBehaviour.newGameManager.UnPause();
            }
            metaCubeVanished.Invoke();
        }
    }
}
