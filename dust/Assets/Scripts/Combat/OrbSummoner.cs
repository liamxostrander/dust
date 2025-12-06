using System;
using UnityEngine;

public class OrbSummoner : MonoBehaviour
{
    private WeaponStats stats;
    private GameObject currentOrb;

    public void SummonOrb()
    {
        if (stats == null)
            stats = GetComponent<WeaponStats>();

        if (stats == null)
        {
            Debug.LogError("OrbSummoner: WeaponStats missing! Add WeaponStats to this object.", this);
        }
        else if (stats.orbPrefab == null)
        {
            Debug.LogError("OrbSummoner: orbPrefab is not assigned in WeaponStats!", this);
        }

        if (currentOrb != null) return; // Only one orb at a time

        Debug.Log(stats);
        Vector3 pos = transform.root.position + Vector3.up * stats.orbHoverHeight;
        currentOrb = Instantiate(stats.orbPrefab, pos, Quaternion.identity);

        OrbBehaviour orb = currentOrb.GetComponent<OrbBehaviour>();
        orb.Initialize(stats, transform.root.gameObject);
    }
}
