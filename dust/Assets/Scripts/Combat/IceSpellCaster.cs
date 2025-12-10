using UnityEngine;

public class IceSpellCaster : MonoBehaviour
{
    public Transform spawnPoint;
    private WeaponStats stats;
    public void CastIceSpell(Vector2 dir)
    {
        if (stats == null)
            stats = GetComponent<WeaponStats>();

        if (spawnPoint == null)
        {
            Debug.LogError("IceSpellCaster: spawnPoint is NULL");
            return;
        }

        Vector3 spawnPos = spawnPoint.position;
        float offset = 5f;
        spawnPos += new Vector3(dir.x * offset, 0f, 0f);

        // Spawn the FX
        GameObject fx = Instantiate(stats.iceSpellFX, spawnPos, Quaternion.identity);

        // Inject stats directly into the freeze zone
        FreezeZone fz = fx.GetComponentInChildren<FreezeZone>();
        if (fz != null)
            fz.Initialize(stats);
    }

}
