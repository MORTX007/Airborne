using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.AI;

public class EnemyManager : MonoBehaviour
{
    public  NavMeshAgent agent;
    public PlayerController player;

    [Header("Patrolling")]
    public LayerMask groundLayer, playerLayer;
    public Vector3 walkPoint;
    public float walkPointRange;
    private bool walkPointSet;

    [Header("Rotation")]
    public float rotSpeed;

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

    private void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        player = FindObjectOfType<PlayerController>();

        healthCanvas = transform.Find("Health Canvas").GetComponent<Canvas>();
        healthText = healthCanvas.transform.Find("Health Text").GetComponent<TextMeshProUGUI>();

        currentHealth = maxHealth;
        UpdateHealthUI();
    }

    private void Update()
    {
        // check if play in sight
        playerInSightRange = Physics.CheckSphere(transform.position, sightRange, playerLayer);
        playerInOptimalRange = Physics.CheckSphere(transform.position, optimalRange, playerLayer);

        if ((playerInSightRange || attacked) && !player.gliding)
        {
            LookAtPlayer();
        }
        else
        {
            Patrol();
        }

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

    public void LookAtPlayer()
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
}
