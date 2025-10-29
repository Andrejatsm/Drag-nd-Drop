using UnityEngine;

public class CameraScript : MonoBehaviour
{
    [Header("Zoom Settings")]
    public float maxZoom = 1920f;   // allow much farther zoom out
    public float minZoom = 50f;     // allow closer zoom in
    public float zoomStep = 200f;   // scroll sensitivity
    [Header("Pan Settings")]
    public float panSpeed = 6f;     // base pan speed
    Vector3 bottomLeft, topRight;
    float cameraMaxX, cameraMinX, cameraMaxY, cameraMinY;
    public Camera cam;

    void Start()
    {
        cam = GetComponent<Camera>();
        topRight = cam.ScreenToWorldPoint(
            new Vector3(cam.pixelWidth, cam.pixelHeight, -transform.position.z));
        bottomLeft = cam.ScreenToWorldPoint(new Vector3(0, 0, -transform.position.z));
        cameraMaxX = topRight.x;
        cameraMinX = bottomLeft.x;
        cameraMaxY = topRight.y;
        cameraMinY = bottomLeft.y;

        // Prevent zooming out beyond the initial view height
        // (orthographicSize is in world units, not pixels)
        maxZoom = cam.orthographicSize;
    }

    // Update is called once per frame
    void Update()
    {
        // Pan (frame-rate independent)
        float x = Input.GetAxis("Mouse X") * panSpeed * Time.unscaledDeltaTime * 60f;
        float y = Input.GetAxis("Mouse Y") * panSpeed * Time.unscaledDeltaTime * 60f;
        transform.Translate(x, y, 0f);

        // Smooth zoom
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (Mathf.Abs(scroll) > 0.0001f)
        {
            cam.orthographicSize = Mathf.Clamp(cam.orthographicSize - scroll * zoomStep, minZoom, maxZoom);
        }

        // Recompute screen corners
        topRight = cam.ScreenToWorldPoint(
            new Vector3(cam.pixelWidth, cam.pixelHeight, -transform.position.z));
        bottomLeft = cam.ScreenToWorldPoint(new Vector3(0, 0, -transform.position.z));

        // Clamp using orthographic extents so clamping adapts to zoom
        float halfH = cam.orthographicSize;
        float halfW = halfH * cam.aspect;
        float minX = cameraMinX + halfW;
        float maxX = cameraMaxX - halfW;
        float minY = cameraMinY + halfH;
        float maxY = cameraMaxY - halfH;
        Vector3 pos = transform.position;
        if (minX <= maxX) pos.x = Mathf.Clamp(pos.x, minX, maxX); else pos.x = (cameraMinX + cameraMaxX) * 0.5f;
        if (minY <= maxY) pos.y = Mathf.Clamp(pos.y, minY, maxY); else pos.y = (cameraMinY + cameraMaxY) * 0.5f;
        transform.position = pos;
    }
}