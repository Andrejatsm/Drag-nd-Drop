using UnityEngine;
using UnityEngine.EventSystems;

public class TransformationScript : MonoBehaviour
{
    public float rotationSpeed = 90f;
    public float scaleSpeed = 0.5f;
    public static bool isTransforming = false;
    private bool rotateCW, rotateCCW, scaleUpY, scaleDownY, scalUpX, scaleDownX;



    void Update()
    {
        if (ObjectScript.lastDragged == null)
            return;

        RectTransform rt = ObjectScript.lastDragged.GetComponent<RectTransform>();

        if (rotateCW)
        {
            rt.Rotate(0, 0, -rotationSpeed * Time.deltaTime);
        }

        if (rotateCCW)
        {
            rt.Rotate(0, 0, rotationSpeed * Time.deltaTime);
        }

        if(scaleUpY && rt.localScale.y < 0.9f)
        {
            rt.localScale += new Vector3(0, scaleSpeed * Time.deltaTime, 0);
        }

        if (scaleDownY && rt.localScale.y > 0.35f)
        {
            rt.localScale -= new Vector3(0, scaleSpeed * Time.deltaTime, 0);
        }

        if (scalUpX && rt.localScale.x < 0.9f)
        {
            rt.localScale += new Vector3(scaleSpeed * Time.deltaTime, 0, 0);
        }

        if (scaleDownX && rt.localScale.x > 0.35f)
        {
            rt.localScale -= new Vector3(scaleSpeed * Time.deltaTime, 0, 0);
        }

        isTransforming = rotateCW || rotateCCW || scaleUpY || scaleDownY || scalUpX || scaleDownX;
    }

    public void StartRotateCW(BaseEventData data) { rotateCW = true; }
    public void StopRotateCW(BaseEventData data) { rotateCW = false; }

    public void StartRotateCCW(BaseEventData data) { rotateCCW = true; }
    public void StopRotateCCW(BaseEventData data) { rotateCCW = false; }

    public void StartScaleUpY(BaseEventData data) { scaleUpY = true; }
    public void StopScaleUpY(BaseEventData data) { scaleUpY = false; }

    public void StartScaleDownY(BaseEventData data) { scaleDownY = true; }
    public void StopScaleDownY(BaseEventData data) { scaleDownY = false; }

    public void StartScaleUpX(BaseEventData data) { scalUpX = true; }
    public void StopScaleUpX(BaseEventData data) { scalUpX = false; }

    public void StartScaleDownX(BaseEventData data) { scaleDownX = true; }
    public void StopScaleDownX(BaseEventData data) { scaleDownX = false; }
}
