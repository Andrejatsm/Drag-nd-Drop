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
        if (eventData.pointerDrag == null || !Input.GetMouseButtonUp(0)) return;

        RectTransform draggedRect = eventData.pointerDrag.GetComponent<RectTransform>();
        RectTransform placeRect = GetComponent<RectTransform>();

        if (eventData.pointerDrag.tag.Equals(tag))
        {
            // Rotation check
            placeZRot = placeRect.eulerAngles.z;
            vehicleZRot = draggedRect.eulerAngles.z;
            rotDiff = Mathf.Abs(placeZRot - vehicleZRot);

            // Scale check
            placeSiz = placeRect.localScale;
            vehicleSiz = draggedRect.localScale;
            xSizeDiff = Mathf.Abs(placeSiz.x - vehicleSiz.x);
            ySizeDiff = Mathf.Abs(placeSiz.y - vehicleSiz.y);

            if ((rotDiff <= 5 || rotDiff >= 355) && xSizeDiff <= 0.05f && ySizeDiff <= 0.05f)
            {
                // Correct placement
                objScript.rightPlace = true;

                // Snap to placeholder
                draggedRect.anchoredPosition = placeRect.anchoredPosition;
                draggedRect.localScale = placeRect.localScale;

                // Play sound based on tag
                PlayPlacementSound(eventData.pointerDrag.tag);
                return;
            }
        }

        // Incorrect placement: reset to spawn
        objScript.rightPlace = false;
        objScript.effects.PlayOneShot(objScript.audioCli[1]);
        ResetCarPosition(eventData.pointerDrag.tag);
    }

    private void PlayPlacementSound(string tag)
    {
        int index = tag switch
        {
            "Garbage" => 2,
            "Ambulance" => 3,
            "Fire" => 4,
            "Buss" => 5,
            "e46" => 6,
            "e61" => 7,
            "b2" => 8,
            "Cement" => 9,
            "Eskovator" => 10,
            "Police" => 11,
            "Tracktor" => 12,
            "Tracktor2" => 13,
            _ => -1
        };

        if (index >= 0 && index < objScript.audioCli.Length)
            objScript.effects.PlayOneShot(objScript.audioCli[index]);
        else
            Debug.Log("Unknown tag: " + tag);
    }

    private void ResetCarPosition(string tag)
    {
        // Find the index in vehicles array
        int index = System.Array.FindIndex(objScript.vehicles, v => v.tag == tag);
        if (index >= 0 && index < objScript.startCoordinates.Length)
        {
            objScript.vehicles[index].GetComponent<RectTransform>().anchoredPosition =
                objScript.startCoordinates[index];
        }
        else
        {
            Debug.LogWarning("Could not reset car: " + tag);
        }
    }
}
