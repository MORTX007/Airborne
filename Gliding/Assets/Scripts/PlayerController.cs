using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class PlayerController : MonoBehaviour
{
    // Player
    [Header("Player")]
    public CharacterController controller;
    public Transform playerObj;
    public Camera mainCamera;

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
    public CinemachineVirtualCamera glidingCam;
    public float glidingSpeed;
    public float movingGlidingGravityValue;
    public float freeFallGlidingGravityValue;
    public float glidingHeight;
    public LayerMask glidingLayerMask;
    public float moveGlidingNoiseFreqValue;
    public float freeFallGlidingNoiseFreqValue;
    public List<GameObject> trails;
    private bool canGlide;
    private bool gliding;

    // Aiming
    [Header("Aiming")]
    public CinemachineVirtualCamera followCam;
    public CinemachineVirtualCamera aimCam;
    public LayerMask aimLayerMask;
    private Vector3 aimTarget;
    private bool aiming;

    // Shooting
    [Header("Shooting")]
    public Transform sphere;

    //Animation
    [Header("Animation")]
    public Animator animator;
    public float followAniamtionBlendTime;
    public float aimAniamtionBlendTime;

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
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

        // player pointing
        PlayerPointing();

        // player rotation
        RotatePlayer();

        // jump
        if (Input.GetKeyDown(KeyCode.Space) && grounded)
        {
            Jump();
        }

        // aim
        if (Input.GetMouseButton(1) & !gliding)
        {
            StartAim();
        }
        else
        {
            CancelAim();
        }
        
        if (Input.GetMouseButtonDown(0) && aiming && !gliding)
        {
            Shoot();
        }

        // check grounded
        CheckGrounded();

        // realistic fall
        if (!grounded && playerVelocity.y < 0 && !gliding)
        {
            playerVelocity += transform.up * normalGravityValue * fallMultiplier;
        }

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

        // deciding speed based on movement type
        var speed = 0f;
        if (!gliding)
        {
            speed = moveSpeed;
        }
        else
        {
            speed = glidingSpeed;
        }

        // apply movement
        controller.Move(Vector3.ClampMagnitude(move, 1f) * Time.deltaTime * speed);

        // if able to glide Check
        if (!Physics.Raycast(playerObj.transform.position, -transform.up, glidingHeight, glidingLayerMask))
        {
            canGlide = true;
        }
        else
        {
            canGlide = false;
        }

        // control gravity for gliding
        if (Input.GetKey(KeyCode.Space) && playerVelocity.y <= 0 && !grounded && canGlide)
        {
            glidingCam.gameObject.SetActive(true);

            if (moving)
            {
                playerVelocity.y = movingGlidingGravityValue;
                glidingCam.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>().m_FrequencyGain = moveGlidingNoiseFreqValue;
            }
            else
            {
                playerVelocity.y = freeFallGlidingGravityValue;
                glidingCam.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>().m_FrequencyGain = freeFallGlidingNoiseFreqValue;
            }

            // enable gliding trails
            foreach (GameObject trail in trails)
            {
                trail.SetActive(true);
            }

            gliding = true;
            jumping = false;
        }
        // control normal gravity
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

    private void PlayerPointing()
    {
        // where is player pointing
        Vector2 screenCenterPoint = new Vector2(Screen.width / 2, Screen.height / 2);
        Ray ray = mainCamera.ScreenPointToRay(screenCenterPoint);
        Physics.Raycast(ray, out RaycastHit hit, 999f, aimLayerMask);
        aimTarget = hit.point;
    }

    private void RotatePlayer()
    {
        // follow cam rotation
        if (!aiming)
        {
            // rotate orientation
            Vector3 viewDir = (transform.position - new Vector3(mainCamera.transform.position.x, transform.position.y, mainCamera.transform.position.z)).normalized;
            orientation.forward = viewDir;

            // rotate player object
            Vector3 inputDir = ((orientation.forward * verticalInput) + (orientation.right * horizontalInput)).normalized;

            if (inputDir != Vector3.zero)
            {
                playerObj.forward = Vector3.Slerp(playerObj.forward, inputDir, followRotationSpeed * Time.deltaTime);
            }
        }

        // aim cam rotation
        if (aiming)
        {
            // rotate player to face center of screen
            Vector3 aimPoint = aimTarget;
            aimPoint.y = transform.position.y;
            Vector3 aimDir = (aimPoint - transform.position).normalized;
            orientation.forward = aimDir;

            playerObj.forward = Vector3.Slerp(playerObj.forward, aimDir, aimRotationSpeed * Time.deltaTime);
        }
    }

    private void Jump()
    {
        // change height of player
        playerVelocity.y += Mathf.Sqrt(jumpHeight * -3.0f * normalGravityValue);
        jumping = true;
    }

    private void StartAim()
    {
        aimCam.gameObject.SetActive(true);
        aiming = true;
    }

    private void CancelAim()
    {
        aimCam.gameObject.SetActive(false);
        aiming = false;
    }

    private void Shoot()
    {
        sphere.position = aimTarget;
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

        // Aiming Aniamtions
        if (aiming)
        {
            animator.SetBool("Aiming", true);
            animator.SetFloat("Aim Speed X", horizontalInput, aimAniamtionBlendTime, Time.deltaTime);
            animator.SetFloat("Aim Speed Y", verticalInput, aimAniamtionBlendTime, Time.deltaTime);
        }
        else
        {
            animator.SetBool("Aiming", false);
        }
        

        // Follow Animations
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
                animator.SetFloat("Follow Speed", move.magnitude, followAniamtionBlendTime, Time.deltaTime);
                animator.SetBool("Jumping", false);
                animator.SetBool("Falling", false);
            }
        }

        // Gliding
        if (gliding)
        {
            animator.SetFloat("Follow Speed", move.magnitude, followAniamtionBlendTime, Time.deltaTime);
            animator.SetBool("Gliding", true);
            animator.SetBool("Jumping", false);
            animator.SetBool("Falling", false);
            animator.SetBool("Aiming", false);
        }
        else
        {
            animator.SetBool("Gliding", false);
        }
    }
}
