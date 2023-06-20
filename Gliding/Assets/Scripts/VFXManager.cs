using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;

public class VFXManager : MonoBehaviour
{
    private Transform disposablePool;

    public float life;

    private void Awake()
    {
        Destroy(gameObject, life);
    }

    void Start()
    {
        disposablePool = GameObject.Find("Disposable Pool").transform;

        transform.parent = disposablePool;
    }
}
