using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using UnityEngine;
using UnityEngine.VFX;
using Cinemachine;
using AudioLocker;
public class DebuffSlow : DebuffEntity
{
    public Sequence sqnc;
    public VisualEffect visualEffect;
    protected override void OnEnable()
    {
        base.OnEnable();
        suctionThreshold = 1;
        suctionRange = (col as SphereCollider).radius * 2;
#if !UNITY_WEBGL
        transform.localScale = Vector3.one * 3;
        visualEffect.SetFloat("DistortionAlpha", 0);
        visualEffect.SetBool("StartCore", false);
        visualEffect.SetFloat("CoreScale", 0);
        visualEffect.SetBool("StartStars", false);
        visualEffect.SetFloat("DistortionScale", 0);
        visualEffect.playRate = 1;
        visualEffect.Stop();
#else
        transform.localScale = Vector3.zero;
#endif
        field.fragmentStore.droppedAllCubeFragments.RemoveAllListeners();
        field.fragmentStore.droppedAllCubeFragments.AddListener(() => OnAllBallFragmentsDropped());
    }
    public void EnterStage()
    {
#if !UNITY_WEBGL
        visualEffect.transform.DOShakePosition(5, 0.01f, 20, 90, false, false, ShakeRandomnessMode.Harmonic).SetLoops(-1);
#endif 
        sqnc = DOTween.Sequence();

        sqnc.Append(transform.DOPath(activeOrbitPath, 30, PathType.Linear).SetEase(Ease.Linear));

        sqnc.SetEase(Ease.Linear);
        sqnc.SetAutoKill(false);
        sqnc.OnComplete(() => sqnc.Restart());
        col.enabled = true;
        field.leftPad.meshR.material.SetFloat("_SuctionRange", suctionRange);
        field.rightPad.meshR.material.SetFloat("_SuctionRange", suctionRange);
    }
    void OnDisable()
    {
        field.leftPad.meshR.material.SetFloat("_SuctionRange", 0);
        field.rightPad.meshR.material.SetFloat("_SuctionRange", 0);
        field.fragmentStore.ResetBallFragmentsMaterials();

        field.leftPad.speedModifier = 1;
        field.rightPad.speedModifier = 1;
        field.ball.speedModifier = 1;
    }
    public override void OnGobbledAllFragments()
    {
#if UNITY_WEBGL
        StartCoroutine(CycleStartWebBlackhole());
#else
        StartCoroutine(CycleStartBlackhole());
#endif 
    }
    protected override void FixedUpdate()
    {
        field.leftPad.meshR.material.SetVector("_SuctionTarget", transform.position);
        field.rightPad.meshR.material.SetVector("_SuctionTarget", transform.position);
        field.fragmentStore.cubeFragments.ForEach(frg => frg.meshR.material.SetVector("_SuctionTarget", transform.position));
    }
    IEnumerator CycleStartBlackhole()
    {
        cm.MainCameraNoise(CameraManager.activeVCam, 0.25f, 5, 5);
        visualEffect.Play();
        float t = 0f;
        while (t < 5)
        {
            t += Time.deltaTime;
            if (t > 5) { t = 5; }
            visualEffect.SetFloat("CoreScale", Mathf.SmoothStep(0, 1, t / 5));
            yield return null;
        }
        t = 0f;
        am.PlayAudio(PongAudioType.SingularityImplosion, transform.position);
        yield return new WaitForSeconds(1);
        visualEffect.playRate = 0.01f;
        visualEffect.SetBool("StartCore", true);
        visualEffect.SetBool("StartStars", true);
        yield return new WaitForSeconds(1);
        cm.MainCameraNoise(CameraManager.activeVCam, 0.5f, 3, 1);
        visualEffect.SetFloat("DistortionAlpha", 0.25f);
        while (t < 1)
        {
            t += Time.deltaTime;
            if (t > 1) { t = 1; }
            visualEffect.playRate = Mathf.SmoothStep(0.01f, 1, t / 1);
            visualEffect.SetFloat("DistortionScale", Mathf.Lerp(0, 10f, 1));
            yield return null;
        }
        EnterStage();
    }
    IEnumerator CycleStartWebBlackhole()
    {
        cm.MainCameraNoise(CameraManager.activeVCam, 0.25f, 5, 5);
        Vector3 initialScale = Vector3.one * 3;
        float t = 0f;
        am.PlayAudio(PongAudioType.SingularityImplosion, transform.position);
        while (t < 5)
        {
            t += Time.deltaTime;
            if (t > 5) { t = 5; }
            transform.localScale = Vector3.Lerp(Vector3.zero, initialScale, t / 5);
            yield return null;
        }
        transform.localScale = initialScale;
        EnterStage();
    }
}
