using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using UnityEngine.Animations.Rigging;

public class PlayerController : MonoBehaviour
{
    // Player
    [Header("Player")]
    public CharacterController controller;
    public Camera mainCamera;

    // Camera
    [Header("Camera")]
    public CinemachineVirtualCamera followCam;
    public CinemachineVirtualCamera aimCam;
    public CinemachineVirtualCamera glidingCam;

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

    // Aim
    [Header("Aiming")]
    public Rig aimRig;
    public Transform aimBall;
    public LayerMask aimLayerMask;
    private bool aiming;

    // Gliding
    [Header("Gliding")]
    public float glidingSpeed;
    public float glidingHeight;
    public float glidingYOffset;
    public LayerMask glidingLayerMask;
    public List<GameObject> trails;
    private bool canGlide;
    private bool gliding;
    private bool wasGliding;

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

        // player rotation
        RotatePlayer();

        // jump
        if (Input.GetKeyDown(KeyCode.Space) && grounded)
        {
            Jump();
        }

        // aim
        if (Input.GetMouseButton(1))
        {
            StartAim();
        }
        else
        {
            CancelAim();
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

        // gliding
        if (playerVelocity.y <= 0 && !grounded && canGlide && !aiming)
        {
            glidingCam.gameObject.SetActive(true);
            playerVelocity.y = 0f;

            // enable gliding trails
            foreach (GameObject trail in trails)
            {
                trail.SetActive(true);
            }

            gliding = true;
            jumping = false;
            wasGliding = true;
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
        if (!aiming && !gliding)
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
        else if (aiming)
        {
            Vector3 aimTarget = aimBall.transform.position;
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

    private void StartAim()
    {
        aimCam.gameObject.SetActive(true);
        aimRig.weight = 1f;
        aiming = true;
    }

    private void CancelAim()
    {
        aimCam.gameObject.SetActive(false);
        aimRig.weight = 0f;
        aiming = false;
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
        // Movement Aniamtion
        if (moving)
        {
            animator.SetBool("Moving", true);
        }
        else
        {
            animator.SetBool("Moving", false);
        }

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
        }
        else
        {
            animator.SetBool("Gliding", false);
        }
    }
}
