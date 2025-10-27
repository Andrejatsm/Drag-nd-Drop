using UnityEngine;

public class ScreenBoundriesScript : MonoBehaviour
{
    [HideInInspector] public Vector3 screenPoint, offset;
    [HideInInspector] public float minX, maxX, minY, maxY;
    public float padding = 0.02f;

    void Awake()
    {
        // Define one shared world-space boundary box for the whole screen
        Vector3 lowerLeft = Camera.main.ScreenToWorldPoint(Vector3.zero);
        Vector3 upperRight = Camera.main.ScreenToWorldPoint(new Vector3(Screen.width, Screen.height, 0));

        float widthReduction = (upperRight.x - lowerLeft.x) * padding;
        float heightReduction = (upperRight.y - lowerLeft.y) * padding;

        minX = lowerLeft.x + widthReduction;
        maxX = upperRight.x - widthReduction;
        minY = lowerLeft.y + heightReduction;
        maxY = upperRight.y - heightReduction;
    }

    // Clamp a world-space point to the screen box
    public Vector2 GetClampedPosition(Vector3 position)
    {
        float shrinkW = worldBounds.widht * padding;
        float shrinkH = worldBounds.height * padding;
        float wbMinX = worldBounds.xMin + shrinkW; //wb = world bounds
        float wbMaxX = worldBounds.xMax - shrinkW;
        float wbMinY = worldBounds.yMin + shrinkH; //wb = world bounds
        float wbMaxY = worldBounds.yMax - shrinkH;

        float cx = Mathf.Clamp(curPosition.x, wbMinX, wbMaxX);
        float cy = Mathf.Clamp(curPosition.y, wbMinY, wbMaxY);
        return new Vector2(cx, cy);
    }
    //For camera movement

    public Vector3 GetClampedCameraPosition(Vector3 desiredCamera)
    {
        float cx = Mathf.Clamp(desiredCamCenter.x, minCamX, maxCamX);
        float cy = Mathf.Clamp(desiredCamCenter.y, minCamY, maxCamY);
        return new Vector2(cx, cy, desiredCamera.z);
    }
}


/*some what of a slider:

ScreenBoundriesScript
▾ maxCamY
public Vector3 screenPoint, offset;
[HideInInspector]
public float minX, maxX, minY, maxY;
-------
public Rect worldBounds = new Rect(-960, -540, 1920, 1080); [Range (0f, 0.5f)]
public float padding = 0.02f;
public Camera targetCamera;
O references
public float minCamX { get;
O references
public float maxCamX { get;
O references
public float minCamY { get;
I
private set; }
private set; }
private set; }
public float maxCamY { get; private set; }|
Unity Message | 0 references
void Awake()
{
    if(targetCamera == null)
{
    targetCamera == Camera.main;
}
RecalculateBounds();
}


Void Update()
{
    if(targetCamera.orthographic)
        {
            if(!MathF.Approximately(targetCamera.orthographicSize, lastOrthoSize))
            changed = true;
           
        }
    
}

*/