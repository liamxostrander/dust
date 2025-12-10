using Microsoft.Unity.VisualStudio.Editor;
using UnityEngine;


public class WeaponStats : MonoBehaviour
{

    [Header("UI")]
    public Sprite weaponIcon;

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
    public bool isFireballStaff = false;
    public bool isIceStaff = false;
    public GameObject projectilePrefab;
    public float projectileSpeed = 12f;
    public Transform projectileSpawnPoint;
    
    [Header("Orb Staff Stats")]
    public GameObject orbPrefab;
    public float orbHoverHeight = 1.2f;
    public float orbDetectRadius = 6f;
    public float orbSpeed = 12f;
    public float orbExplosionRadius = 2f;
    public float orbExplosionDamage = 30f;
    public GameObject orbExplosionFX;
    public AudioClip orbExplosionSFX;

    [Header ("Fireball Staff Stats")]
    public GameObject fireballPrefab;
    public float fireballSpeed = 12f;
    public float fireballCooldown = 0.25f;

    [Header ("Ice Staff Stats")]
    public AnimationClip iceSpellAnimation;
    public GameObject iceSpellFX;
    public float iceDamage = 5f;
    public float iceRange = 5f;
    public float iceRadius = 1.2f;
    public float iceSlowAmount = 0.4f;
    public float iceSlowDuration = 5f;
}

