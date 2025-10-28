using UnityEngine;
using UnityEngine.EventSystems;

public class DragAndDropScript : MonoBehaviour, IPointerDownHandler, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    private CanvasGroup canvasGro;
    private RectTransform rectTra;
    public ObjectScript objectScr;
    public ScreenBoundries screenBou;

    private Vector3 dragOffsetWorld;
    private Camera uiCamera;
    private Canvas canvas;

    [HideInInspector] public bool isPlaced = false;

    void Awake()
    {
        canvasGro = GetComponent<CanvasGroup>();
        rectTra = GetComponent<RectTransform>();

        if (objectScr == null) objectScr = Object.FindFirstObjectByType<ObjectScript>();
        if (screenBou == null) screenBou = Object.FindFirstObjectByType<ScreenBoundries>();

        canvas = GetComponentInParent<Canvas>();
        if (canvas != null) uiCamera = canvas.worldCamera;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (isPlaced) return;
        ObjectScript.lastDragged = gameObject;

        if (objectScr != null && objectScr.effects != null && objectScr.audioCli.Length > 0)
            objectScr.effects.PlayOneShot(objectScr.audioCli[0]);
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (isPlaced) return;

        ObjectScript.drag = true;
        ObjectScript.lastDragged = gameObject;
        canvasGro.blocksRaycasts = false;
        canvasGro.alpha = 0.6f;

        int lastIndex = transform.parent.childCount - 1;
        transform.SetSiblingIndex(Mathf.Max(0, lastIndex - 1));

        Vector3 pointerWorld;
        if (ScreenPointToWorld(eventData.position, out pointerWorld))
            dragOffsetWorld = transform.position - pointerWorld;
        else
            dragOffsetWorld = Vector3.zero;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (isPlaced) return;

        Vector3 pointerWorld;
        if (!ScreenPointToWorld(eventData.position, out pointerWorld)) return;

        Vector3 desiredPosition = pointerWorld + dragOffsetWorld;
        desiredPosition.z = transform.position.z;

        if (screenBou != null) screenBou.RecalculateBounds();
        Vector2 clamped = screenBou != null ? screenBou.GetClampedPosition(desiredPosition) : (Vector2)desiredPosition;

        transform.position = new Vector3(clamped.x, clamped.y, desiredPosition.z);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        ObjectScript.drag = false;
        canvasGro.blocksRaycasts = true;
        canvasGro.alpha = 1f;

        if (objectScr != null && objectScr.rightPlace)
        {
            isPlaced = true;
            canvasGro.blocksRaycasts = false;
            ObjectScript.lastDragged = null;
        }

        if (objectScr != null) objectScr.rightPlace = false;
    }

    private bool ScreenPointToWorld(Vector2 screenPoint, out Vector3 worldPoint)
    {
        worldPoint = Vector3.zero;
        if (uiCamera == null) return false;

        float z = Mathf.Abs(uiCamera.transform.position.z - transform.position.z);
        worldPoint = uiCamera.ScreenToWorldPoint(new Vector3(screenPoint.x, screenPoint.y, z));
        return true;
    }
}
