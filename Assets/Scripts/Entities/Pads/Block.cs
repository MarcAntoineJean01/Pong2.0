using UnityEngine;
using PongLocker;
public class Block : PongEntity
{
    public Pad pad;
    public Side sd;
    void OnCollisionExit(Collision collision)
    {
        if (lct != Time.time)
        {
            lct = Time.time;
            if (collision.gameObject.GetComponent<DebuffBurn>() != null)
            {
                if (sd == Side.Top)
                {
                    mm.ResizeMeshBottom(meshF.mesh, -(PongManager.sizes.padWidth * 0.5f));
                    (col as MeshCollider).convex = true;
                    collision.gameObject.GetComponent<DebuffBurn>().TriggerExplosion();
                }
                else
                {
                    mm.ResizeMeshTop(meshF.mesh, -(PongManager.sizes.padWidth * 0.5f));
                    (col as MeshCollider).convex = true;
                    collision.gameObject.GetComponent<DebuffBurn>().TriggerExplosion();
                }
            }
            if (col.bounds.size.y <= PongManager.sizes.padWidth*0.5f)
            {
                pad.RemovePadBlock(sd);
            }
        }
    }
}
