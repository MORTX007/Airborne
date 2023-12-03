using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LaserBeamManager : MonoBehaviour
{
    LineRenderer laser;

    void Start()
    {
        laser = GetComponent<LineRenderer>();

        laser.SetPosition(0, transform.position);
    }

    void Update()
    {
        RaycastHit hit;
        if (Physics.Raycast(transform.position, transform.forward, out hit, 999f))
        {
            laser.SetPosition(1, hit.point);
        }

        if (hit.transform.CompareTag("Player"))
        {
            hit.transform.GetComponent<PlayerController>().TakeDamage(100f);
            hit.transform.GetComponent<PlayerController>().ShakeCamera(2f, 15f, 1f);
        }
    }
}
