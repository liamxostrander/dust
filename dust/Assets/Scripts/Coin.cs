using UnityEngine;

public class Coin : MonoBehaviour
{
    [Header("Collection Settings")]
    [SerializeField] private int coinValue = 1;
    [SerializeField] private string playerTag = "Player";
    
    [Header("Audio")]
    [SerializeField] private AudioClip collectSound;
    
    private bool isCollected = false;
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (isCollected)
            return;
        
        bool isPlayer = other.CompareTag(playerTag);
        if (!isPlayer && other.transform.parent != null)
        {
            isPlayer = other.transform.parent.CompareTag(playerTag);
        }
        
        if (isPlayer)
        {
            PlayerCurrency currency = 
                other.GetComponentInParent<PlayerCurrency>() ??
                other.GetComponent<PlayerCurrency>();

            CollectCoin(currency);
        }
    }
    
    private void CollectCoin(PlayerCurrency currency)
    {
        isCollected = true;

        if (collectSound != null && GlobalAudio.SFX != null)
        {
            GlobalAudio.SFX.PlayOneShot(collectSound);
        }

        if (currency != null)
        {
            currency.AddCoins(coinValue);
        }
        else
        {
            Debug.LogWarning("Coin collected, but no PlayerCurrency found on player!");
        }

        Destroy(gameObject);
    }
    
    public int GetCoinValue()
    {
        return coinValue;
    }
}
