using UnityEngine;


public class WeaponStats : MonoBehaviour
{
    public AnimationClip slashAnimation;
    public AudioClip slashSound; 
    public AnimationClip dashSliceAnimation;
    public AudioClip dashSliceSound; 
    public float damage = 25f;
    public float knockback = 5f;
    public float hitstopDuration = 0.08f;
    public bool isRanged = false;
    public GameObject projectilePrefab;
    public float projectileSpeed = 12f;
    public Transform projectileSpawnPoint;
}

