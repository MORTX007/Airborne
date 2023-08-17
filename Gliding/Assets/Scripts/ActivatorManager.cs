using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActivatorManager : MonoBehaviour
{
    private LaserShooter laserShooter;

    public Material activeMat;
    private Material inactiveMat;

    public bool activated;

    void Start()
    {
        laserShooter = FindObjectOfType<LaserShooter>();

        inactiveMat = GetComponentInChildren<MeshRenderer>().material;
    }

    void Update()
    {
        if (laserShooter.shooting && laserShooter.hits.Contains(transform))
        {
            GetComponentInChildren<MeshRenderer>().material = activeMat;
            activated = true;
        }
        else
        {
            GetComponentInChildren<MeshRenderer>().material = inactiveMat;
            activated = false;
        }
    }
}
