using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;
using DG.Tweening;
using UnityEngine.VFX;
public class VFX : PongManager
{
    public GameObject padMagnetPrefab;
    public GameObject intersectionGhostPrefab;
    public GameObject wallAttractorEffectPrefab;
    public GameObject explosionPrefab;
    public GameObject ballghost;
    public GameObject leftPadGhost;
    public GameObject rightPadGhost;
    public Material ghostBallMaterial;
    public Material ghostPadMaterial;
    public Material energyShieldMaterial;
    public Color webEdgeBaseColor;
    public Color webWallAttractorColor;
    public VisualEffect activeWallAttractor;
    [SerializeField]
    AnimationCurve repulsorCurve;
    [SerializeField]
    AnimationCurve attractorCurve;
    [SerializeField]
    AnimationCurve explosionCurve;
    [SerializeField]
    AnimationCurve fieldBeatCurve;
    List<GameObject> liveEffects = new List<GameObject>();
    Sequence polyIdleAnimation;
    public AnimationCurve polyIdleAnimationCurve;
    [Range(0.1f, 100)]
    public float skyboxSpeed;
    public float styleLerpValue = 0;
    void Update()
    {
        RenderSettings.skybox.SetFloat("_Rotation", Time.unscaledTime * skyboxSpeed);
    }
    public void MakeIntersectionGhosts()
    {
        Vector3 currentPLeft = field.leftPad.transform.position;
        Vector3 currentPRight = field.rightPad.transform.position;
        Vector3 currentPBall = field.ball.transform.position;
        Quaternion currentRLeft = field.leftPad.transform.rotation;
        Quaternion currentRRight = field.rightPad.transform.rotation;
        Quaternion currentRBall = field.ball.transform.rotation;

        ballghost = GameObject.Instantiate(intersectionGhostPrefab, currentPBall, currentRBall, fieldParent.transform);
        leftPadGhost = GameObject.Instantiate(intersectionGhostPrefab, currentPLeft, currentRLeft, fieldParent.transform);
        rightPadGhost = GameObject.Instantiate(intersectionGhostPrefab, currentPRight, currentRRight, fieldParent.transform);

        ballghost.GetComponent<MeshRenderer>().material = ghostBallMaterial;
        leftPadGhost.GetComponent<MeshRenderer>().material = rightPadGhost.GetComponent<MeshRenderer>().material = ghostPadMaterial;

        ballghost.GetComponent<MeshFilter>().mesh = field.ball.meshF.mesh;
        leftPadGhost.GetComponent<MeshFilter>().mesh = field.leftPad.meshF.mesh;
        rightPadGhost.GetComponent<MeshFilter>().mesh = field.rightPad.meshF.mesh;

        ballghost.transform.localScale = field.ball.transform.localScale;
        leftPadGhost.transform.localScale = field.leftPad.transform.localScale;
        rightPadGhost.transform.localScale = field.rightPad.transform.localScale;
        field.ball.meshR.shadowCastingMode = ShadowCastingMode.ShadowsOnly;
        field.leftPad.meshR.shadowCastingMode = ShadowCastingMode.ShadowsOnly;
        field.rightPad.meshR.shadowCastingMode = ShadowCastingMode.ShadowsOnly;
    }
    public void DestroyIntersectionGhosts(bool lerpGhostsFromWall)
    {
        GameObject.Destroy(ballghost);
        GameObject.Destroy(rightPadGhost);
        GameObject.Destroy(leftPadGhost);
        if (lerpGhostsFromWall)
        {
            field.ball.meshR.shadowCastingMode = ShadowCastingMode.On;
        }
    }
    public void TriggerExplosion(Vector3 pos)
    {
        GameObject newEffect = GameObject.Instantiate(explosionPrefab, pos, Quaternion.Euler(Vector3.zero));
        liveEffects.Add(newEffect);
        newEffect.name = "Explosion";
        ParticleSystem ps = newEffect.GetComponent<ParticleSystem>();
        var main = ps.main;
        main.startSize = sizes.ballDiameter * 0.5f * pm.gameEffects.explosionSize;// *0.5f === /2
        main.duration = pm.gameEffects.explosionDuration;
        main.startLifetime = pm.gameEffects.explosionDuration;
        var sizeOverLifetime = ps.sizeOverLifetime;
        sizeOverLifetime.enabled = true;
        sizeOverLifetime.size = new ParticleSystem.MinMaxCurve(1f, explosionCurve);

        ExplosionCollider(newEffect.GetComponent<SphereCollider>(), sizes.ballDiameter * 0.25f * pm.gameEffects.explosionSize);// *0.25f === /4
        PlayAndKill(ps, pm.gameEffects.explosionDuration);
    }
    public void MakePadMagnetEffect(Transform prnt, Vector3 pos, float radius, bool invert = false)
    {
        GameObject newEffect = GameObject.Instantiate(padMagnetPrefab, pos, Quaternion.Euler(Vector3.zero));
        newEffect.transform.SetParent(prnt);
        liveEffects.Add(newEffect);
        newEffect.name = invert ? "MagnetAttract" : "MagnetRepulse";
        ParticleSystem ps = newEffect.GetComponent<ParticleSystem>();
        var main = ps.main;
        main.startSize = radius * 2 * prnt.localScale.x;
        main.duration = pm.gameEffects.magnetDuration;
        main.startLifetime = pm.gameEffects.magnetDuration;
        var sizeOverLifetime = ps.sizeOverLifetime;
        sizeOverLifetime.enabled = true;
        if (invert)
        {
            sizeOverLifetime.size = new ParticleSystem.MinMaxCurve(1f, attractorCurve);
            AttractorCollider(newEffect.GetComponent<SphereCollider>(), radius);
        }
        else
        {
            sizeOverLifetime.size = new ParticleSystem.MinMaxCurve(1f, repulsorCurve);
            RepulsorCollider(newEffect.GetComponent<SphereCollider>(), radius);
        }
        PlayAndKill(ps, pm.gameEffects.magnetDuration);
    }
    void ExplosionCollider(SphereCollider col, float radius)
    {
        StartCoroutine(CycleExplosionCollider(col, radius));
    }
    IEnumerator CycleExplosionCollider(SphereCollider col, float radius)
    {
        float t = 0f;
        while (t < pm.gameEffects.explosionDuration)
        {
            t += Time.deltaTime;
            if (t > pm.gameEffects.explosionDuration) { t = pm.gameEffects.explosionDuration; }
            var normalizedProgress = t / pm.gameEffects.explosionDuration;
            var easing = explosionCurve.Evaluate(normalizedProgress);
            col.radius = easing * radius;
            yield return null;
        }
    }
    void RepulsorCollider(SphereCollider col, float radius)
    {
        StartCoroutine(CycleRepulsorCollider(col, radius));
    }
    IEnumerator CycleRepulsorCollider(SphereCollider col, float radius)
    {
        float t = 0f;
        am.PlayAudio(AudioType.Repulsor, col.transform.position);
        while (t < pm.gameEffects.magnetDuration)
        {
            t += Time.deltaTime;
            if (t > pm.gameEffects.magnetDuration) { t = pm.gameEffects.magnetDuration; }
            var normalizedProgress = t / pm.gameEffects.magnetDuration;
            var easing = repulsorCurve.Evaluate(normalizedProgress);
            col.radius = easing * radius;
            yield return null;
        }
    }
    void AttractorCollider(SphereCollider col, float radius)
    {
        StartCoroutine(CycleAttractorCollider(col, radius));
    }
    IEnumerator CycleAttractorCollider(SphereCollider col, float radius)
    {
        float t = 0f;
        am.PlayAudio(AudioType.Attractor, col.transform.position);
        while (t < pm.gameEffects.magnetDuration)
        {
            t += Time.deltaTime;
            if (t > pm.gameEffects.magnetDuration) { t = pm.gameEffects.magnetDuration; }
            var normalizedProgress = t / pm.gameEffects.magnetDuration;
            var easing = attractorCurve.Evaluate(normalizedProgress);
            col.radius = easing * radius;
            yield return null;
        }
    }
    void PlayAndKill(ParticleSystem effect, float playTime)
    {
        effect.Play();
        GameObject.Destroy(effect.gameObject, playTime + 0.1f);
        StartCoroutine(CycleRemoveFromLive(effect, playTime));
    }
    IEnumerator CycleRemoveFromLive(ParticleSystem effect, float playTime)
    {
        yield return new WaitForSeconds(playTime);
        liveEffects.Remove(effect.gameObject);
    }
    public void KillAllLiveEffects()
    {
        StopAllCoroutines();
        field.background.meshR.material.SetFloat("_TilesFlatness", 0.5f);
        field.topFloor.meshR.material.SetFloat("_TilesFlatness", 0.5f);
        field.bottomFloor.meshR.material.SetFloat("_TilesFlatness", 0.5f);
        field.leftWall.meshR.material.SetFloat("_TilesFlatness", 0.5f);
        field.rightWall.meshR.material.SetFloat("_TilesFlatness", 0.5f);
        for (int i = 0; i < liveEffects.Count; i++)
        {
            GameObject.Destroy(liveEffects[i]);
        }
        liveEffects.Clear();
#if UNITY_WEBGL
        field.topFloor.meshR.material.SetColor("_BaseColor", webEdgeBaseColor);
        field.bottomFloor.meshR.material.SetColor("_BaseColor", webEdgeBaseColor);
        field.leftWall.meshR.material.SetColor("_BaseColor", webEdgeBaseColor);
        field.rightWall.meshR.material.SetColor("_BaseColor", webEdgeBaseColor);
#endif
    }
    public void WallAttractorMaterial(Edge edge)
    {
#if UNITY_WEBGL
        StopCoroutine("CycleWallAttractorEffect");
        field.topFloor.meshR.material.SetColor("_BaseColor", webEdgeBaseColor);
        field.bottomFloor.meshR.material.SetColor("_BaseColor", webEdgeBaseColor);
        field.leftWall.meshR.material.SetColor("_BaseColor", webEdgeBaseColor);
        field.rightWall.meshR.material.SetColor("_BaseColor", webEdgeBaseColor);
        edge.meshR.material.SetColor("_BaseColor", webWallAttractorColor);
        StartCoroutine("CycleWallAttractorEffect");
#else
        MakeWallAttractorEffect(edge);
#endif
    }
    public void SetFreeMovePad(Side padSide)
    {
        float two = 2 / field.leftPad.transform.localScale.x;
        float threePointFive = 3.5f / field.leftPad.transform.localScale.x;
        float four = 4 / field.leftPad.transform.localScale.x;
        float fivePointFive = 5.5f / field.leftPad.transform.localScale.x;
        switch (padSide)
        {
            case Side.Left:
                field.leftPad.SetPadForStage();
                field.fragmentStore.leftPadFragments[0].transform.DOLocalPath(new Vector3[] { new Vector3(fivePointFive, 0, 0), new Vector3(four, 0, 0) }, 2, PathType.Linear).SetEase(Ease.OutFlash).SetAutoKill(true);
                field.fragmentStore.leftPadFragments[1].transform.DOLocalPath(new Vector3[] { new Vector3(-fivePointFive, 0, 0), new Vector3(-four, 0, 0) }, 2, PathType.Linear).SetEase(Ease.OutFlash).SetAutoKill(true);
                field.fragmentStore.leftPadFragments[2].transform.DOLocalPath(new Vector3[] { new Vector3(0, threePointFive, 0), new Vector3(0, two, 0) }, 2, PathType.Linear).SetEase(Ease.OutFlash).SetAutoKill(true);
                field.fragmentStore.leftPadFragments[3].transform.DOLocalPath(new Vector3[] { new Vector3(0, -threePointFive, 0), new Vector3(0, -two, 0) }, 2, PathType.Linear).SetEase(Ease.OutFlash).SetAutoKill(true);
                break;
            case Side.Right:
                field.rightPad.SetPadForStage();
                field.fragmentStore.rightPadFragments[0].transform.DOLocalPath(new Vector3[] { new Vector3(fivePointFive, 0, 0), new Vector3(four, 0, 0) }, 2, PathType.Linear).SetEase(Ease.OutFlash).SetAutoKill(true);
                field.fragmentStore.rightPadFragments[1].transform.DOLocalPath(new Vector3[] { new Vector3(-fivePointFive, 0, 0), new Vector3(-four, 0, 0) }, 2, PathType.Linear).SetEase(Ease.OutFlash).SetAutoKill(true);
                field.fragmentStore.rightPadFragments[2].transform.DOLocalPath(new Vector3[] { new Vector3(0, threePointFive, 0), new Vector3(0, two, 0) }, 2, PathType.Linear).SetEase(Ease.OutFlash).SetAutoKill(true);
                field.fragmentStore.rightPadFragments[3].transform.DOLocalPath(new Vector3[] { new Vector3(0, -threePointFive, 0), new Vector3(0, -two, 0) }, 2, PathType.Linear).SetEase(Ease.OutFlash).SetAutoKill(true);
                break;
            case Side.None:
                field.leftPad.SetPadForStage();
                field.rightPad.SetPadForStage();
                field.fragmentStore.leftPadFragments[0].transform.DOLocalPath(new Vector3[] { new Vector3(fivePointFive, 0, 0), new Vector3(four, 0, 0) }, 2, PathType.Linear).SetEase(Ease.OutFlash).SetAutoKill(true);
                field.fragmentStore.leftPadFragments[1].transform.DOLocalPath(new Vector3[] { new Vector3(-fivePointFive, 0, 0), new Vector3(-four, 0, 0) }, 2, PathType.Linear).SetEase(Ease.OutFlash).SetAutoKill(true);
                field.fragmentStore.leftPadFragments[2].transform.DOLocalPath(new Vector3[] { new Vector3(0, threePointFive, 0), new Vector3(0, two, 0) }, 2, PathType.Linear).SetEase(Ease.OutFlash).SetAutoKill(true);
                field.fragmentStore.leftPadFragments[3].transform.DOLocalPath(new Vector3[] { new Vector3(0, -threePointFive, 0), new Vector3(0, -two, 0) }, 2, PathType.Linear).SetEase(Ease.OutFlash).SetAutoKill(true);
                field.fragmentStore.rightPadFragments[0].transform.DOLocalPath(new Vector3[] { new Vector3(fivePointFive, 0, 0), new Vector3(four, 0, 0) }, 2, PathType.Linear).SetEase(Ease.OutFlash).SetAutoKill(true);
                field.fragmentStore.rightPadFragments[1].transform.DOLocalPath(new Vector3[] { new Vector3(-fivePointFive, 0, 0), new Vector3(-four, 0, 0) }, 2, PathType.Linear).SetEase(Ease.OutFlash).SetAutoKill(true);
                field.fragmentStore.rightPadFragments[2].transform.DOLocalPath(new Vector3[] { new Vector3(0, threePointFive, 0), new Vector3(0, two, 0) }, 2, PathType.Linear).SetEase(Ease.OutFlash).SetAutoKill(true);
                field.fragmentStore.rightPadFragments[3].transform.DOLocalPath(new Vector3[] { new Vector3(0, -threePointFive, 0), new Vector3(0, -two, 0) }, 2, PathType.Linear).SetEase(Ease.OutFlash).SetAutoKill(true);
                break;

        }
    }
    public void RebuildPads()
    {
        field.fragmentStore.GatherPadFragments(false);
        field.fragmentStore.leftPadFragments.ForEach(frg =>
        {
            frg.transform.DOLocalMove(Vector3.zero, 1).SetAutoKill(true);
            frg.transform.DOLocalRotate(Vector3.zero, 1).SetAutoKill(true);
        });
        field.fragmentStore.rightPadFragments.ForEach(frg =>
        {
            frg.transform.DOLocalMove(Vector3.zero, 1).SetAutoKill(true);
            frg.transform.DOLocalRotate(Vector3.zero, 1).SetAutoKill(true);
        });
    }
    public void StartPolyIdleAnimation()
    {
        polyIdleAnimation = DOTween.Sequence();
        polyIdleAnimation.Append(field.ball.transform.DOBlendableLocalMoveBy(Vector3.up * 0.4f, 1.5f).SetEase(polyIdleAnimationCurve));
        if (currentStage != Stage.DD) { polyIdleAnimation.Join(field.ball.transform.DOBlendableLocalRotateBy(Vector3.up * 180, 1.5f, RotateMode.Fast).SetEase(Ease.Linear)); }
        polyIdleAnimation.Append(field.ball.transform.DOBlendableLocalMoveBy(-Vector3.up * 0.4f, 1.5f).SetEase(polyIdleAnimationCurve));
        if (currentStage != Stage.DD) { polyIdleAnimation.Join(field.ball.transform.DOBlendableLocalRotateBy(Vector3.up * 180, 1.5f, RotateMode.Fast).SetEase(Ease.Linear)); }
        polyIdleAnimation.SetEase(Ease.Linear);
        polyIdleAnimation.SetAutoKill(false);
        polyIdleAnimation.OnComplete(() => polyIdleAnimation.Restart());
    }
    public void StopPolyIdleAnimation()
    {
        polyIdleAnimation.OnComplete(null);
        polyIdleAnimation.Kill(true);
        polyIdleAnimation = null;
    }
    public void MakeWallAttractorEffect(Edge edge)
    {
        GameObject newEffect = GameObject.Instantiate(wallAttractorEffectPrefab, new Vector3(0, 0, stagePosZ), Quaternion.Euler(Vector3.zero));
        newEffect.transform.SetParent(fieldParent.transform);
        liveEffects.Add(newEffect);
        newEffect.name = "WallAttractorEffect";
        StopCoroutine("CycleWallAttractorEffect");
        if (activeWallAttractor != null)
        {
            liveEffects.Remove(activeWallAttractor.transform.parent.gameObject);
            GameObject.Destroy(activeWallAttractor.transform.parent.gameObject);
        }
        activeWallAttractor = newEffect.GetComponentInChildren<VisualEffect>();

        switch (edge.sd)
        {
            case Side.Top:
                activeWallAttractor.transform.position = new Vector3(field.topFloor.transform.position.x, field.topFloor.transform.position.y, stagePosZ);
                activeWallAttractor.SetVector3("ParticlesVelocity", new Vector3(0, 10, 0));
                activeWallAttractor.SetVector3("SpawnBoxPosition", new Vector3(0, -(field.leftWall.col.bounds.size.y / 4 + field.topFloor.col.bounds.size.y / 2), 0));
                activeWallAttractor.SetVector3("SpawnBoxSize", new Vector3(field.topFloor.col.bounds.size.x, field.leftWall.col.bounds.size.y / 2, sizes.ballDiameter * 5));
                activeWallAttractor.SetVector3("KillBoxPosition", Vector3.zero);
                activeWallAttractor.SetVector3("KillBoxSize", field.topFloor.col.bounds.size);
                activeWallAttractor.SetVector3("SparksMinVelocity", new Vector3(1, -1, 1));
                activeWallAttractor.SetVector3("SparksMaxVelocity", new Vector3(-1, -1, -1));
                break;
            case Side.Bottom:
                activeWallAttractor.transform.position = new Vector3(field.bottomFloor.transform.position.x, field.bottomFloor.transform.position.y, stagePosZ);
                activeWallAttractor.SetVector3("ParticlesVelocity", new Vector3(0, -10, 0));
                activeWallAttractor.SetVector3("SpawnBoxPosition", new Vector3(0, field.leftWall.col.bounds.size.y / 4 + field.bottomFloor.col.bounds.size.y / 2, 0));
                activeWallAttractor.SetVector3("SpawnBoxSize", new Vector3(field.bottomFloor.col.bounds.size.x, field.leftWall.col.bounds.size.y / 2, sizes.ballDiameter * 5));
                activeWallAttractor.SetVector3("KillBoxPosition", Vector3.zero);
                activeWallAttractor.SetVector3("KillBoxSize", field.bottomFloor.col.bounds.size);
                activeWallAttractor.SetVector3("SparksMinVelocity", new Vector3(-1, 1, -1));
                activeWallAttractor.SetVector3("SparksMaxVelocity", new Vector3(1, 1, 1));
                break;
            case Side.Left:
                activeWallAttractor.transform.position = new Vector3(field.leftWall.transform.position.x, field.leftWall.transform.position.y, stagePosZ);
                activeWallAttractor.SetVector3("ParticlesVelocity", new Vector3(-10, 0, 0));
                activeWallAttractor.SetVector3("SpawnBoxPosition", new Vector3(field.topFloor.col.bounds.size.x / 4 + field.leftWall.col.bounds.size.x / 2, 0, 0));
                activeWallAttractor.SetVector3("SpawnBoxSize", new Vector3(field.topFloor.col.bounds.size.x / 2, field.leftWall.col.bounds.size.y, sizes.ballDiameter * 5));
                activeWallAttractor.SetVector3("KillBoxPosition", Vector3.zero);
                activeWallAttractor.SetVector3("KillBoxSize", field.leftWall.col.bounds.size);
                activeWallAttractor.SetVector3("SparksMinVelocity", new Vector3(1, -1, -1));
                activeWallAttractor.SetVector3("SparksMaxVelocity", new Vector3(1, 1, 1));
                break;
            case Side.Right:
                activeWallAttractor.transform.position = new Vector3(field.rightWall.transform.position.x, field.rightWall.transform.position.y, stagePosZ);
                activeWallAttractor.SetVector3("ParticlesVelocity", new Vector3(10, 0, 0));
                activeWallAttractor.SetVector3("SpawnBoxPosition", new Vector3(-(field.topFloor.col.bounds.size.x / 4 + field.rightWall.col.bounds.size.x / 2), 0, 0));
                activeWallAttractor.SetVector3("SpawnBoxSize", new Vector3(field.topFloor.col.bounds.size.x / 2, field.rightWall.col.bounds.size.y, sizes.ballDiameter * 5));
                activeWallAttractor.SetVector3("KillBoxPosition", Vector3.zero);
                activeWallAttractor.SetVector3("KillBoxSize", field.rightWall.col.bounds.size);
                activeWallAttractor.SetVector3("SparksMinVelocity", new Vector3(-1, 1, 1));
                activeWallAttractor.SetVector3("SparksMaxVelocity", new Vector3(-1, -1, -1));
                break;
        }
        StartCoroutine("CycleWallAttractorEffect");
    }
    IEnumerator CycleWallAttractorEffect()
    {
        yield return null;
        while (field.ball.attracted)
        {
            // activeWallAttractor.SetVector3("BallPosition", activeWallAttractor.transform.InverseTransformPoint(field.ball.transform.position));
            yield return null;
        }
#if UNITY_WEBGL
        field.topFloor.meshR.material.SetColor("_BaseColor", webEdgeBaseColor);
        field.bottomFloor.meshR.material.SetColor("_BaseColor", webEdgeBaseColor);
        field.leftWall.meshR.material.SetColor("_BaseColor", webEdgeBaseColor);
        field.rightWall.meshR.material.SetColor("_BaseColor", webEdgeBaseColor);
#else
        liveEffects.Remove(activeWallAttractor.transform.parent.gameObject);
        GameObject.Destroy(activeWallAttractor.transform.parent.gameObject);
#endif
    }
    public void PunchSelectedCubeToBeat()
    {
        if (um.menuOn)
        {
            PongUiCube selectedCube = um.currentActiveMenu.pongUiCubes.First(c => c.sqnc.IsPlaying());
            if (selectedCube != null)
            {
                selectedCube.transform.DOPunchScale(new Vector3(0.1f, 0.1f, 0.1f), 0.2f, 5);
            }
        }
    }
    public void PunchCubesToBeat()
    {
        if (um.menuOn)
        {
            foreach (PongUiCube cube in um.currentActiveMenu.pongUiCubes)
            {
                if (cube.sqnc != null && !cube.sqnc.IsPlaying())
                {
                    cube.transform.DOPunchScale(-cube.transform.localScale * 0.05f, 0.1f);
                }
            }
            foreach (FakeUiCube cube in um.currentActiveMenu.fakeUiCubes)
            {
                if (cube.sqnc != null && !cube.sqnc.IsPlaying())
                {
                    cube.transform.DOPunchScale(-cube.transform.localScale * 0.05f, 0.1f);
                }
            }
        }
    }
    public void PunchTitleToBeat()
    {
        if (um.menuOn)
        {
            um.currentActiveMenu.GetComponent<PongUiMenu>().title.transform.DOPunchScale(-um.currentActiveMenu.title.transform.localScale * 0.05f, 0.1f);
        }
    }
    public void PunchFieldToBeat()
    {
        if (!um.menuOn)
        {
            StopCoroutine("CyclePunchFieldToBeat");
            StartCoroutine("CyclePunchFieldToBeat");
        }

    }
    IEnumerator CyclePunchFieldToBeat()
    {
        float duration = 0.4f;
        float max = 0.5f;
        float t = 0;
        if (currentStage == Stage.Neon)
        {
            Vector2 baseLineSize = new Vector2(0.05f, 0.6f);
            Vector2 targetLineSize = new Vector2(0.3f, 0.6f);
            while (t < duration)
            {
                t += Time.unscaledDeltaTime;
                if (t > duration) { t = duration; }
                var easing = fieldBeatCurve.Evaluate(t / duration);
                // field.background.meshR.material.SetFloat("_TilesFlatness", Mathf.Lerp(max, 0, easing));
                field.topFloor.meshR.material.SetVector("_NeonLinesSize", Vector2.Lerp(targetLineSize, baseLineSize, easing));
                field.bottomFloor.meshR.material.SetVector("_NeonLinesSize", Vector2.Lerp(targetLineSize, baseLineSize, easing));
                field.leftWall.meshR.material.SetVector("_NeonLinesSize", Vector2.Lerp(targetLineSize, baseLineSize, easing));
                field.rightWall.meshR.material.SetVector("_NeonLinesSize", Vector2.Lerp(targetLineSize, baseLineSize, easing));
                yield return null;
            }
            // field.background.meshR.material.SetFloat("_TilesFlatness", max);
            field.topFloor.meshR.material.SetVector("_NeonLinesSize", baseLineSize);
            field.bottomFloor.meshR.material.SetVector("_NeonLinesSize", baseLineSize);
            field.leftWall.meshR.material.SetVector("_NeonLinesSize", baseLineSize);
            field.rightWall.meshR.material.SetVector("_NeonLinesSize", baseLineSize);
        }
        else
        {
            while (t < duration)
            {
                t += Time.unscaledDeltaTime;
                if (t > duration) { t = duration; }
                var easing = fieldBeatCurve.Evaluate(t / duration);
                // field.background.meshR.material.SetFloat("_TilesFlatness", Mathf.Lerp(max, 0, easing));
                field.topFloor.meshR.material.SetFloat("_TilesFlatness", Mathf.Lerp(max, 0, easing));
                field.bottomFloor.meshR.material.SetFloat("_TilesFlatness", Mathf.Lerp(max, 0, easing));
                field.leftWall.meshR.material.SetFloat("_TilesFlatness", Mathf.Lerp(max, 0, easing));
                field.rightWall.meshR.material.SetFloat("_TilesFlatness", Mathf.Lerp(max, 0, easing));
                yield return null;
            }
            // field.background.meshR.material.SetFloat("_TilesFlatness", max);
            field.topFloor.meshR.material.SetFloat("_TilesFlatness", max);
            field.bottomFloor.meshR.material.SetFloat("_TilesFlatness", max);
            field.leftWall.meshR.material.SetFloat("_TilesFlatness", max);
            field.rightWall.meshR.material.SetFloat("_TilesFlatness", max);
        }
    }
    public void LerpStartMenuStyle()
    {
        StartCoroutine("CycleMenuStyle");
    }
    public void FragmentWall(Side side)
    {
        switch (side)
        {
            case Side.Left:
                leftPlayer.wallNoiseBase += 0.5f;
                field.leftWall.meshR.material.SetFloat("_NoiseAngleOffset", leftPlayer.wallNoiseBase);
                break;
            case Side.Right:
                rightPlayer.wallNoiseBase += 0.5f;
                field.rightWall.meshR.material.SetFloat("_NoiseAngleOffset", rightPlayer.wallNoiseBase);
                break;
        }
    }
    public void UpdateFieldNoiseIntensity()
    {
        StopCoroutine("CycleFieldNoiseIntensity");
        StartCoroutine("CycleFieldNoiseIntensity");
    }
    public void StopUpdateFieldNoiseIntensity()
    {
        StopCoroutine("CycleFieldNoiseIntensity");
    }
    IEnumerator CycleFieldNoiseIntensity()
    {
        float currentIntensity = 10;
        float newIntensity = currentIntensity + 5;
        float t = 0;
        while (t < options.timeThreshold)
        {
            t += Time.deltaTime;
            if (t > options.timeThreshold) { t = options.timeThreshold; }
            field.background.meshR.material.SetFloat("_NoiseIntensity", Mathf.Lerp(currentIntensity, newIntensity, t / options.timeThreshold));
            field.topFloor.meshR.material.SetFloat("_NoiseIntensity", Mathf.Lerp(currentIntensity, newIntensity, t / options.timeThreshold));
            field.bottomFloor.meshR.material.SetFloat("_NoiseIntensity", Mathf.Lerp(currentIntensity, newIntensity, t / options.timeThreshold));
            field.leftWall.meshR.material.SetFloat("_NoiseIntensity", Mathf.Lerp(currentIntensity, newIntensity, t / options.timeThreshold));
            field.rightWall.meshR.material.SetFloat("_NoiseIntensity", Mathf.Lerp(currentIntensity, newIntensity, t / options.timeThreshold));
            yield return null;
        }
    }
    IEnumerator CycleMenuStyle()
    {
        yield return new WaitForSeconds(1);
        float t = 0;
        int duration = 15; //THIS SHOULD BE CHANGED, RIGHT NOW IF THE INTRO MUSIC CHANGES I'D HAVE TO UPDATE THIS INT
        while (t < duration)
        {
            t += Time.unscaledDeltaTime;
            if (t > duration) { t = duration; }
            styleLerpValue = t / duration;
            foreach (PongUiMenu menu in um.metaCubeSides)
            {
                foreach (PongUiCube cube in menu.pongUiCubes)
                {
                    if (cube.highlighted)
                    {
                        cube.mat.SetColor("_BaseColor", Color.Lerp(Color.grey, um.cubeHighlightedColor, t / duration));
                    }
                    else
                    {
                        cube.mat.SetColor("_BaseColor", Color.Lerp(Color.white, um.cubeNormalColor, t / duration));
                    }

                }
                foreach (FakeUiCube cube in menu.fakeUiCubes)
                {
                    cube.mat.SetColor("_BaseColor", Color.Lerp(Color.white, cube.normalColor, t / duration));
                }
                menu.grid.spacing = Vector2.Lerp(Vector2.zero, Vector2.one * um.uiCubePadding, t / duration);
            }
            um.fontMaterial.SetColor("_OutlineColor", Color.Lerp(Color.white, um.fontGlowColor, t / duration));
            yield return null;
        }
    }
}
