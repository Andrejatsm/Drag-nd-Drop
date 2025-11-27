using System.Collections.Generic;
using UnityEngine;

public class HanoiRod : MonoBehaviour
{
    [Header("Rod index (0 = left, 1 = middle, 2 = right)")]
    public int rodIndex;

    [Header("Where disks should stack (bottom point)")]
    public Transform stackRoot;

    [Header("Vertical spacing between disks")]
    public float diskHeight = 0.6f;

    private readonly List<HanoiDisk> disks = new List<HanoiDisk>();

    public bool CanPlace(HanoiDisk disk)
    {
        if (disks.Count == 0) return true;
        return disk.size < disks[disks.Count - 1].size;
    }

    public bool IsTopDisk(HanoiDisk disk)
    {
        return disks.Count > 0 && disks[disks.Count - 1] == disk;
    }

    public void PlaceDisk(HanoiDisk disk)
    {
        // Remove if it was in some rod list already
        if (disks.Contains(disk) == false)
            disks.Add(disk);

        disk.currentRod = this;

        UpdateDiskPositions();
    }

    public void RemoveDisk(HanoiDisk disk)
    {
        if (IsTopDisk(disk))
        {
            disks.RemoveAt(disks.Count - 1);
        }
    }

    public void UpdateDiskPositions()
    {
        Vector3 basePos = stackRoot != null ? stackRoot.position : transform.position;

        for (int i = 0; i < disks.Count; i++)
        {
            HanoiDisk d = disks[i];
            Vector3 pos = basePos + Vector3.up * diskHeight * i;
            d.transform.position = new Vector3(pos.x, pos.y, d.transform.position.z);
        }
    }

    public int DiskCount => disks.Count;
}
