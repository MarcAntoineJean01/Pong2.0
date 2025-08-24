using UnityEngine;
namespace PongGame
{
    public class SpikePadPiece : SpikeEntity
    {
        void OnCollisionEnter(Collision collision)
        {
            if (lct != Time.time)
            {
                lct = Time.time;
                if (collision.gameObject.transform.GetComponent<Pad>() != null)
                {
                    Pad pad = collision.gameObject.transform.GetComponent<Pad>();
                    if (pad.CanAddPiece())
                    {
                        pad.AddPadPiece();
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
}

