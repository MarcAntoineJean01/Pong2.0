using UnityEngine;

public class SpikeMagnet : SpikeEntity
{
    bool attractor = false;
    protected override void OnEnable()
    {
        base.OnEnable();
        int polarity = Random.Range(0, 2);
        if (polarity > 0)
        {
            attractor = true;
            transform.rotation = Quaternion.Euler(180, 0, 0);
            meshR.material = mm.materials.spikeMaterials.attractorMaterial;
        }
        else
        {
            attractor = false;
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
                if (pad.CanAddCharge(attractor))
                {
                    pad.AddCharge(attractor);
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
