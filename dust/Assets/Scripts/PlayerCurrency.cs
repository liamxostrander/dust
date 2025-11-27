using UnityEngine;
using System;

[DisallowMultipleComponent]
public class PlayerCurrency : MonoBehaviour
{
    [SerializeField] private int startingCoins = 0;
    [SerializeField] public CoinCounter coinCounter;


    public int Coins { get; private set; }

    // notify UI / other systems when coins change
    public event Action<int> OnCoinsChanged;

    private void Awake()
    {
        Coins = startingCoins;
        coinCounter.setValue(startingCoins);
        OnCoinsChanged?.Invoke(Coins);
    }

    public void AddCoins(int amount)
    {
        if (amount <= 0) return;
        Coins += amount;
        coinCounter.setValue(Coins);
        OnCoinsChanged?.Invoke(Coins);
    }

    public bool TrySpendCoins(int amount)
    {
        if (amount <= 0) return true;

        if (Coins < amount)
            return false;

        Coins -= amount;
        coinCounter.setValue(Coins);
        OnCoinsChanged?.Invoke(Coins);
        return true;
    }

    public void SetCoins(int value)
    {
        Coins = Mathf.Max(0, value);
        OnCoinsChanged?.Invoke(Coins);
    }
}
