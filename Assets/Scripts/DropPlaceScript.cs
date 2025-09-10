using UnityEngine;
using UnityEngine.EventSystems;

public class DropPlaceScript : MonoBehaviour, IDropHandler
{
    private float placeZRot, vehicleZRot, rotDiff;
    private Vector3 placeSiz, vehicleSiz;
    private float xSizeDiff, ySizeDiff;
    public ObjectScript objScript;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public void OnDrop(PointerEventData eventData)
    {
        if ((eventData.pointerDrag != null ) && Input.GetMouseButtonUp(0) && !Input.GetMouseButton(1)
                && !Input.GetMouseButton(2))
        {
            if (eventData.pointerDrag.tag.Equals(tag))
            {
                placeZRot = eventData.pointerDrag.GetComponent<RectTransform>().transform.eulerAngles.z;

                vehicleZRot = GetComponent<RectTransform>().transform.eulerAngles.z;

                rotDiff = Mathf.Abs(placeZRot - vehicleZRot);
                Debug.Log("Rotation difference: " + rotDiff);

                placeSiz = eventData.pointerDrag.GetComponent<RectTransform>().localScale;
                vehicleSiz = GetComponent<RectTransform>().localScale;
                xSizeDiff = Mathf.Abs(placeSiz.x - vehicleSiz.x);
                ySizeDiff = Mathf.Abs(placeSiz.y - vehicleSiz.y);
                Debug.Log("X size difference: " + xSizeDiff);
                Debug.Log("Y size difference: " + ySizeDiff);

                if ((rotDiff <= 5 || (rotDiff >= 355 && rotDiff <= 360)) &&
                    (xSizeDiff <= 0.05 && ySizeDiff <= 0.05))
                {
                    Debug.Log("Correct place fk u!");
                }
            }
            else
            {
                objScript.rightPlace = false;
                objScript.effects.PlayOneShot(objScript.audioCli[1]);

                switch (eventData.pointerDrag.tag)
                {
                    case "Garbage":
                        objScript.vehicles[0].GetComponent<RectTransform>().localPosition = objScript.startCoordinates[0];
                        break;

                    case "Ambulance":
                        objScript.vehicles[1].GetComponent<RectTransform>().localPosition = objScript.startCoordinates[1];
                        break;

                    case "Fire":
                        objScript.vehicles[2].GetComponent<RectTransform>().localPosition = objScript.startCoordinates[2];
                        break;

                    case "Buss":
                        objScript.vehicles[3].GetComponent<RectTransform>().localPosition = objScript.startCoordinates[3];
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
                        Debug.Log("Uknown Tag lil bro ;)...");
                        break;
                }
            }
        }
    }
}
