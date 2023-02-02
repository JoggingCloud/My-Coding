using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerRigidBody : MonoBehaviour
{
    Rigidbody rb;
    public GameObject mainCamera;
    public Animator animator;
    Vector3 moveDirection;

    [Header("Ground Check:")]
    public float playerHeight;
    public bool isGrounded;
    public LayerMask groundLayer;


    [Header("Movement:")]
    float moveForward;
    float moveSide;
    public float currentSpeed;
    public float walkSpeed;
    public float sprintSpeed;
    //public bool isWalking = false;
    public bool isRunning = false;
    //public bool isBackwards = false;
    public float backwardRunSpeed;

    [Header("Slope Handling:")]
    public float maxSlopeAngle;
    private RaycastHit slopeHit;
    public bool exitingSlope;

    [Header("Jumping:")]
    public Transform groundPt;
    public bool jumpKeyPressed;
    public float jumpForce = 2f;
    public float newGravity = -9.81f;
    public float gravityMultiplier = 2f;
    public bool isJumping = false;

    [Header("Crouch Movement")]
    public float crouchYScale;
    public float startYScale;
    public bool isCrouched = false;
    public float crouchSpeed;

    [Header("Sliding:")]
    public float slideSpeed;

    [Header("Key Count:")]
    public int keyCount;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        startYScale = transform.localScale.y;
        currentSpeed = walkSpeed;

        // Gets reference to main camera 
        if (mainCamera == null)
        {
            mainCamera = GameObject.FindGameObjectWithTag("MainCamera");
        }
    }

    private void Update()
    {
        moveForward = Input.GetAxis("Vertical") * currentSpeed;
        moveSide = Input.GetAxis("Horizontal") * currentSpeed;
        jumpKeyPressed = Input.GetButtonDown("Jump");

        isGrounded = Physics.CheckSphere(groundPt.position, 0.5f, groundLayer);

        // Upright Movement 
        // Walking forward 
        if (Input.GetKey(KeyCode.W))
        {
            //isWalking = true;
            animator.SetBool("isIdle", false);
            currentSpeed = walkSpeed;
            animator.SetBool("isWalking", true);
        }
        else if (!Input.GetKey(KeyCode.W))
        {
            animator.SetBool("isWalking", false);
            animator.SetBool("isIdle", true);
        }

        // Running forward
        if (Input.GetKey(KeyCode.W) && Input.GetKey(KeyCode.LeftShift))
        {
            isRunning = true;
            //isWalking = false;
            currentSpeed = sprintSpeed;
            animator.SetBool("isBackward", false);
            animator.SetBool("isWalking", false);
            animator.SetBool("isRunning", true);
        }
        else if (!Input.GetKey(KeyCode.W) && !Input.GetKey(KeyCode.LeftShift))
        {
            isRunning = false;
            currentSpeed = walkSpeed;
            animator.SetBool("isRunning", false);
        }
        else if (Input.GetKey(KeyCode.W) && !Input.GetKey(KeyCode.LeftShift))
        {
            //isRunning = false;
            //currentSpeed = walkSpeed;
            animator.SetBool("isRunning", false);
            animator.SetBool("isWalking", true);
        }
        // Ensure holding shift after letting go of W key does not continue the running forward animation
        else if (Input.GetKey(KeyCode.LeftShift))
        {
            //isRunning = false;
            animator.SetBool("isRunning", false);
            animator.SetBool("isIdle", true);
        }

        // Walking Backward Animation
        if (Input.GetKey(KeyCode.S))
        {
            //isWalking = true;
            animator.SetBool("isIdle", false);
            currentSpeed = walkSpeed;
            animator.SetBool("isNegative", true);
        }
        else if (!Input.GetKey(KeyCode.S))
        {
            animator.SetBool("isNegative", false);
            animator.SetBool("isIdle", true);
        }

        // Running Backward Animation 
        if (Input.GetKey(KeyCode.S) && Input.GetKey(KeyCode.LeftShift))
        {
            //isRunning = false;
            //isBackwards = true;
            //isWalking = false;
            animator.SetBool("isRunning", false);
            animator.SetBool("isNegative", false);
            currentSpeed = backwardRunSpeed;
            animator.SetBool("isBackward", true);
        }
        else if (!Input.GetKey(KeyCode.S) && !Input.GetKey(KeyCode.LeftShift))
        {
            currentSpeed = walkSpeed;
            animator.SetBool("isBackward", false);
        }
        else if (Input.GetKey(KeyCode.S) && !Input.GetKey(KeyCode.LeftShift))
        {
            currentSpeed = walkSpeed;
            animator.SetBool("isBackward", false);
            animator.SetBool("isNegative", true);
        }
        // Ensure holding shift after letting go of S key does not continue the running backward animation
        else if (Input.GetKey(KeyCode.LeftShift))
        {
            //isRunning = false;
            animator.SetBool("isBackward", false);
            animator.SetBool("isIdle", true);
        }

        // Right Strafe
        if (Input.GetKey(KeyCode.D))
        {
            currentSpeed = walkSpeed;
            animator.SetBool("isRight", true);
        }
        else if (!Input.GetKey(KeyCode.D))
        {
            animator.SetBool("isRight", false);
        }
        // Left Strafe 
        if (Input.GetKey(KeyCode.A))
        {
            currentSpeed = walkSpeed;
            animator.SetBool("isLeft", true);
        }
        else if (!Input.GetKey(KeyCode.A))
        {
            animator.SetBool("isLeft", false);
        }

        // Sliding 
        if (isRunning && Input.GetKey(KeyCode.C))
        {
            animator.SetBool("isSliding", true);
            rb.AddForce(transform.forward * slideSpeed, ForceMode.VelocityChange);
        }
        else
        {
            animator.SetBool("isSliding", false);
        }

        // On Slope
        if (OnSlope() && !exitingSlope)
        {
            rb.AddForce(GetSlopeMoveDirection() * currentSpeed * 20f, ForceMode.Force);
            // Keeps player constantly on the slope
            if (rb.velocity.y > 0)
            {
                rb.AddForce(Vector3.down * 80f, ForceMode.Force);
            }
            
            if (rb.velocity.magnitude > currentSpeed)
            {
                rb.velocity = rb.velocity.normalized * currentSpeed;
            }
        }
        rb.useGravity = !OnSlope();

        //Makes Character Jump
        if (isGrounded && jumpKeyPressed)
        {
            isJumping = true;
            exitingSlope = true;
            rb.AddForce(transform.up * jumpForce * rb.mass, ForceMode.Impulse);
            animator.SetBool("isJumped", true);
            Debug.Log("Player is jumping");
        }
        else if (isGrounded || !jumpKeyPressed)
        {
            animator.SetBool("isJumped", false);
        }
        else if (isRunning && jumpKeyPressed)
        {
            isJumping = false;
        }
        if (isRunning && jumpKeyPressed)
        {
            animator.SetBool("isJumping", true);
        }
        else if (isRunning && !jumpKeyPressed)
        {
            animator.SetBool("isJumping", false);
        }

        // Crouch Movement
        if (isGrounded && Input.GetKeyDown(KeyCode.LeftControl))
        {
            transform.localScale = new Vector3(transform.localScale.x, crouchYScale, transform.localScale.z);
            rb.AddForce(Vector3.down * 5f, ForceMode.Impulse);
            isCrouched = true;
            currentSpeed = crouchSpeed;
            animator.SetBool("isCrouched", true);
        }
        else if (Input.GetKeyUp(KeyCode.LeftControl))
        {
            transform.localScale = new Vector3(transform.localScale.x, startYScale, transform.localScale.z);
            isCrouched = false;
            currentSpeed = walkSpeed;
            animator.SetBool("isCrouched", false);
        }

        // Crouch Move Forward
        if (isCrouched && Input.GetKey(KeyCode.W))
        {
            //currentSpeed = crouchSpeed;
            animator.SetBool("isCrouchWalking", true);
        }
        else if (isCrouched && !Input.GetKey(KeyCode.W))
        {
            animator.SetBool("isCrouchWalking", false);
            animator.SetBool("isCrouched", true);
        }

        // Crouch Move Backward
        if (isCrouched && Input.GetKey(KeyCode.S))
        {
            animator.SetBool("isCrouchBackwards", true);
        }
        else if (isCrouched && !Input.GetKey(KeyCode.S))
        {
            animator.SetBool("isCrouchBackwards", false);
            animator.SetBool("isCrouched", true);
        }

        // Crouch Walk Strafe Left
        if (isCrouched && Input.GetKey(KeyCode.A))
        {
            animator.SetBool("isCrouchLeft", true);
        }
        else if (isCrouched && !Input.GetKey(KeyCode.A))
        {
            animator.SetBool("isCrouchLeft", false);
            animator.SetBool("isCrouched", true);
        }

        // Crouch Walk Strafe Right 
        if (isCrouched && Input.GetKey(KeyCode.D))
        {
            animator.SetBool("isCrouchRight", true);
        }
        else if (isCrouched && !Input.GetKey(KeyCode.D))
        {
            animator.SetBool("isCrouchRight", false);
            animator.SetBool("isCrouched", true);
        }
    }

    private void FixedUpdate()
    {
        // Adds gravity when player is in the air so the player can come down at an increase rate
        if (rb.velocity.y < -0.05f)
        {
            rb.AddForce(Physics.gravity * gravityMultiplier, ForceMode.Acceleration);
        }
        else
        {
            Physics.gravity = new Vector3(0, newGravity, 0);
            rb.mass = 1;
        }

        rb.velocity = (transform.forward * moveForward) + (transform.right * moveSide) + (transform.up * rb.velocity.y);
    }

    private bool OnSlope()
    {
        if (Physics.Raycast(transform.position, Vector3.down, out slopeHit, playerHeight * 0.5f + 0.3f))
        {
            float angle = Vector3.Angle(Vector3.up, slopeHit.normal);
            return angle < maxSlopeAngle && angle != 0;
        }
        return false;
    }

    private Vector3 GetSlopeMoveDirection()
    {
        return Vector3.ProjectOnPlane(moveDirection, slopeHit.normal).normalized;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Key")
        {
            keyCount += 1;
            Destroy(other.gameObject);
        }
    }
}