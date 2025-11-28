using UnityEngine;
using System.Collections.Generic;

public class PlayerWeaponController : MonoBehaviour
{
    [Header("Available Swords")]
    public List<GameObject> weaponSlots;
    public GameObject currentWeapon;
    public Transform pivot;
    public int currentWeaponIdx;

    [Header("UI")]
    [SerializeField] public WeaponSlots weaponSlotsUI;
    public void EquipWeapon(int index)
    {
        if (index < 0 || index >= weaponSlots.Count) return;
        if (weaponSlots[index] == null) return;
        currentWeaponIdx = index;
        weaponSlotsUI.SetActive(index);

        if (currentWeapon != null)
            Destroy(currentWeapon);

        currentWeapon = Instantiate(weaponSlots[index], pivot);
        currentWeapon.transform.localPosition = Vector3.zero;
        currentWeapon.transform.localRotation = Quaternion.identity;
    }

    void CheckWeaponEquipped()
    {
        if (Input.GetKey(KeyCode.Alpha1))
        {
            EquipWeapon(0);
        }
        else if (Input.GetKey(KeyCode.Alpha2))
        {
            EquipWeapon(1);
        }
        else if (Input.GetKey(KeyCode.Alpha3))
        {
            EquipWeapon(2);
        }
    }

    void Start()
    {
        EquipWeapon(0);
    }
    void Update()
    {
        CheckWeaponEquipped();
    }

    public void AssignPurchasedWeapon(Shopkeeper.ShopItem item)
    {
        int slotIndex = -1;

        switch (item.category)
        {
            case Shopkeeper.WeaponCategory.Melee:
                slotIndex = 0;
                break;
            case Shopkeeper.WeaponCategory.Ranged:
                slotIndex = 1;
                break;
            case Shopkeeper.WeaponCategory.Magic:
                slotIndex = 2;
                break;
        }

        if (slotIndex == -1)
        {
            Debug.LogWarning("Invalid category on purchased item.");
            return;
        }

        if (slotIndex < weaponSlots.Count)
        {
            weaponSlots[slotIndex] = item.weaponPrefab;
        }
        else
        {
            Debug.LogWarning("WeaponSlots list too small.");
            return;
        }

        EquipWeapon(slotIndex);

        if (weaponSlotsUI != null)
            weaponSlotsUI.UpdateIcons();
    }

}

