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

    private List<HanoiDisk> disks = new List<HanoiDisk>();

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
        if (disks.Contains(disk) == false)
            disks.Add(disk);

        disk.currentRod = this;

        // Compute world position for this disk in the stack
        int index = disks.Count - 1;
        Vector3 basePos = stackRoot != null ? stackRoot.position : transform.position;
        Vector3 pos = basePos + Vector3.up * diskHeight * index;

        disk.transform.position = new Vector3(pos.x, pos.y, disk.transform.position.z);
    }

    public void RemoveDisk(HanoiDisk disk)
    {
        if (IsTopDisk(disk))
        {
            disks.RemoveAt(disks.Count - 1);
        }
    }

    public int DiskCount => disks.Count;
}
