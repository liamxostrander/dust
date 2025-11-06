using System.Collections.Generic;
using UnityEngine;

public class ChestSpawner : MonoBehaviour
{
    [Header("Setup")]
    public GameObject chestPrefab;
    public Transform[] spawnPoints;

    [Header("Behavior")]
    public bool onlyOneAtATime = true;
    [Tooltip("If true, remove the spawn point GameObject from the scene after its chest is collected.")]
    public bool destroySpawnPointGO = false;

    private readonly List<Transform> _available = new List<Transform>();
    private GameObject _current;

    void Awake()
    {
        RebuildAvailableList();
    }

    public void RebuildAvailableList()
    {
        _available.Clear();
        if (spawnPoints != null)
        {
            foreach (var t in spawnPoints)
                if (t) _available.Add(t);
        }
    }

    public void ResetAllSpawnPoints(Transform[] newPoints = null)
    {
        if (newPoints != null) spawnPoints = newPoints;
        RebuildAvailableList();
        _current = null;
    }

    public bool HasAvailablePoint => _available.Count > 0;

    public void SpawnRandomChest()
    {
        if (!chestPrefab) return;
        if (!HasAvailablePoint) return;
        if (onlyOneAtATime && _current) return;

        int idx = Random.Range(0, _available.Count);
        Transform p = _available[idx];

        var chest = Instantiate(chestPrefab, p.position, p.rotation);
        _current = chest;

        var origin = chest.GetComponent<ChestOrigin>();
        if (!origin) origin = chest.AddComponent<ChestOrigin>();
        origin.spawner = this;
        origin.originPoint = p;
    }

    // called by Chest when it is opened/consumed
    public void NotifyChestConsumed(GameObject chestGO)
    {
        if (_current == chestGO) _current = null;

        var origin = chestGO ? chestGO.GetComponent<ChestOrigin>() : null;
        if (origin && origin.originPoint)
        {
            _available.Remove(origin.originPoint);

            if (destroySpawnPointGO)
            {
                Destroy(origin.originPoint.gameObject);
            }

            origin.spawner = null;
            origin.originPoint = null;
        }
    }
}
