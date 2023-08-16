using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using UnityEngine.UI;

public class PlayerController : MonoBehaviour
{
    // Player
    [Header("Player")]
    private CharacterController controller;
    private LaserShooter laserShooter;
    private Camera mainCamera;

    // Camera
    [Header("Camera")]
    public CinemachineVirtualCamera followCam;
    public CinemachineVirtualCamera aimCam;
    public CinemachineVirtualCamera glidingCam;
    private CinemachineVirtualCamera activeCam;

    // Health
    [Header("Health")]
    public float maxHealth;
    public float currentHealth;
    public Slider healthBarSlider;
    public float healthBarAnimSpeed;
    private bool animateHealthBar;
    private float currentVelocity = 0f;

    // Movement
    [Header("Movement")]
    public float moveSpeed;

    private float horizontalInput;
    private float verticalInput;
    private Vector3 move;
    private Vector3 playerVelocity;
    private bool moving;
    private bool grounded;

    // Jump
    [Header("Jump")]
    public float jumpHeight;
    public float normalGravityValue;
    public float fallMultiplier;
    private bool jumping;

    // Rotation
    [Header("Rotation")]
    public Transform orientation;
    public float followRotationSpeed;
    public float aimRotationSpeed;

    // Gliding
    [Header("Gliding")]
    public float glidingSpeed;
    public float glidingHeight;
    public float glidingYOffset;
    public LayerMask glidingLayerMask;
    public List<GameObject> trails;
    public Transform glidingCheck1;
    public Transform glidingCheck2;
    private bool canGlide;
    private bool canInitJump;
    public bool gliding;
    private bool wasGliding;

    // Camera Shake
    private float startingNoiseAmp;
    private float startingNoiseFreq;
    private float shakeTimer;

    //Animation
    [Header("Animation")]
    public Animator animator;


    private void Start()
    {
        controller = GetComponent<CharacterController>();
        laserShooter = GetComponent<LaserShooter>();
        mainCamera = Camera.main;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        currentHealth = maxHealth;
        healthBarSlider.maxValue = maxHealth;
        healthBarSlider.value = maxHealth;

        startingNoiseAmp = followCam.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>().m_AmplitudeGain;
        startingNoiseFreq = followCam.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>().m_FrequencyGain;
    }

    private void Update()
    {
        // move
        MovePlayer();

        if (move.magnitude != 0)
        {
            moving = true;
        }
        else
        {
            moving = false;
        }

        // player rotation
        RotatePlayer();

        // jump
        if (Input.GetKeyDown(KeyCode.Space) && grounded)
        {
            Jump();
        }

        // animate health bar
        if (animateHealthBar && healthBarSlider.value != currentHealth)
        {
            healthBarSlider.value = Mathf.SmoothDamp(healthBarSlider.value, currentHealth, ref currentVelocity, healthBarAnimSpeed * Time.deltaTime);
        }
        else if (healthBarSlider.value == currentHealth)
        {
            animateHealthBar = true;
        }

        // check grounded
        CheckGrounded();

        // realistic fall
        if (!grounded && playerVelocity.y < 0 && !gliding)
        {
            playerVelocity += transform.up * normalGravityValue * fallMultiplier;
        }

        // change active cam
        UpdateActiveCam();

        // shake timer
        ShakeTimer();

        // animation
        Animate();
    }

    private void MovePlayer()
    {
        // move input
        horizontalInput = Input.GetAxis("Horizontal");
        verticalInput = Input.GetAxis("Vertical");
        move = horizontalInput * orientation.right + verticalInput * orientation.forward;
        move.y = 0;

        // apply movement
        if (!gliding)
        {
            controller.Move(Vector3.ClampMagnitude(move, 1f) * moveSpeed * Time.deltaTime);
        }
        else
        {
            controller.Move(orientation.forward * glidingSpeed * Time.deltaTime);
        }

        // if able to glide Check
        if (!Physics.Raycast(transform.position, -transform.up, glidingHeight, glidingLayerMask))
        {
            canGlide = true;
        }
        else if (Physics.Raycast(transform.position, -transform.up, 3f, glidingLayerMask))
        {
            canGlide = false;
        }

        if (!Physics.Raycast(glidingCheck1.position, -glidingCheck1.up, glidingHeight, glidingLayerMask) || !Physics.Raycast(glidingCheck2.position, -glidingCheck2.up, glidingHeight, glidingLayerMask))
        {
            canInitJump = true;
        }
        else if (Physics.Raycast(glidingCheck1.position, -glidingCheck1.up, 10f, glidingLayerMask) && Physics.Raycast(glidingCheck2.position, -glidingCheck2.up, 10f, glidingLayerMask))
        {
            canInitJump = false;
        }

        // gliding
        if (playerVelocity.y <= 0 && !grounded && canGlide)
        {
            playerVelocity.y = 0f;
            glidingCam.gameObject.SetActive(true);

            // enable gliding trails
            foreach (GameObject trail in trails)
            {
                trail.SetActive(true);
            }

            gliding = true;
            jumping = false;
            wasGliding = true;
        }
        // init jump
        else if (canInitJump && grounded && moving)
        {
            Jump();
        }
        // normal
        else
        {
            playerVelocity.y += normalGravityValue * Time.deltaTime;
            glidingCam.gameObject.SetActive(false);
            gliding = false;

            // disable gliding trails
            foreach (GameObject trail in trails)
            {
                trail.SetActive(false);
            }
        }

        // apply gravity
        controller.Move(playerVelocity * Time.deltaTime);
    }

    private void RotatePlayer()
    {
        // rotate player object
        if (!laserShooter.aiming && !gliding)
        {
            // rotate orientation
            Vector3 viewDir = (transform.position - new Vector3(mainCamera.transform.position.x, transform.position.y, mainCamera.transform.position.z)).normalized;
            orientation.forward = viewDir;

            Vector3 inputDir = ((orientation.forward * verticalInput) + (orientation.right * horizontalInput)).normalized;

            transform.forward = Vector3.Slerp(transform.forward, inputDir, followRotationSpeed * Time.deltaTime);

            if (wasGliding)
            {
                transform.forward = orientation.forward;
                wasGliding = false;
            }
        }
        else if (laserShooter.aiming)
        {
            Vector3 aimTarget = laserShooter.aimBalls[0].transform.position;
            aimTarget.y = transform.position.y;
            Vector3 viewDir = (aimTarget - transform.position).normalized;
            orientation.forward = viewDir;

            transform.forward = Vector3.Slerp(transform.forward, viewDir, aimRotationSpeed * Time.deltaTime);

        }
        else if (gliding)
        {
            // rotate orientation
            Vector3 viewDir = (transform.position - new Vector3(mainCamera.transform.position.x, mainCamera.transform.position.y + glidingYOffset, mainCamera.transform.position.z)).normalized;
            orientation.forward = viewDir;

            transform.forward = Vector3.Slerp(transform.forward, orientation.forward, followRotationSpeed * Time.deltaTime);
        }
    }

    private void Jump()
    {
        // change height of player
        playerVelocity.y += Mathf.Sqrt(jumpHeight * -3.0f * normalGravityValue);
        jumping = true;
    }

    private void CheckGrounded()
    {
        grounded = controller.isGrounded;
        if (grounded && playerVelocity.y < 0)
        {
            playerVelocity.y = 0f;
            jumping = false;
            gliding = false;
        }
    }

    public void TakeDamage(float damage)
    {
        currentHealth -= damage;
        animateHealthBar = true;
    }

    private void UpdateActiveCam()
    {
        activeCam = followCam;
        if (aimCam.gameObject.activeSelf)
        {
            activeCam = aimCam;
        }
        else if (glidingCam.gameObject.activeSelf)
        {
            activeCam = glidingCam;
        }
    }

    public void ShakeCamera(float amp, float freq, float time)
    {
        CinemachineBasicMultiChannelPerlin perlin = activeCam.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();

        if (amp >= perlin.m_AmplitudeGain || freq >= perlin.m_FrequencyGain)
        {
            perlin.m_AmplitudeGain = amp;
            perlin.m_FrequencyGain = freq;
            shakeTimer = time;
        }
    }

    private void ShakeTimer()
    {
        if (shakeTimer > 0)
        {
            shakeTimer -= Time.deltaTime;
        }

        if (shakeTimer <= 0f)
        {
            CinemachineBasicMultiChannelPerlin perlin = activeCam.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();

            perlin.m_AmplitudeGain = Mathf.Lerp(perlin.m_AmplitudeGain, startingNoiseAmp, 0.008f);
            perlin.m_FrequencyGain = Mathf.Lerp(perlin.m_FrequencyGain, startingNoiseFreq, 0.008f);
        }
    }

    private void Animate()
    {
        if (moving)
        {
            animator.SetBool("Moving", true);
        }
        else
        {
            animator.SetBool("Moving", false);
        }

        // Follow Animation

        if (jumping)
        {
            animator.SetBool("Jumping", true);
            animator.SetBool("Grounded", false);
            animator.SetBool("Falling", false);
        }
        if (playerVelocity.y < 0f)
        {
            animator.SetBool("Falling", true);
            animator.SetBool("Jumping", false);
            animator.SetBool("Grounded", false);
        }
        if (grounded)
        {
            animator.SetBool("Grounded", true);
            animator.SetBool("Jumping", false);
            animator.SetBool("Falling", false);

            if (!jumping)
            {
                animator.SetFloat("Speed", move.magnitude);
                animator.SetBool("Jumping", false);
                animator.SetBool("Falling", false);
            }
        }

        // Aiming Animation
        if (laserShooter.aiming)
        {
            animator.SetBool("Aiming", true);
            animator.SetFloat("Input X", horizontalInput);
            animator.SetFloat("Input Y", verticalInput);
        }
        else
        {
            animator.SetBool("Aiming", false);
        }

        // Gliding Animation
        if (gliding)
        {
            animator.SetFloat("Speed", move.magnitude);
            animator.SetBool("Gliding", true);
            animator.SetBool("Jumping", false);
            animator.SetBool("Falling", false);
        }
        else
        {
            animator.SetBool("Gliding", false);
        }
    }
}
