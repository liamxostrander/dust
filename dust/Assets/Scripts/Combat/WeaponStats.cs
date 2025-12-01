using UnityEngine;


public class WeaponStats : MonoBehaviour
{
    [Header("Animations")]
    public AnimationClip slashAnimation;
    public AnimationClip dashSliceAnimation;

    [Header("SFX")]
    public AudioClip slashSound; 
    public AudioClip dashSliceSound; 

    [Header("General Stats")]
    public float damage = 25f;
    public float knockback = 5f;
    public float hitstopDuration = 0.08f;

    [Header("Weapon Type")]
    public bool isOrbStaff = false;
    public bool isRanged = false;
    public GameObject projectilePrefab;
    public float projectileSpeed = 12f;
    public Transform projectileSpawnPoint;
    
    public GameObject orbPrefab;
    public float orbHoverHeight = 1.2f;
    public float orbDetectRadius = 6f;
    public float orbSpeed = 12f;
    public float orbExplosionRadius = 2f;
    public float orbExplosionDamage = 30f;

}

