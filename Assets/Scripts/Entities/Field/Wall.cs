using System.Collections;
using UnityEngine;
using System;
using UnityEngine.Events;
using PongGame.BallLocker;
namespace PongGame
{
    public class Wall : Edge
    {
        [Serializable]
        public class BallTouchedWall : UnityEvent { }
        [SerializeField]
        protected BallTouchedWall m_BallTouchedWall = new();
        public BallTouchedWall ballTouchedWall
        {
            get { return m_BallTouchedWall; }
            set { m_BallTouchedWall = value; }
        }
        public bool turnOff = false;
        bool inGameTurnOff => turnOff || PongManager.stillTransitioning;
        void OnCollisionEnter(Collision collision)
        {

            if (lct != Time.time && !inGameTurnOff)
            {
                lct = Time.time;
                if (collision.gameObject.GetComponent<BallEntity>() != null && field.ball.st == State.Live)
                {
                    ballTouchedWall.Invoke();
                }
            }
        }
        public void WarnLowHealth()
        {
            StopCoroutine("CycleWarnLowHealth");
            StartCoroutine("CycleWarnLowHealth");
        }
        IEnumerator CycleWarnLowHealth()
        {
            float t = 0;
            float burstTime = am.audioList.sfxAudioClips.warningLowHealth.length / 8;
            for (int i = 0; i < 8; i++)
            {
                t = 0;
                while (t < burstTime)
                {
                    t += Time.unscaledDeltaTime;
                    if (t > burstTime) { t = burstTime; }
                    meshR.material.SetFloat("_DissolveEdgeDepth", Mathf.Lerp(0, 1, t / burstTime));
                    meshR.material.SetFloat("_EmissionIntensity", Mathf.Lerp(0, 1, t / burstTime));
                    yield return null;
                }
            }
            meshR.material.SetFloat("_DissolveEdgeDepth", 0);
            meshR.material.SetFloat("_EmissionIntensity", 0);

        }
        public override void SwitchNeonStage(bool neonOn = true)
        {
            meshR.material.SetVector("_NeonLinesDensity", neonOn ? new Vector2(300, 1) : new Vector2(300, 15));
            base.SwitchNeonStage(neonOn);
        }
    }
}
