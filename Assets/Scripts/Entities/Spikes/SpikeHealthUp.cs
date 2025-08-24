using AudioLocker;
using UnityEngine;

public class SpikeHealthUp : SpikeEntity
{
    void OnCollisionEnter(Collision collision)
    {
        if (lct != Time.time)
        {
            lct = Time.time;
            if (collision.gameObject.transform.GetComponent<Pad>() != null)
            {
                Pad pad = collision.gameObject.transform.GetComponent<Pad>();
                if (pad.CanAddHealth())
                {
                    newGameManager.AddHealth(pad.sd);
                    am.PlayAudio(PongAudioType.GainedHealth, pad.transform.position);
                    displayHud.Invoke(pad.sd);
                    PostMortem(true);
                    return;
                }

            }
            else if (collision.gameObject.transform.GetComponent<Wall>() != null)
            {
                PostMortem();
                return;
            }
            bounces += 1;
            if (bounces >= bounceLimit)
            {
                PostMortem();
            }
        }
    }
}
