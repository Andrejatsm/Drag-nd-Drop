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
        if ((eventData.pointerDrag != null) &&
            Input.GetMouseButtonUp(0) && !Input.GetMouseButton(1) && !Input.GetMouseButton(2))
        {
            if (eventData.pointerDrag.tag.Equals(tag))
            {
                placeZRot = eventData.pointerDrag.GetComponent<RectTransform>().transform.eulerAngles.z;
                vehicleZRot = GetComponent<RectTransform>().transform.eulerAngles.z;
                rotDiff = Mathf.Abs(placeZRot - vehicleZRot);

                placeSiz = eventData.pointerDrag.GetComponent<RectTransform>().localScale;
                vehicleSiz = GetComponent<RectTransform>().localScale;
                xSizeDiff = Mathf.Abs(placeSiz.x - vehicleSiz.x);
                ySizeDiff = Mathf.Abs(placeSiz.y - vehicleSiz.y);

                if ((rotDiff <= 5 || (rotDiff >= 355 && rotDiff <= 360)) &&
                    (xSizeDiff <= 0.05 && ySizeDiff <= 0.05))
                {
                    //  Correct place: snap to placeholder and keep it there
                    objScript.rightPlace = true;

                    RectTransform dragRT = eventData.pointerDrag.GetComponent<RectTransform>();
                    RectTransform placeRT = GetComponent<RectTransform>();

                    dragRT.SetParent(placeRT, worldPositionStays: false);
                    dragRT.anchorMin = new Vector2(0.5f, 0.5f);
                    dragRT.anchorMax = new Vector2(0.5f, 0.5f);
                    dragRT.pivot = placeRT.pivot;
                    dragRT.anchoredPosition = Vector2.zero;
                    dragRT.localRotation = placeRT.localRotation;
                    dragRT.localScale = placeRT.localScale;

                    var drag = eventData.pointerDrag.GetComponent<DragAndDropScript>();
                    if (drag != null) drag.isPlaced = true;

                    var cg = eventData.pointerDrag.GetComponent<CanvasGroup>();
                    if (cg != null)
                    {
                        cg.alpha = 1f;
                        cg.blocksRaycasts = false;
                    }

                    //  ADD THIS LINE → tells GameManager a car was placed correctly
                    GameManager.Instance.CarPlacedCorrectly();

                    // SFX by tag
                    switch (eventData.pointerDrag.tag)
                    {
                        case "Garbage": objScript.effects.PlayOneShot(objScript.audioCli[2]); break;
                        case "Medicine": objScript.effects.PlayOneShot(objScript.audioCli[3]); break;
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
                    return;
                }
            }
            else
            {
                // ❌ Wrong placement
                objScript.rightPlace = false;
                objScript.effects.PlayOneShot(objScript.audioCli[1]);

                // Reset car to start position
                switch (eventData.pointerDrag.tag)
                {
                    case "Garbage":
                        objScript.vehicles[0].GetComponent<RectTransform>().localPosition = objScript.startCoordinates[0];
                        break;
                    case "Medicine":
                        objScript.vehicles[1].GetComponent<RectTransform>().localPosition = objScript.startCoordinates[1];
                        break;
                    case "Buss":
                        objScript.vehicles[2].GetComponent<RectTransform>().localPosition = objScript.startCoordinates[2];
                        break;
                    case "e46":
                        objScript.vehicles[4].GetComponent<RectTransform>().localPosition = objScript.startCoordinates[4];
                        break;
                    case "e61":
                        objScript.vehicles[5].GetComponent<RectTransform>().localPosition = objScript.startCoordinates[5];
                        break;
                    case "b2":
                        objScript.vehicles[6].GetComponent<RectTransform>().localPosition = objScript.startCoordinates[6];
                        break;
                    case "Cement":
                        objScript.vehicles[7].GetComponent<RectTransform>().localPosition = objScript.startCoordinates[7];
                        break;
                    case "Eskovator":
                        objScript.vehicles[8].GetComponent<RectTransform>().localPosition = objScript.startCoordinates[8];
                        break;
                    case "Police":
                        objScript.vehicles[9].GetComponent<RectTransform>().localPosition = objScript.startCoordinates[9];
                        break;
                    case "Tracktor":
                        objScript.vehicles[10].GetComponent<RectTransform>().localPosition = objScript.startCoordinates[10];
                        break;
                    case "Tracktor2":
                        objScript.vehicles[11].GetComponent<RectTransform>().localPosition = objScript.startCoordinates[11];
                        break;
                    default:
                        Debug.Log("Unknown tag detected");
                        break;
                }
            }
        }
    }
}
