using UnityEngine;

public class PlayerInventory : MonoBehaviour
{
    [Header("Weapon Slots")]
    [SerializeField] public GameObject meleeWeapon;
    [SerializeField] public GameObject rangedWeapon;
    [SerializeField] public GameObject magicWeapon;

    void Start()
    {
    }
}
