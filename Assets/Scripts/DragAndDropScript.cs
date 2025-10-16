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

    [HideInInspector] public bool isPlaced = false;

    private RectTransform parentRect;
    private Canvas parentCanvas;

    void Start()
    {
        canvasGro = GetComponent<CanvasGroup>();
        if (canvasGro == null) canvasGro = gameObject.AddComponent<CanvasGroup>();

        rectTra = GetComponent<RectTransform>();
        if (rectTra == null)
        {
            Debug.LogWarning("DragAndDropScript requires a RectTransform. Disabling component on '" + name + "'.");
            // If this script was mistakenly added to a non-UI object (e.g., ScriptHolder), disable it safely.
            enabled = false;
            return;
        }

        parentRect = rectTra.parent as RectTransform;
        parentCanvas = GetComponentInParent<Canvas>();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (!enabled || rectTra == null || isPlaced) return;

        ObjectScript.lastDragged = gameObject;
        ObjectScript.drag = false;

        if (Input.GetMouseButton(0) && !Input.GetMouseButton(1) && !Input.GetMouseButton(2))
        {
            objectScr.effects.PlayOneShot(objectScr.audioCli[0]);
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (!enabled || rectTra == null || isPlaced) return;

        if (Input.GetMouseButton(0) && !Input.GetMouseButton(1) && !Input.GetMouseButton(2))
        {
            ObjectScript.drag = true;
            ObjectScript.lastDragged = gameObject;
            canvasGro.blocksRaycasts = false;
            canvasGro.alpha = 0.6f;

            int lastIndex = transform.parent.childCount - 1;
            int position = Mathf.Max(0, lastIndex - 1);
            transform.SetSiblingIndex(position);

            // Initialize world-space cursor offset for clamping with shared screen bounds
            if (screenBou != null)
            {
                screenBou.screenPoint = Camera.main.WorldToScreenPoint(rectTra.position);
                screenBou.offset = rectTra.position - Camera.main.ScreenToWorldPoint(
                    new Vector3(Input.mousePosition.x, Input.mousePosition.y, screenBou.screenPoint.z));
            }
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!enabled || rectTra == null || isPlaced) return;

        if (Input.GetMouseButton(0) && !Input.GetMouseButton(1) && !Input.GetMouseButton(2))
        {
            // Convert cursor to world space, apply original offset, then clamp to shared screen bounds
            if (screenBou != null)
            {
                Vector3 curScreenPoint = new Vector3(Input.mousePosition.x, Input.mousePosition.y, screenBou.screenPoint.z);
                Vector3 curWorld = Camera.main.ScreenToWorldPoint(curScreenPoint) + screenBou.offset;

                Vector2 clamped = screenBou.GetClampedPosition(curWorld);
                rectTra.position = new Vector3(clamped.x, clamped.y, rectTra.position.z);
            }
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (!enabled || rectTra == null || isPlaced)
        {
            ObjectScript.drag = false;
            ObjectScript.lastDragged = null;
            if (canvasGro != null)
            {
                canvasGro.alpha = 1f;
                canvasGro.blocksRaycasts = false;
            }
            return;
        }

        if (Input.GetMouseButtonUp(0))
        {
            ObjectScript.drag = false;
            canvasGro.blocksRaycasts = true;
            canvasGro.alpha = 1.0f;

            if (objectScr.rightPlace)
            {
                isPlaced = true;
                canvasGro.blocksRaycasts = false;
                ObjectScript.lastDragged = null;
            }

            objectScr.rightPlace = false;
        }
    }
}
