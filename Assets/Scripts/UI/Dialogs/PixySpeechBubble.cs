using System.Collections;
using TMPro;
using UnityEngine;
using System.Linq;
using UnityEngine.UI;
using UnityEngine.Events;
namespace PongGame
{
    public class PixySpeechBubble : NewSpeechBubble
    {
        public bool orientation; //orientation => true: landscape, false: portrait
        public RectTransform rect;
        public GridLayoutGroup grid;
        [SerializeField]
        RectOffset padding;
        public UnityEvent pixyBubbleDied;
        void OnEnable()
        {
            transform.rotation = Quaternion.Euler(Vector3.zero);
            grid.cellSize = orientation ? new Vector2(PongManager.sizes.canvasSize.x-padding.left, PongManager.sizes.canvasSize.y*0.4f-padding.top) : new Vector2(PongManager.sizes.canvasSize.x*0.4f-padding.left, PongManager.sizes.canvasSize.y-padding.top);
            grid.padding = padding;
            text.maxVisibleCharacters = 0;
            text.gameObject.SetActive(false);
            text.text = "";
            GetComponent<CanvasGroup>().alpha = 0;
            StartCoroutine(CycleInflateBubble());

            TMPro_EventManager.TEXT_CHANGED_EVENT.Add(PrepareForNewText);
        }
        IEnumerator CycleInflateBubble()
        {
            float t = 0f;
            float inflateDuration = 0.25f;
            while (t < inflateDuration)
            {
                t += Time.unscaledDeltaTime;
                if (t > inflateDuration) { t = inflateDuration; }
                GetComponent<CanvasGroup>().alpha = Mathf.Lerp(0, 1, t/inflateDuration);
                yield return null;
            }
            bubbleControls = new PongPlayerControls();
            bubbleControls.Disable();
            bubbleControls.PadControls.Cancel.Enable();
            bubbleControls.PadControls.Cancel.performed += ctx => SkipText();
            bubbleControls.PadControls.Confirm.Enable();
            bubbleControls.PadControls.Confirm.performed += ctx => SkipText();
            text.gameObject.SetActive(true);
            text.text = speech.First();
        }
        IEnumerator CycleDeflateBubble()
        {
            float t = 0f;
            float inflateDuration = 0.25f;
            while (t < inflateDuration)
            {
                t += Time.unscaledDeltaTime;
                if (t > inflateDuration) { t = inflateDuration; }
                GetComponent<CanvasGroup>().alpha = Mathf.Lerp(1, 0, t/inflateDuration);
                yield return null;
            }
            pixyBubbleDied.Invoke();
            text.text = "";
            gameObject.SetActive(false);

        }
    }
}
