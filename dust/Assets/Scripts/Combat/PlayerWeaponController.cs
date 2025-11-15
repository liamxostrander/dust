using UnityEngine;
using System.Collections.Generic;

public class PlayerWeaponController : MonoBehaviour
{
    [Header("Available Swords")]
    public List<GameObject> swordPrefabs;
    public Transform swordPivot;         
    public GameObject currentSword;
    

    public void EquipSword(int index)
    {
        if (index < 0 || index >= swordPrefabs.Count) return;

        if (currentSword != null)
            Destroy(currentSword);

        currentSword = Instantiate(swordPrefabs[index], swordPivot);
        currentSword.transform.localPosition = Vector3.zero;
        currentSword.transform.localRotation = Quaternion.identity;
    }
}

