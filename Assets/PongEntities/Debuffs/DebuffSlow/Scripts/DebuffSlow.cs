using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using UnityEngine;
using UnityEngine.VFX;
using Cinemachine;
public class DebuffSlow : DebuffEntity
{
    bool contained = false;
    bool waitForNextFragment = false;
    // public CinemachineVirtualCamera testcam;
    public Sequence sqnc;
    public OrbitPath orbitPath;
    public VisualEffect visualEffect;
    public bool doneSpawningBlackhole = false;
    public bool enteredStage = false;
    // float scaleMultiplier = 2;
    float padSuctionThreshold = 0f;
    float ballFragmentSuctionThreshold = 1;
    float padSuctionRange => (col as SphereCollider).radius * 2;
    float ballFragmentSuctionRange => (col as SphereCollider).radius * 2;
    List<Fragment> caughtFragments = new List<Fragment>();
    bool firstFragmentCaught = false;
    Vector3 initialPosition => new Vector3(0, 0, stagePosZ + PongManager.sizes.fieldDepth * 0.5f);
    // Vector3 originalScale = Vector3.one;
    Vector3[] newCalculatedPath
    {
        get
        {
            Vector3[] points = new Vector3[segments];
            Vector3 point = Vector3.zero;
            switch (orbitPath)
            {
                default:
                case OrbitPath.Ellipse:
                    for (int i = 0; i < (segments - 1); i++)
                    {
                        float angle = (float)i / (float)(segments - 1) * 360 * Mathf.Deg2Rad;
                        point.x = Mathf.Sin(angle) * fieldBounds.x * 0.9f;
                        point.y = Mathf.Cos(angle) * fieldBounds.y * 0.9f;
                        point.z = stagePosZ + PongManager.sizes.ballDiameter * 2.5f;
                        points[i] = point;
                        // points[i] = new Vector3(point.x, point.y, stagePosZ);
                    }
                    points[segments - 1] = points[0];
                    return points;
                case OrbitPath.Gerono:
                    for (int i = 0; i < (segments - 1); i++)
                    {
                        float angle = (float)i / (float)(segments - 1) * 360 * Mathf.Deg2Rad;
                        point.x = Mathf.Cos(angle) * fieldBounds.x * 0.9f;
                        point.y = Mathf.Sin(2 * angle) * fieldBounds.y * 0.9f;
                        point.z = stagePosZ + PongManager.sizes.ballDiameter * 2.5f;
                        points[i] = point;
                        // points[i] = new Vector3(point.x, point.y, stagePosZ);
                    }
                    points[segments - 1] = points[0];
                    // SHIFT START BY 1/4 TO START CENTER SCREEN (CROSS SECTION OF THE FIGURE8 SHAPE OF THE PATH)
                    return points.ToList().GetRange(segments / 4, (int)(segments * 0.75f)).Concat(points.ToList().GetRange(0, segments / 4)).ToArray();
                case OrbitPath.Bernoulli:
                    for (int i = 0; i < (segments - 1); i++)
                    {
                        float angle = (float)i / (float)(segments - 1) * 360 * Mathf.Deg2Rad;
                        point.x = Mathf.Cos(angle) * fieldBounds.x * 0.9f;
                        point.y = Mathf.Sin(2 * angle) / 1.5f * fieldBounds.y * 0.9f;
                        point.z = stagePosZ + PongManager.sizes.ballDiameter * 2.5f;
                        points[i] = point;
                        // points[i] = new Vector3(point.x, point.y, stagePosZ);
                    }
                    points[segments - 1] = points[0];
                    // SHIFT START BY 1/4 TO START CENTER SCREEN (CROSS SECTION OF THE FIGURE8 SHAPE OF THE PATH)
                    return points.ToList().GetRange(segments / 4, (int)(segments * 0.75f)).Concat(points.ToList().GetRange(0, segments / 4)).ToArray();
            }
        }
    }
    // Vector3[] calculatedPath
    // {
    //     get
    //     {
    //         Vector3[] points = new Vector3[segments];
    //         Vector3 point = Vector3.zero;
    //         switch (orbitPath)
    //         {
    //             default:
    //             case OrbitPath.Ellipse:
    //                 for (int i = 0; i < (segments - 1); i++)
    //                 {
    //                     float angle = (float)i / (float)(segments - 1) * 360 * Mathf.Deg2Rad;
    //                     point.x = Mathf.Sin(angle) * fieldBounds.x * 0.9f;
    //                     point.y = Mathf.Cos(angle) * fieldBounds.y * 0.9f;

    //                     float normalizedProgress = Mathf.Abs(point.x) / (fieldBounds.x * 0.9f);
    //                     float easing = orbitalEase.Evaluate(normalizedProgress);
    //                     point.z = Mathf.Lerp(initialPosition.z, stagePosZ, easing);
    //                     points[i] = point;
    //                     // points[i] = new Vector3(point.x, point.y, stagePosZ);
    //                 }
    //                 points[segments - 1] = points[0];
    //                 return points;
    //             case OrbitPath.Gerono:
    //                 for (int i = 0; i < (segments - 1); i++)
    //                 {
    //                     float angle = (float)i / (float)(segments - 1) * 360 * Mathf.Deg2Rad;
    //                     point.x = Mathf.Cos(angle) * fieldBounds.x * 0.9f;
    //                     point.y = Mathf.Sin(2 * angle) * fieldBounds.y * 0.9f;

    //                     float normalizedProgress = Mathf.Abs(point.x) / (fieldBounds.x * 0.9f);
    //                     float easing = orbitalEase.Evaluate(normalizedProgress);
    //                     point.z = Mathf.Lerp(initialPosition.z, stagePosZ, easing);
    //                     points[i] = point;
    //                     // points[i] = new Vector3(point.x, point.y, stagePosZ);
    //                 }
    //                 points[segments - 1] = points[0];
    //                 // SHIFT START BY 1/4 TO START CENTER SCREEN (CROSS SECTION OF THE FIGURE8 SHAPE OF THE PATH)
    //                 return points.ToList().GetRange(segments / 4, (int)(segments * 0.75f)).Concat(points.ToList().GetRange(0, segments / 4)).ToArray();
    //             case OrbitPath.Bernoulli:
    //                 for (int i = 0; i < (segments - 1); i++)
    //                 {
    //                     float angle = (float)i / (float)(segments - 1) * 360 * Mathf.Deg2Rad;
    //                     point.x = Mathf.Cos(angle) * fieldBounds.x * 0.9f;
    //                     point.y = Mathf.Sin(2 * angle) / 1.5f * fieldBounds.y * 0.9f;

    //                     float normalizedProgress = Mathf.Abs(point.x) / (fieldBounds.x * 0.9f);
    //                     float easing = orbitalEase.Evaluate(normalizedProgress);
    //                     point.z = Mathf.Lerp(initialPosition.z, stagePosZ, easing);
    //                     points[i] = point;
    //                     // points[i] = new Vector3(point.x, point.y, stagePosZ);
    //                 }
    //                 points[segments - 1] = points[0];
    //                 // SHIFT START BY 1/4 TO START CENTER SCREEN (CROSS SECTION OF THE FIGURE8 SHAPE OF THE PATH)
    //                 return points.ToList().GetRange(segments / 4, (int)(segments * 0.75f)).Concat(points.ToList().GetRange(0, segments / 4)).ToArray();
    //         }
    //     }
    // }
    // Vector3[] calculatedPathFlattened
    // {
    //     get
    //     {
    //         Vector3[] oldPath = calculatedPath;
    //         for (int i = 0; i < oldPath.Length; i++)
    //         {
    //             oldPath[i].z = stagePosZ;
    //         }
    //         return oldPath;

    //     }
    // }
    protected override void OnEnable()
    {
        base.OnEnable();
        transform.position = initialPosition;
        // originalScale = transform.localScale;
        transform.position = newCalculatedPath[0];
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
        col.enabled = false;
    }
    public void StartBlackhole()
    {
#if UNITY_WEBGL
        StartCoroutine(CycleStartWebBlackhole());
#else
        StartCoroutine(CycleStartBlackhole());
#endif 
    }
    public void EnterStage()
    {
#if !UNITY_WEBGL
        visualEffect.transform.DOShakePosition(5, 0.01f, 20, 90, false, false, ShakeRandomnessMode.Harmonic).SetLoops(-1);
#endif 
        sqnc = DOTween.Sequence();

        sqnc.Append(transform.DOPath(newCalculatedPath, 30, PathType.Linear).SetEase(Ease.Linear));

        sqnc.SetEase(Ease.Linear);
        sqnc.SetAutoKill(false);
        sqnc.OnComplete(() => sqnc.Restart());
        col.enabled = true;
        enteredStage = true;
        field.leftPad.meshR.material.SetFloat("_SuctionRange", padSuctionRange);
        field.leftPad.meshR.material.SetFloat("_SuctionThreshold", padSuctionThreshold);
        field.rightPad.meshR.material.SetFloat("_SuctionRange", padSuctionRange);
        field.rightPad.meshR.material.SetFloat("_SuctionThreshold", padSuctionThreshold);
        // field.ball.meshR.material.SetFloat("_SuctionRange", ballSuctionRange);
        // field.ball.meshR.material.SetFloat("_SuctionThreshold", ballSuctionThreshold);
        if (field.ball.fragmented)
        {
            field.ball.fragments[0].meshR.material.SetFloat("_SuctionRange", ballFragmentSuctionRange);
            field.ball.fragments[0].meshR.material.SetFloat("_SuctionThreshold", ballFragmentSuctionThreshold);
        }
    }
    void ExitStage()
    {
        field.leftPad.meshR.material.SetFloat("_SuctionRange", 0);
        field.leftPad.meshR.material.SetFloat("_SuctionThreshold", 0);
        field.rightPad.meshR.material.SetFloat("_SuctionRange", 0);
        field.rightPad.meshR.material.SetFloat("_SuctionThreshold", 0);
        // field.ball.meshR.material.SetFloat("_SuctionRange", 0);
        // field.ball.meshR.material.SetFloat("_SuctionThreshold", 0);
        if (field.ball.fragmented)
        {
            field.ball.fragments[0].meshR.material.SetFloat("_SuctionRange", 0);
            field.ball.fragments[0].meshR.material.SetFloat("_SuctionThreshold", 0);
        }

        field.leftPad.speedModifier = 1;
        field.rightPad.speedModifier = 1;
        field.ball.speedModifier = 1;
        foreach (Fragment frag in caughtFragments)
        {
            GameObject.Destroy(frag.gameObject);
        }
        caughtFragments = new List<Fragment>();
    }
    protected override void FixedUpdate()
    {

        field.leftPad.meshR.material.SetVector("_SuctionTarget", transform.position);
        field.rightPad.meshR.material.SetVector("_SuctionTarget", transform.position);
        foreach (Fragment fragment in caughtFragments)
        {
            fragment.meshR.material.SetVector("_SuctionTarget", transform.position);
            // fragment.meshR.material.SetFloat("_SuctionRange", ballFragmentSuctionRange * transform.localScale.x);
        }
        if (!contained)
        {
            // float normalizedProgress = Mathf.Abs(transform.position.x) / (fieldBounds.x * 0.9f);
            // float easing = orbitalEase.Evaluate(normalizedProgress);
            // transform.localScale = Vector3.Lerp(originalScale * scaleMultiplier, originalScale, easing);
            // (col as SphereCollider).center = Vector3.Lerp(new Vector3(0, 0, 1), Vector3.zero, easing);
            // field.ball.meshR.material.SetVector("_SuctionTarget", transform.position);
            if (field.ball.fragmented && !waitForNextFragment && col.enabled)
            {
                field.ball.fragments[0].meshR.material.SetVector("_SuctionTarget", transform.position);
                // if (Vector3.Distance(transform.position, field.ball.fragments[0].transform.position) <= ballFragmentSuctionRange)
                if (field.ball.fragmented)
                {
                    field.ball.fragments[0].col.enabled = false;
                    field.ball.fragments[0].transform.SetParent(PongManager.fieldParent.transform, true);
                    caughtFragments.Add(field.ball.fragments[0]);
                    field.ball.fragments.Remove(field.ball.fragments[0]);
                    StartCoroutine(CycleGobbleFragment(caughtFragments.Last()));
                    if (field.ball.fragments.Count <= 0)
                    {
                        BallEntity newBall = builder.MakeFullBall(BallMesh.IcosahedronRough, 1.2f);
                        newBall.SetBallForStage();
                        field.ReplaceEntity(Entity.Ball, newBall);
                        // StartCoroutine(CyclePrepareForSwitch());
                        if (PongManager.mainSettings.cutScenesOn)
                        {
                            csm.PlayScene(CutScene.PolyFeelsEvenLighter);
                        }

                    }
                    else
                    {
                        if (!firstFragmentCaught)
                        {
                            firstFragmentCaught = true;
                            if (PongManager.mainSettings.cutScenesOn)
                            {
                                csm.PlayScene(CutScene.PolyFeelsLighter);
                            }
                        }
                        field.ball.fragments[0].meshR.material.SetFloat("_SuctionRange", ballFragmentSuctionRange);
                        field.ball.fragments[0].meshR.material.SetFloat("_SuctionThreshold", ballFragmentSuctionThreshold);
                        StartCoroutine(CycleWaitForNextFragment());
                    }
                }
            }
        }
    }
    void OnDisable()
    {
        ExitStage();
    }
    // IEnumerator CyclePrepareForSwitch()
    // {
    //     contained = true;
    //     (col as SphereCollider).center = Vector3.zero;
    //     Vector3 currentScale = transform.localScale;
    //     sqnc.SetAutoKill(true);
    //     sqnc.OnComplete(() => sqnc.Kill());
    //     yield return null;
    //     while (sqnc.active)
    //     {
    //         yield return null;
    //     }
    //     float t = 0f;
    //     Vector3 initalPos = transform.position;
    //     while (t < 3)
    //     {
    //         t += Time.deltaTime;
    //         if (t > 3) { t = 3; }
    //         transform.position = Vector3.Lerp(initalPos, calculatedPathFlattened[0], t / 3);
    //         visualEffect.SetFloat("DistortionScale", Mathf.Lerp(2.5f, 10, t / 3));
    //         transform.localScale = Vector3.Lerp(currentScale, originalScale, t / 3);
    //         yield return null;
    //     }
    //     visualEffect.SetFloat("DistortionScale", 10);
    //     transform.localScale = originalScale;
    //     transform.position = calculatedPathFlattened[0];

    //     Sequence newSequence = DOTween.Sequence();
    //     newSequence.Append(transform.DOPath(calculatedPathFlattened, 30, PathType.Linear).SetEase(Ease.Linear));
    //     newSequence.SetEase(Ease.Linear);
    //     newSequence.Pause();
    //     newSequence.SetAutoKill(false);
    //     newSequence.OnComplete(() => newSequence.Restart());
    //     sqnc = newSequence;
    //     sqnc.Play();
    //     if (PongManager.mainSettings.cutScenesOn)
    //     {
    //         csm.PlayScene(CutScene.ScoldingFromPixy);
    //     }
    //     else
    //     {
    //         List<Edge> edges = new List<Edge>()
    //         {
    //             field.leftWall,
    //             field.rightWall,
    //             field.topFloor,
    //             field.bottomFloor,
    //             field.background
    //         };
    //         edges.ForEach(edge => { edge.meshR.material.SetFloat("_EmissionIntensity", 1); edge.meshR.material.SetFloat("_DissolveEdgeDepth", 0.01f); });
    //         t = 0f;
    //         while (t < pm.gameEffects.wallDissolveSpeed)
    //         {
    //             t += Time.unscaledDeltaTime;
    //             if (t > pm.gameEffects.wallDissolveSpeed) { t = pm.gameEffects.wallDissolveSpeed; }
    //             edges.ForEach(edge => edge.meshR.material.SetFloat("_DissolveProgress", Mathf.SmoothStep(1, 0, t / pm.gameEffects.wallDissolveSpeed)));
    //             yield return null;
    //         }
    //         edges.ForEach(edge => { edge.meshR.material.SetFloat("_EmissionIntensity", 0); edge.meshR.material.SetFloat("_DissolveEdgeDepth", 0); });
    //         csm.unsuspendUniverseStage.Invoke();
    //     }
    // }
    IEnumerator CycleWaitForNextFragment()
    {
        waitForNextFragment = true;
        yield return new WaitForSeconds(2);
        waitForNextFragment = false;
    }
    IEnumerator CycleGobbleFragment(Fragment fragment)
    {
        int direction = UnityEngine.Random.Range(0, 2) > 0 ? -1 : 1;
        float angle = 1 * direction;
        float radius = 10;
        float angleSpeed = 50;
        float radialSpeed = 0.5f;
        float maxDistanceDelta = PongManager.sizes.fieldWidth / 90; // testing this is +/- the distance the ball moves in one fixed update
        // fragment.transform.DOBlendableMoveBy(transform.position, 10);
        while (Vector3.Distance(transform.position, fragment.transform.position) > ballFragmentSuctionThreshold)
        {
            fragment.transform.LookAt(transform.position);

            angle += Time.unscaledDeltaTime * angleSpeed;
            radius -= Time.unscaledDeltaTime * radialSpeed;
            radius = Mathf.Max(0, radius);

            float x = transform.position.x + radius * Mathf.Cos(Mathf.Deg2Rad * angle);
            float z = transform.position.z + radius * Mathf.Sin(Mathf.Deg2Rad * angle);
            float y = transform.position.y + radius * Mathf.Cos(Mathf.Deg2Rad * angle);
            // float y = transform.position.y;

            fragment.transform.position = Vector3.MoveTowards(fragment.transform.position, new Vector3(x, y, z), maxDistanceDelta);
            yield return null;
        }
        caughtFragments.Remove(fragment);
        GameObject.Destroy(fragment.gameObject);
    }
    IEnumerator CycleStartBlackhole()
    {
        yield return new WaitForSeconds(2);
        CinemachineBasicMultiChannelPerlin noise = CameraManager.activeVCam.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();
        // CinemachineBasicMultiChannelPerlin noise = testcam.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();
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
        doneSpawningBlackhole = true;
    }
    IEnumerator CycleStartWebBlackhole()
    {
        yield return new WaitForSeconds(2);
        CinemachineBasicMultiChannelPerlin noise = CameraManager.activeVCam.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();
        // CinemachineBasicMultiChannelPerlin noise = testcam.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();
        noise.m_AmplitudeGain = 0.25f;
        noise.m_FrequencyGain = 5;
        Vector3 initialScale = Vector3.one*3;
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
        doneSpawningBlackhole = true;
    }

}
