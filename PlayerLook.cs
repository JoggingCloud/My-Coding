using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Cinemachine;

public class PlayerLook : MonoBehaviour
{
    // Input reference to the player the camera is connected to
    public Transform playerBody;
    public PlayerRigidBody player;

    // How fast the player can look around the fov
    [Header("Camera Sensitivity:")]
    public float mouseSensitivity;
    private float defaultValue = 250;
    public Slider slider;
    public TextMeshProUGUI text;

    // How far/close the player can see in front of them
    [Header("Camera FOV:")]
    public CinemachineVirtualCamera cinemachineVirtualCamera;
    private float defaultView = 60;
    public Slider fovSlider;
    public TextMeshProUGUI fovText;

    // Float numbers of rotation along the X axis 
    float xRotation = 0f;

    // Dot Canvas 
    [Header("Dot Canvas:")]
    public GameObject dotCanvas;
    [SerializeField] float maxDistance;
    public LayerMask itemLayer;

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        // Mouse Values. In pause screen the player can adjust using the sliders
        mouseSensitivity = defaultValue;
        slider.value = mouseSensitivity;
        text.text = mouseSensitivity.ToString();

        // Camera FOV values. In pause screen the player can adjust using the sliders
        cinemachineVirtualCamera.m_Lens.FieldOfView = defaultView;
        fovSlider.value = cinemachineVirtualCamera.m_Lens.FieldOfView;
        fovText.text = cinemachineVirtualCamera.m_Lens.FieldOfView.ToString();

        // Set default clipping plane at start of game
        cinemachineVirtualCamera.m_Lens.NearClipPlane = 0.001f;

        dotCanvas.SetActive(false);
    }


    private void Update()
    {
        if (player.isCrouched)
        {
            cinemachineVirtualCamera.m_Lens.NearClipPlane = 0.4f;
        }
        else
        {
            cinemachineVirtualCamera.m_Lens.NearClipPlane = 0.001f;
        }

        // Dot Canvas 
        RaycastHit hit;
        int layerMask = itemLayer;
        if (Physics.Raycast(transform.position, transform.forward, out hit, maxDistance, itemLayer))
        {
            if (hit.collider)
            {
                Debug.Log("I hit the Item layer");
                dotCanvas.SetActive(true);
            }
        }
        else
        {
            dotCanvas.SetActive(false);
        }
    }

    void LateUpdate()
    {
        // Accesses buttons, keys, and inputs from Input Manager in the edit tab and scroll down to Project Manager

        // Accesses movement of mouse along the X axis from 0-100float numbers in real time per frame
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;

        // Accesses movement of mouse along the Y axis from 0-100float numbers in real time per frame 
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        // Rotate camera on y axis 
        xRotation -= mouseY;

        // 
        transform.localRotation = Quaternion.Euler(xRotation, 0, 0);

        // Rotate camera on x axis
        playerBody.Rotate(Vector3.up * mouseX);

        // Insures that player can't over rotate and look behind themself 
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);
    }

    public void MouseSenseAdjust(float newSpeed)
    {
        mouseSensitivity = newSpeed;
        text.text = mouseSensitivity.ToString();
        Debug.Log("Mouse sensitivity is adjusted to" + " " + newSpeed);
    }

    public void FovAdjust(float newSize)
    {
        cinemachineVirtualCamera.m_Lens.FieldOfView = newSize;
        fovText.text = cinemachineVirtualCamera.m_Lens.FieldOfView.ToString();
        Debug.Log("Field of View is adjusted to" + " " + newSize);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawLine(transform.position, transform.position + transform.forward * maxDistance);
    }
}
