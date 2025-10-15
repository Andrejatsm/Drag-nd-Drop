using UnityEngine;

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

        if (Input.GetKey(KeyCode.Z))
        {
            rt.transform.Rotate(0, 0, Time.deltaTime * 15f);
        }

        if (Input.GetKey(KeyCode.X))
        {
            rt.transform.Rotate(0, 0, -Time.deltaTime * 15f);
        }

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
