using System;
using UnityEngine;

public class HanoiManager : MonoBehaviour
{
    [Header("Rods (left to right)")]
    public HanoiRod[] rods;

    [Header("Total disks in puzzle")]
    public int totalDisks = 5;

    private static HanoiManager _instance;
    public static HanoiManager Instance => _instance;

    // >>> NEW: fire this when the puzzle is solved
    public event Action OnSolved;

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }
        _instance = this;
    }

    void Start()
    {
        HanoiDisk[] disks = FindObjectsOfType<HanoiDisk>();

        Array.Sort(disks, (a, b) => a.size.CompareTo(b.size));

        if (rods.Length == 0)
        {
            Debug.LogError("HanoiManager: rods array is empty!");
            return;
        }

        HanoiRod startRod = rods[0];

        for (int i = disks.Length - 1; i >= 0; i--)
        {
            startRod.PlaceDisk(disks[i]);
        }

        Debug.Log($"HanoiManager: found {disks.Length} disks in scene");
    }

    public bool TryMoveDisk(HanoiDisk disk, HanoiRod targetRod)
    {
        if (disk == null || targetRod == null) return false;

        HanoiRod fromRod = disk.currentRod;
        if (fromRod == null)
        {
            Debug.LogWarning("HanoiManager.TryMoveDisk: disk has no currentRod");
            return false;
        }

        if (!fromRod.IsTopDisk(disk))
            return false;

        if (!targetRod.CanPlace(disk))
            return false;

        fromRod.RemoveDisk(disk);
        targetRod.PlaceDisk(disk);

        CheckWin();
        return true;
    }

    void CheckWin()
    {
        if (rods.Length == 0) return;

        HanoiRod lastRod = rods[rods.Length - 1];

        if (lastRod.DiskCount == totalDisks)
        {
            Debug.Log("🎉 Tower of Hanoi solved!");
            OnSolved?.Invoke();          // <<< notify listeners
        }
    }
}
