using System.Collections;
using TMPro;
using UnityEngine;
using System.Linq;
using System;
using UnityEngine.Rendering;
public class PolySpeechBubble : NewSpeechBubble
{
    public TMP_FontAsset shadowsOnlyFont;
    public TMP_FontAsset visibleTextFont;
    [Range(0.01f, 10f)]
    public float moveSpeed;
    Vector2 fieldBounds => PongEntity.fieldBounds;
    Vector3[] initialCorners;
    Side currentSide => field.ball.transform.position.x > 0 ? Side.Left : Side.Right;
    Vector3 coneOrigin => transform.position.x < field.ball.transform.position.x ? Vector3.Lerp(field.ball.transform.position, initialCorners[1], 0.5f) : Vector3.Lerp(field.ball.transform.position, initialCorners[0], 0.5f);
    Vector3 coneEndA => transform.position.x < field.ball.transform.position.x ? Vector3.Lerp(initialCorners[1], initialCorners[2], 0.35f) :  Vector3.Lerp(initialCorners[0], initialCorners[3], 0.60f);
    Vector3 coneEndB => transform.position.x < field.ball.transform.position.x ? Vector3.Lerp(initialCorners[1], initialCorners[2], 0.60f) :  Vector3.Lerp(initialCorners[0], initialCorners[3], 0.35f);
    Vector3 updatedTarget
    {
        get
        {
            switch (currentSide)
            {
                default:
                case Side.Left:
                    return new Vector3(field.ball.transform.position.x - bubbleSize.x, field.ball.transform.position.y, stagePosZ);
                case Side.Right:
                    return new Vector3(field.ball.transform.position.x + bubbleSize.x, field.ball.transform.position.y, stagePosZ);
            }
        }
    }
    void OnEnable()
    {

        bubbleControls = new PongPlayerControls();
        bubbleControls.Disable();
        bubbleControls.PadControls.Cancel.Enable();
        bubbleControls.PadControls.Cancel.performed += ctx => SkipText();
        bubbleControls.PadControls.Confirm.Enable();
        bubbleControls.PadControls.Confirm.performed += ctx => SkipText();

        transform.rotation = Quaternion.Euler(Vector3.zero);
        RectTransform rt = text.rectTransform;
        rt.sizeDelta = initialBubbleSize;
        transform.position = updatedTarget;
        initialCorners = new Vector3[4]
        {
            new Vector3(text.transform.position.x - bubbleSize.x * 0.5f, text.transform.position.y - bubbleSize.y * 0.5f, stagePosZ),
            new Vector3(text.transform.position.x + bubbleSize.x * 0.5f, text.transform.position.y - bubbleSize.y * 0.5f, stagePosZ),
            new Vector3(text.transform.position.x + bubbleSize.x * 0.5f, text.transform.position.y + bubbleSize.y * 0.5f, stagePosZ),
            new Vector3(text.transform.position.x - bubbleSize.x * 0.5f, text.transform.position.y + bubbleSize.y * 0.5f, stagePosZ)
        };
        text.text = "";
        text.maxVisibleCharacters = text.textInfo.characterCount;
        lineRenderer.positionCount = 7;
        if (transform.position.x < field.ball.transform.position.x)
        {
            lineRenderer.SetPosition(0, initialCorners[0]);
            lineRenderer.SetPosition(1, initialCorners[1]);
            lineRenderer.SetPosition(2, coneEndA);
            lineRenderer.SetPosition(3, coneOrigin);
            lineRenderer.SetPosition(4, coneEndB);
            lineRenderer.SetPosition(5, initialCorners[2]);
            lineRenderer.SetPosition(6, initialCorners[3]);
        }
        else
        {
            lineRenderer.SetPosition(0, initialCorners[0]);
            lineRenderer.SetPosition(1, initialCorners[1]);
            lineRenderer.SetPosition(2, initialCorners[2]);
            lineRenderer.SetPosition(3, initialCorners[3]);
            lineRenderer.SetPosition(4, coneEndA);
            lineRenderer.SetPosition(5, coneOrigin);
            lineRenderer.SetPosition(6, coneEndB);
        }
        TMPro_EventManager.TEXT_CHANGED_EVENT.Add(PrepareForNewText);
        if (currentStage == Stage.DD)
        {
            lineRenderer.shadowCastingMode = ShadowCastingMode.ShadowsOnly;
            text.GetComponent<MeshRenderer>().shadowCastingMode = ShadowCastingMode.ShadowsOnly;
            text.font = shadowsOnlyFont;
        }
        else
        {
            lineRenderer.shadowCastingMode = ShadowCastingMode.Off;
            text.GetComponent<MeshRenderer>().shadowCastingMode = ShadowCastingMode.Off;
            text.font = visibleTextFont;
        }
        text.gameObject.SetActive(true);
        text.text = speech.First();
        currentTextIndex = 0;
        StartCoroutine("FollowBall");
    }
    IEnumerator FollowBall()
    {
        while (true)
        {
            if (transform.position.x < field.ball.transform.position.x)
            {
                lineRenderer.SetPosition(3, coneOrigin);
                yield return null;
            }
            else
            {
                lineRenderer.SetPosition(5, coneOrigin);
                yield return null;                
            }

        }
    }


    IEnumerator CycleDeflateBubble()
    {
        StopCoroutine("FollowBall");
        if (makeGhost)
        {
            dm.polyBubbleGhost = GameObject.Instantiate(dm.polySpeechBubble.gameObject);
            GameObject.Destroy(dm.polyBubbleGhost.GetComponent<PolySpeechBubble>());
            dm.polyBubbleGhost.GetComponentInChildren<TMP_Text>().maxVisibleCharacters = text.text.Length;
            dm.polyBubbleGhost.GetComponentInChildren<TMP_Text>().text = text.text;
            dm.polyBubbleGhost.GetComponent<LineRenderer>().positionCount = lineRenderer.positionCount;
            Vector3[] vertices = new Vector3[lineRenderer.positionCount];
            lineRenderer.GetPositions(vertices);
            dm.polyBubbleGhost.GetComponent<LineRenderer>().SetPositions(vertices);
        }
        else
        {
            float t = 0f;
            float inflateDuration = 0.25f;
            text.gameObject.SetActive(false);
            Vector3[] lastPositions;
            lastPositions = new Vector3[7]
            {
                lineRenderer.GetPosition(0),
                lineRenderer.GetPosition(1),
                lineRenderer.GetPosition(2),
                lineRenderer.GetPosition(3),
                lineRenderer.GetPosition(4),
                lineRenderer.GetPosition(5),
                lineRenderer.GetPosition(6)
            };
            while (t < inflateDuration)
            {
                t += Time.unscaledDeltaTime;
                if (t > inflateDuration) { t = inflateDuration; }
                lineRenderer.SetPosition(0, Vector3.Lerp(lastPositions[0], updatedTarget, t / inflateDuration));
                lineRenderer.SetPosition(1, Vector3.Lerp(lastPositions[1], updatedTarget, t / inflateDuration));
                lineRenderer.SetPosition(2, Vector3.Lerp(lastPositions[2], updatedTarget, t / inflateDuration));
                lineRenderer.SetPosition(3, Vector3.Lerp(lastPositions[3], updatedTarget, t / inflateDuration));
                lineRenderer.SetPosition(4, Vector3.Lerp(lastPositions[4], updatedTarget, t / inflateDuration));
                lineRenderer.SetPosition(5, Vector3.Lerp(lastPositions[5], updatedTarget, t / inflateDuration));
                lineRenderer.SetPosition(6, Vector3.Lerp(lastPositions[6], updatedTarget, t / inflateDuration));
                yield return null;
            }
        }
        makeGhost = false;
        gameObject.SetActive(false);
    }
}
