using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class DragAndDropScript : MonoBehaviour, IPointerDownHandler, IBeginDragHandler, 
    IDragHandler, IEndDragHandler
{
    private CanvasGroup canvasGro;
    private RectTransform rectTra;
    public ObjectScript objectScr;
    public ScreenBoundriesScript screenBou;

    // Prevent further dragging once placed
    [HideInInspector] public bool isPlaced = false;

    // For stable, non-jumpy UI dragging
    private RectTransform parentRect;
    private Canvas parentCanvas;
    private Vector2 pointerOffset;

    void Start()
    {
        canvasGro = GetComponent<CanvasGroup>();
        if (canvasGro == null) canvasGro = gameObject.AddComponent<CanvasGroup>();

        rectTra = GetComponent<RectTransform>();
        if (rectTra == null) Debug.LogError("DragAndDropScript requires a RectTransform.");

        parentRect = rectTra.parent as RectTransform;
        parentCanvas = GetComponentInParent<Canvas>();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (isPlaced) return;

        if (Input.GetMouseButton(0) && !Input.GetMouseButton(1) && !Input.GetMouseButton(2))
        {
            objectScr.effects.PlayOneShot(objectScr.audioCli[0]);
        } 
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (isPlaced) return;

        if (Input.GetMouseButton(0) && !Input.GetMouseButton(1) && !Input.GetMouseButton(2))
        {
            ObjectScript.drag = true;
            ObjectScript.lastDragged = eventData.pointerDrag;
            canvasGro.blocksRaycasts = false;
            canvasGro.alpha = 0.6f;

            int lastIndex = transform.parent.childCount - 1;
            int position = Mathf.Max(0, lastIndex - 1);
            transform.SetSiblingIndex(position);

            // Use UI space to prevent jumping
            Camera cam = (parentCanvas != null && parentCanvas.renderMode != RenderMode.ScreenSpaceOverlay)
                ? parentCanvas.worldCamera
                : null;

            if (parentRect != null &&
                RectTransformUtility.ScreenPointToLocalPointInRectangle(parentRect, eventData.position, cam, out var localPoint))
            {
                // Keep initial cursor-to-pivot offset so it stays under the cursor
                pointerOffset = rectTra.anchoredPosition - localPoint;
            }
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (isPlaced) return;

        if (Input.GetMouseButton(0) && !Input.GetMouseButton(1) && !Input.GetMouseButton(2))
        {
            Camera cam = (parentCanvas != null && parentCanvas.renderMode != RenderMode.ScreenSpaceOverlay)
                ? parentCanvas.worldCamera
                : null;

            if (parentRect != null &&
                RectTransformUtility.ScreenPointToLocalPointInRectangle(parentRect, eventData.position, cam, out var localPoint))
            {
                // Move strictly in UI local space so the image follows the cursor precisely
                rectTra.anchoredPosition = localPoint + pointerOffset;
            }
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (isPlaced) return;

        if (Input.GetMouseButtonUp(0))
        {
            ObjectScript.drag = false;
            canvasGro.blocksRaycasts = true;
            canvasGro.alpha = 1.0f;

            if (objectScr.rightPlace)
            {
                isPlaced = true;                 // lock it if placed correctly
                canvasGro.blocksRaycasts = false;
                ObjectScript.lastDragged = null;
            }

            objectScr.rightPlace = false;
        }
    }
}
