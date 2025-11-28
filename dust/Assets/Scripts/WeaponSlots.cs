using UnityEngine;
using UnityEngine.UI;

public class WeaponSlots : MonoBehaviour
{
    [SerializeField] public PlayerWeaponController weaponController;
    [SerializeField] public Image meleeHighlight;
    [SerializeField] public Image meleeIcon;
    [SerializeField] public Image rangedHighlight;
    [SerializeField] public Image rangedIcon;
    [SerializeField] public Image spellHighlight;
    [SerializeField] public Image spellIcon;
    public void SetActive(int index)
    {
        meleeHighlight.enabled = (index == 0);
        rangedHighlight.enabled = (index == 1);
        spellHighlight.enabled = (index == 2);
    }

    public void UpdateIcons()
    {
        for (int i = 0; i < weaponController.weaponSlots.Count; i++)
        {
            GameObject currentWeapon = weaponController.weaponSlots[i];
            Sprite currentSprite;
            Color currentAlpha = Color.white;
            if (currentWeapon != null){
                currentSprite = currentWeapon.GetComponent<SpriteRenderer>().sprite;
                currentAlpha.a = 1f;
            }
            else{
                currentSprite = null;
                currentAlpha.a = 0f;
            }
            switch (i)
            {
                case 0:
                    meleeIcon.sprite = currentSprite;
                    meleeIcon.color = currentAlpha;
                break;
                case 1:
                    rangedIcon.sprite = currentSprite;
                    rangedIcon.color = currentAlpha;
                break;
                case 2:
                    spellIcon.sprite = currentSprite;
                    spellIcon.color = currentAlpha;
                break;
            }
        }
    }

    void Start()
    {
        UpdateIcons();
    }
}
