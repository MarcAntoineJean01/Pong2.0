using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using System;

public class NewSpeechBubble : PongBehaviour
{
    public bool permanent = false;
    public bool makeGhost;
    public LineRenderer lineRenderer;
    protected Vector2 bubbleSize
    {
        get
        {
            Vector2 bs = text.rectTransform.rect.size;
            bs.x += bs.x * 0.1f;
            return bs;
        }
    }
    protected PongPlayerControls bubbleControls;
    [SerializeField]
    protected Vector2 initialBubbleSize = new Vector2();
    public TMP_Text text;
    [SerializeField]
    protected float charactersPerSecond = 20;
    [SerializeField]
    protected float interpunctuationDelayDuration = 0.5f;
    protected int currentVisibleCharacterIndex;
    protected bool readyForNewText = true;
    protected bool currentlySpeeding = false;
    [SerializeField]
    [Range(0.1f, 0.5f)]
    protected float sendDoneDelay = 0.25f;
    public static event Action CompleteTextRevealed;
    public static event Action<char> CharacterRevealed;
    [SerializeField]
    [Min(1)]
    protected int SpeedUpSpeed = 5;
    protected int currentTextIndex = 0;
    public List<string> speech = new List<string>() { "PLACEHOLDER" };
    protected bool autoKillCalled = false;
    protected void PrepareForNewText(System.Object obj)
    {
        if (obj != (text as System.Object) || !readyForNewText)
        {
            return;
        }

        currentlySpeeding = false;
        readyForNewText = false;

        StopCoroutine("CycleTypeText");
        text.maxVisibleCharacters = 0;
        currentVisibleCharacterIndex = 0;
        StartCoroutine("CycleTypeText");
    }
    protected void SkipText()
    {
        if (text.maxVisibleCharacters <= text.textInfo.characterCount - 1 && !autoKillCalled)
        {
            SpeedUp();
        }
        else if (currentTextIndex < speech.Count - 1)
        {
            currentTextIndex += 1;
            StopCoroutine("CycleTypeText");
            StopCoroutine("CycleSpeedupReset");
            StopCoroutine("CycleAutoKill");
            readyForNewText = true;
            autoKillCalled = false;
            text.gameObject.SetActive(true);
            text.text = speech[currentTextIndex];
        }
        else if (!currentlySpeeding && readyForNewText)
        {
            StartCoroutine("CycleDeflateBubble");
        }
    }
    protected void SpeedUp()
    {
        if (currentlySpeeding)
        {
            return;
        }
        currentlySpeeding = true;

        StartCoroutine("CycleSpeedupReset");
    }
    protected void KillCoroutines()
    {
        StopAllCoroutines();
        readyForNewText = true;
        autoKillCalled = false;
        text.gameObject.SetActive(true);
    }
    public void KillBubble()
    {
        StopAllCoroutines();
        StartCoroutine("CycleDeflateBubble");
    }
    void OnDisable()
    {
        KillCoroutines();
        bubbleControls.Disable();
        bubbleControls.PadControls.Cancel.Disable();
        bubbleControls.PadControls.Confirm.Disable();
        TMPro_EventManager.TEXT_CHANGED_EVENT.Remove(PrepareForNewText);
    }
    protected IEnumerator CycleTypeText()
    {
        string newText = text.text;
        if (this is PolySpeechBubble)
        {
            RectTransform rt = text.rectTransform;
            rt.sizeDelta = initialBubbleSize;            
        }

        text.text = "";
        yield return null;
        text.text = newText;
        
        if (this is PolySpeechBubble)
        {
            Vector2 newBubbleSize = text.GetRenderedValues(false);
            RectTransform rt = text.rectTransform;
            rt.sizeDelta = new Vector2(newBubbleSize.x < initialBubbleSize.x ? initialBubbleSize.x : newBubbleSize.x, newBubbleSize.y);

            float t = 0f;
            float inflateDuration = 0.25f;
            Vector3[] lastPositions;
            if (transform.position.x < field.ball.transform.position.x)
            {
                lastPositions = new Vector3[4]
                {
                    lineRenderer.GetPosition(0),
                    lineRenderer.GetPosition(1),
                    lineRenderer.GetPosition(5),
                    lineRenderer.GetPosition(6)
                };
            }
            else
            {
                lastPositions = new Vector3[4]
                {
                    lineRenderer.GetPosition(0),
                    lineRenderer.GetPosition(1),
                    lineRenderer.GetPosition(2),
                    lineRenderer.GetPosition(3)
                };
            }

            while (t < inflateDuration)
            {
                t += Time.unscaledDeltaTime;
                if (t > inflateDuration) { t = inflateDuration; }
                lineRenderer.SetPosition(0, Vector3.Lerp(lastPositions[0], new Vector3(text.transform.position.x - bubbleSize.x * 0.5f, text.transform.position.y - bubbleSize.y * 0.5f, stagePosZ), t / inflateDuration));
                lineRenderer.SetPosition(1, Vector3.Lerp(lastPositions[1], new Vector3(text.transform.position.x + bubbleSize.x * 0.5f, text.transform.position.y - bubbleSize.y * 0.5f, stagePosZ), t / inflateDuration));
                if (transform.position.x < field.ball.transform.position.x)
                {
                    lineRenderer.SetPosition(5, Vector3.Lerp(lastPositions[2], new Vector3(text.transform.position.x + bubbleSize.x * 0.5f, text.transform.position.y + bubbleSize.y * 0.5f, stagePosZ), t / inflateDuration));
                    lineRenderer.SetPosition(6, Vector3.Lerp(lastPositions[3], new Vector3(text.transform.position.x - bubbleSize.x * 0.5f, text.transform.position.y + bubbleSize.y * 0.5f, stagePosZ), t / inflateDuration));
                }
                else
                {
                    lineRenderer.SetPosition(2, Vector3.Lerp(lastPositions[2], new Vector3(text.transform.position.x + bubbleSize.x * 0.5f, text.transform.position.y + bubbleSize.y * 0.5f, stagePosZ), t / inflateDuration));
                    lineRenderer.SetPosition(3, Vector3.Lerp(lastPositions[3], new Vector3(text.transform.position.x - bubbleSize.x * 0.5f, text.transform.position.y + bubbleSize.y * 0.5f, stagePosZ), t / inflateDuration));
                }
                yield return null;
            }
        }

        while (currentVisibleCharacterIndex < text.textInfo.characterCount + 1)
        {
            var lastCharacterIndex = text.textInfo.characterCount - 1;
            if (currentVisibleCharacterIndex >= lastCharacterIndex)
            {
                text.maxVisibleCharacters++;
                yield return new WaitForSecondsRealtime(sendDoneDelay);
                CompleteTextRevealed?.Invoke();
                readyForNewText = true;
                if (!permanent)
                {
                    StartCoroutine("CycleAutoKill");
                }
                yield break;
            }

            char character = text.textInfo.characterInfo[currentVisibleCharacterIndex].character;

            text.maxVisibleCharacters++;

            if (!currentlySpeeding &&
                (character == '?' || character == '.' || character == ',' || character == ':' ||
                    character == ';' || character == '!' || character == '-'))
            {
                yield return new WaitForSecondsRealtime(interpunctuationDelayDuration);
            }
            else
            {
                WaitForSecondsRealtime delay = currentlySpeeding ? new WaitForSecondsRealtime(1 / (charactersPerSecond * SpeedUpSpeed)) : new WaitForSecondsRealtime(1 / charactersPerSecond);
                yield return delay;
            }

            CharacterRevealed?.Invoke(character);
            currentVisibleCharacterIndex++;
        }
    }
    protected IEnumerator CycleSpeedupReset()
    {
        yield return new WaitUntil(() => text.maxVisibleCharacters == text.textInfo.characterCount - 1);
        currentlySpeeding = false;
    }
    protected IEnumerator CycleAutoKill()
    {
        autoKillCalled = true;
        yield return new WaitForSecondsRealtime(1.5f);
        SkipText();
        autoKillCalled = false;
    }
}
