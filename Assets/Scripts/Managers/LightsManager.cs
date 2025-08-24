using System.Collections;
using UnityEngine;
namespace PongGame
{
    public class LightsManager : PongManager
    {
        public Light mainLight;
        public Light leftLight;
        public Light rightLight;
        public bool mainLightRotating;
        public void RotateMainLight()
        {
            if (!mainLightRotating)
            {
                leftLight.intensity = 0;
                rightLight.intensity = 0;
                leftLight.gameObject.SetActive(true);
                rightLight.gameObject.SetActive(true);
                StartCoroutine("CycleRotateMainLight");
            }
        }

        IEnumerator CycleRotateMainLight()
        {
            mainLightRotating = true;
            float t = 0;
            while (mainLightRotating)
            {
                if (leftLight.intensity < 2)
                {
                    t += Time.deltaTime;
                    if (t > 5) { t = 5; }
                    leftLight.intensity = Mathf.Lerp(0, 2, t / 5);
                    rightLight.intensity = Mathf.Lerp(0, 2, t / 5);
                }
                mainLight.transform.Rotate(Vector3.one * 0.001f);
                yield return null;
            }
        }
    }
}
