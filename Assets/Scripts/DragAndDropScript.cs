using UnityEngine;
using UnityEngine.EventSystems;

public class DragAndDropScript : MonoBehaviour, IPointerDownHandler, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    private CanvasGroup canvasGro;
    private RectTransform rectTra;

    public ObjectScript objectScr;
    public ScreenBoundriesScript screenBou;

    void Start()
    {
        canvasGro = GetComponent<CanvasGroup>();
        rectTra = GetComponent<RectTransform>();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (objectScr.rightPlace) return; // Already placed correctly
        if (Input.GetMouseButton(0))
        {
            objectScr.effects.PlayOneShot(objectScr.audioCli[0]);
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (objectScr.rightPlace) return; // Don't drag if correct
        if (Input.GetMouseButton(0))
        {
            ObjectScript.lastDragged = null;
            canvasGro.blocksRaycasts = false;
            canvasGro.alpha = 0.6f;
            rectTra.SetAsLastSibling();

            Vector3 cursorWorldPos = Camera.main.ScreenToWorldPoint(
                new Vector3(Input.mousePosition.x, Input.mousePosition.y, screenBou.screenPoint.z));
            rectTra.position = cursorWorldPos;

            screenBou.screenPoint = Camera.main.WorldToScreenPoint(rectTra.localPosition);
            screenBou.offset = rectTra.localPosition - Camera.main.ScreenToWorldPoint(
                new Vector3(Input.mousePosition.x, Input.mousePosition.y, screenBou.screenPoint.z));
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (objectScr.rightPlace) return; // Don't drag if correct
        if (Input.GetMouseButton(0))
        {
            Vector3 curScreenPoint = new Vector3(Input.mousePosition.x, Input.mousePosition.y, screenBou.screenPoint.z);
            Vector3 curPosition = Camera.main.ScreenToWorldPoint(curScreenPoint) + screenBou.offset;
            rectTra.position = screenBou.GetClampedPosition(curPosition);
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        canvasGro.blocksRaycasts = true;
        canvasGro.alpha = 1.0f;

        if (objectScr.rightPlace)
        {
            // Correct placement: lock in position
            ObjectScript.lastDragged = null;
            canvasGro.blocksRaycasts = false;
        }
        else
        {
            // Incorrect placement: allow reset to spawn handled by DropPlaceScript
            ObjectScript.lastDragged = eventData.pointerDrag;
        }
    }
}
