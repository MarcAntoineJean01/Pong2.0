using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using UnityEngine;
using UnityEngine.VFX;
using Cinemachine;
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
        field.fragmentStore.removedAllCubeFragments.RemoveAllListeners();
        field.fragmentStore.droppedCubeFragment.RemoveAllListeners();
        field.fragmentStore.removedAllCubeFragments.AddListener(() => OnAllBallFragmentsDropped());
        field.fragmentStore.droppedCubeFragment.AddListener(frg => AddFragment(frg));
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
        if (!field.fragmentStore.cubeFragmentsEmpty)
        {
            field.fragmentStore.cubeFragments[0].meshR.material.SetFloat("_SuctionRange", suctionRange);
            field.fragmentStore.cubeFragments[0].meshR.material.SetFloat("_SuctionThreshold", suctionThreshold);
        }
    }
    void OnDisable()
    {
        field.leftPad.meshR.material.SetFloat("_SuctionRange", 0);
        field.rightPad.meshR.material.SetFloat("_SuctionRange", 0);
        field.fragmentStore.ResetBallFragmentsMaterials();

        field.leftPad.speedModifier = 1;
        field.rightPad.speedModifier = 1;
        field.ball.speedModifier = 1;
        foreach (DebuffFragment debuffFragment in fragments)
        {
            GameObject.Destroy(debuffFragment.fragment.gameObject);
        }
        fragments.Clear();
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
        foreach (DebuffFragment debuffFragment in fragments)
        {
            debuffFragment.fragment.meshR.material.SetVector("_SuctionTarget", transform.position);
        }
    }
    IEnumerator CycleStartBlackhole()
    {
        yield return new WaitForSeconds(2);
        CinemachineBasicMultiChannelPerlin noise = CameraManager.activeVCam.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();
        visualEffect.Play();
        noise.m_AmplitudeGain = 0.25f;
        noise.m_FrequencyGain = 5;
        float t = 0f;
        while (t < 5)
        {
            t += Time.deltaTime;
            if (t > 5) { t = 5; }
            visualEffect.SetFloat("CoreScale", Mathf.SmoothStep(0, 1, t / 5));
            yield return null;
        }
        noise.m_AmplitudeGain = 0;
        noise.m_FrequencyGain = 0;
        t = 0f;
        am.PlayAudio(AudioType.SingularityImplosion, transform.position);
        yield return new WaitForSeconds(1);
        visualEffect.playRate = 0.01f;
        visualEffect.SetBool("StartCore", true);
        visualEffect.SetBool("StartStars", true);
        yield return new WaitForSeconds(1);
        noise.m_AmplitudeGain = 0.5f;
        noise.m_FrequencyGain = 3;
        visualEffect.SetFloat("DistortionAlpha", 0.25f);
        while (t < 1)
        {
            t += Time.deltaTime;
            if (t > 1) { t = 1; }
            visualEffect.playRate = Mathf.SmoothStep(0.01f, 1, t / 1);
            visualEffect.SetFloat("DistortionScale", Mathf.Lerp(0, 10f, 1));
            yield return null;
        }
        noise.m_AmplitudeGain = 0;
        noise.m_FrequencyGain = 0;
        EnterStage();
    }
    IEnumerator CycleStartWebBlackhole()
    {
        yield return new WaitForSeconds(2);
        CinemachineBasicMultiChannelPerlin noise = CameraManager.activeVCam.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();
        noise.m_AmplitudeGain = 0.25f;
        noise.m_FrequencyGain = 5;
        Vector3 initialScale = Vector3.one * 3;
        float t = 0f;
        am.PlayAudio(AudioType.SingularityImplosion, transform.position);
        while (t < 5)
        {
            t += Time.deltaTime;
            if (t > 5) { t = 5; }
            transform.localScale = Vector3.Lerp(Vector3.zero, initialScale, t / 5);
            yield return null;
        }
        noise.m_AmplitudeGain = 0;
        noise.m_FrequencyGain = 0;
        transform.localScale = initialScale;
        EnterStage();
    }
    public override void DestroyAllFragments()
    {
        field.fragmentStore.cubeFragments.ForEach(frg => GameObject.Destroy(frg.gameObject));
        base.DestroyAllFragments();
    }
}
