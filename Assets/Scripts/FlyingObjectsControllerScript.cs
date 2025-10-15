using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class FlyingObjectsControllerScript : MonoBehaviour
{
    [HideInInspector]
    public float speed = 1f;
    public float fadeDuration = 1.5f;
    public float waveAmplitude = 25f;
    public float waveFrequency = 1f;
    private ObjectScript objectScript;
    private ScreenBoundriesScript scrreenBoundriesScript;
    private CanvasGroup canvasGroup;
    private RectTransform rectTransform;
    private bool isFadingOut = false;
    private bool isExploding = false;
    private Image image;
    private Color originalColor;

    void Start()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }

        rectTransform = GetComponent<RectTransform>();

        image = GetComponent<Image>();
        originalColor = image.color;
        // Prefer ScriptHolder wiring for consistency with SpawnManager
        var holder = GameObject.Find("ScriptHolder");
        if (holder != null)
        {
            objectScript = holder.GetComponent<ObjectScript>();
            scrreenBoundriesScript = holder.GetComponent<ScreenBoundriesScript>();
        }
        if (objectScript == null) objectScript = FindFirstObjectByType<ObjectScript>();
        if (scrreenBoundriesScript == null) scrreenBoundriesScript = FindFirstObjectByType<ScreenBoundriesScript>();
        if (objectScript == null)
        {
            Debug.LogWarning("FlyingObjectsControllerScript: ObjectScript not found. Lose UI will not show.");
        }
        StartCoroutine(FadeIn());
    }

    // Update is called once per frame
    void Update()
    {
        float waveOffset = Mathf.Sin(Time.time * waveFrequency) * waveAmplitude;
        rectTransform.anchoredPosition += new Vector2(-speed * Time.deltaTime, waveOffset * Time.deltaTime);
        // <-
        if (speed > 0 && transform.position.x < (scrreenBoundriesScript.minX + 80) && !isFadingOut)
        {
            StartCoroutine(FadeOutAndDestroy());
            isFadingOut = true;
        }

        // ->
        if (speed < 0 && transform.position.x > (scrreenBoundriesScript.maxX - 80) && !isFadingOut)
        {
            StartCoroutine(FadeOutAndDestroy());
            isFadingOut = true;
        }

        if(CompareTag("bomb") && !isExploding && RectTransformUtility.RectangleContainsScreenPoint(rectTransform, Input.mousePosition, Camera.main))
        {
            Debug.Log("The cursor collided with a bomb! (without a car)");
            TriggerExplosion();
        }

        // Cursor-only destruction path: only when actually dragging an UNPLACED car
        if (ObjectScript.drag && !isFadingOut &&
            RectTransformUtility.RectangleContainsScreenPoint(rectTransform, Input.mousePosition, Camera.main))
        {
            var dragged = ObjectScript.lastDragged;
            var d = dragged != null ? dragged.GetComponent<DragAndDropScript>() : null;
            if (d != null && d.enabled && !d.isPlaced)
            {
                Debug.Log("The cursor collided with a flying object!");
                StartCoroutine(ShrinkAndDestroy(dragged, 0.5f));
                ObjectScript.lastDragged = null;
                ObjectScript.drag = false;
                StartDestroy();

                // Notify gameplay logic that a car was destroyed
                if (objectScript != null)
                {
                    objectScript.CarDestroyed();
                }
            }
        }
    }

    public void TriggerExplosion()
    {
        isExploding = true;
        objectScript.effects.PlayOneShot(objectScript.audioCli[14], 1000000f);

        if (TryGetComponent<Animator>(out Animator animator))
        {
            animator.SetBool("explode", true);
        }

        image.color = Color.red;
        StartCoroutine(RecoverColor(0.4f));

        StartCoroutine(Vibrate());
        StartCoroutine(WaitBeforeExplode());
    }

    IEnumerator WaitBeforeExplode()
    {
        float radius = 0f;
        if (TryGetComponent<CircleCollider2D>(out CircleCollider2D circleCollider))
        {
            radius = circleCollider.radius * transform.lossyScale.x;

        }
        ExplodeAndDestroy(radius);
        yield return new WaitForSeconds(1f);
        ExplodeAndDestroy(radius);
        Destroy(gameObject);
    }

    void ExplodeAndDestroy(float radius)
    {
        Collider2D[] hitColliders = Physics2D.OverlapCircleAll(transform.position, radius);
        foreach (var hitCollider in hitColliders)
        {
            if (hitCollider != null && hitCollider.gameObject != gameObject)
            {
                FlyingObjectsControllerScript obj =
                    hitCollider.gameObject.GetComponent<FlyingObjectsControllerScript>();

                if (obj != null && !obj.isExploding)
                {
                    obj.StartDestroy();
                }
            }
        }
    }

    public void StartDestroy()
    {
        if (!isFadingOut)
        {
            StartCoroutine(FadeOutAndDestroy());
            isFadingOut = true;

            image.color = Color.cyan;
            StartCoroutine(RecoverColor(0.5f));

            objectScript.effects.PlayOneShot(objectScript.audioCli[13]);

            StartCoroutine(Vibrate());
        }
        }
        IEnumerator Vibrate()
    {
        Vector2 originalPosition = rectTransform.anchoredPosition;
        float duration = 0.3f;
        float elpased = 0f;
        float intensity = 5f;

        while (elpased < duration)
        {
            rectTransform.anchoredPosition =
                originalPosition + Random.insideUnitCircle * intensity;
            elpased += Time.deltaTime;
            yield return null;
        }
        rectTransform.anchoredPosition = originalPosition;
    }

    IEnumerator FadeIn()
    {
        float t = 0f;
        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(0f, 1f, t / fadeDuration);
            yield return null;
        }
        canvasGroup.alpha = 1f;
    }

    IEnumerator FadeOutAndDestroy()
    {
        float t = 0f;
        float startAlpha = canvasGroup.alpha;

        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(startAlpha, 0f, t / fadeDuration);
            yield return null;
        }
        canvasGroup.alpha = 0f;
        Destroy(gameObject);
    }

    IEnumerator ShrinkAndDestroy(GameObject target, float duration)
    {
        Vector3 orginalScale = target.transform.localScale;
        Quaternion orginalRotation = target.transform.rotation;
        float t = 0f;

        while (t < duration)
        {
            t += Time.deltaTime;
            target.transform.localScale = Vector3.Lerp(orginalScale, Vector3.zero, t / duration);
            float angle = Mathf.Lerp(0f, 360f, t / duration);
            target.transform.rotation = Quaternion.Euler(0f, 0f, angle);

            yield return null;
        }
        Destroy(target);
    }

    IEnumerator RecoverColor(float seconds)
    {
        yield return new WaitForSeconds(seconds);
        image.color = originalColor;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        NotifyGameOverIfCar(other.gameObject);
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        NotifyGameOverIfCar(collision.collider.gameObject);
    }

    private void NotifyGameOverIfCar(GameObject go)
    {
        // Treat as car only if DragAndDropScript exists, is enabled, and not placed
        var drag = go.GetComponent<DragAndDropScript>();
        if (drag != null && drag.enabled && !drag.isPlaced)
        {
            // Inform ObjectScript which drives win/lose in this project.
            if (objectScript != null)
            {
                objectScript.CarDestroyed();
            }
        }
    }
}
