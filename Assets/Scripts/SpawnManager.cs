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

    // Diagnostics: keep track of spawned placeholders for tag validation
    private readonly List<GameObject> spawnedPlaceholders = new List<GameObject>();

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

            // Initialize ObjectScript state (counters, totals, etc.) now that vehicles are known
            objectScript.Initialize();
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

            // Track for diagnostics
            if (instance != null && !spawnedPlaceholders.Contains(instance))
                spawnedPlaceholders.Add(instance);
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

            // ==== NEW: Random rotation and scale ====
            float randomZ = Random.Range(0f, 360f);
            instance.transform.localRotation = Quaternion.Euler(0f, 0f, randomZ);

            float randomScale = Random.Range(0.8f, 1.2f); // adjust min/max as needed
            instance.transform.localScale = new Vector3(randomScale, randomScale, 1f);
            // ======================================

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
            // Scene object – safe to reparent (keep prefab-authored local transform)
            instance = source;
            instance.transform.SetParent(parent, worldPositionStays: false);
        }
        else
        {
            // Prefab asset – instantiate safely (keep prefab-authored local transform)
            instance = Instantiate(source, parent, false);
            instance.name = source.name;
            try { instance.tag = source.tag; } catch { Debug.LogWarning($"SpawnManager: Tag '{source.tag}' is not defined in Tags. Using current tag on '{instance.name}'."); }
            instance.layer = source.layer;
        }

        ResetTransform(instance.transform);
        return instance;
    }

    // Preserve prefab rotation, scale, anchors and pivot. Only zero the local position.
    private void ResetTransform(Transform t)
    {
        if (t == null) return;

        if (t is RectTransform rt)
        {
            // Do NOT touch anchorMin/Max, pivot, rotation, or scale.
            rt.anchoredPosition = Vector2.zero;
        }
        else
        {
            // Do NOT touch rotation or scale.
            t.localPosition = Vector3.zero;
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

    // Diagnostics only: verify tag usage between cars and placeholders
    private void ValidateTags(GameObject[] cars, List<GameObject> placeholders)
    {
        if ((cars == null || cars.Length == 0) && (placeholders == null || placeholders.Count == 0)) return;

        var carTags = new Dictionary<string, int>();
        var phTags = new Dictionary<string, int>();

        System.Action<GameObject[], Dictionary<string, int>, string> addRange = (arr, dict, label) =>
        {
            if (arr == null) return;
            foreach (var go in arr)
            {
                if (go == null) continue;
                string tag = go.tag;
                if (string.IsNullOrEmpty(tag) || tag == "Untagged")
                {
                    Debug.LogWarning($"SpawnManager: {label} '{go.name}' has no tag or is 'Untagged'.");
                }
                if (!dict.ContainsKey(tag)) dict[tag] = 0;
                dict[tag]++;
            }
        };

        System.Action<List<GameObject>, Dictionary<string, int>, string> addList = (list, dict, label) =>
        {
            if (list == null) return;
            foreach (var go in list)
            {
                if (go == null) continue;
                string tag = go.tag;
                if (string.IsNullOrEmpty(tag) || tag == "Untagged")
                {
                    Debug.LogWarning($"SpawnManager: {label} '{go.name}' has no tag or is 'Untagged'.");
                }
                if (!dict.ContainsKey(tag)) dict[tag] = 0;
                dict[tag]++;
            }
        };

        addRange(cars, carTags, "Car");
        addList(placeholders, phTags, "Placeholder");

        foreach (var kv in phTags)
        {
            if (kv.Key == "Untagged") continue;
            if (!carTags.ContainsKey(kv.Key))
            {
                Debug.LogWarning($"SpawnManager: No car found with tag '{kv.Key}' to match {kv.Value} placeholder(s).");
            }
        }

        foreach (var kv in carTags)
        {
            if (kv.Key == "Untagged") continue;
            if (!phTags.ContainsKey(kv.Key))
            {
                Debug.LogWarning($"SpawnManager: No placeholder found with tag '{kv.Key}' to match {kv.Value} car(s).");
            }
        }

        // Summary
        Debug.Log($"SpawnManager: Tag summary -> Cars: [{string.Join(", ", carTags)}] | Placeholders: [{string.Join(", ", phTags)}]");
    }
}
