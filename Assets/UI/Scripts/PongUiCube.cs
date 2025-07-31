using System;
using System.Collections;
using UnityEngine.Events;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine;
using TMPro;
using DG.Tweening;
using System.Linq;
using System.Collections.Generic;
public class PongUiCube : Selectable, IPointerClickHandler, ISubmitHandler
{
    public Material mat;
    public bool vanishMetaCubeOnClick = true;
    public bool highlighted = false;
    protected float fadeDuration = 0.1f;
    public GameObject[] sides = new GameObject[6];
    protected float size => GetComponentInParent<GridLayoutGroup>().cellSize.x;
    public Sequence sqnc;
    [Serializable]
    public class CubeClickedEvent : UnityEvent { }
    [SerializeField]
    protected CubeClickedEvent m_OnClick = new CubeClickedEvent();
    public CubeClickedEvent onClick
    {
        get { return m_OnClick; }
        set { m_OnClick = value; }
    }
    [Serializable]
    public class OnMetaCubeVanishedEvent : UnityEvent { }
    [SerializeField]
    protected OnMetaCubeVanishedEvent m_OnMetaCubeVanished = new OnMetaCubeVanishedEvent();
    public OnMetaCubeVanishedEvent onMetaCubeVanished
    {
        get { return m_OnMetaCubeVanished; }
        set { m_OnMetaCubeVanished = value; }
    }
    public bool submitted = false;
    protected void SetCube()
    {
        sides[0].transform.localPosition = new Vector3(0, 0, -size * 0.5f);
        sides[0].transform.localRotation = Quaternion.Euler(0, 0, 0);

        sides[1].transform.localPosition = new Vector3(0, 0, size * 0.5f);
        sides[1].transform.localRotation = Quaternion.Euler(0, 180, 180);

        sides[2].transform.localPosition = new Vector3(0, -size * 0.5f, 0);
        sides[2].transform.localRotation = Quaternion.Euler(-90, 0, 0);

        sides[3].transform.localPosition = new Vector3(0, size * 0.5f, 0);
        sides[3].transform.localRotation = Quaternion.Euler(90, 0, 0);

        sides[4].transform.localPosition = new Vector3(-size * 0.5f, 0, 0);
        sides[4].transform.localRotation = Quaternion.Euler(0, 90, 0);

        sides[5].transform.localPosition = new Vector3(size * 0.5f, 0, 0);
        sides[5].transform.localRotation = Quaternion.Euler(0, 270, 0);
    }
    protected override void Awake()
    {
        List<GameObject> tempSides = new List<GameObject>();
        foreach (Transform child in transform)
        {
            tempSides.Add(child.gameObject);
        }
        sides = tempSides.ToArray();
        mat = new Material(PongBehaviour.um.cubeMaterial);
        base.Awake();
    }
    protected override void Start()
    {
        base.Start();
        SetCube();
        // mat = new Material(PongBehaviour.um.cubeMaterial);
        string txt = sides[0].GetComponentInChildren<TMP_Text>().text;
        foreach (GameObject side in sides)
        {
            side.GetComponent<Image>().material = mat;
            TMP_Text txtObj = side.GetComponentInChildren<TMP_Text>();
            txtObj.text = txt;
            txtObj.color = PongBehaviour.um.cubeTextColor;
            txtObj.alpha = 0;
            txtObj.transform.localPosition = new Vector3(0, 0, -(size * 0.01f));
        }
        transform.localPosition = new Vector3(transform.localPosition.x, transform.localPosition.y, -(size * 0.5f));
        sqnc = DOTween.Sequence();
        sqnc.Append(transform.DOLocalRotate(new Vector3(2.5f, 2.5f, 2.5f), 0.2f).SetEase(Ease.Linear));
        sqnc.Append(transform.DOLocalRotate(new Vector3(-2.5f, 0, 0), 0.2f).SetEase(Ease.Linear));
        sqnc.Append(transform.DOLocalRotate(new Vector3(2.5f, -2.5f, -2.5f), 0.2f).SetEase(Ease.Linear));
        sqnc.Append(transform.DOLocalRotate(new Vector3(0, 0, 0), 0.2f).SetEase(Ease.Linear));
        sqnc.SetEase(Ease.Linear);
        sqnc.SetAutoKill(false);
        sqnc.OnComplete(() => sqnc.Restart());
        sqnc.Complete();
    }
    protected override void OnEnable()
    {
        base.OnEnable();
        submitted = false;
        transform.DOKill();
        StopAllCoroutines();
        if (PongBehaviour.am.beatsOn)
        {
            mat.SetColor("_BaseColor", PongBehaviour.um.cubeNormalColor);
        }
        else
        {
            mat.SetColor("_BaseColor", Color.Lerp(Color.white, PongBehaviour.um.cubeNormalColor, PongBehaviour.vfx.styleLerpValue));
        }
        mat.SetFloat("_DissolveProgress", 0);
        transform.localScale = Vector3.one;
        foreach (GameObject side in sides)
        {
            side.GetComponentInChildren<TMP_Text>().color = PongBehaviour.um.cubeTextColor;
        }
    }
    public void CubeInteractionOn()
    {
        interactable = true;
        NavOn();
        foreach (GameObject side in sides)
        {
            side.GetComponentInChildren<TMP_Text>().alpha = 1;
        }
    }
    public void CubeInteractionOff()
    {
        interactable = false;
        NavOff();
        foreach (GameObject side in sides)
        {
            side.GetComponentInChildren<TMP_Text>().alpha = 0;
        }
    }
    public override void OnSelect(BaseEventData eventData)
    {
        base.OnSelect(eventData);
        StopTransitions();
        transform.DOComplete();
        StartCoroutine("CycleGlow");
    }
    public override void OnDeselect(BaseEventData eventData)
    {
        base.OnDeselect(eventData);
        StopTransitions();
        StartCoroutine("CycleDim");
    }
    public virtual void OnPointerClick(PointerEventData eventData)
    {
        if (!submitted)
        {
            submitted = true;
            StartCoroutine("CycleDimSelected");
        }
    }
    public virtual void OnSubmit(BaseEventData eventData)
    {
        if (!submitted)
        {
            submitted = true;
            StartCoroutine("CycleDimSelected");
        }
    }
    public void StopTransitions()
    {
        StopCoroutine("CycleGlow");
        StopCoroutine("CycleDim");
        StopCoroutine("CycleDimSelected");
    }
    protected IEnumerator CycleGlow()
    {
        float t = 0f;
        highlighted = true;
        while (t < fadeDuration)
        {
            t += Time.unscaledDeltaTime;
            if (t > fadeDuration) { t = fadeDuration; }
            if (PongBehaviour.am.beatsOn)
            {
                mat.SetColor("_BaseColor", Color.Lerp(PongBehaviour.um.cubeNormalColor, PongBehaviour.um.cubeHighlightedColor, t / fadeDuration));
            }
            else
            {
                mat.SetColor("_BaseColor", Color.Lerp(Color.Lerp(Color.white, PongBehaviour.um.cubeNormalColor, PongBehaviour.vfx.styleLerpValue), Color.Lerp(Color.grey, PongBehaviour.um.cubeHighlightedColor, PongBehaviour.vfx.styleLerpValue), t / fadeDuration));
            }
            
            yield return null;
        }
        sqnc.OnComplete(() => sqnc.Restart());
        sqnc.SetAutoKill(false);
        sqnc.Restart();
    }
    protected IEnumerator CycleDim()
    {
        highlighted = false;
        sqnc.OnComplete(null);
        sqnc.SetAutoKill(true);
        float t = 0f;
        if (interactable)
        {
            PongManager.am.PlayAudio(AudioType.UiSwitchCubes, transform.position);            
        }
        while (t < fadeDuration)
        {
            t += Time.unscaledDeltaTime;
            if (t > fadeDuration) { t = fadeDuration; }
            mat.SetColor("_BaseColor", Color.Lerp(PongBehaviour.um.cubeHighlightedColor, PongBehaviour.um.cubeNormalColor, t / fadeDuration));
            if (PongBehaviour.am.beatsOn)
            {
                mat.SetColor("_BaseColor", Color.Lerp(PongBehaviour.um.cubeHighlightedColor, PongBehaviour.um.cubeNormalColor, t / fadeDuration));
            }
            else
            {
                mat.SetColor("_BaseColor", Color.Lerp(Color.Lerp(Color.grey, PongBehaviour.um.cubeHighlightedColor, PongBehaviour.vfx.styleLerpValue), Color.Lerp(Color.white, PongBehaviour.um.cubeNormalColor, PongBehaviour.vfx.styleLerpValue), t / fadeDuration));
            }
            yield return null;
        }
    }
    protected IEnumerator CycleDimSelected()
    {
        NavOff();
        highlighted = false;
        onClick.Invoke();
        float t = 0f;
        PongManager.am.PlayAudio(AudioType.UiConfirm, transform.position);
        sqnc.OnComplete(null);
        sqnc.SetAutoKill(true);
        sqnc.Complete();
        while (t < fadeDuration)
        {
            t += Time.unscaledDeltaTime;
            if (t > fadeDuration) { t = fadeDuration; }
            mat.SetColor("_BaseColor", Color.Lerp(PongBehaviour.um.cubeHighlightedColor, PongBehaviour.um.cubeNormalColor, t / fadeDuration));
            if (PongBehaviour.am.beatsOn)
            {
                mat.SetColor("_BaseColor", Color.Lerp(PongBehaviour.um.cubeHighlightedColor, PongBehaviour.um.cubeNormalColor, t / fadeDuration));
            }
            else
            {
                mat.SetColor("_BaseColor", Color.Lerp(Color.Lerp(Color.grey, PongBehaviour.um.cubeHighlightedColor, PongBehaviour.vfx.styleLerpValue), Color.Lerp(Color.grey*1.5f, PongBehaviour.um.cubePressedColor, PongBehaviour.vfx.styleLerpValue), t / fadeDuration));
            }
            yield return null;
        }
    }
    public void NavOff()
    {
        Navigation nav = navigation;
        nav.mode = Navigation.Mode.None;
        navigation = nav;
    }
    public void NavOn()
    {
        Navigation nav = navigation;
        nav.mode = Navigation.Mode.Automatic;
        navigation = nav;
    }
}
