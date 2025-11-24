using UnityEngine;

// CHANGES FOR ANDROID
public class ScreenBoundries : MonoBehaviour
{
    [HideInInspector]
    public Vector3 screenPoint, offset;
    [HideInInspector]
    public float minX, maxX, minY, maxY;

    [Header("World Bounds (authoring)")]
    // Your designed world/map size (1920x1080 around (0,0))
    public Rect worldBounds = new Rect(-960, -540, 1920, 1080);

    [Range(0f, 0.5f)]
    public float padding = 0.02f;

    public Camera targetCamera;

    [Header("Auto Adjust to Screen Aspect")]
    public AspectAdjustMode aspectAdjust = AspectAdjustMode.KeepHeight; // Portrait-friendly by default

    public float minCamX { get; private set; }
    public float maxCamX { get; private set; }
    public float minCamY { get; private set; }
    public float maxCamY { get; private set; }

    float lastOrthoSize;
    float lastAspect;
    Vector3 lastCamPos;
    int lastScreenW, lastScreenH;

    Rect originalWorldBounds; // keep the authored reference

    public enum AspectAdjustMode
    {
        None,
        KeepWidth,
        KeepHeight,
        ExpandToFit
    }

    void Awake()
    {
        if (targetCamera == null)
        {
            targetCamera = Camera.main;
        }

        originalWorldBounds = worldBounds;
        lastScreenW = Screen.width;
        lastScreenH = Screen.height;
        RecalculateBounds();
    }

    void Update()
    {
        if (targetCamera == null)
        {
            return;
        }

        bool changed = false;

        if (targetCamera.orthographic)
        {
            if (!Mathf.Approximately(targetCamera.orthographicSize, lastOrthoSize))
                changed = true;
        }

        if (!Mathf.Approximately(targetCamera.aspect, lastAspect))
            changed = true;

        if (targetCamera.transform.position != lastCamPos)
            changed = true;

        if (Screen.width != lastScreenW || Screen.height != lastScreenH)
            changed = true;

        if (changed)
        {
            RecalculateBounds();
        }
    }

    public void RecalculateBounds()
    {
        if (targetCamera == null)
            return;

        // Auto-adjust world bounds to the device aspect if requested
        if (aspectAdjust != AspectAdjustMode.None)
        {
            ApplyAspectAdjustment();
        }

        float wbMinX = worldBounds.xMin;
        float wbMaxX = worldBounds.xMax;
        float wbMinY = worldBounds.yMin;
        float wbMaxY = worldBounds.yMax;

        if (targetCamera.orthographic)
        {
            float halfH = targetCamera.orthographicSize;
            float halfW = halfH * targetCamera.aspect;

            // Horizontal camera center range
            if (halfW * 2f >= (wbMaxX - wbMinX))
            {
                // Camera is as wide or wider than world → lock to center X
                minCamX = maxCamX = (wbMinX + wbMaxX) * 0.5f;
            }
            else
            {
                minCamX = wbMinX + halfW;
                maxCamX = wbMaxX - halfW;
            }

            // Vertical camera center range
            if (halfH * 2f >= (wbMaxY - wbMinY))
            {
                // Camera is as tall or taller than world → lock to center Y
                minCamY = maxCamY = (wbMinY + wbMaxY) * 0.5f;
            }
            else
            {
                minCamY = wbMinY + halfH;
                maxCamY = wbMaxY - halfH;
            }

            // Expose world bounds
            minY = wbMinY;
            maxY = wbMaxY;
            minX = wbMinX;
            maxX = wbMaxX;
        }

        lastOrthoSize = targetCamera.orthographicSize;
        lastAspect = targetCamera.aspect;
        lastCamPos = targetCamera.transform.position;
        lastScreenW = Screen.width;
        lastScreenH = Screen.height;
    }

    void ApplyAspectAdjustment()
    {
        // Keep center stable while changing size
        Vector2 center = originalWorldBounds.center;
        float baseW = Mathf.Max(0.0001f, originalWorldBounds.width);
        float baseH = Mathf.Max(0.0001f, originalWorldBounds.height);
        float deviceAspect = Mathf.Max(0.0001f, targetCamera.aspect);

        float newW = originalWorldBounds.width;
        float newH = originalWorldBounds.height;

        switch (aspectAdjust)
        {
            case AspectAdjustMode.KeepWidth:
                // Keep world width fixed.
                // Only expand height if needed so the camera never sees outside,
                // but NEVER shrink below the authored height.
                newW = baseW;
                float neededHeight = baseW / deviceAspect;
                newH = Mathf.Max(baseH, neededHeight);
                break;

            case AspectAdjustMode.KeepHeight:
                // Keep world height fixed.
                // Only expand width if needed so the camera never sees outside,
                // but NEVER shrink below the authored width.
                newH = baseH;
                float neededWidth = baseH * deviceAspect;
                newW = Mathf.Max(baseW, neededWidth);
                break;

            case AspectAdjustMode.ExpandToFit:
                // Expand the smaller dimension so the camera can never see outside,
                // but never shrink either dimension below authored size.
                float fitW = baseH * deviceAspect;
                float fitH = baseW / deviceAspect;
                newW = Mathf.Max(baseW, fitW);
                newH = Mathf.Max(baseH, fitH);
                break;
        }

        worldBounds = new Rect(center.x - newW * 0.5f, center.y - newH * 0.5f, newW, newH);
    }

    // For draggable objects
    public Vector2 GetClampedPosition(Vector3 curPosition)
    {
        float shrinkW = worldBounds.width * padding;
        float shrinkH = worldBounds.height * padding;
        float wbMinX = worldBounds.xMin + shrinkW;
        float wbMaxX = worldBounds.xMax - shrinkW;
        float wbMinY = worldBounds.yMin + shrinkH;
        float wbMaxY = worldBounds.yMax - shrinkH;

        float cx = Mathf.Clamp(curPosition.x, wbMinX, wbMaxX);
        float cy = Mathf.Clamp(curPosition.y, wbMinY, wbMaxY);
        return new Vector2(cx, cy);
    }

    // For camera movement
    public Vector3 GetClampedCameraPosition(Vector3 desiredCamCenter)
    {
        float cx = Mathf.Clamp(desiredCamCenter.x, minCamX, maxCamX);
        float cy = Mathf.Clamp(desiredCamCenter.y, minCamY, maxCamY);
        return new Vector3(cx, cy, desiredCamCenter.z);
    }
}
