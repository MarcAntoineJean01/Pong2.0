using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : PongBehaviour
{
    public List<ParticleSystem> projectiles = new List<ParticleSystem>();
    public Vector3 direction;
    public Pad pad;
    public void EnableProjectile(ProjectileType tp)
    {
        switch (tp)
        {
            case ProjectileType.Ice:
                direction = (field.ball.transform.position - transform.position).normalized;
                break;
            case ProjectileType.Fire:
                direction = (field.ball.transform.position - transform.position).normalized;
                break;
        }

    }
    public void Fire(int p)
    {
        ParticleSystem.VelocityOverLifetimeModule vol = projectiles[p].velocityOverLifetime;
        vol.speedModifierMultiplier = p == 0 ? pm.gameEffects.iceProjectileSpeed : pm.gameEffects.fireProjectileSpeed;
        var main = projectiles[p].main;
        main.startSize = p == 0 ? pm.gameEffects.iceProjectileSize : pm.gameEffects.fireProjectileSize;
        if (p == 0)
        {
            projectiles[p].transform.LookAt(field.ball.transform.position);            
        } else
        {
            projectiles[p].transform.localRotation = Quaternion.Euler(Vector3.zero);
        }
        projectiles[p].Play();
    }
    public void KillProjectiles()
    {
        for (int i = 0; i < projectiles.Count; i++)
        {
            projectiles[i].Clear();
            projectiles[i].Stop();
        }
    }
}
