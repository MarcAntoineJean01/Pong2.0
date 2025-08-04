using UnityEngine;

public class SpikeRandomDirection : SpikeEntity
{
    void OnCollisionEnter(Collision collision)
    {
        if (lct != Time.time)
        {
            lct = Time.time;
            if (collision.gameObject.transform.GetComponent<Pad>() != null)
            {
                if (!field.ball.cyclingRandomDirection)
                {
                    field.ball.RandomDirection();
                    PostMortem(true);
                    return;
                }
            }
            bounces += 1;
            if (bounces >= bounceLimit)
            {
                PostMortem();
            }
        }
    }
}
