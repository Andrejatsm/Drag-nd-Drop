using UnityEngine;
using System.Collections.Generic;

public class SpawnManager : MonoBehaviour
{
    [Header("Placeholders")]
    public GameObject[] placeholderPrefabs;
    public Transform[] placeholderEmpties;

[Header("Cars")]
    public GameObject[] carPrefabs;
    public Transform[] carEmpties;

    private ObjectScript objectScript;
    private ScreenBoundries screenBoundries;

    private readonly List<GameObject> spawnedPlaceholders = new List<GameObject>();

    void Awake()
    {
        // Try to find the ScriptHolder
        GameObject holder = GameObject.Find("ScriptHolder");
        if (holder != null)
        {
            objectScript = holder.GetComponent<ObjectScript>();
            screenBoundries = holder.GetComponent<ScreenBoundries>();
        }

        // Fallback search if missing
        if (objectScript == null) objectScript = FindFirstObjectByType<ObjectScript>();
        if (screenBoundries == null) screenBoundries = FindFirstObjectByType<ScreenBoundries>();

        if (objectScript == null)
            Debug.LogError("SpawnManager: ObjectScript not found.");
        if (screenBoundries == null)
            Debug.LogError("SpawnManager: ScreenBoundries not found.");
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

            DropPlaceScript drop = instance.GetComponent<DropPlaceScript>();
            if (drop == null) drop = instance.AddComponent<DropPlaceScript>();
            drop.objScript = objectScript;

            if (!spawnedPlaceholders.Contains(instance))
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

            // Ensure a CanvasGroup exists for DragAndDropScript usage (blocksRaycasts/alpha control)
            var cg = instance.GetComponent<CanvasGroup>();
            if (cg == null)
            {
                cg = instance.AddComponent<CanvasGroup>();
                cg.alpha = 1f;
                cg.blocksRaycasts = true;
                cg.interactable = true;
            }

            // Give cars their drag and screen references
            DragAndDropScript drag = instance.GetComponent<DragAndDropScript>();
            if (drag == null) drag = instance.AddComponent<DragAndDropScript>();
            drag.objectScr = objectScript;
            drag.screenBou = screenBoundries;

            // Random rotation and scale (Android-friendly)
            float randomZ = Random.Range(0f, 360f);
            instance.transform.localRotation = Quaternion.Euler(0f, 0f, randomZ);

            float randomScale = Random.Range(0.8f, 1.2f);
            instance.transform.localScale = new Vector3(randomScale, randomScale, 1f);

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
            instance = source;
            instance.transform.SetParent(parent, false);
        }
        else
        {
            instance = Instantiate(source, parent, false);
            instance.name = source.name;
            try { instance.tag = source.tag; }
            catch { Debug.LogWarning($"SpawnManager: Tag '{source.tag}' missing in Tag Manager."); }
            instance.layer = source.layer;
        }

        ResetTransform(instance.transform);
        return instance;
    }

    private void ResetTransform(Transform t)
    {
        if (t == null) return;

        if (t is RectTransform rt)
            rt.anchoredPosition = Vector2.zero;
        else
            t.localPosition = Vector3.zero;
    }

    List<int> BuildShuffledIndices(int length)
    {
        List<int> indices = new List<int>(length);
        for (int i = 0; i < length; i++) indices.Add(i);

        for (int i = 0; i < indices.Count; i++)
        {
            int randomIndex = Random.Range(i, indices.Count);
            (indices[i], indices[randomIndex]) = (indices[randomIndex], indices[i]);
        }
        return indices;
    }

}
