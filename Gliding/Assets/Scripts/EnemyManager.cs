using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.AI;

public class EnemyManager : MonoBehaviour
{
    private  NavMeshAgent agent;
    private PlayerController player;

    [Header("Patrolling")]
    public LayerMask groundLayer, playerlayer;
    public Vector3 walkPoint;
    public float walkPointRange;
    private bool walkPointSet;

    [Header("Attacking")]
    public GameObject bulletProjectile;
    public float bulletSpeed;
    public Transform[] weapons;
    public float maxTimeBetweenAttacks;
    public float rotSpeed;
    private float timeBetweenAttacks;
    private bool initalTurned;
    private bool alreadyAttacked;

    [Header("Health")]
    public float maxHealth;
    public float currentHealth;
    private Canvas healthCanvas;
    private TextMeshProUGUI healthText;

    [Header("States")]
    public float sightRange;
    public float optimalRange;
    public bool playerInSightRange;
    public bool playerInOptimalRange;
    public bool attacked;

    [Header("Animations")]
    public float hoverSpeed;
    public float hoverAmp;
    private float startYPos;

    private void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        player = FindObjectOfType<PlayerController>();

        startYPos = transform.position.y;

        healthCanvas = transform.Find("Health Canvas").GetComponent<Canvas>();
        healthText = healthCanvas.transform.Find("Health Text").GetComponent<TextMeshProUGUI>();

        currentHealth = maxHealth;
        UpdateHealthUI();

        timeBetweenAttacks = Random.Range(1, maxTimeBetweenAttacks);

        hoverSpeed = Random.Range(0.1f, hoverSpeed);
        hoverAmp = Random.Range(0.3f, hoverAmp);
    }

    private void Update()
    {
        // check if play in sight
        playerInSightRange = Physics.CheckSphere(transform.position, sightRange, playerlayer);
        playerInOptimalRange = Physics.CheckSphere(transform.position, optimalRange, playerlayer);

        if ((playerInSightRange || attacked) && !player.gliding)
        {
            Attack();
        }
        else
        {
            Patrol();
        }

        Hover();

        MakeHealthCanvasFacePlayer();
    }

    private void Patrol()
    {
        if (walkPointSet)
        {
            agent.SetDestination(walkPoint);
        }
        else
        {
            SearchWalkPoint();
        }

        // check if walk point is reached
        Vector3 distanceToWalkPoint = transform.position - walkPoint;
        distanceToWalkPoint.y = 0;
        if (distanceToWalkPoint.magnitude < 1f)
        {
            walkPointSet = false;
        }
    }

    private void Attack()
    {
        LookAtPlayer();

        if (playerInOptimalRange)
        {
            agent.SetDestination(transform.position);
        }
        else
        {
            agent.SetDestination(player.transform.position);
        }

        if (!alreadyAttacked)
        {
            // attack code
            foreach(Transform weapon in weapons)
            {
                Instantiate(bulletProjectile, weapon.transform.position, transform.rotation);
            }

            alreadyAttacked = true;
            // interval in between attacks
            Invoke(nameof(ResetAttack), timeBetweenAttacks);
        }
    }

    private void ResetAttack()
    {
        alreadyAttacked = false;
    }

    private void LookAtPlayer()
    {
        // smooth look at
        Vector3 dir = player.transform.position - transform.position;
        Quaternion rot = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(dir), rotSpeed * Time.deltaTime);
        transform.rotation = rot;
    }

    private void SearchWalkPoint()
    {
        // calculate random point in range
        float randomZ = Random.Range(-walkPointRange, walkPointRange);
        float randomX = Random.Range(-walkPointRange, walkPointRange);

        walkPoint = new Vector3(transform.position.x + randomX, transform.position.y, transform.position.z + randomZ);

        if (Physics.Raycast(walkPoint, -transform.up, 10f, groundLayer))
        {
            walkPointSet = true;
        }
    }

    public void TakeDamage(float damage)
    {
        if (currentHealth > 0)
        {
            currentHealth -= damage;
            UpdateHealthUI();
        }

        // die
        if (currentHealth <= 0)
        {
            Die();
        }

        attacked = true;
    }

    private void UpdateHealthUI()
    {
        var healthPercent = ((int)((currentHealth / maxHealth) * 100));
        healthText.text = healthPercent.ToString() + "%";
    }

    private void MakeHealthCanvasFacePlayer()
    {
        healthCanvas.transform.rotation = Quaternion.LookRotation(transform.position - Camera.main.transform.position);
    }

    private void Hover()
    {
        transform.position = new Vector3(transform.position.x, startYPos + Mathf.Sin(Time.time * hoverSpeed) * hoverAmp, transform.position.z);
    }

    private void Die()
    {
        Destroy(gameObject);
    }
}
