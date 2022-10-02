using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public Transform playerObj;
    public Transform glider;
    public Transform feet;

    [Header("Movement")]
    public float moveSpeed;

    public float groundDrag;

    [Header("Jump")]
    public float jumpForce;
    public float fallMultiplier;
    public float jumpCooldown;
    bool canJump;

    [Header("Gliding")]
    public float glidingSpeed;

    [Header("Ground Check")]
    public float groundCheckRange;
    public LayerMask whatIsGround;
    bool grounded;

    [Header("Step Detection")]
    public float stepDetectionRange;
    bool stepDetected;

    public Transform orientation;

    float horizontalInput;
    float verticalInput;

    Vector3 moveDirection;

    Rigidbody rb;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();

        ResetJump();
    }

    private void Update()
    {
        // ground check
        grounded = Physics.Raycast(feet.position, Vector3.down, groundCheckRange, whatIsGround);

        MyInput();
        SpeedControl();

        // handle drag
        rb.drag = groundDrag;

        // step detection
        stepDetected = Physics.Raycast(feet.transform.position, playerObj.forward, stepDetectionRange, whatIsGround);
        if (stepDetected && grounded && canJump)
        {
            canJump = false;
            Jump();

            Invoke(nameof(ResetJump), jumpCooldown);
        }
    }

    private void FixedUpdate()
    {
        MovePlayer();

        // realistic fall
        if (rb.velocity.y < 0)
        {
            rb.velocity += transform.up * Physics.gravity.y * fallMultiplier;
        }
    }
        
    private void MyInput()
    {
        // movement input
        horizontalInput = Input.GetAxisRaw("Horizontal");
        verticalInput = Input.GetAxisRaw("Vertical");


        // jump input
        if (Input.GetKeyDown(KeyCode.Space) && canJump && grounded)
        {
            canJump = false;
            Jump();

            Invoke(nameof(ResetJump), jumpCooldown);
        }

        // gliding input
        if (Input.GetKey(KeyCode.Space) && rb.velocity.y <= 0 && !grounded)
        {
            rb.useGravity = false;
            rb.velocity = new Vector3(rb.velocity.x, -glidingSpeed, rb.velocity.z);

            if (!glider.gameObject.activeSelf)
            {
                glider.gameObject.SetActive(true);
            }
        }
        else
        {
            rb.useGravity = true;
            glider.gameObject.SetActive(false);
        }
    }

    private void MovePlayer()
    {
        // calculate movement direction
        moveDirection = orientation.forward * verticalInput + orientation.right * horizontalInput;

        rb.AddForce(moveDirection.normalized * moveSpeed, ForceMode.Force);
    }

    private void SpeedControl()
    {
        Vector3 flatVel = new Vector3(rb.velocity.x, 0f, rb.velocity.z);

        // limit velocity if needed
        if (flatVel.magnitude > moveSpeed)
        {
            Vector3 limitedVel = flatVel.normalized * moveSpeed;
            rb.velocity = new Vector3(limitedVel.x, rb.velocity.y, limitedVel.z);
        }
    }

    private void Jump()
    {
        // reset y velocity
        rb.velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);

        rb.AddForce(transform.up * jumpForce, ForceMode.Impulse);
    }

    private void ResetJump()
    {
        canJump = true;
    }
}
