using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

    [Header("Jump")]
    public float jumpHeight;
    public float gravityValue;
    public float fallMultiplier;

    // Rotation
    [Header("Rotation")]
    public Transform orientation;
    public float rotationSpeed;

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void Update()
    {
        CheckGrounded();

        // move
        MovePlayer();

        // player rotation
        RotatePlayer();

        // jump
        if (Input.GetKeyDown(KeyCode.Space) && groundedPlayer)
        {
            Jump();
        }

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

    private void RotatePlayer()
    {
        // rotate orientation
        Vector3 viewDir = transform.position - new Vector3(mainCamera.transform.position.x, transform.position.y, mainCamera.transform.position.z);
        orientation.forward = viewDir.normalized;

        // rotate player object
        Vector3 inputDir = (orientation.forward * verticalInput) + (orientation.right * horizontalInput);

        if (inputDir != Vector3.zero)
        {
            playerObj.forward = Vector3.Slerp(playerObj.forward, inputDir.normalized, Time.deltaTime * rotationSpeed);
        }
    }

    private void Jump()
    {
        // change height of player
        playerVelocity.y += Mathf.Sqrt(jumpHeight * -3.0f * gravityValue);
    }

    private void CheckGrounded()
    {
        // check grounded
        groundedPlayer = controller.isGrounded;
        if (groundedPlayer && playerVelocity.y < 0)
        {
            playerVelocity.y = 0f;
        }
    }
}
