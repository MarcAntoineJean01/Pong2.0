using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using System.Linq;
using TMPro;
using System;
using UnityEngine.Events;
public class PongUiMenu : MonoBehaviour
{
    public GridLayoutGroup grid;
    public TMP_Text title;
    public Color titleColor;
    public List<PongUiCube> pongUiCubes = new List<PongUiCube>();
    public List<FakeUiCube> fakeUiCubes = new List<FakeUiCube>();
    public List<FakeUiCube> fakeUiCubesWithText = new List<FakeUiCube>();
    [Serializable]
    public class MetaCubeVanishedEvent : UnityEvent { }
    [SerializeField]
    protected MetaCubeVanishedEvent m_MetaCubeVanished = new MetaCubeVanishedEvent();
    public MetaCubeVanishedEvent metaCubeVanished
    {
        get { return m_MetaCubeVanished; }
        set { m_MetaCubeVanished = value; }
    }
    void Start()
    {
        foreach (PongUiCube cube in pongUiCubes)
        {
            cube.onClick.AddListener(() => CubeClickInvoke(cube));
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
    }
    public void MenuInteractionOff()
    {
        pongUiCubes.ForEach(cube => cube.CubeInteractionOff());
        title.alpha = 0;
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
        PongBehaviour.um.metaCube.transform.DOScale(new Vector3(0.01f, 0.01f, 0.01f), 1).SetEase(Ease.InSine).OnComplete(() =>  PongBehaviour.um.metaCube.transform.DOKill());
        yield return null;
        while (DOTween.IsTweening( PongBehaviour.um.metaCube.transform))
        {
            yield return null;
        }
        PongBehaviour.um.menuOn = false;
        cube.onMetaCubeVanished.Invoke();
        metaCubeVanished.Invoke();
    }
}
