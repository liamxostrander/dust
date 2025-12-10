using UnityEngine;

public class PlayerMana : MonoBehaviour
{
    [Header("Mana Settings")]
    public float maxMana = 100f;
    public float regenRate = 5f; // mana per second
    public float currentMana;

    [Header("UI")]
    public StatusBar manaBar;    // drag the ManaBar object in Inspector

    void Start()
    {
        currentMana = maxMana;
        if (manaBar != null)
            manaBar.setMaxValue(maxMana);
    }

    void Update()
    {
        RegenerateMana();
    }

    public bool TrySpendMana(float amount)
    {
        if (currentMana < amount)
            return false; // not enough mana

        currentMana -= amount;

        if (manaBar != null)
            manaBar.setValue(currentMana);

        return true;
    }

    private void RegenerateMana()
    {
        if (currentMana < maxMana)
        {
            currentMana += regenRate * Time.deltaTime;
            currentMana = Mathf.Clamp(currentMana, 0, maxMana);

            if (manaBar != null)
                manaBar.setValue(currentMana);
        }
    }
}
