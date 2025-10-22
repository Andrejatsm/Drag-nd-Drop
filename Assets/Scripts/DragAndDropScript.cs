using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

// DragAndDropScript
// Handles UI-based drag of vehicle RectTransforms within screen bounds, while leaving
// rotation/scale to TransformationScript via ObjectScript.lastDragged when not dragging.
// Notes for future mobile support:
// - Prefer using eventData.position / eventData.pressEventCamera for pointer positions
//   instead of Input.GetMouse* polling. Current code keeps Input checks by request.
// - Track pointerId if you add multi-touch to avoid mixing different touches.
// - Consider RectTransformUtility.ScreenPointToWorldPointInRectangle for Canvas Space conversions.
public class DragAndDropScript : MonoBehaviour, IPointerDownHandler, IBeginDragHandler, 
    IDragHandler, IEndDragHandler
{
    // Raycast/visibility control for UI interaction during drag
    private CanvasGroup canvasGro;

    // The RectTransform we move while dragging
    private RectTransform rectTra;

    // Cross-script references: used for SFX and game state (rightPlace flag)
    public ObjectScript objectScr;
    public ScreenBoundriesScript screenBou; // Provides clamping and pointer offset storage

    [HideInInspector] public bool isPlaced = false; // Becomes true when dropped correctly

    // Cached parents (not heavily used here but kept for layout-awareness/future use)
    private RectTransform parentRect;
    private Canvas parentCanvas;

    // Internal state: only move while a valid left-button drag is in progress
    private bool isDragging = false;

    void Start()
    {
        // Ensure there is a CanvasGroup so we can control raycasts/alpha while dragging
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

        // Mark this object as the current selection so TransformationScript can rotate/scale it
        ObjectScript.lastDragged = gameObject;
        ObjectScript.drag = false; // not dragging yet

        // Left-click SFX
        if (Input.GetMouseButton(0) && !Input.GetMouseButton(1) && !Input.GetMouseButton(2))
        {
            if (objectScr != null && objectScr.effects != null && objectScr.audioCli != null && objectScr.audioCli.Length > 0)
                objectScr.effects.PlayOneShot(objectScr.audioCli[0]);
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (!enabled || rectTra == null || isPlaced) return;

        // Only begin drag with left mouse (keeps right/middle free for other actions)
        if (Input.GetMouseButton(0) && !Input.GetMouseButton(1) && !Input.GetMouseButton(2))
        {
            isDragging = true;
            ObjectScript.drag = true; // lets TransformationScript know to pause transforms
            ObjectScript.lastDragged = gameObject;

            // Allow drop targets to receive raycasts while we drag
            canvasGro.blocksRaycasts = false;
            canvasGro.alpha = 0.6f; // visual feedback

            // Bring near-top (keep last sibling slot free if you have overlays)
            int lastIndex = transform.parent.childCount - 1;
            int position = Mathf.Max(0, lastIndex - 1);
            transform.SetSiblingIndex(position);

            // Initialize world-space cursor offset for clamping with shared screen bounds
            // TODO(Android): prefer eventData.pressEventCamera and eventData.position for robustness
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
        if (!isDragging) return; // ignore stray drag events

        if (Input.GetMouseButton(0) && !Input.GetMouseButton(1) && !Input.GetMouseButton(2))
        {
            // Convert cursor to world space, apply original offset, then clamp to shared screen bounds
            // TODO(Android): use eventData.position + eventData.pressEventCamera for screen->world conversion
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
        // Always finalize drag; don't rely on Input state here (prevents stuck non-interactable cars)
        isDragging = false;
        if (!enabled || rectTra == null)
        {
            ObjectScript.drag = false;
            // Restore interactability if not placed
            if (canvasGro != null)
            {
                canvasGro.alpha = 1f;
                canvasGro.blocksRaycasts = !isPlaced ? true : false;
            }
            return;
        }

        ObjectScript.drag = false;
        if (canvasGro != null)
        {
            canvasGro.alpha = 1.0f;
            canvasGro.blocksRaycasts = true; // become interactable again
        }

        // If the DropPlaceScript marked a correct place, lock the item against further interaction
        if (objectScr != null && objectScr.rightPlace)
        {
            isPlaced = true;
            if (canvasGro != null) canvasGro.blocksRaycasts = false;
            ObjectScript.lastDragged = null; // clear selection when placed
        }

        // Reset the flag either way for the next drop
        if (objectScr != null) objectScr.rightPlace = false;
    }
}
