using UnityEngine;
using PongGame.PongLocker;
namespace PongGame
{
    public class SpikePadBlock : SpikeEntity
    {
        bool addTop = false;
        protected override void OnEnable()
        {
            base.OnEnable();
            int top = Random.Range(0, 2);
            if (top > 0)
            {
                addTop = true;
                transform.rotation = Quaternion.Euler(180, 0, 0);
            } else
            {
                addTop = false;
            }            
        }
        void OnCollisionEnter(Collision collision)
        {
            if (lct != Time.time)
            {
                lct = Time.time;
                if (collision.gameObject.transform.GetComponent<Pad>() != null)
                {
                    Pad pad = collision.gameObject.transform.GetComponent<Pad>();
                    if (pad.CanAddBlock(addTop ? Side.Top : Side.Bottom))
                    {
                        pad.AddPadBlock(addTop ? Side.Top : Side.Bottom);
                        PostMortem(true);
                        return;
                    }
                }
                else if (collision.gameObject.transform.GetComponent<Block>() != null)
                {
                    Block block = collision.gameObject.transform.GetComponent<Block>();
                    if ((addTop && block.sd == Side.Top) || (!addTop && block.sd == Side.Bottom))
                    {
                        if (block.pad.CanAddBlock(addTop ? Side.Top : Side.Bottom))
                        {
                            block.pad.AddPadBlock(addTop ? Side.Top : Side.Bottom);
                            PostMortem(true);
                            return;
                        }
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

