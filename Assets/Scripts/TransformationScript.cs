using UnityEngine;

public class TransformationScript : MonoBehaviour
{
    public ObjectScript objScript;

    void Update()
    {
        if (objScript.lastDragged != null)
        {
            if (Input.GetKey(KeyCode.Z))
            {
                objScript.lastDragged.GetComponent<RectTransform>().transform.Rotate(
                    0, 0, Time.deltaTime * 45f);
            }

            if (Input.GetKey(KeyCode.X))
            {
                objScript.lastDragged.GetComponent<RectTransform>().transform.Rotate(
                    0, 0, -Time.deltaTime * 45f);
            }

            if (Input.GetKey(KeyCode.UpArrow))
            {
                if (objScript.lastDragged.GetComponent<RectTransform>().transform.localScale.y < 1.1f)
                {
                    objScript.lastDragged.GetComponent<RectTransform>().transform.localScale =
                        new Vector3(objScript.lastDragged.GetComponent<RectTransform>().transform.localScale.x,
                                    objScript.lastDragged.GetComponent<RectTransform>().transform.localScale.y + 0.005f, 1f);
                }
            }
            if (Input.GetKey(KeyCode.DownArrow))
            {
                if (objScript.lastDragged.GetComponent<RectTransform>().transform.localScale.y > 0.3f)
                {
                    objScript.lastDragged.GetComponent<RectTransform>().transform.localScale =
                        new Vector3(objScript.lastDragged.GetComponent<RectTransform>().transform.localScale.x,
                                    objScript.lastDragged.GetComponent<RectTransform>().transform.localScale.y - 0.005f, 1f);
                }
            }
            if (Input.GetKey(KeyCode.LeftArrow))
            {
                if (objScript.lastDragged.GetComponent<RectTransform>().transform.localScale.y > 0.3f)
                {
                    objScript.lastDragged.GetComponent<RectTransform>().transform.localScale =
                        new Vector3(objScript.lastDragged.GetComponent<RectTransform>().transform.localScale.x - 0.005f,
                                    objScript.lastDragged.GetComponent<RectTransform>().transform.localScale.y, 1f);
                }
            }

            if (Input.GetKey(KeyCode.RightArrow))
            {
                if (objScript.lastDragged.GetComponent<RectTransform>().transform.localScale.y < 1.1f)
                {
                    objScript.lastDragged.GetComponent<RectTransform>().transform.localScale =
                        new Vector3(objScript.lastDragged.GetComponent<RectTransform>().transform.localScale.x + 0.005f,
                                    objScript.lastDragged.GetComponent<RectTransform>().transform.localScale.y , 1f);
                }
            }
        }
    }
}
