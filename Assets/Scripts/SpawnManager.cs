using UnityEngine;
using System.Collections.Generic;

public class SpawnManager : MonoBehaviour
{
    [Header("Placeholders")]
    public GameObject[] placeholderPrefabs;  // Can be prefab assets or scene instances
    public Transform[] placeholderEmpties;   // Pempty1 - Pempty21

    [Header("Cars")]
    public GameObject[] carPrefabs;          // Can be prefab assets or scene instances
    public Transform[] carEmpties;           // Cempty1 - Cempty13

    // Auto-resolved references (taken from ScriptHolder if present)
    private ObjectScript objectScript;
    private ScreenBoundriesScript screenBoundries;

    void Awake()
    {
        // Prefer the explicit ScriptHolder in your scene
        GameObject holder = GameObject.Find("ScriptHolder");
        if (holder != null)
        {
            objectScript = holder.GetComponent<ObjectScript>();
            screenBoundries = holder.GetComponent<ScreenBoundriesScript>();
        }

        // Fallbacks if ScriptHolder is missing components
        if (objectScript == null) objectScript = FindObjectOfType<ObjectScript>();
        if (screenBoundries == null) screenBoundries = FindObjectOfType<ScreenBoundriesScript>();

        if (objectScript == null)
            Debug.LogError("SpawnManager: No ObjectScript found (looked for ScriptHolder first).");
        if (screenBoundries == null)
            Debug.LogError("SpawnManager: No ScreenBoundriesScript found (looked for ScriptHolder first).");
    }

    void Start()
    {
        SpawnPlaceholders(placeholderPrefabs, placeholderEmpties);
        GameObject[] cars = SetupCars(carPrefabs, carEmpties);

        if (objectScript != null && cars != null)
        {
            objectScript.vehicles = cars;

            Vector2[] starts = new Vector2[cars.Length];
            for (int i = 0; i < cars.Length; i++)
            {
                if (cars[i] == null) continue;
                RectTransform rt = cars[i].GetComponent<RectTransform>();
                starts[i] = rt != null ? (Vector2)rt.localPosition : (Vector2)cars[i].transform.localPosition;
            }
            objectScript.startCoordinates = starts;
        }
    }

    void SpawnPlaceholders(GameObject[] objects, Transform[] empties)
    {
        if (objects == null || empties == null) return;

        int count = Mathf.Min(objects.Length, empties.Length);
        List<int> indices = BuildShuffledIndices(empties.Length);

        for (int i = 0; i < count; i++)
        {
            Transform parent = empties[indices[i]];
            GameObject instance = EnsureInstance(objects[i], parent);

            // Give placeholders the ObjectScript (from ScriptHolder)
            DropPlaceScript drop = instance.GetComponent<DropPlaceScript>();
            if (drop == null) drop = instance.AddComponent<DropPlaceScript>();
            drop.objScript = objectScript;
        }
    }

    GameObject[] SetupCars(GameObject[] objects, Transform[] empties)
    {
        if (objects == null || empties == null) return null;

        int count = Mathf.Min(objects.Length, empties.Length);
        List<int> indices = BuildShuffledIndices(empties.Length);

        GameObject[] carsInGivenOrder = new GameObject[count];

        for (int i = 0; i < count; i++)
        {
            Transform parent = empties[indices[i]];
            GameObject instance = EnsureInstance(objects[i], parent);

            // Give cars DragAndDropScript wired with ObjectScript and ScreenBoundriesScript (from ScriptHolder)
            DragAndDropScript drag = instance.GetComponent<DragAndDropScript>();
            if (drag == null) drag = instance.AddComponent<DragAndDropScript>();
            drag.objectScr = objectScript;
            drag.screenBou = screenBoundries;

            carsInGivenOrder[i] = instance;
        }

        return carsInGivenOrder;
    }

    private GameObject EnsureInstance(GameObject source, Transform parent)
    {
        if (source == null || parent == null) return null;

        GameObject instance;
        if (source.scene.IsValid())
        {
            // Scene object – safe to reparent
            instance = source;
            instance.transform.SetParent(parent, worldPositionStays: false);
        }
        else
        {
            // Prefab asset – instantiate safely and explicitly preserve tag/layer
            instance = Instantiate(source, parent, false);
            instance.name = source.name;
            // Ensure tag/layer match the prefab's (Unity should preserve, but make it explicit)
            try { instance.tag = source.tag; } catch { Debug.LogWarning($"SpawnManager: Tag '{source.tag}' is not defined in Tags. Using current tag on '{instance.name}'."); }
            instance.layer = source.layer;
        }

        ResetTransform(instance.transform);
        return instance;
    }

    private void ResetTransform(Transform t)
    {
        if (t == null) return;

        var rt = t as RectTransform;
        if (rt != null)
        {
            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = Vector2.zero;
            rt.localRotation = Quaternion.identity;
            rt.localScale = Vector3.one;
        }
        else
        {
            t.localPosition = Vector3.zero;
            t.localRotation = Quaternion.identity;
            t.localScale = Vector3.one;
        }
    }

    List<int> BuildShuffledIndices(int length)
    {
        List<int> indices = new List<int>(length);
        for (int i = 0; i < length; i++) indices.Add(i);

        for (int i = 0; i < indices.Count; i++)
        {
            int temp = indices[i];
            int randomIndex = Random.Range(i, indices.Count);
            indices[i] = indices[randomIndex];
            indices[randomIndex] = temp;
        }
        return indices;
    }
}
