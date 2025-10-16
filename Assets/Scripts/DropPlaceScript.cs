using UnityEngine;
using UnityEngine.EventSystems;

public class DropPlaceScript : MonoBehaviour, IDropHandler
{
    private float placeZRot, vehicleZRot, rotDiff;
    private Vector3 placeSiz, vehicleSiz;
    private float xSizeDiff, ySizeDiff;
    public ObjectScript objScript;

    public void OnDrop(PointerEventData eventData)
    {
        if ((eventData.pointerDrag == null) ||
            !(Input.GetMouseButtonUp(0) && !Input.GetMouseButton(1) && !Input.GetMouseButton(2)))
            return;

        var dragGO = eventData.pointerDrag;
        var dragRT = dragGO.GetComponent<RectTransform>();
        var placeRT = GetComponent<RectTransform>();

        // Wrong placeholder: reset immediately to spawn
        if (!dragGO.tag.Equals(tag))
        {
            WrongPlaceFeedback();
            ResetToStart(dragGO);
            return;
        }

        // Tag matches – check rotation/scale tolerance
        placeZRot = dragRT.transform.eulerAngles.z;
        vehicleZRot = placeRT.transform.eulerAngles.z;
        rotDiff = Mathf.Abs(placeZRot - vehicleZRot);

        placeSiz = dragRT.localScale;
        vehicleSiz = placeRT.localScale;
        xSizeDiff = Mathf.Abs(placeSiz.x - vehicleSiz.x);
        ySizeDiff = Mathf.Abs(placeSiz.y - vehicleSiz.y);

        bool fits = (rotDiff <= 5 || (rotDiff >= 355 && rotDiff <= 360)) &&
                    (xSizeDiff <= 0.05f && ySizeDiff <= 0.05f);

        if (fits)
        {
            objScript.rightPlace = true;

            // Preserve the car's visual size (world scale) when reparenting
            Vector3 worldScaleBefore = dragRT.lossyScale;

            dragRT.SetParent(placeRT, worldPositionStays: true);
            dragRT.position = placeRT.position;
            dragRT.rotation = placeRT.rotation;

            // Recalculate local scale to preserve visual size
            Vector3 parentLossy = placeRT.lossyScale;
            float sx = Mathf.Approximately(parentLossy.x, 0f) ? 1f : worldScaleBefore.x / parentLossy.x;
            float sy = Mathf.Approximately(parentLossy.y, 0f) ? 1f : worldScaleBefore.y / parentLossy.y;
            float sz = Mathf.Approximately(parentLossy.z, 0f) ? 1f : worldScaleBefore.z / parentLossy.z;
            dragRT.localScale = new Vector3(sx, sy, sz);

            // Disable interactions
            var drag = dragGO.GetComponent<DragAndDropScript>();
            if (drag != null)
            {
                drag.isPlaced = true;
                drag.enabled = false;
            }
            var transformCtrl = dragGO.GetComponent<TransformationScript>();
            if (transformCtrl != null) transformCtrl.enabled = false;

            ObjectScript.drag = false;
            ObjectScript.lastDragged = null;

            var cg = dragGO.GetComponent<CanvasGroup>();
            if (cg != null)
            {
                cg.alpha = 1f;
                cg.blocksRaycasts = false;
            }

            // Disable all colliders
            foreach (var c in dragGO.GetComponentsInChildren<Collider>(true)) c.enabled = false;
            foreach (var c2 in dragGO.GetComponentsInChildren<Collider2D>(true)) c2.enabled = false;

            // SFX by tag
            switch (dragGO.tag)
            {
                case "Garbage": objScript.effects.PlayOneShot(objScript.audioCli[2]); break;
                case "Ambulance": objScript.effects.PlayOneShot(objScript.audioCli[3]); break;
                case "Fire": objScript.effects.PlayOneShot(objScript.audioCli[4]); break;
                case "Buss": objScript.effects.PlayOneShot(objScript.audioCli[5]); break;
                case "e46": objScript.effects.PlayOneShot(objScript.audioCli[6]); break;
                case "e61": objScript.effects.PlayOneShot(objScript.audioCli[7]); break;
                case "b2": objScript.effects.PlayOneShot(objScript.audioCli[8]); break;
                case "Cement": objScript.effects.PlayOneShot(objScript.audioCli[9]); break;
                case "Eskovator": objScript.effects.PlayOneShot(objScript.audioCli[10]); break;
                case "Police": objScript.effects.PlayOneShot(objScript.audioCli[11]); break;
                case "Tracktor": objScript.effects.PlayOneShot(objScript.audioCli[12]); break;
                case "Tracktor2": objScript.effects.PlayOneShot(objScript.audioCli[13]); break;
                default: Debug.Log("Unknown tag detected"); break;
            }

            // Count a successful placement towards win condition
            if (objScript != null)
            {
                objScript.CarPlaced();
            }

            return;
        }

        // Tag matches but not aligned yet: keep where dropped so the player can adjust
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
            objScript.vehicles[idx].GetComponent<RectTransform>().localPosition = objScript.startCoordinates[idx];
            return;
        }

        Debug.LogWarning("DropPlaceScript: Could not reset position (vehicle not found in registry).");
    }
}
