using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileController : MonoBehaviour
{
    [SerializeField] ProjectileTest _Projectile;
    [SerializeField] List<Transform> _ProjectilePoints;
    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if(Physics.Raycast(ray,out hit))
            {
                Transform point = _ProjectilePoints[Random.Range(0, _ProjectilePoints.Count)];
                ProjectileTest projectile = Instantiate(_Projectile, point.position, Quaternion.identity);
                projectile.transform.forward = (hit.point - point.position).normalized;
            }
        }
    }
}
