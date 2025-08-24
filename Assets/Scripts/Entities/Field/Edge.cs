using System.Collections;
using UnityEngine;
using PongGame.PongLocker;
namespace PongGame
{
    public class Edge : PongEntity
    {
        public Side sd;

        public virtual void SwitchNeonStage(bool neonOn = true)
        {
            if (neonOn)
            {
                meshR.material.SetFloat("_NeonStage", 1);
            }
            StartCoroutine(CycleSwitchNeonStage(neonOn));
        }
        IEnumerator CycleSwitchNeonStage(bool neonOn = true)
        {
            float t = 0;
            while (t < 1)
            {
                t += Time.unscaledDeltaTime;
                if (t > 1) { t = 1; }
                meshR.material.SetFloat("_DissolveEdgeDepth", Mathf.Lerp(neonOn ? 0 : 1, neonOn ? 1 : 0, t / 1));
                meshR.material.SetFloat("_EmissionIntensity", Mathf.Lerp(neonOn ? 0 : 0.5f, neonOn ? 0.5f : 0, t / 1));
                yield return null;
            }
            if (!neonOn)
            {
                meshR.material.SetFloat("_NeonStage", 0);
            }
        }
    }   
}

