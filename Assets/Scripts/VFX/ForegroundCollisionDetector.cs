using System.Collections;
using UnityEngine;
using UnityEngine.Events;
namespace PongGame
{
    public class ForegroundCollisionDetector : MonoBehaviour
    {
        public UnityEvent screenBounce;
        bool colliding = false;
        float lct;
        [SerializeField]
        MeshRenderer meshR;
        Vector2 ballPos;
        void OnCollisionEnter(Collision collision)
        {

            if (lct != Time.time && collision.gameObject.GetComponent<BallEntity>() != null && !colliding)
            {
                lct = Time.time;
                ballPos = Camera.main.WorldToViewportPoint(collision.gameObject.transform.position);
                StopCollisionRipples();
                StartCoroutine("CycleCollisionRipples");
                screenBounce.Invoke();
            }
        }
        void StopCollisionRipples()
        {
            StopCoroutine("CycleCollisionRipples");
            meshR.material.SetVector("_HitPosition", Vector2.zero);
            meshR.material.SetFloat("_Speed", 0);
            meshR.material.SetFloat("_Strength", 0);
            meshR.material.SetFloat("_Expanding", 1);
            meshR.material.SetFloat("_RecedeProgress", 0);
            meshR.material.SetFloat("_ExpandProgress", 0);
            colliding = false;
        }

        IEnumerator CycleCollisionRipples()
        {
            float timeThreshold = 0.7f;
            colliding = true;
            meshR.material.SetVector("_HitPosition", ballPos);
            float t = 0;
            while (t < timeThreshold)
            {
                t += Time.deltaTime;
                if (t > timeThreshold) { t = timeThreshold; }
                meshR.material.SetFloat("_Speed", Mathf.Lerp(0, 2, t / timeThreshold));
                meshR.material.SetFloat("_Strength", Mathf.Lerp(0, 10, t / timeThreshold));
                meshR.material.SetFloat("_ExpandProgress", Mathf.Lerp(0, 0.4f, t / timeThreshold));
                yield return null;

            }
            t = 0;
            meshR.material.SetFloat("_Expanding", 0);
            while (t < timeThreshold)
            {
                t += Time.deltaTime;
                if (t > timeThreshold) { t = timeThreshold; }
                meshR.material.SetFloat("_Speed", Mathf.Lerp(2, 4, t / timeThreshold));
                meshR.material.SetFloat("_Strength", Mathf.Lerp(10, 0, t / timeThreshold));
                meshR.material.SetFloat("_RecedeProgress", Mathf.Lerp(0, 1, t / timeThreshold));
                meshR.material.SetFloat("_ExpandProgress", Mathf.Lerp(0.4f, 1, t / timeThreshold));
                yield return null;

            }
            meshR.material.SetVector("_HitPosition", Vector2.zero);
            meshR.material.SetFloat("_Speed", 0);
            meshR.material.SetFloat("_Strength", 0);
            meshR.material.SetFloat("_Expanding", 1);
            meshR.material.SetFloat("_RecedeProgress", 0);
            meshR.material.SetFloat("_ExpandProgress", 0);
            colliding = false;
        }
    }
}
