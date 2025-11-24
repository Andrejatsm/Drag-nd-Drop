using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

// HANOJAS: Landscape-only camera controller (mouse + touch, locked to 1920x1080 world)
public class HanojasCamera : MonoBehaviour
{
    [Header("Zoom limits (acts as caps over dynamic range)")]
    public float maxZoom = 700f;   // will be clamped to fit 1920x1080
    public float minZoom = 80f;    // how far you can zoom in

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

    // End-screen control
    bool lockInputForEndScreen = false;

    private void Awake()
    {
        // ---------- FORCE LANDSCAPE ORIENTATION ----------
        Screen.orientation = ScreenOrientation.LandscapeLeft;
        Screen.autorotateToLandscapeLeft = true;
        Screen.autorotateToLandscapeRight = true;
        Screen.autorotateToPortrait = false;
        Screen.autorotateToPortraitUpsideDown = false;

        if (cam == null)
        {
            cam = GetComponent<Camera>();
        }

        if (screenBoundries == null)
        {
            screenBoundries = FindFirstObjectByType<ScreenBoundries>();
        }

        if (screenBoundries != null)
        {
            // Hard-lock world bounds to EXACT 1920x1080 around (0,0)
            screenBoundries.worldBounds = new Rect(-960f, -540f, 1920f, 1080f);

            // Do NOT auto-expand/shrink worldBounds based on aspect
            screenBoundries.aspectAdjust = ScreenBoundries.AspectAdjustMode.None;
        }
    }

    void Start()
    {
        if (cam == null || screenBoundries == null)
            return;

        // Make sure bounds use current aspect / resolution
        screenBoundries.RecalculateBounds();

        // ---- ZOOM SETUP TO FIT EXACT 1920x1080 ----
        float worldHalfH = screenBoundries.worldBounds.height * 0.5f; // 540
        float worldHalfW = screenBoundries.worldBounds.width * 0.5f;  // 960

        // Max ortho size so the camera view stays INSIDE the 1920x1080 rect
        float maxByHeight = worldHalfH;                                      // cannot exceed half height
        float maxByWidth = worldHalfW / Mathf.Max(0.0001f, cam.aspect);      // convert half-width to half-height units
        float dynamicMaxZoom = Mathf.Min(maxByHeight, maxByWidth);

        // Full zoom-out = exactly entire 1920x1080 field (no outside)
        maxZoom = dynamicMaxZoom;

        // Start fully zoomed out or as close as minZoom allows
        startZoom = Mathf.Clamp(dynamicMaxZoom, minZoom, maxZoom);
        cam.orthographicSize = startZoom;

        // Clamp position with these new bounds
        screenBoundries.RecalculateBounds();
        transform.position = screenBoundries.GetClampedCameraPosition(transform.position);
    }

    void Update()
    {
        if (cam == null || screenBoundries == null)
            return;

        // Always update bounds (aspect / resolution can still change in editor)
        screenBoundries.RecalculateBounds();

        if (!lockInputForEndScreen && !TransformationScript.isTransforming)
        {
#if UNITY_EDITOR || UNITY_STANDALONE
            // MOUSE PANNING + ZOOM (Editor / Standalone)
            DesktopFollowCursor();

            // Try old input axis first
            float scroll = Input.GetAxis("Mouse ScrollWheel");

            // If axis returns 0 (e.g., Input Manager changed), fall back to mouseScrollDelta
            if (Mathf.Approximately(scroll, 0f))
                scroll = Input.mouseScrollDelta.y;

            if (Mathf.Abs(scroll) > Mathf.Epsilon)
            {
                // Positive scroll → zoom in (decrease orthoSize)
                cam.orthographicSize -= scroll * mouseZoomSpeed;
            }
#else
            // TOUCH PANNING (device)
            HandleTouch();
#endif

            // TOUCH PINCH ZOOM (device)
            if (Input.touchCount == 2)
                HandlePinch();
        }

        // Clamp zoom to world so blue background never appears
        ClampZoomToWorld();

        // Also clamp to min/max range caps
        cam.orthographicSize = Mathf.Clamp(cam.orthographicSize, minZoom, maxZoom);

        // Clamp position every frame
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

        // Slightly faster & independent of timescale if you ever use slowmo
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
        else if (t.phase == TouchPhase.Moved && isTouchPanning &&
                 t.fingerId == panFingerId)
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
            {
                return true;
            }
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

        // Scale down a bit so it isn't crazy sensitive
        float zoomDelta = delta * puncZoomSpeed * 0.01f;

        cam.orthographicSize -= zoomDelta;
    }

    void ClampZoomToWorld()
    {
        if (cam == null || screenBoundries == null) return;
        if (!cam.orthographic) return;

        float worldHalfH = screenBoundries.worldBounds.height * 0.5f;
        float worldHalfW = screenBoundries.worldBounds.width * 0.5f;
        float maxByHeight = worldHalfH; // cannot exceed half world height
        float maxByWidth = worldHalfW / Mathf.Max(0.0001f, cam.aspect); // half width in world converted to half height via aspect
        float dynamicMax = Mathf.Min(maxByHeight, maxByWidth);

        // Respect designer's maxZoom too (but this will already be <= dynamicMax)
        float allowedMax = Mathf.Min(maxZoom, dynamicMax);

        cam.orthographicSize = Mathf.Clamp(cam.orthographicSize, minZoom, allowedMax);
    }

    Vector3 ScreenDeltaToWorldDelta(Vector2 delta)
    {
        float worldPerPixel =
            (cam.orthographicSize * 2f) / Screen.height;
        return new Vector3(delta.x * worldPerPixel, delta.y * worldPerPixel, 0f);
    }

    IEnumerator ResetZoomSmooth()
    {
        float duration = 0.25f;
        float elapsed = 0f;
        float initialZoom = cam.orthographicSize;

        // Reset to startZoom, which is already set so the whole 1920x1080 fits
        float targetZoom = Mathf.Clamp(startZoom, minZoom, maxZoom);

        while (elapsed < duration)
        {
            // Use unscaled time if you ever use slow-motion
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

    // Public API to focus camera at center for win/lose and lock controls
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
            t += Time.unscaledDeltaTime; // unaffected by Time.timeScale in end screens
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
