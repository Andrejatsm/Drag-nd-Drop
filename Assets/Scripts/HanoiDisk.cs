using UnityEngine;

public class HanoiDisk : MonoBehaviour
{
    [Header("Logical size (1 = smallest, bigger number = larger disk)")]
    public int size = 1;

    [HideInInspector] public HanoiRod currentRod;
}
