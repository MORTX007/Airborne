using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.VFX;

[RequireComponent(typeof(EnemyManager))]
public class LightWeightDroneManager : MonoBehaviour
{
    private EnemyManager enemyManager;

    [Header("Attacking")]
    public GameObject bulletProjectile;
    public float bulletSpeed;
    public Transform[] weapons;
    public float maxTimeBetweenAttacks;
    private float timeBetweenAttacks;
    private bool alreadyAttacked;

    [Header("Animations")]
    public float hoverSpeed;
    public float hoverAmp;
    public Animator animator;
    private float startYPos;

    [Header("Death")]
    public Transform explosionPos;
    public VisualEffect explosionVFX;

    void Start()
    {
        enemyManager = GetComponent<EnemyManager>();

        timeBetweenAttacks = Random.Range(1f, maxTimeBetweenAttacks);

        startYPos = transform.position.y;

        hoverSpeed = Random.Range(0.1f, hoverSpeed);
        hoverAmp = Random.Range(0.3f, hoverAmp);
    }

    void Update()
    {
        Hover();

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

        if (!alreadyAttacked)
        {
            animator.SetBool("Shoot", true);

            // shoot projectiles from both guns
            foreach (Transform weapon in weapons)
            {
                Instantiate(bulletProjectile, weapon.transform.position, transform.rotation, transform);
            }

            alreadyAttacked = true;

            // interval in between attacks
            Invoke(nameof(ResetAttack), timeBetweenAttacks);
        }
        else
        {
            animator.SetBool("Shoot", false);
        }
    }

    private void ResetAttack()
    {
        alreadyAttacked = false;
    }

    private void Hover()
    {
        // sin wave for hovering
        transform.position = new Vector3(transform.position.x, startYPos + Mathf.Sin(Time.time * hoverSpeed) * hoverAmp, transform.position.z);
    }

    public void Die()
    {
        Instantiate(explosionVFX, explosionPos.position, Quaternion.identity);
        enemyManager.player.ShakeCamera(2f, 15f, 1f);
        Destroy(gameObject);
    }
}
