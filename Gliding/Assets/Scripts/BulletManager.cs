using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletManager : MonoBehaviour
{
    private Rigidbody rb;

    private Transform bulletPool;

    public float speed;
    public float life;

    private void Awake()
    {
        Destroy(gameObject, life);
    }

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        bulletPool = GameObject.Find("Bullet Pool").transform;

        transform.parent = bulletPool;
    }

    private void Update()
    {
        rb.AddForce(transform.forward * speed * Time.deltaTime, ForceMode.Impulse);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") || other.CompareTag("Enemy") || other.CompareTag("Environment"))
        {
            Destroy(gameObject);
        }
    }
}
