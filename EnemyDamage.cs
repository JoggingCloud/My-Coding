using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyDamage : MonoBehaviour
{
    bool canDealDamage;
    bool hasDealtDamage;

    [SerializeField] float weaponLength;
    public int weaponDamage = 10;

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
            int layerMask = 1 << 6;
            if(Physics.Raycast(transform.position, -transform.up, out hit, weaponLength, layerMask))
            {
                if(hit.transform.TryGetComponent(out PlayerHealth currentHealth))
                {
                    currentHealth.TakingDamage(weaponDamage);
                    hasDealtDamage = true;
                }

            }
        }
    }

    public void StartDealDamage()
    {
        canDealDamage = true;
        hasDealtDamage = false;
    }

    public void EndDealDamage()
    {
        canDealDamage = false;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(transform.position, transform.position - transform.up * weaponLength);
    }
}
