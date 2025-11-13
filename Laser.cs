using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Laser : MonoBehaviour
{
    int maxBounces = 5;
    [SerializeField] private LineRenderer lr;
    [SerializeField] private Transform startPoint;
    [SerializeField] private int LaserLength = 300;

    void Start()
    {
        lr.SetPosition(0, startPoint.position);
    }
    void Update()
    {
        CastLaser(transform.position, -transform.forward);
    }
    void CastLaser(Vector3 position, Vector3 direction)
    {
        lr.SetPosition(0, startPoint.position);
        for (int i = 0; i < maxBounces; i++)
        {
            Ray ray = new Ray(position, direction);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, LaserLength))
            {
                position = hit.point;
                direction = Vector3.Reflect(direction, hit.normal);
                lr.SetPosition(i + 1, hit.point);

            }
        }
    }
}
