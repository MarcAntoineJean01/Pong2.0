using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using System.Linq;
using UnityEngine.VFX;
using System;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using UnityEngine.Assertions;
using DG.Tweening.Plugins.Core.PathCore;
using TMPro;
using System.Reflection;
public class TestingScene : MonoBehaviour
{
    public RenderTexture texture;
    public Camera cam;
    public MeshRenderer foreground;
    [SerializeField]
    private FullScreenPassRendererFeature rendererFeature;
    public AnimationCurve curve;
    void Start()
    {
        texture = new RenderTexture(Screen.width, Screen.height, 16, RenderTextureFormat.ARGB32);
        cam.targetTexture = texture;
        foreground.material.SetTexture("_BaseTexture", texture);
        StartCoroutine(tesadt());
    }
    IEnumerator tesadt()
    {
        int timeThreshold = 1;
        float t = 0;
        yield return new WaitForSeconds(2);
        while (true)
        {
            while (t < timeThreshold)
            {
                t += Time.deltaTime;
                if (t > timeThreshold) { t = timeThreshold; }
                var normalizedProgress = t / timeThreshold;
                var easing = curve.Evaluate(normalizedProgress);
                foreground.material.SetFloat("_Speed", Mathf.Lerp(0, 2, t / timeThreshold));
                foreground.material.SetFloat("_Strength", Mathf.Lerp(0, 4, t / timeThreshold));
                foreground.material.SetFloat("_ExpandProgress", Mathf.Lerp(0, 1, t / timeThreshold));
                yield return null;

            }
            t = 0;
            foreground.material.SetFloat("_Expanding", 0);
            while (t < timeThreshold)
            {
                t += Time.deltaTime;
                if (t > timeThreshold) { t = timeThreshold; }
                var normalizedProgress = t / timeThreshold;
                var easing = curve.Evaluate(normalizedProgress);
                foreground.material.SetFloat("_Speed", Mathf.Lerp(2, 4, t / timeThreshold));
                foreground.material.SetFloat("_Strength", Mathf.Lerp(4, 0, t / timeThreshold));
                foreground.material.SetFloat("_RecedeProgress", Mathf.Lerp(0, 1, t / timeThreshold));
                yield return null;

            }
            foreground.material.SetVector("_HitPosition", new Vector2(UnityEngine.Random.Range(-(Screen.width / 2), Screen.width / 2), UnityEngine.Random.Range(-(Screen.height / 2), Screen.height / 2)));
            foreground.material.SetFloat("_Speed", 0);
            foreground.material.SetFloat("_Strength", 0);
            foreground.material.SetFloat("_Expanding", 1);
            foreground.material.SetFloat("_RecedeProgress", 0);
            foreground.material.SetFloat("_ExpandProgress", 0);
            t = 0;
            yield return new WaitForSeconds(2);
        }

    }
    IEnumerator tesat()
    {
        int timeThreshold = 3;
        float t = 0;
        yield return new WaitForSeconds(2);
        while (true)
        {
            while (t < timeThreshold)
            {
                t += Time.deltaTime;
                if (t > timeThreshold) { t = timeThreshold; }
                var normalizedProgress = t / timeThreshold;
                var easing = curve.Evaluate(normalizedProgress);
                rendererFeature.passMaterial.SetFloat("_Speed", Mathf.Lerp(0, 2, t / timeThreshold));
                rendererFeature.passMaterial.SetFloat("_Strength", Mathf.Lerp(0, 4, t / timeThreshold));
                rendererFeature.passMaterial.SetFloat("_ExpandProgress", Mathf.Lerp(0, 5, easing));
                yield return null;

            }
            t = 0;
            rendererFeature.passMaterial.SetFloat("_Expanding", 0);
            while (t < timeThreshold)
            {
                t += Time.deltaTime;
                if (t > timeThreshold) { t = timeThreshold; }
                var normalizedProgress = t / timeThreshold;
                var easing = curve.Evaluate(normalizedProgress);
                rendererFeature.passMaterial.SetFloat("_Speed", Mathf.Lerp(2, 4, t / timeThreshold));
                rendererFeature.passMaterial.SetFloat("_Strength", Mathf.Lerp(4, 0, t / timeThreshold));
                rendererFeature.passMaterial.SetFloat("_RecedeProgress", Mathf.Lerp(0, 5, easing));
                yield return null;

            }
            // rendererFeature.passMaterial.SetVector("_HitPosition", new Vector2(UnityEngine.Random.Range(-(Screen.width / 2), Screen.width / 2), UnityEngine.Random.Range(-(Screen.height / 2), Screen.height / 2)));
            rendererFeature.passMaterial.SetFloat("_Speed", 0);
            rendererFeature.passMaterial.SetFloat("_Strength", 0);
            rendererFeature.passMaterial.SetFloat("_Expanding", 1);
            rendererFeature.passMaterial.SetFloat("_RecedeProgress", 0);
            rendererFeature.passMaterial.SetFloat("_ExpandProgress", 0);
            t = 0;
            yield return new WaitForSeconds(2);
        }

    }
    // public VisualEffect visualEffect;
    // public BoxCollider bottomFloor;
    // public BoxCollider topFloor;
    // public BoxCollider LeftWall;
    // public BoxCollider RightWall;
    // public enum TargetSide
    // {
    //     Top,
    //     Bottom,
    //     Left,
    //     Right
    // }
    // public TargetSide targetVFXSide;
    // public TargetSide currentVFXSide;

    // public RectTransform canvas;
    // public RectTransform metaCube;
    // protected float padding => canvas.sizeDelta.y / 10;
    // float metaCubeSize => smallCubeSize * 4 + padding * 3;
    // float smallCubeSize => (canvas.sizeDelta.y - padding * 5) / 4;
    // public GameObject[] sides = new GameObject[6];
    // protected void SetMetaCube()
    // {
    //     metaCube.sizeDelta = new Vector2(metaCubeSize, metaCubeSize);
    //     sides[0].transform.localPosition = new Vector3(0, 0, -metaCubeSize * 0.5f);
    //     sides[0].transform.localRotation = Quaternion.Euler(0, 0, 0);

    //     sides[1].transform.localPosition = new Vector3(0, 0, metaCubeSize * 0.5f);
    //     sides[1].transform.localRotation = Quaternion.Euler(0, 180, 180);

    //     sides[2].transform.localPosition = new Vector3(0, -metaCubeSize * 0.5f, 0);
    //     sides[2].transform.localRotation = Quaternion.Euler(-90, 0, 0);

    //     sides[3].transform.localPosition = new Vector3(0, metaCubeSize * 0.5f, 0);
    //     sides[3].transform.localRotation = Quaternion.Euler(90, 0, 0);

    //     sides[4].transform.localPosition = new Vector3(-metaCubeSize * 0.5f, 0, 0);
    //     sides[4].transform.localRotation = Quaternion.Euler(0, 90, 0);

    //     sides[5].transform.localPosition = new Vector3(metaCubeSize * 0.5f, 0, 0);
    //     sides[5].transform.localRotation = Quaternion.Euler(0, 270, 0);
    // }

    // void OnEnable()
    // {
    //     SetMetaCube();   
    // }
    // void OnEnableVFX()
    // {

    //     currentVFXSide = targetVFXSide = TargetSide.Bottom;
    //     visualEffect.transform.position = bottomFloor.transform.position;
    //     visualEffect.SetVector3("ParticlesVelocity", new Vector3(0, -10, 0));
    //     visualEffect.SetVector3("SpawnBoxPosition", new Vector3(0, LeftWall.bounds.size.y/4+bottomFloor.bounds.size.y/2, 0));
    //     visualEffect.SetVector3("SpawnBoxSize", new Vector3(bottomFloor.bounds.size.x, LeftWall.bounds.size.y/2, bottomFloor.bounds.size.z));
    //     visualEffect.SetVector3("KillBoxPosition", Vector3.zero);
    //     visualEffect.SetVector3("KillBoxSize", bottomFloor.bounds.size);
    // }

    // void UpdateVFX()
    // {
    //     if (currentVFXSide != targetVFXSide)
    //     {
    //         currentVFXSide = targetVFXSide;
    //         switch (currentVFXSide)
    //         {
    //             case TargetSide.Top:
    //                 visualEffect.transform.position = topFloor.transform.position;
    //                 // new Vector3(topFloor.bounds.size.x, LeftWall.bounds.size.y/2, topFloor.bounds.size.z)
    //                 // new Vector3(0, -(LeftWall.bounds.size.y/4+topFloor.bounds.size.y/2), 0)
    //                 visualEffect.SetVector3("ParticlesVelocity", new Vector3(0, 10, 0));
    //                 visualEffect.SetVector3("SpawnBoxPosition", new Vector3(0, -(LeftWall.bounds.size.y/4+topFloor.bounds.size.y/2), 0));
    //                 visualEffect.SetVector3("SpawnBoxSize", new Vector3(topFloor.bounds.size.x, LeftWall.bounds.size.y/2, topFloor.bounds.size.z));
    //                 visualEffect.SetVector3("KillBoxPosition", Vector3.zero);
    //                 visualEffect.SetVector3("KillBoxSize", topFloor.bounds.size);
    //                 visualEffect.SetVector3("SparksMinVelocity", new Vector3(1,-1,1));
    //                 visualEffect.SetVector3("SparksMaxVelocity", new Vector3(-1,-1,-1));
    //                 break;
    //             case TargetSide.Bottom:
    //                 visualEffect.transform.position = bottomFloor.transform.position;
    //                 visualEffect.SetVector3("ParticlesVelocity", new Vector3(0, -10, 0));
    //                 visualEffect.SetVector3("SpawnBoxPosition", new Vector3(0, LeftWall.bounds.size.y/4+bottomFloor.bounds.size.y/2, 0));
    //                 visualEffect.SetVector3("SpawnBoxSize", new Vector3(bottomFloor.bounds.size.x, LeftWall.bounds.size.y/2, bottomFloor.bounds.size.z));
    //                 visualEffect.SetVector3("KillBoxPosition", Vector3.zero);
    //                 visualEffect.SetVector3("KillBoxSize", bottomFloor.bounds.size);
    //                 visualEffect.SetVector3("SparksMinVelocity", new Vector3(-1,1,-1));
    //                 visualEffect.SetVector3("SparksMaxVelocity", new Vector3(1,1,1));
    //                 break;
    //             case TargetSide.Left:
    //                 visualEffect.transform.position = LeftWall.transform.position;
    //                 visualEffect.SetVector3("ParticlesVelocity", new Vector3(-10, 0, 0));
    //                 visualEffect.SetVector3("SpawnBoxPosition", new Vector3(topFloor.bounds.size.x/4+LeftWall.bounds.size.x/2, 0, 0));
    //                 visualEffect.SetVector3("SpawnBoxSize", new Vector3(topFloor.bounds.size.x/2, LeftWall.bounds.size.y, LeftWall.bounds.size.z));
    //                 visualEffect.SetVector3("KillBoxPosition", Vector3.zero);
    //                 visualEffect.SetVector3("KillBoxSize", LeftWall.bounds.size);
    //                 visualEffect.SetVector3("SparksMinVelocity", new Vector3(1,-1,-1));
    //                 visualEffect.SetVector3("SparksMaxVelocity", new Vector3(1,1,1));
    //                 break;
    //             case TargetSide.Right:
    //                 visualEffect.transform.position = RightWall.transform.position;
    //                 visualEffect.SetVector3("ParticlesVelocity", new Vector3(10, 0, 0));
    //                 visualEffect.SetVector3("SpawnBoxPosition", new Vector3(-(topFloor.bounds.size.x/4+RightWall.bounds.size.x/2), 0, 0));
    //                 visualEffect.SetVector3("SpawnBoxSize", new Vector3(topFloor.bounds.size.x/2, RightWall.bounds.size.y, RightWall.bounds.size.z));
    //                 visualEffect.SetVector3("KillBoxPosition", Vector3.zero);
    //                 visualEffect.SetVector3("KillBoxSize", RightWall.bounds.size);
    //                 visualEffect.SetVector3("SparksMinVelocity", new Vector3(-1,1,1));
    //                 visualEffect.SetVector3("SparksMaxVelocity", new Vector3(-1,-1,-1));
    //                 break;
    //         }
    //     }
    // }

}