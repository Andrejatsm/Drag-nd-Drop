using UnityEngine;

[RequireComponent(typeof(HanoiDisk))]
public class HanoiDiskDrag : MonoBehaviour
{
    private HanoiDisk disk;
    private Camera cam;
    private HanoiManager manager;

    private bool isDragging = false;
    private Vector3 dragOffset;
    private Vector3 originalPos;
    private float zPos;

    void Awake()
    {
        disk = GetComponent<HanoiDisk>();
        cam = Camera.main;
    }

    void Start()
    {
        // Make sure we catch the manager even if its Awake ran after ours
        if (manager == null)
            manager = HanoiManager.Instance ?? FindFirstObjectByType<HanoiManager>();
    }

    void OnMouseDown()
    {
        // Safety net: re-grab the manager if for some reason it’s still null
        if (manager == null)
            manager = HanoiManager.Instance ?? FindFirstObjectByType<HanoiManager>();

        if (manager == null || disk == null || disk.currentRod == null)
        {
            Debug.Log("HanoiDiskDrag: cannot start drag, manager or currentRod is null", this);
            return;
        }

        // Only allow dragging if this is the top disk on its rod
        if (!disk.currentRod.IsTopDisk(disk))
        {
            Debug.Log("HanoiDiskDrag: clicked disk is not top disk, ignoring", this);
            return;
        }

        isDragging = true;
        originalPos = transform.position;
        zPos = transform.position.z;

        Vector3 mouseWorld = cam.ScreenToWorldPoint(Input.mousePosition);
        dragOffset = transform.position - new Vector3(mouseWorld.x, mouseWorld.y, zPos);

        Debug.Log($"HanoiDiskDrag: start dragging disk {disk.size}", this);
    }

    void OnMouseDrag()
    {
        if (!isDragging) return;

        Vector3 mouseWorld = cam.ScreenToWorldPoint(Input.mousePosition);
        Vector3 newPos = new Vector3(mouseWorld.x, mouseWorld.y, zPos) + dragOffset;
        transform.position = newPos;
    }

    void OnMouseUp()
    {
        if (!isDragging) return;
        isDragging = false;

        if (manager == null)
        {
            transform.position = originalPos;
            return;
        }

        // World position of mouse
        Vector3 mouseWorld = cam.ScreenToWorldPoint(Input.mousePosition);
        Vector2 point = new Vector2(mouseWorld.x, mouseWorld.y);

        // Check ALL colliders under the mouse and look for a rod
        Collider2D[] hits = Physics2D.OverlapPointAll(point);

        HanoiRod targetRod = null;
        foreach (var h in hits)
        {
            var rod = h.GetComponent<HanoiRod>();
            if (rod != null)
            {
                targetRod = rod;
                break;
            }
        }

        if (targetRod != null)
        {
            // Ask manager to perform legal move
            bool moved = manager.TryMoveDisk(disk, targetRod);
            if (!moved)
            {
                // Illegal move -> snap back
                transform.position = originalPos;
            }
        }
        else
        {
            // Not dropped on any rod -> snap back
            transform.position = originalPos;
        }
    }
}
