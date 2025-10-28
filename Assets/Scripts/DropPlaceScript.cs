using UnityEngine;
using UnityEngine.EventSystems;

public class DropPlaceScript : MonoBehaviour, IDropHandler
{
    private float placeZRot, vehicleZRot, rotDiff;
    private Vector3 placeSiz, vehicleSiz;
    private float xSizeDiff, ySizeDiff;
    public ObjectScript objScript;

    void Start()
    {
        if (objScript == null)
            objScript = Object.FindFirstObjectByType<ObjectScript>();
    }

    public void OnDrop(PointerEventData eventData)
    {
        if (eventData.pointerDrag == null) return;

        var dragGO = eventData.pointerDrag;
        var dragRT = dragGO.GetComponent<RectTransform>();
        var placeRT = GetComponent<RectTransform>();

        // Wrong placeholder: reset immediately
        if (!dragGO.tag.Equals(tag))
        {
            WrongPlaceFeedback();
            ResetToStart(dragGO);
            return;
        }

        // Tag matches – check rotation/scale tolerance
        placeZRot = placeRT.eulerAngles.z;
        vehicleZRot = dragRT.eulerAngles.z;
        rotDiff = Mathf.Abs(placeZRot - vehicleZRot);

        placeSiz = placeRT.localScale;
        vehicleSiz = dragRT.localScale;
        xSizeDiff = Mathf.Abs(placeSiz.x - vehicleSiz.x);
        ySizeDiff = Mathf.Abs(placeSiz.y - vehicleSiz.y);

        bool fits = (rotDiff <= 5 || (rotDiff >= 355 && rotDiff <= 360)) &&
                    (xSizeDiff <= 0.05f && ySizeDiff <= 0.05f);

        if (fits)
        {
            objScript.rightPlace = true;

            // Preserve world size
            Vector3 worldScaleBefore = dragRT.lossyScale;
            dragRT.SetParent(placeRT, true);
            dragRT.position = placeRT.position;
            dragRT.rotation = placeRT.rotation;

            Vector3 parentLossy = placeRT.lossyScale;
            dragRT.localScale = new Vector3(
                Mathf.Approximately(parentLossy.x, 0f) ? 1f : worldScaleBefore.x / parentLossy.x,
                Mathf.Approximately(parentLossy.y, 0f) ? 1f : worldScaleBefore.y / parentLossy.y,
                Mathf.Approximately(parentLossy.z, 0f) ? 1f : worldScaleBefore.z / parentLossy.z
            );

            // Disable drag
            var drag = dragGO.GetComponent<DragAndDropScript>();
            if (drag != null) { drag.isPlaced = true; drag.enabled = false; }

            ObjectScript.drag = false;
            ObjectScript.lastDragged = null;

            var cg = dragGO.GetComponent<CanvasGroup>();
            if (cg != null) { cg.alpha = 1f; cg.blocksRaycasts = false; }

            foreach (var c in dragGO.GetComponentsInChildren<Collider>(true)) c.enabled = false;
            foreach (var c2 in dragGO.GetComponentsInChildren<Collider2D>(true)) c2.enabled = false;

            // Play tag-specific SFX
            for (int i = 0; i < objScript.vehicles.Length; i++)
            {
                if (objScript.vehicles[i] == dragGO)
                {
                    objScript.effects.PlayOneShot(objScript.audioCli[i + 2]); // matches SpawnManager audio mapping
                    break;
                }
            }

            objScript.CarPlaced();
            return;
        }

        // Tag matches but not aligned yet
        objScript.rightPlace = false;
    }

    private void WrongPlaceFeedback()
    {
        objScript.rightPlace = false;
        if (objScript.effects != null && objScript.audioCli != null && objScript.audioCli.Length > 1)
            objScript.effects.PlayOneShot(objScript.audioCli[1]);
    }

    private void ResetToStart(GameObject dragged)
    {
        int idx = System.Array.IndexOf(objScript.vehicles, dragged);
        if (idx >= 0 && idx < objScript.startCoordinates.Length)
        {
            dragged.GetComponent<RectTransform>().localPosition = objScript.startCoordinates[idx];
            return;
        }

        Debug.LogWarning("DropPlaceScript: Could not reset position (vehicle not found in registry).");
    }
}
