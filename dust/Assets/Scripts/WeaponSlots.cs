using UnityEngine;
using UnityEngine.UI;

public class WeaponSlots : MonoBehaviour
{
    [SerializeField] public Image meleeIcon;
    [SerializeField] public Image rangedIcon;
    [SerializeField] public Image spellIcon;

    void Start()
    {
        meleeIcon.enabled = true;
        rangedIcon.enabled = false;
        spellIcon.enabled = false;
    }
    public void SetMeleeActive()
    {
        meleeIcon.enabled = true;
        rangedIcon.enabled = false;
        spellIcon.enabled = false;
    }
    public void SetRangedActive()
    {
        meleeIcon.enabled = false;
        rangedIcon.enabled = true;
        spellIcon.enabled = false;
    }
    public void SetSpellActive()
    {
        meleeIcon.enabled = false;
        rangedIcon.enabled = false;
        spellIcon.enabled = true;
    }
}
