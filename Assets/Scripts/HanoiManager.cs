using System;
using System.Linq;
using UnityEngine;

public class HanoiManager : MonoBehaviour
{
    [Header("Rods (left to right)")]
    public HanoiRod[] rods;         // size 3

    [Header("Total disks in puzzle")]
    public int totalDisks = 5;

    void Start()
    {
        // Collect all disks in the scene
        HanoiDisk[] disks = FindObjectsOfType<HanoiDisk>();

        // Sort by size: smallest on top
        Array.Sort(disks, (a, b) => a.size.CompareTo(b.size));

        // Put all disks onto rod 0 (left) from bottom (largest) to top (smallest)
        HanoiRod startRod = rods[0];

        // Clear any existing list then place from largest to smallest
        for (int i = disks.Length - 1; i >= 0; i--)
        {
            startRod.PlaceDisk(disks[i]);
        }
    }

    public bool TryMoveDisk(HanoiDisk disk, HanoiRod targetRod)
    {
        if (disk.currentRod == null)
            return false;

        HanoiRod fromRod = disk.currentRod;

        // Only the top disk on a rod can be moved
        if (!fromRod.IsTopDisk(disk))
            return false;

        // Rule: cannot place bigger disk on smaller one
        if (!targetRod.CanPlace(disk))
            return false;

        fromRod.RemoveDisk(disk);
        targetRod.PlaceDisk(disk);

        CheckWin(targetRod);
        return true;
    }

    void CheckWin(HanoiRod lastRod)
    {
        // Win if all disks are on the last rod (rodIndex == rods.Length - 1)
        if (lastRod.rodIndex == rods.Length - 1 && lastRod.DiskCount == totalDisks)
        {
            Debug.Log("🎉 Hanoi solved!");
            // Here you can show a win UI
        }
    }
}
