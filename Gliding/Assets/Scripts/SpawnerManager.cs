using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnerManager : MonoBehaviour
{
    private PlayerController player;

    public int spawnNumber;
    public int count;

    public float maxSpawnWidth;
    public float maxSpawnLength;
    public float height;

    public List<GameObject> enemies;
    public List<GameObject> enemiesLeft;

    public bool waitForAnimation;
    public bool animationComplete = true;
    public bool activateReady;

    void Start()
    {
        player = FindObjectOfType<PlayerController>();

        if (waitForAnimation)
        {
            animationComplete = false;
        }
    }

    void Update()
    {
        if (activateReady && count < spawnNumber && animationComplete)
        {
            Vector3 randPos = player.transform.position;
            while (Vector3.Distance(randPos, player.transform.position) < 2f)
            {
                randPos = transform.position + new Vector3(Random.Range(-maxSpawnWidth / 2, maxSpawnWidth / 2),
                                                   height,
                                                   Random.Range(-maxSpawnLength / 2, maxSpawnLength / 2));
            }

            enemiesLeft.Add(Instantiate(enemies[Random.Range(0, enemies.Count)], randPos, Quaternion.Euler(0, Random.Range(0f, 360f), 0)));

            count++;
        }

        for (int i = 0; i < enemiesLeft.Count; i++)
        {
            if (enemiesLeft[i] == null)
            {
                enemiesLeft.Remove(enemiesLeft[i]);
                i--;
            }
        }

        if (waitForAnimation && GetComponent<ActivatedManager>().animationComplete)
        {
            animationComplete = true;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !player.gliding)
        {
            activateReady = true;
        }
    }
}
