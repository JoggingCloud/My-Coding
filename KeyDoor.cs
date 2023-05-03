using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KeyDoor : MonoBehaviour
{
    public PlayerRigidBody playerKey;
    public Animator anim;

    [Header("Door checks:")]
    public bool canOpen = false;
    public bool hasOpened = false;
    public GameObject rightButtonUI, leftButtonUI;

    [Header("Audio:")]
    public AudioSource openDoorAudio;
    public AudioSource closeDoorAudio;

    private void Update()
    {
        if (canOpen)
        {
            if (Input.GetKeyDown(KeyCode.E))
            {
                anim.SetTrigger("OpenDoor");
                openDoorAudio.Play();
                hasOpened = true;
            }
        }
        else
        {
            rightButtonUI.SetActive(false); leftButtonUI.SetActive(false);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player") && playerKey.redKeyCount == 1)
        {
            rightButtonUI.SetActive(true); leftButtonUI.SetActive(true);
            canOpen = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.CompareTag("Player") && hasOpened)
        {
            canOpen = false;
            anim.SetTrigger("CloseDoor");
            closeDoorAudio.Play();
            hasOpened = false;
            rightButtonUI.SetActive(false); leftButtonUI.SetActive(false);
        }

        if (other.gameObject.CompareTag("Player"))
        {
            rightButtonUI.SetActive(false); leftButtonUI.SetActive(false);
        }
    }
}