using UnityEngine;
using PongGame.PongLocker;
namespace PongGame
{
    public class SpikeDissolve : SpikeEntity
    {
        void OnCollisionEnter(Collision collision)
        {
            if (lct != Time.time)
            {
                lct = Time.time;
                if (collision.gameObject.transform.GetComponent<Pad>() != null)
                {
                    Pad pad = collision.gameObject.transform.GetComponent<Pad>();
                    field.ball.TriggerDissolve(pad.sd == Side.Right ? Side.Left : Side.Right);
                    PostMortem(true);
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
}

