using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(HanoiDisk))]
public class HanoiDiskDrag : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public Camera cam;
    public HanoiManager manager;

    Vector3 startPos;
    int startRod;
    HanoiDisk disk;

    void Awake()
    {
        disk = GetComponent<HanoiDisk>();
        if (cam == null)
            cam = Camera.main;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        startPos = transform.position;
        startRod = disk.currentRodIndex;
    }

    public void OnDrag(PointerEventData eventData)
    {
        Vector3 world = cam.ScreenToWorldPoint(new Vector3(eventData.position.x, eventData.position.y, cam.nearClipPlane));
        world.z = transform.position.z;
        transform.position = world;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        // Raycast to see which rod we dropped on
        Ray ray = cam.ScreenPointToRay(eventData.position);
        RaycastHit2D hit = Physics2D.GetRayIntersection(ray);

        if (hit.collider != null)
        {
            HanoiRod rod = hit.collider.GetComponent<HanoiRod>();
            if (rod != null)
            {
                manager.MoveDisk(disk, rod.rodIndex);
                return;
            }
        }

        // No valid rod hit => snap back
        manager.SnapDiskToRod(disk, startRod);
    }
}
