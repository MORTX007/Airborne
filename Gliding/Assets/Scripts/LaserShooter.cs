using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations.Rigging;
using UnityEngine.UI;

public class LaserShooter : MonoBehaviour
{
    // Player
    [Header("Player")]
    private PlayerController player;
    private Camera mainCamera;
    public Transform aimBallPool;
    public Transform head;

    // Aim
    [Header("Aiming")]
    public Rig aimRig;
    public List<Transform> aimBalls;
    public LayerMask aimLayerMask;
    private Ray lastRayShot;
    public LineRenderer aimLine;
    public Vector3 aimLineOffset;
    public bool aiming;

    // Shoot
    [Header("Shooting")]
    public float laserDamage;
    public float laserRange;
    public List<Transform> hits;
    private RaycastHit lastRayHit;
    public float maxLaserAmount;
    public float currentLaserAmount;
    public float laserUsage;
    public float laserGain;
    public LineRenderer laserLine;
    public Light laserImpactLight;
    public Slider laserSlider;
    public bool shooting;

    private void Start()
    {
        player = GetComponent<PlayerController>();
        mainCamera = Camera.main;

        foreach (Transform aimBall in aimBallPool)
        {
            aimBalls.Add(aimBall);
            aimBall.GetComponentInChildren<ParticleSystem>().Stop();
        }

        currentLaserAmount = maxLaserAmount;
        laserSlider.maxValue = maxLaserAmount;
        laserSlider.value = maxLaserAmount;
    }

    private void Update()
    {
        // aim
        if (Input.GetMouseButton(1) && !player.gliding)
        {
            StartCoroutine(StartAim());
        }
        else
        {
            CancelAim();
        }

        // shoot
        if (Input.GetMouseButton(0) && currentLaserAmount > 0 && aiming)
        {
            Shoot();
        }
        else
        {
            laserLine.gameObject.SetActive(false);

            aimBalls[0].GetComponentInChildren<ParticleSystem>().Stop();
            laserImpactLight.intensity = 0f;

            shooting = false;
        }

        // aim balls
        if (shooting && hits.Count > 0)
        {
            for (int i = 0; i < aimBalls.Count; i++)
            {
                aimBalls[i].GetComponent<MeshRenderer>().enabled = i < hits.Count;
            }
        }
        else if (!shooting)
        {
            foreach (Transform aimBall in aimBalls)
            {
                aimBall.GetComponent<MeshRenderer>().enabled = false;
            }
        }

        if (aiming)
        {
            if (hits.Count > 0)
            {
                aimBalls[0].GetComponent<MeshRenderer>().enabled = true;
            }
            else if (hits.Count == 0)
            {
                aimBalls[0].GetComponent<MeshRenderer>().enabled = false;
            }
        }

        // sparks
        if (shooting)
        {
            for (int i = 0; i < aimBalls.Count; i++)
            {
                if (i < hits.Count && !aimBalls[i].GetComponentInChildren<ParticleSystem>().isEmitting)
                {
                    aimBalls[i].GetComponentInChildren<ParticleSystem>().Play();
                }
                else if (i >= hits.Count)
                {
                    aimBalls[i].GetComponentInChildren<ParticleSystem>().Stop();
                }
            }
        }
        else
        {
            foreach (Transform aimBall in aimBalls)
            {
                aimBall.GetComponentInChildren<ParticleSystem>().Stop();
            }
        }

        // animate laser bar
        laserSlider.value = currentLaserAmount;
    }

    private IEnumerator StartAim()
    {
        player.aimCam.gameObject.SetActive(true);
        yield return new WaitForSeconds(0.04f);
        RepositionAimBall();

        aimRig.weight = 1f;

        aimLine.gameObject.SetActive(true);

        aimLine.SetPosition(0, head.position + aimLineOffset);
        aimLine.SetPosition(1, aimBalls[0].position);

        if (hits.Count > 0 && hits[0].gameObject.layer == LayerMask.NameToLayer("charging station") && currentLaserAmount < maxLaserAmount)
        {
            currentLaserAmount += laserGain * Time.deltaTime;
        }

        aiming = true;
    }

    private void CancelAim()
    {
        player.aimCam.gameObject.SetActive(false);
        aimRig.weight = 0f;

        aimLine.gameObject.SetActive(false);

        aiming = false;
    }

    private void Shoot()
    {
        RepositionAimBall();

        if (hits.Count > 0)
        {
            foreach (Transform hit in hits)
            {
                if (hit.gameObject.layer == LayerMask.NameToLayer("enemy"))
                {
                    hit.GetComponentInParent<EnemyManager>().TakeDamage(laserDamage);
                }
            }
        }

        laserLine.gameObject.SetActive(true);
        laserImpactLight.intensity = 4.5f;
        aimLine.gameObject.SetActive(false);

        if (hits.Count > 0 && hits[0].gameObject.layer == LayerMask.NameToLayer("deflector"))
        {
            if (hits[hits.Count - 1].gameObject.layer == LayerMask.NameToLayer("deflector"))
            {
                if (hits.Count < aimBalls.Count)
                {
                    laserLine.positionCount = hits.Count + 2;
                }
                else if (hits.Count == aimBalls.Count)
                {
                    laserLine.positionCount = hits.Count + 1;
                }
            }
            else
            {
                laserLine.positionCount = hits.Count + 1;
            }
        }
        else
        {
            laserLine.positionCount = 2;
        }

        laserLine.SetPosition(0, head.position + aimLineOffset);
        for (int i = 1; i < laserLine.positionCount; i++)
        {
            laserLine.SetPosition(i, aimBalls[i - 1].position);
        }

        if (currentLaserAmount <= 0)
        {
            currentLaserAmount = 0;
        }
        else
        {
            currentLaserAmount -= laserUsage * Time.deltaTime;
        }

        shooting = true;
    }

    private void RepositionAimBall()
    {
        Vector2 screenCenterPoint = new Vector2(Screen.width / 2f, Screen.height / 2f);
        Ray ray = mainCamera.ScreenPointToRay(screenCenterPoint);
        RaycastHit hit;
        hits.Clear();
        if (Physics.Raycast(ray, out hit, aimLayerMask))
        {
            hits.Add(hit.transform);
            aimBalls[0].position = hit.point;
            lastRayShot = ray;
            lastRayHit = hit;

            // deflect laser
            if (hits[0].gameObject.layer == LayerMask.NameToLayer("deflector"))
            {
                DeflectLaser();
            }
        }
        else
        {
            aimBalls[0].localPosition = new Vector3(0, 0, laserRange);
        }
    }

    private void DeflectLaser()
    {
        int i = 1;
        while (lastRayHit.transform.gameObject.layer == LayerMask.NameToLayer("deflector"))
        {
            Vector3 dir = Vector3.Reflect(lastRayShot.direction, lastRayHit.normal);
            Ray ray = new Ray(lastRayHit.point, dir);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, aimLayerMask))
            {
                if (!hits.Contains(hit.transform))
                {
                    hits.Add(hit.transform);
                }
                aimBalls[i].position = hit.point;
                lastRayShot = ray;
                lastRayHit = hit;
            }
            else
            {
                aimBalls[i].position = lastRayHit.point + (ray.direction * laserRange);
                return;
            }
            i++;
        }
    }
}
