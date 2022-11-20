using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SparksManager : MonoBehaviour
{
    public ParticleSystem redSparks;
    public ParticleSystem blueSparks;
    private PlayerController player;

    private void Start()
    {
        player = FindObjectOfType<PlayerController>();
    }

    private void Update()
    {
        if (!player.shooting && (redSparks.isPlaying || blueSparks.isPlaying))
        {
            redSparks.Stop();
            blueSparks.Stop();
        }

        if (redSparks.isStopped && blueSparks.isStopped && redSparks.particleCount == 0 && blueSparks.particleCount == 0)
        {
            Destroy(gameObject);
        }
    }
}
