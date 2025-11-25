using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

// CHANGES FOR ANDROID / DYNAMIC CAMERA (like Hanojas)
public class CameraScript : MonoBehaviour
{
    [Header("Zoom limits (caps over dynamic range)")]
<<<<<<< HEAD
    public float maxZoom = 700f;
    public float minZoom = 80f;
=======
    public float maxZoom = 700f;   // upper cap for zoom-out
    public float minZoom = 80f;    // lower cap for zoom-in
>>>>>>> 012ec15275a0d82bbfd1a1ac69db1ff0c019cdbf

    [Header("Zoom speeds")]
    public float puncZoomSpeed = 0.9f;
    public float mouseZoomSpeed = 150f;

    [Header("Pan speeds")]
    public float mouseFollowSpeed = 4f;
    public float touchPanSpeed = 2f;

    [Header("Refs")]
    public ScreenBoundries screenBoundries;
    public Camera cam;

    float startZoom;
    Vector2 lastTouchPos;
    int panFingerId = -1;
    bool isTouchPanning = false;

    float lastTapTime = 0f;
    public float doubleTapMaxDelay = 0.4f;
    public float doubleTapMaxDistance = 100f;

    bool lockInputForEndScreen = false;

    private void Awake()
    {
<<<<<<< HEAD
        // Force landscape here too, if this game is also landscape-only:
=======
        // ---------- FORCE LANDSCAPE ORIENTATION ----------
>>>>>>> 012ec15275a0d82bbfd1a1ac69db1ff0c019cdbf
        Screen.orientation = ScreenOrientation.LandscapeLeft;
        Screen.autorotateToLandscapeLeft = true;
        Screen.autorotateToLandscapeRight = true;
        Screen.autorotateToPortrait = false;
        Screen.autorotateToPortraitUpsideDown = false;

        if (cam == null)
            cam = GetComponent<Camera>();

        if (!cam.orthographic)
            cam.orthographic = true;

        if (screenBoundries == null)
            screenBoundries = FindFirstObjectByType<ScreenBoundries>();

        // If you want this scene also locked to 1920x1080 world:
        if (screenBoundries != null)
        {
            screenBoundries.worldBounds = new Rect(-960f, -540f, 1920f, 1080f);
            screenBoundries.aspectAdjust = ScreenBoundries.AspectAdjustMode.KeepHeight;
        }

        // If you ALSO want this scene hard-locked to 1920x1080 world like Hanojas,
        // uncomment this block:
        //
        // if (screenBoundries != null)
        // {
        //     screenBoundries.worldBounds = new Rect(-960f, -540f, 1920f, 1080f);
        //     screenBoundries.aspectAdjust = ScreenBoundries.AspectAdjustMode.None;
        // }
    }

    void Start()
    {
        if (cam == null || screenBoundries == null)
            return;

<<<<<<< HEAD
        screenBoundries.RecalculateBounds();

        float worldHalfH = screenBoundries.worldBounds.height * 0.5f;
        float worldHalfW = screenBoundries.worldBounds.width * 0.5f;
=======
        // Make sure bounds use current aspect / resolution
        screenBoundries.RecalculateBounds();

        // ---- DYNAMIC ZOOM SETUP (like Hanojas) ----
        float worldHalfH = screenBoundries.worldBounds.height * 0.5f;
        float worldHalfW = screenBoundries.worldBounds.width * 0.5f;

        // Max orthographic size so camera view stays inside worldBounds
>>>>>>> 012ec15275a0d82bbfd1a1ac69db1ff0c019cdbf
        float maxByHeight = worldHalfH;
        float maxByWidth = worldHalfW / Mathf.Max(0.0001f, cam.aspect);
        float dynamicMaxZoom = Mathf.Min(maxByHeight, maxByWidth);

<<<<<<< HEAD
        maxZoom = Mathf.Min(maxZoom, dynamicMaxZoom);

        float initial = Mathf.Clamp(cam.orthographicSize, minZoom, maxZoom);
        cam.orthographicSize = initial;
        startZoom = initial;

        ClampZoomToWorld();

=======
        // Respect designer cap, but adapt to world + device
        maxZoom = Mathf.Min(maxZoom, dynamicMaxZoom);

        // Start zoom: fully showing world (or as close as minZoom allows)
        startZoom = Mathf.Clamp(dynamicMaxZoom, minZoom, maxZoom);
        cam.orthographicSize = startZoom;

        // Clamp initial position
>>>>>>> 012ec15275a0d82bbfd1a1ac69db1ff0c019cdbf
        screenBoundries.RecalculateBounds();
        transform.position = screenBoundries.GetClampedCameraPosition(transform.position);
    }

    void Update()
    {
        if (cam == null || screenBoundries == null)
            return;

<<<<<<< HEAD
=======
        // Always update bounds (aspect / resolution can still change in editor)
>>>>>>> 012ec15275a0d82bbfd1a1ac69db1ff0c019cdbf
        screenBoundries.RecalculateBounds();

        if (!lockInputForEndScreen && !TransformationScript.isTransforming)
        {
#if UNITY_EDITOR || UNITY_STANDALONE
            DesktopFollowCursor();

            float scroll = Input.GetAxis("Mouse ScrollWheel");
            if (Mathf.Approximately(scroll, 0f))
                scroll = Input.mouseScrollDelta.y;

            if (Mathf.Abs(scroll) > Mathf.Epsilon)
            {
<<<<<<< HEAD
                float zoomDelta = -scroll * mouseZoomSpeed * 0.01f;
                ZoomAtScreenPoint(zoomDelta, Input.mousePosition);
=======
                // Zoom at cursor position
                ZoomAtScreenPoint(-scroll * mouseZoomSpeed * 0.01f, Input.mousePosition);
>>>>>>> 012ec15275a0d82bbfd1a1ac69db1ff0c019cdbf
            }
#else
            HandleTouch();
#endif
            if (Input.touchCount == 2)
            {
                HandlePinch();
            }
        }

<<<<<<< HEAD
=======
        // Clamp zoom so blue background never appears
>>>>>>> 012ec15275a0d82bbfd1a1ac69db1ff0c019cdbf
        ClampZoomToWorld();
        cam.orthographicSize = Mathf.Clamp(cam.orthographicSize, minZoom, maxZoom);

        screenBoundries.RecalculateBounds();
        transform.position = screenBoundries.GetClampedCameraPosition(transform.position);
    }

    void DesktopFollowCursor()
    {
        Vector3 mouse = Input.mousePosition;
        if (mouse.x < 0 || mouse.x > Screen.width || mouse.y < 0 || mouse.y > Screen.height)
            return;

        bool isPressing = Input.GetMouseButton(0);
        if (!isPressing)
            return;

        Vector3 screenPoint = new Vector3(mouse.x, mouse.y, cam.nearClipPlane);
        Vector3 targetWorld = cam.ScreenToWorldPoint(screenPoint);
        Vector3 desired = new Vector3(targetWorld.x, targetWorld.y, transform.position.z);

        transform.position =
            Vector3.Lerp(transform.position, desired, mouseFollowSpeed * Time.unscaledDeltaTime);
    }

    void HandleTouch()
    {
        if (Input.touchCount != 1)
            return;

        Touch t = Input.GetTouch(0);

        if (IsTouchingUIButton(t.position))
            return;

        if (t.phase == TouchPhase.Began)
        {
            float dt = Time.time - lastTapTime;
            if (dt <= doubleTapMaxDelay &&
                Vector2.Distance(t.position, lastTouchPos) <= doubleTapMaxDistance)
            {
                StartCoroutine(ResetZoomSmooth());
                lastTapTime = 0f;
            }
            else
            {
                lastTapTime = Time.time;
            }

            lastTouchPos = t.position;
            panFingerId = t.fingerId;
            isTouchPanning = true;
        }
<<<<<<< HEAD
        else if (t.phase == TouchPhase.Moved && isTouchPanning && t.fingerId == panFingerId)
=======
        else if (t.phase == TouchPhase.Moved && isTouchPanning &&
                 t.fingerId == panFingerId)
>>>>>>> 012ec15275a0d82bbfd1a1ac69db1ff0c019cdbf
        {
            Vector2 delta = t.position - lastTouchPos;
            transform.Translate(ScreenDeltaToWorldDelta(delta) * touchPanSpeed, Space.World);
            lastTouchPos = t.position;
        }
        else if (t.phase == TouchPhase.Ended || t.phase == TouchPhase.Canceled)
        {
            isTouchPanning = false;
            panFingerId = -1;
        }
    }

    bool IsTouchingUIButton(Vector2 touchPos)
    {
        if (EventSystem.current == null) return false;

        PointerEventData pointerData = new PointerEventData(EventSystem.current);
        pointerData.position = touchPos;

        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(pointerData, results);

        foreach (RaycastResult result in results)
        {
            if (result.gameObject.GetComponent<UnityEngine.UI.Button>() != null)
                return true;
        }

        return false;
    }

    void HandlePinch()
    {
        if (Input.touchCount < 2)
            return;

        Touch t0 = Input.GetTouch(0);
        Touch t1 = Input.GetTouch(1);

        Vector2 prev0 = t0.position - t0.deltaPosition;
        Vector2 prev1 = t1.position - t1.deltaPosition;

        float prevDist = (prev0 - prev1).magnitude;
        float currDist = (t0.position - t1.position).magnitude;

        float delta = currDist - prevDist;
        Vector2 mid = (t0.position + t1.position) * 0.5f;

<<<<<<< HEAD
        float zoomDelta = -delta * puncZoomSpeed * 0.01f;
        ZoomAtScreenPoint(zoomDelta, mid);
    }

    void ZoomAtScreenPoint(float zoomDelta, Vector2 screenPoint)
    {
        if (cam == null || screenBoundries == null) return;

        float current = cam.orthographicSize;
        float worldHalfH = screenBoundries.worldBounds.height * 0.5f;
        float worldHalfW = screenBoundries.worldBounds.width * 0.5f;
        float maxByHeight = worldHalfH;
        float maxByWidth = worldHalfW / Mathf.Max(0.0001f, cam.aspect);
        float dynamicMax = Mathf.Min(maxByHeight, maxByWidth);

        float newSize = current + zoomDelta;
=======
        // Compute midpoint in screen coords
        Vector2 mid = (t0.position + t1.position) * 0.5f;

        // Determine desired zoom change (positive delta -> zoom in when we pass negative sign accordingly)
        float zoomDelta = delta * puncZoomSpeed * 0.01f;
        ZoomAtScreenPoint(-zoomDelta, mid);
    }

    void ZoomAtScreenPoint(float rawDelta, Vector2 screenPoint)
    {
        // rawDelta is a rough delta controlling how much to change orthographicSize.
        if (cam == null || screenBoundries == null) return;

        float current = cam.orthographicSize;
        // Apply sensitivity: rawDelta already includes speed factors; convert to size change
        float newSize = current + rawDelta;

        // Clamp against dynamic world-based max and designer min/max
        float dynamicMax = SafeMaxZoom();
>>>>>>> 012ec15275a0d82bbfd1a1ac69db1ff0c019cdbf
        newSize = Mathf.Clamp(newSize, minZoom, Mathf.Min(maxZoom, dynamicMax));

        if (Mathf.Approximately(newSize, current)) return;

<<<<<<< HEAD
        Vector3 worldBefore = cam.ScreenToWorldPoint(new Vector3(screenPoint.x, screenPoint.y, cam.nearClipPlane));

        cam.orthographicSize = newSize;
=======
        // Keep the world point under screenPoint stable: compute world before/after and shift camera
        Vector3 worldBefore = cam.ScreenToWorldPoint(new Vector3(screenPoint.x, screenPoint.y, cam.nearClipPlane));

        cam.orthographicSize = newSize;
        // recalc bounds immediately
>>>>>>> 012ec15275a0d82bbfd1a1ac69db1ff0c019cdbf
        ClampZoomToWorld();

        Vector3 worldAfter = cam.ScreenToWorldPoint(new Vector3(screenPoint.x, screenPoint.y, cam.nearClipPlane));
        Vector3 diff = worldBefore - worldAfter;

<<<<<<< HEAD
=======
        // Move camera by diff to keep the zoom centered under pointer
>>>>>>> 012ec15275a0d82bbfd1a1ac69db1ff0c019cdbf
        transform.position += new Vector3(diff.x, diff.y, 0f);
    }

    void ClampZoomToWorld()
    {
        if (cam == null || screenBoundries == null) return;
        if (!cam.orthographic) return;

        float worldHalfH = screenBoundries.worldBounds.height * 0.5f;
        float worldHalfW = screenBoundries.worldBounds.width * 0.5f;
<<<<<<< HEAD
        float maxByHeight = worldHalfH;
        float maxByWidth = worldHalfW / Mathf.Max(0.0001f, cam.aspect);
=======
        float maxByHeight = worldHalfH; // cannot exceed half world height
        float maxByWidth = worldHalfW / Mathf.Max(0.0001f, cam.aspect); // half width→height
>>>>>>> 012ec15275a0d82bbfd1a1ac69db1ff0c019cdbf
        float dynamicMax = Mathf.Min(maxByHeight, maxByWidth);

        float allowedMax = Mathf.Min(maxZoom, dynamicMax);

        cam.orthographicSize = Mathf.Clamp(cam.orthographicSize, minZoom, allowedMax);
    }

    Vector3 ScreenDeltaToWorldDelta(Vector2 delta)
    {
        float worldPerPixel = (cam.orthographicSize * 2f) / Screen.height;
        return new Vector3(delta.x * worldPerPixel, delta.y * worldPerPixel, 0f);
    }

    IEnumerator ResetZoomSmooth()
    {
        float duration = 0.25f;
        float elapsed = 0f;
        float initialZoom = cam.orthographicSize;

<<<<<<< HEAD
=======
        // Choose a safe target zoom inside world (based on dynamic max)
>>>>>>> 012ec15275a0d82bbfd1a1ac69db1ff0c019cdbf
        float worldHalfH = screenBoundries.worldBounds.height * 0.5f;
        float worldHalfW = screenBoundries.worldBounds.width * 0.5f;
        float maxByHeight = worldHalfH;
        float maxByWidth = worldHalfW / Mathf.Max(0.0001f, cam.aspect);
        float dynamicMaxZoom = Mathf.Min(maxByHeight, maxByWidth);

        float targetZoomRaw = Mathf.Clamp(startZoom, minZoom, dynamicMaxZoom);
        float targetZoom = Mathf.Clamp(targetZoomRaw, minZoom, maxZoom);

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;

            cam.orthographicSize = Mathf.Lerp(initialZoom, targetZoom, elapsed / duration);
            ClampZoomToWorld();
            screenBoundries.RecalculateBounds();
            transform.position = screenBoundries.GetClampedCameraPosition(transform.position);
            yield return null;
        }

        cam.orthographicSize = targetZoom;
        ClampZoomToWorld();
        screenBoundries.RecalculateBounds();
        transform.position = screenBoundries.GetClampedCameraPosition(transform.position);
    }

    float SafeMaxZoom()
    {
        float worldHalfH = screenBoundries.worldBounds.height * 0.5f;
        float worldHalfW = screenBoundries.worldBounds.width * 0.5f;
        float maxByHeight = worldHalfH;
        float maxByWidth = worldHalfW / Mathf.Max(0.0001f, cam.aspect);
        return Mathf.Min(maxZoom, Mathf.Min(maxByHeight, maxByWidth));
    }

    public void FocusToCenterForEndScreen(float duration = 0.35f)
    {
        if (screenBoundries == null) return;

        Vector3 center = new Vector3(
            screenBoundries.worldBounds.center.x,
            screenBoundries.worldBounds.center.y,
            transform.position.z
        );
        float targetZoom = Mathf.Min(cam.orthographicSize, SafeMaxZoom());
        StartCoroutine(FocusRoutine(center, targetZoom, duration));
    }

    IEnumerator FocusRoutine(Vector3 targetPos, float targetZoom, float duration)
    {
        lockInputForEndScreen = true;
        Vector3 startPos = transform.position;
        float startZoom = cam.orthographicSize;
        float t = 0f;

        while (t < duration)
        {
            t += Time.unscaledDeltaTime;
            float k = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(t / duration));
            cam.orthographicSize = Mathf.Lerp(startZoom, targetZoom, k);
            ClampZoomToWorld();
            Vector3 pos = Vector3.Lerp(startPos, targetPos, k);
            screenBoundries.RecalculateBounds();
            transform.position = screenBoundries.GetClampedCameraPosition(pos);
            yield return null;
        }

        cam.orthographicSize = targetZoom;
        ClampZoomToWorld();
        screenBoundries.RecalculateBounds();
        transform.position = screenBoundries.GetClampedCameraPosition(targetPos);
    }
}
