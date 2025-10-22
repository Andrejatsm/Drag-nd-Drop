using UnityEngine;

// TransformationScript
// Handles rotation and scaling for the currently selected vehicle (ObjectScript.lastDragged)
// when it is NOT being dragged (ObjectScript.drag == false).
// PC controls (kept as-is):
// - Rotate: Z (ccw), X (cw)
// - Scale: Arrow keys (Up/Down change Y, Left/Right change X) with min/max clamps
// Notes for future mobile support:
// - Add pinch-to-zoom and two-finger rotate here, guarded behind platform checks or input system.
// - Respect drag state and isPlaced so gestures don't interfere with dragging or placed items.
// - Consider centralizing min/max scale and rotation tolerance in a config.
public class TransformationScript : MonoBehaviour
{
    void Update()
    {
        // Work on the last clicked/selected object
        var go = ObjectScript.lastDragged;
        if (go == null) return;

        // Transform only when NOT dragging (requested behavior)
        if (ObjectScript.drag) return;

        // Do not allow changes after the car is placed
        var drag = go.GetComponent<DragAndDropScript>();
        if (drag != null && drag.isPlaced) return;

        var rt = go.GetComponent<RectTransform>();
        if (rt == null) return;

        // Rotation (PC): Z/X keys
        if (Input.GetKey(KeyCode.Z))
        {
            rt.transform.Rotate(0, 0, Time.deltaTime * 15f);
        }

        if (Input.GetKey(KeyCode.X))
        {
            rt.transform.Rotate(0, 0, -Time.deltaTime * 15f);
        }

        // Non-uniform scaling (PC): Arrow keys with clamps
        if (Input.GetKey(KeyCode.UpArrow))
        {
            if (rt.transform.localScale.y < 1.5f)
            {
                rt.transform.localScale = new Vector3(
                    rt.transform.localScale.x,
                    rt.transform.localScale.y + 0.005f,
                    1f);
            }
        }

        if (Input.GetKey(KeyCode.DownArrow))
        {
            if (rt.transform.localScale.y > 0.1f)
            {
                rt.transform.localScale = new Vector3(
                    rt.transform.localScale.x,
                    rt.transform.localScale.y - 0.005f,
                    1f);
            }
        }

        if (Input.GetKey(KeyCode.LeftArrow))
        {
            if (rt.transform.localScale.x > 0.1f)
            {
                rt.transform.localScale = new Vector3(
                    rt.transform.localScale.x - 0.005f,
                    rt.transform.localScale.y,
                    1f);
            }
        }

        if (Input.GetKey(KeyCode.RightArrow))
        {
            if (rt.transform.localScale.x < 1.5f)
            {
                rt.transform.localScale = new Vector3(
                    rt.transform.localScale.x + 0.005f,
                    rt.transform.localScale.y,
                    1f);
            }
        }
    }
}
