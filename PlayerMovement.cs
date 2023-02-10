using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public CharacterController controller;
    private GameObject mainCamera;

    public float speed = 0f;

    [Header("Upright Movement:")]
    public float runSpeed = 0f;
    public float gravity = -9.81f;
    public float jumpHeight = 0f;
    public float normalHeight;
    public Vector3 yNormalCenterHeight;

    [Header("Crouch Movement:")]
    public float crouchSpeed = 0f;
    public float crouchHeight;
    public Vector3 yCrouchCenterHeight;
    public bool isCrouched = false;

    [Header("Ground Detection:")]
    // Access to obstacles that has this layer selected in the inspector 
    public Transform groundCheck;

    // Radius of sphere that is used to check the ground distance 
    public float groundDistance = 0.4f;

    // Control what objects the sphere should check for 
    public LayerMask groundMask;

    // Stores current velocity
    Vector3 velocity;

    // If grounded or not 
    public bool isGrounded;

    Animator animator;

    [Header("Sound Effects:")]
    //public AudioSource runningAudioSource;
    public bool isRunning;

    private void Awake()
    {
        // Gets reference to main camera 
        if (mainCamera == null)
        {
            mainCamera = GameObject.FindGameObjectWithTag("MainCamera");
        }    
    }

    private void Start()
    {
        animator = GetComponent<Animator>();
        //runningAudioSource = GetComponent<AudioSource>();    
    }

    private void Update()
    {
        // Creates a tiny invisible sphere 
        // Checking the position and distance the player is from the ground if player is falling and or currently on the ground 
        isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);

        // If on the ground and the velocity is less than 0 then the velocity is reset. Set at -2f to ensure that the player is on the ground
        if (isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
            animator.SetBool("isGrounded", true);
        }

        // Gets the horizontal axis 
        float x = Input.GetAxis("Horizontal");

        // Gets vertical axis  
        float z = Input.GetAxis("Vertical");

        // player movement which takes the direction the player is moving based on where the player is facing
        Vector3 move = transform.right * x + transform.forward * z;
        if (isRunning)
        {
            //runningAudioSource.Play();
        }
        
        // Running Forward Animation 
        if (Input.GetKey(KeyCode.W) && !isCrouched)
        {
            animator.SetBool("isRunning", true);
            isRunning = true;
            //Debug.Log("sound playing");
        }
        else
        {
            animator.SetBool("isRunning", false);
            animator.SetBool("isIdle", true);
            //runningAudioSource.Play();
            //Debug.Log("Sound is suppose to playing");
        }

        // Running Backward Animation
        if (Input.GetKey(KeyCode.S))
        {
            animator.SetBool("isNegative", true);
        }
        else
        {
            animator.SetBool("isNegative", false);
            animator.SetBool("isIdle", true);
        }

        // Left Strafe Running
        if (Input.GetKey(KeyCode.A))
        {
            animator.SetBool("isLeft", true);
        }
        else
        {
            animator.SetBool("isLeft", false);
            animator.SetBool("isIdle", true);
        }

        // Right Strafe Running
        if (Input.GetKey(KeyCode.D))
        {
            animator.SetBool("isRight", true);
        }
        else
        {
            animator.SetBool("isRight", false);
            animator.SetBool("isIdle", true);
        }
        
        // player movement speed over time per frame rate 
        controller.Move(move * speed * Time.deltaTime);

        // If player presses the jump button and is on the ground then the player will jump in the y direction and come down to the ground because of gravity
        // Jumping and Falling Animation
        if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
        {
            animator.SetBool("isJumping", true);
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }
        else if (isGrounded && velocity.y < 0)
        {
            animator.SetBool("isJumping", false);
            animator.SetBool("isGrounded", true); 
        }

        // Crouching Inputs
        if (Input.GetKeyDown(KeyCode.LeftControl) && isGrounded)
        {
            isCrouched = true; 
            speed = crouchSpeed;
            controller.height = crouchHeight;
            controller.center = yCrouchCenterHeight;
            animator.SetBool("isCrouched", true);
        }
        else if (Input.GetKeyUp(KeyCode.LeftControl))
        {
            isCrouched = false;
            speed = runSpeed;
            controller.height = normalHeight;
            controller.center = yNormalCenterHeight;
            animator.SetBool("isCrouched", false);
        }

        // Crouch Move Forward
        if (isCrouched && Input.GetKey(KeyCode.W))
        {
            animator.SetBool("isCrouchWalking", true);
            controller.Move(move * crouchSpeed * Time.deltaTime);
        }
        else
        {
            animator.SetBool("isCrouchWalking", false);
        }

        // Crouch Move Backward
        if (isCrouched && Input.GetKey(KeyCode.S))
        {
            animator.SetBool("isCrouchBackwards", true);
        }
        else
        {
            animator.SetBool("isCrouchBackwards", false);
        }
        
        // Crouch Walk Strafe Left
        if (isCrouched && Input.GetKey(KeyCode.A))
        {
            animator.SetBool("isCrouchLeft", true);
        }
        else
        {
            animator.SetBool("isCrouchLeft", false);
        }

        // Crouch Walk Strafe Right 
        if (isCrouched && Input.GetKey(KeyCode.D))
        {
            animator.SetBool("isCrouchRight", true);
        }
        else
        {
            animator.SetBool("isCrouchRight", false);
        }

        // Velocity on the y axis is increased with gravity every frame per second 
        velocity.y += gravity * Time.deltaTime;

        // Player is moving down per frame 
        controller.Move(velocity * Time.deltaTime);
    }
}
