using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerRigidBody : MonoBehaviour
{
    Rigidbody rb;
    Vector3 moveDirection;
    public Animator animator;

    [Header("Reference Scripts:")]
    public Axe axe;
    public PlayerHealth playerHealth;
    public PauseMenu pauseMenu;

    [Header("Main Camera:")]
    public GameObject mainCamera;
    public Transform cameraHolder;
    public float cameraSpeed;
    public Transform target;
    public Vector3 velocity = Vector3.zero;

    [Header("Ground Check:")]
    public float playerHeight;
    public Transform groundPt;
    public bool isGrounded;
    public LayerMask groundLayer;

    [Header("Movement:")]
    float moveForward;
    float moveSide;
    public float currentSpeed;
    public float walkSpeed;
    public bool isWalking = false;
    public float sprintSpeed;
    public bool isRunning = false;
    public float backwardRunSpeed;
    public bool isStrafing = false;

    [Header("Slope Handling:")]
    public float maxSlopeAngle;
    private RaycastHit slopeHit;
    public bool exitingSlope;

    [Header("Jumping:")]
    public bool jumpKeyPressed;
    public float idleJumpForce = 2f;
    public float runJumpForce = 4f;
    public float newGravity = -9.81f;
    public float gravityMultiplier = 2f;
    public bool isJumping = false;

    [Header("Idle Jump Cooldown:")]
    public bool idleJumpReady;
    public float idleJumpCoolDown = 1.7f;
    public float idleJumpCoolDownCurrentTime = 0f;

    [Header("Run Jump Cooldown:")]
    public bool runJumpReady;
    public float runJumpCoolDown = 1.5f;
    public float runJumpCoolDownCurrentTime = 0f;

    [Header("Crouch Movement")]
    public float crouchYScale;
    public float startYScale;
    public bool isCrouched = false;
    public float crouchSpeed;

    [Header("Sliding:")]
    public float slideSpeed;
    public bool isSilding = false;

    [Header("Sliding Cooldown:")]
    public bool slideReady;
    public float slideCoolDown = 1.2f;
    public float slideCoolDownCurrentTime = 0f;

    [Header("Stamina:")]
    public GameObject staminaSlider;
    public Slider staminaBar;
    public float currentStamina;
    public float maxStamina;
    public float negativeStamina;
    private WaitForSeconds regenTick = new WaitForSeconds(0.1f);
    private Coroutine regen;
    public AudioSource staminaAudioSource;

    [Header("Stamina Flash:")]
    public Image staminaFlash;

    [Header("Footstep Audio:")]
    public bool useFootsteps = true;
    public float baseStepSpeed = 0.5f;
    public float crouchStepMultiplier = 1.5f;
    public float sprintStepMultiplier = 0.6f;
    public AudioSource footstepAudioSource = default;
    public AudioClip[] grassClips = default;    
    public AudioClip[] floorClips = default;
    public AudioClip[] carpetClips = default;
    public float footStepTimer = 0;
    public float audioRaycast = 1;
    private float GetCurrentOffset => isCrouched ? baseStepSpeed * crouchStepMultiplier : isRunning ? baseStepSpeed + sprintStepMultiplier : baseStepSpeed;

    [Header("Key Count:")]
    public int greenKeyCount;
    public int redKeyCount;
    public int blueKeyCount;

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

        currentStamina = maxStamina; // Set current stamina to full 
        staminaBar.value = currentStamina; // Set bar to player stamina which is max stamina
    }

    private void Update()
    {
        if (playerHealth.livesRemaining != 0)
        {
            moveForward = Input.GetAxis("Vertical") * currentSpeed;
            moveSide = Input.GetAxis("Horizontal") * currentSpeed;
            jumpKeyPressed = Input.GetButtonDown("Jump");
        }

        rb.mass = 1f;
        isGrounded = Physics.CheckSphere(groundPt.position, 0.5f, groundLayer);

        // Upright Movement 

        // Walking forward 
        if (Input.GetKey(KeyCode.W))
        {
            isWalking = true;
            animator.SetBool("isIdle", false);
            currentSpeed = walkSpeed;
            animator.SetBool("isWalking", true);
            FootstepsHandler();
            //Debug.Log(grassClips + " " + floorClips + " " + carpetClips);
        }
        else if (!Input.GetKey(KeyCode.W))
        {
            isWalking = false;
            animator.SetBool("isWalking", false);
            animator.SetBool("isIdle", true);
        }

        // Running forward
        if (Input.GetKey(KeyCode.W) && Input.GetKey(KeyCode.LeftShift))
        {
            isWalking = false;
            isRunning = true;
            currentSpeed = sprintSpeed;
            animator.SetBool("isBackward", false);
            animator.SetBool("isWalking", false);
            animator.SetBool("isRunning", true);
            FootstepsHandler();
            //Debug.Log(grassClips + " " + floorClips + " " + carpetClips);
        }
        else if (!Input.GetKey(KeyCode.W) && !Input.GetKey(KeyCode.LeftShift))
        {
            isRunning = false;
            currentSpeed = walkSpeed;
            animator.SetBool("isRunning", false);
        }
        else if (Input.GetKey(KeyCode.W) && !Input.GetKey(KeyCode.LeftShift))
        {
            isRunning = false;
            animator.SetBool("isRunning", false);
            animator.SetBool("isWalking", true);
        }
        // Ensure holding shift after letting go of W key does not continue the running forward animation
        else if (Input.GetKey(KeyCode.LeftShift))
        {
            isRunning = false;
            animator.SetBool("isRunning", false);
            animator.SetBool("isIdle", true);
        }

        // Walking Backward Animation
        if (Input.GetKey(KeyCode.S))
        {
            isWalking = true;
            animator.SetBool("isIdle", false);
            currentSpeed = walkSpeed;
            animator.SetBool("isNegative", true);
            FootstepsHandler();
            //Debug.Log(grassClips + " " + floorClips + " " + carpetClips);
        }
        else if (!Input.GetKey(KeyCode.S))
        {
            isWalking = false;
            animator.SetBool("isNegative", false);
            animator.SetBool("isIdle", true);
        }

        // Running Backward Animation 
        if (Input.GetKey(KeyCode.S) && Input.GetKey(KeyCode.LeftShift))
        {
            isWalking = false;
            isRunning = true;
            animator.SetBool("isBackward", true);
            animator.SetBool("isRunning", false);
            animator.SetBool("isNegative", false);
            currentSpeed = backwardRunSpeed;
            FootstepsHandler();
            //Debug.Log(grassClips + " " + floorClips + " " + carpetClips);
        }
        else if (!Input.GetKey(KeyCode.S) && !Input.GetKey(KeyCode.LeftShift))
        {
            isRunning = false;
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
            isWalking = false;
            isStrafing = true;
            currentSpeed = walkSpeed;
            animator.SetBool("isRight", true);
            FootstepsHandler();
            //Debug.Log(grassClips + " " + floorClips + " " + carpetClips);
        }
        else if (!Input.GetKey(KeyCode.D))
        {
            isStrafing = false;
            animator.SetBool("isRight", false);
        }

        // Left Strafe 
        if (Input.GetKey(KeyCode.A))
        {
            isWalking = false;
            isStrafing = true;
            currentSpeed = walkSpeed;
            animator.SetBool("isLeft", true);
            FootstepsHandler();
            //Debug.Log(grassClips + " " + floorClips + " " + carpetClips);
        }
        else if (!Input.GetKey(KeyCode.A))
        {
            //isStrafing = false;
            animator.SetBool("isLeft", false);
        }

        // When sliding is ready
        if (slideCoolDownCurrentTime >= slideCoolDown)
        {
            slideReady = true;
        }
        else
        {
            slideReady = false;
            slideCoolDownCurrentTime += Time.deltaTime;
            slideCoolDownCurrentTime = Mathf.Clamp(slideCoolDownCurrentTime, 0f, slideCoolDown);
        }

        // Sliding 
        if (isRunning && isGrounded && Input.GetKey(KeyCode.C) && slideReady)
        {
            isSilding = true;
            animator.SetBool("isSliding", true);
            slideCoolDownCurrentTime = 0f;
            rb.AddForce(transform.forward * slideSpeed, ForceMode.VelocityChange);
        }
        else
        {
            isSilding = false;
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

        // When idle jump is ready 
        if (idleJumpCoolDownCurrentTime >= idleJumpCoolDown)
        {
            idleJumpReady = true;
        }
        else
        {
            idleJumpReady = false;
            idleJumpCoolDownCurrentTime += Time.deltaTime;
            idleJumpCoolDownCurrentTime = Mathf.Clamp(idleJumpCoolDownCurrentTime, 0f, idleJumpCoolDown);
        }

        //Makes Character Jump in Idle 
        if (isGrounded && jumpKeyPressed && !isWalking && !isRunning && !isStrafing && !isCrouched && !Input.GetKey(KeyCode.W) && idleJumpReady)
        {
            isJumping = true;
            exitingSlope = true;
            StartCoroutine(IdleJump());
            animator.SetBool("isJumped", true);
            idleJumpCoolDownCurrentTime = 0f;
            Debug.Log("Player is idle jumping");
        }
        else if (isGrounded || !jumpKeyPressed)
        {
            isJumping = false;
            animator.SetBool("isJumped", false);
        }
        else if (isRunning && jumpKeyPressed)
        {
            isJumping = false;
        }

        IEnumerator IdleJump()
        {
            yield return new WaitForSeconds(.25f);
            rb.AddForce(transform.up * idleJumpForce * rb.mass, ForceMode.Impulse); 
        }

        // When idle jump is ready 
        if (runJumpCoolDownCurrentTime >= runJumpCoolDown)
        {
            runJumpReady = true;
        }
        else
        {
            runJumpReady = false;
            runJumpCoolDownCurrentTime += Time.deltaTime;
            runJumpCoolDownCurrentTime = Mathf.Clamp(runJumpCoolDownCurrentTime, 0f, runJumpCoolDown);
        }

        // Make character run jump 
        if (isRunning && isGrounded && jumpKeyPressed && runJumpReady)
        {
            rb.AddForce(transform.up * runJumpForce * rb.mass, ForceMode.Impulse);
            animator.SetBool("isJumping", true);
            runJumpCoolDownCurrentTime = 0f;
            Debug.Log("Player is run jumping");
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
            isRunning = false;
            animator.SetBool("isCrouched", true);
            Debug.Log("Im crouched" + " " + currentSpeed);
        }
        else if (Input.GetKeyUp(KeyCode.LeftControl) && !Physics.Raycast(cameraHolder.transform.position, Vector3.up, 2f))
        {
            transform.localScale = new Vector3(transform.localScale.x, startYScale, transform.localScale.z);
            isCrouched = false;
            currentSpeed = walkSpeed;
            animator.SetBool("isCrouched", false);
        }

        // Crouch speed check 
        if (isCrouched && Input.GetKey(KeyCode.W) && Input.GetKey(KeyCode.LeftShift))
        {
            isRunning = false;
            Debug.Log("Im crouched" + " " + currentSpeed);
        }

        // Crouch Move Forward
        if (isCrouched && Input.GetKey(KeyCode.W))
        {
            currentSpeed = crouchSpeed;
            animator.SetBool("isCrouchWalking", true);
            FootstepsHandler();
            //Debug.Log(grassClips + " " + floorClips + " " + carpetClips);
        }
        else if (isCrouched && !Input.GetKey(KeyCode.W))
        {
            animator.SetBool("isCrouchWalking", false);
            animator.SetBool("isCrouched", true);
        }

        // Crouch Move Backward
        if (isCrouched && Input.GetKey(KeyCode.S))
        {
            currentSpeed = crouchSpeed;
            animator.SetBool("isCrouchBackwards", true);
            FootstepsHandler();
            //Debug.Log(grassClips + " " + floorClips + " " + carpetClips);
        }
        else if (isCrouched && !Input.GetKey(KeyCode.S))
        {
            animator.SetBool("isCrouchBackwards", false);
            animator.SetBool("isCrouched", true);
        }

        // Crouch Walk Strafe Left
        if (isCrouched && Input.GetKey(KeyCode.A))
        {
            currentSpeed = crouchSpeed;
            animator.SetBool("isCrouchLeft", true);
            FootstepsHandler();
            //Debug.Log(grassClips + " " + floorClips + " " + carpetClips);
        }
        else if (isCrouched && !Input.GetKey(KeyCode.A))
        {
            animator.SetBool("isCrouchLeft", false);
            animator.SetBool("isCrouched", true);
        }

        // Crouch Walk Strafe Right 
        if (isCrouched && Input.GetKey(KeyCode.D))
        {
            currentSpeed = crouchSpeed;
            animator.SetBool("isCrouchRight", true);
            FootstepsHandler();
            //Debug.Log(grassClips + " " + floorClips + " " + carpetClips);
        }
        else if (isCrouched && !Input.GetKey(KeyCode.D))
        {
            animator.SetBool("isCrouchRight", false);
            animator.SetBool("isCrouched", true);
        }

        // Camera Move when Sliding 
        if (isSilding)
        {
            cameraHolder.localPosition = new Vector3(0.4f, 0.5f, 0.825f);
        }
        else if (!isSilding)
        {
            cameraHolder.localPosition = new Vector3(0, 1.8f, 0.5f);
        }

        // Camera Move when Crouched
        if (isCrouched)
        {
            //cameraHolder.localPosition = Vector3.MoveTowards(target.localPosition, cameraHolder.localPosition, cameraSpeed * Time.deltaTime);
            cameraHolder.localPosition = Vector3.SmoothDamp(target.localPosition, cameraHolder.localPosition, ref velocity,  cameraSpeed * Time.deltaTime);
            //cameraHolder.localPosition = new Vector3(0.25f, 1.2f, 0.5f);
        }

        // Stamina 
        if (isRunning)
        {
            staminaSlider.SetActive(true);
            UseStamina(negativeStamina);
            if (staminaBar.value < 50)
            {
                staminaFlash.color = ChangeColor();
            }

            if (staminaBar.value < 10)
            {
                currentSpeed = walkSpeed;
                animator.SetBool("isRunning", false);
                animator.SetBool("isWalking", true);
            }
            else if (isRunning && staminaBar.value > 15)
            {
                animator.SetBool("isRunning", true);
                currentSpeed = sprintSpeed;
            }
        }
        else if (!isRunning)
        {
            if (staminaBar.value == 100)
            {
                staminaSlider.SetActive(false);
            }
        }
        staminaBar.value = currentStamina;

        if (staminaBar.value < 50 && !staminaAudioSource.isPlaying)
        {
            staminaAudioSource.Play();
            Debug.Log(staminaAudioSource.isPlaying);
        }
        else if (staminaBar.value > 50)
        {
            staminaAudioSource.Stop();
        }
        if (pauseMenu.GameIsPaused)
        {
            staminaAudioSource.Stop();
        }

        // Player Attacking
        axe.timer += Time.deltaTime;
        if (Input.GetButtonDown("Fire1") && axe.isHolding && axe.timer >= axe.nextTimeToAttack)
        {
            if (pauseMenu.GameIsPaused)
            {
                return;
            }
            axe.timer = 0f;
            Debug.Log("Im Clicking to attack");
            animator.SetBool("isAttacking", true);
            /*if (!axe.hasDealtDamage)
            {
                axe.missSwingAudioSource.PlayOneShot(axe.missSwingAudioClip);
            }*/
        }
        else
        {
            animator.SetBool("isAttacking", false);
        }
    }

    private void FootstepsHandler()
    {
        if (!isGrounded)
        {
            return;
        }

        footStepTimer -= Time.deltaTime;

        if (footStepTimer <= 0)
        {
            if(Physics.Raycast(groundPt.transform.position, Vector3.down, out RaycastHit hit, audioRaycast))
            {
                switch (hit.collider.tag)
                {
                    case "Organic":
                        footstepAudioSource.PlayOneShot(grassClips[Random.Range(0, grassClips.Length - 1)]);
                        break;
                    case "Hard floor":
                        footstepAudioSource.PlayOneShot(floorClips[Random.Range(0, floorClips.Length - 1)]);
                        break;
                    case "Carpet":
                        footstepAudioSource.PlayOneShot(carpetClips[Random.Range(0, carpetClips.Length - 1)]);
                        break;
                    default:
                        footstepAudioSource.PlayOneShot(grassClips[Random.Range(0, grassClips.Length - 1)]);
                        break;
                }
            }
            footStepTimer = GetCurrentOffset;
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, audioRaycast);
    }

    void UseStamina(float amount)
    {
        if (currentStamina - amount >= 0)
        {
            currentStamina -= amount * Time.deltaTime;
            staminaBar.value = currentStamina;

            // If we are already regenerating stamina 
            if (regen != null)
            {
                StopCoroutine(regen);
            }
            regen = StartCoroutine(RegenerateStamina());
        }
    }

    private IEnumerator RegenerateStamina()
    {
        yield return new WaitForSeconds(2);
        while (currentStamina < maxStamina)
        {
            currentStamina += maxStamina / 100;
            staminaBar.value = currentStamina;
            staminaFlash.color = Color.white;
            yield return regenTick;
        }
        regen = null;
    }

    public Color ChangeColor()
    {
        return Color.Lerp(a: Color.white, b: Color.red, t: Mathf.Sin(Time.time * 8));
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

    public void Damage()
    {
        axe.canDealDamage = true;
        axe.hasDealtDamage = false;
    }

    public void NoDamage()
    {
        axe.canDealDamage = false;
    }
}
