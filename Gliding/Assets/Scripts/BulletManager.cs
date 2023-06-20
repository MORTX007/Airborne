using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletManager : MonoBehaviour
{
    protected Rigidbody rb;

    private Transform disposablePool;

    public float damage;
    public float speed;
    public float life;

    private void Awake()
    {
        Destroy(gameObject, life);
    }

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        disposablePool = GameObject.Find("Disposable Pool").transform;

        transform.parent = disposablePool;
    }

    void Update()
    {
        rb.AddForce(transform.forward * speed * Time.deltaTime, ForceMode.Impulse);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            other.GetComponent<PlayerController>().TakeDamage(damage);
            other.GetComponent<PlayerController>().ShakeCamera(1f, 10f, 0.1f);
            Destroy(gameObject);
        }
        else if (other.CompareTag("Enemy") || other.CompareTag("Environment"))
        {
            Destroy(gameObject);
        }
    }
}
