using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.VFX;

[RequireComponent(typeof(EnemyManager))]
public class LightWeightSpiderManager : MonoBehaviour
{
    private EnemyManager enemyManager;

    [Header("Attacking")]
    public float damage;
    public float explosionRadius;

    [Header("Death")]
    public Transform explosionPos;
    public VisualEffect explosionVFX;

    void Start()
    {
        enemyManager = GetComponent<EnemyManager>();
    }

    void Update()
    {
        if ((enemyManager.playerInSightRange || enemyManager.attacked) && !enemyManager.player.gliding)
        {
            Attack();
        }

        // die
        if (enemyManager.currentHealth <= 0)
        {
            Die();
        }
    }

    private void Attack()
    {
        if (enemyManager.playerInOptimalRange)
        {
            enemyManager.agent.SetDestination(transform.position);
        }
        else
        {
            enemyManager.agent.SetDestination(enemyManager.player.transform.position);
        }

        // check player in explosion radius
        if (Physics.CheckSphere(transform.position, explosionRadius, enemyManager.playerLayer))
        {
            enemyManager.player.TakeDamage(damage);
            Die();
        }
    }

    public void Die()
    {
        Instantiate(explosionVFX, explosionPos.position, Quaternion.identity);
        enemyManager.player.ShakeCamera(2f, 15f, 1f);
        Destroy(gameObject);
    }
}
