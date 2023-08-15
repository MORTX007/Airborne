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
    public Transform head;

    // Aim
    [Header("Aiming")]
    public Rig aimRig;
    public Transform initAimBall;
    public GameObject aimBallPrefab;
    public LayerMask aimLayerMask;
    public LineRenderer aimLine;
    public Vector3 aimLineOffset;
    public bool aiming;

    // Shoot
    [Header("Shooting")]
    public float laserDamage;
    public float laserRange;
    public float maxLaserAmount;
    public float currentLaserAmount;
    public float laserUsage;
    public float laserGain;
    public LineRenderer laserLine;
    public Light laserImpactLight;
    public ParticleSystem sparksPartSys;
    public Slider laserSlider;
    public bool shooting;
    private RaycastHit hit;

    private void Start()
    {
        player = GetComponent<PlayerController>();
        mainCamera = Camera.main;

        currentLaserAmount = maxLaserAmount;
        laserSlider.maxValue = maxLaserAmount;
        laserSlider.value = maxLaserAmount;
    }

    private void Update()
    {
        // aim
        if (Input.GetMouseButton(1) && !player.gliding)
        {
            StartAim();
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

            sparksPartSys.Stop();
            laserImpactLight.intensity = 0f;

            shooting = false;
        }

        // animate laser bar
        laserSlider.value = currentLaserAmount;
    }

    private void StartAim()
    {
        player.aimCam.gameObject.SetActive(true);
        aimRig.weight = 1f;

        aimLine.gameObject.SetActive(true);

        RepositionAimBall();

        aimLine.SetPosition(0, head.position + aimLineOffset);
        aimLine.SetPosition(1, initAimBall.position);

        var targetHit = RepositionAimBall();

        if (targetHit && hit.transform.gameObject.layer == LayerMask.NameToLayer("charging station") && currentLaserAmount < maxLaserAmount)
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
        var targetHit = RepositionAimBall();

        if (targetHit && hit.transform.gameObject.layer == LayerMask.NameToLayer("enemy"))
        {
            hit.transform.GetComponentInParent<EnemyManager>().TakeDamage(laserDamage);
        }

        laserLine.gameObject.SetActive(true);
        laserImpactLight.intensity = 4.5f;
        aimLine.gameObject.SetActive(false);

        laserLine.SetPosition(0, head.position + aimLineOffset);
        laserLine.SetPosition(1, initAimBall.position);

        if (targetHit)
        {
            sparksPartSys.Play();
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

    private bool RepositionAimBall()
    {
        Vector2 screenCenterPoint = new Vector2(Screen.width / 2f, Screen.height / 2f);
        Ray ray = mainCamera.ScreenPointToRay(screenCenterPoint);
        if (Physics.Raycast(ray, out hit, aimLayerMask))
        {
            initAimBall.position = hit.point;
            return true;
        }
        else
        {
            initAimBall.localPosition = new Vector3(0, 0, laserRange);
            return false;
        }
    }
}
