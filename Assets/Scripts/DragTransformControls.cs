using UnityEngine;

public class DragTransformControls : MonoBehaviour
{
    public float rotateSpeed = 180f;   // degrees/sec
    public float scaleSpeed = 1f;      // units/sec
    public Vector2 scaleClamp = new Vector2(0.5f, 2f);

    void Update()
    {
        if (!ObjectScript.drag || ObjectScript.lastDragged != gameObject) return;

        // Rotate with Q/E
        float rot = (Input.GetKey(KeyCode.Q) ? 1 : 0) - (Input.GetKey(KeyCode.E) ? 1 : 0);
        if (rot != 0)
            transform.Rotate(0, 0, rot * rotateSpeed * Time.deltaTime, Space.Self);

        // Scale with Z/X
        float scl = (Input.GetKey(KeyCode.Z) ? -1 : 0) + (Input.GetKey(KeyCode.X) ? 1 : 0);
        if (Mathf.Abs(scl) > 0.0001f)
        {
            var rt = GetComponent<RectTransform>();
            if (rt != null)
            {
                Vector3 s = rt.localScale + Vector3.one * (scl * scaleSpeed * Time.deltaTime);
                float clamped = Mathf.Clamp(s.x, scaleClamp.x, scaleClamp.y);
                rt.localScale = new Vector3(clamped, clamped, 1f);
            }
        }
    }
}