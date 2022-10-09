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
    public float playerSpeed;

    private float horizontalInput;
    private float verticalInput;
    private Vector3 playerVelocity;
    private bool groundedPlayer;

    // Jump
    [Header("Jump")]
    public float jumpHeight;
    public float gravityValue;
    public float fallMultiplier;

    // Rotation
    [Header("Rotation")]
    public Transform orientation;
    public float followRotationSpeed;
    public float aimRotationSpeed;

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

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void Update()
    {
        // move
        MovePlayer();

        // player pointing
        PlayerPointing();

        // player rotation
        RotatePlayer();

        // jump
        if (Input.GetKeyDown(KeyCode.Space) && groundedPlayer)
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
        
        if (Input.GetMouseButtonDown(0) && aiming)
        {
            Shoot();
        }

        // check grounded
        CheckGrounded();

        // realistic fall
        if (!groundedPlayer && playerVelocity.y < 0)
        {
            playerVelocity += transform.up * gravityValue * fallMultiplier;
        }
    }

    private void MovePlayer()
    {
        // move input
        horizontalInput = Input.GetAxis("Horizontal"); 
        verticalInput = Input.GetAxis("Vertical");
        var move = horizontalInput * orientation.right.normalized + verticalInput * orientation.forward.normalized;
        move.y = 0;
        controller.Move(move * Time.deltaTime * playerSpeed);
        
        // apply movement
        playerVelocity.y += gravityValue * Time.deltaTime;
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
        playerVelocity.y += Mathf.Sqrt(jumpHeight * -3.0f * gravityValue);
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
        groundedPlayer = controller.isGrounded;
        if (groundedPlayer && playerVelocity.y < 0)
        {
            playerVelocity.y = 0f;
        }
    }
}
