using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MagnifyingObject : MonoBehaviour
{
    Renderer _renderer;
    public Camera _cam;
    void Start()
    {
        _renderer = GetComponent<Renderer>();
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 screenPoint = _cam.WorldToScreenPoint(transform.position);
        screenPoint.x = screenPoint.x / Screen.width;
        screenPoint.y = screenPoint.y / Screen.height;
        _renderer.material.SetVector("_ObjScreenPos", screenPoint);
    }
}
