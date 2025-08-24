using UnityEngine;
using PongLocker;
public class SpikeWallAttractor : SpikeEntity
{
    public Side wallSide;
    protected override void OnEnable()
    {
        base.OnEnable();
        int side = Random.Range(0, 5);
        switch (side)
        {
            default:
            case 0:
                wallSide = Side.Bottom;
                break;
            case 1:
                wallSide = Side.Bottom;
                break;
            case 2:
                wallSide = Side.Left;
                break;
            case 3:
                wallSide = Side.Right;
                break;
        }
    }
    void OnCollisionEnter(Collision collision)
    {
        if (lct != Time.time)
        {
            lct = Time.time;
            if (collision.gameObject.transform.GetComponent<Pad>() != null && !field.ball.attracted)
            {
                Pad pad = collision.gameObject.transform.GetComponent<Pad>();
                if (wallSide == pad.sd)
                {
                    wallSide = pad.sd == Side.Left ? Side.Right : Side.Left;
                }
                field.ball.AttractWall(wallSide);
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
