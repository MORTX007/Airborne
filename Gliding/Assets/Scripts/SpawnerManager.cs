using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnerManager : MonoBehaviour
{
    private PlayerController player;

    public int spawnNumber;
    private int count;

    public float spawnWidth;
    public float spawnLength;
    public float height;

    public List<GameObject> enemies;

    public bool activated;

    void Start()
    {
        player = FindObjectOfType<PlayerController>();
    }

    void Update()
    {
        if (activated && count < spawnNumber)
        {
            Vector3 randPos = player.transform.position;
            while (Vector3.Distance(randPos, player.transform.position) < 2f)
            {
                randPos = transform.position + new Vector3(Random.Range(-spawnWidth / 2, spawnWidth / 2),
                                                   height,
                                                   Random.Range(-spawnLength / 2, spawnLength / 2));
            }

            Instantiate(enemies[Random.Range(0, enemies.Count)], randPos, Quaternion.Euler(0, Random.Range(0f, 360f), 0));

            count++;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (GetComponent<ActivatedManager>().animationComplete && other.CompareTag("Player") && !player.gliding)
        {
            activated = true;
        }
    }
}
