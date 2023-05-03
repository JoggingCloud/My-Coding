using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Axe : MonoBehaviour
{
    public GameObject mainCamera;

    GameObject itemCurrentlyHolding;

    [Header("Item Pickup:")]
    public float maxPickupDistance = 20;
    public bool isHolding = false;
    public Transform attachPointAxe;

    [Header("Attack Speed:")]
    public float nextTimeToAttack = 1f;
    public float timer = 0f;

    [Header("Weapon Damage:")]
    public int weaponDamage;
    public bool canDealDamage;
    public bool hasDealtDamage;
    [SerializeField] float weaponLength;

    [Header("Attack Audio:")]
    public AudioSource hitAudioSource;
    public AudioClip[] axeHitClips;
    public AudioSource missSwingAudioSource;
    public AudioClip missSwingAudioClip;

    private void Start()
    {
        canDealDamage = false;
        hasDealtDamage = false;
    }

    private void Update()
    {
        if (canDealDamage && !hasDealtDamage)
        {
            RaycastHit hit;
            if (Physics.Raycast(transform.position, transform.up , out hit, weaponLength))
            {
                EnemyMovement enemyhealth = hit.transform.GetComponent<EnemyMovement>();
                //Debug.LogError("Whats goind on");
                if (enemyhealth != null)
                {
                    //Debug.LogError("Whats going on");
                    Debug.Log("I hit the enemy target");
                    enemyhealth.LosingHealth(weaponDamage);
                    hasDealtDamage = true;
                    Debug.Log("I dealt damage" + " " + hasDealtDamage + " " + enemyhealth.enemyHealth);
                    switch (hit.collider.CompareTag("Enemy"))
                    {
                        default:
                            hitAudioSource.PlayOneShot(axeHitClips[Random.Range(0, axeHitClips.Length - 1)]);
                            break;
                    }
                }
            }
        }
    }

    public void PickupAxe()
    {
        // Use a Raycast hit to determine what the player is looking at, if its an item add the item after E has been pressed
        RaycastHit hit;
        if (Physics.Raycast(mainCamera.transform.position, mainCamera.transform.forward, out hit, maxPickupDistance))
        {
            if (!isHolding && hit.transform.tag == "Axe")
            {
                isHolding = true;
                itemCurrentlyHolding = hit.transform.gameObject;

                foreach (var c in hit.transform.GetComponentsInParent<Collider>()) if (c != null) c.enabled = false;
                foreach (var r in hit.transform.GetComponentsInParent<Rigidbody>())
                {
                    if (r != null)
                    {
                        r.isKinematic = true;
                    }
                }
                itemCurrentlyHolding.transform.parent = attachPointAxe.transform;
                itemCurrentlyHolding.transform.localPosition = Vector3.zero;
                itemCurrentlyHolding.transform.localEulerAngles = Vector3.zero;

                isHolding = true;
            }
        }
    }

    public void Drop()
    {
        itemCurrentlyHolding.transform.parent = null;
        foreach (var c in itemCurrentlyHolding.GetComponentsInChildren<Collider>()) if (c != null) c.enabled = true;
        foreach (var r in itemCurrentlyHolding.GetComponentsInChildren<Rigidbody>()) if (r != null) r.isKinematic = false;
        isHolding = false;
        RaycastHit hitDown;
        Physics.Raycast(transform.position, -Vector3.up, out hitDown);

        itemCurrentlyHolding.transform.position = hitDown.point + new Vector3(transform.forward.x, 0, transform.forward.z);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, maxPickupDistance);
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(transform.position, transform.position - transform.up * weaponLength);
    }
}
