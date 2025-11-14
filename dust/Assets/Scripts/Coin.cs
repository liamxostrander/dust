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
        
        // Check if the collider or its parent has the Player tag
        bool isPlayer = other.CompareTag(playerTag);
        if (!isPlayer && other.transform.parent != null)
        {
            isPlayer = other.transform.parent.CompareTag(playerTag);
        }
        
        if (isPlayer)
        {
            CollectCoin();
        }
    }
    
    private void CollectCoin()
    {
        Debug.Log("Coin collected!");
        isCollected = true;
        if (collectSound != null)
        {
            GlobalAudio.SFX.PlayOneShot(collectSound);
        }
        
        // TODO: Add coin value to player's currency system
        // Delay for audio
        Destroy(gameObject);
    }
    
    public int GetCoinValue()
    {
        return coinValue;
    }
}