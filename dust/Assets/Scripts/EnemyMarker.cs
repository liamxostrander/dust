using UnityEngine;

public class EnemyMarker : MonoBehaviour
{
    public event System.Action<EnemyMarker> OnEnemyDied;

    public int waveId = -1;

    public void NotifyDeath()
    {
        OnEnemyDied?.Invoke(this);
    }

    private void OnDestroy()
    {
        OnEnemyDied?.Invoke(this);
    }
}
