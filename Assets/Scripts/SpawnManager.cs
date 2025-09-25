using UnityEngine;
using System.Collections.Generic;

public class SpawnManager : MonoBehaviour
{
    [Header("Placeholders")]
    public GameObject[] placeholderPrefabs;  // Placeholder prefabs
    public Transform[] placeholderEmpties;   // Pempty1 - Pempty21

    [Header("Cars")]
    public GameObject[] carPrefabs;          // Car prefabs
    public Transform[] carEmpties;           // Cempty1 - Cempty13

    [Header("Script Holders")]
    public ObjectScript objectScriptHolder;           // ObjectScript holder
    public ScreenBoundriesScript screenBoundriesHolder; // ScreenBoundriesScript holder

    void Start()
    {
        SpawnPlaceholders(placeholderPrefabs, placeholderEmpties);
        SpawnCars(carPrefabs, carEmpties);
    }

    void SpawnPlaceholders(GameObject[] prefabs, Transform[] empties)
    {
        List<int> indices = BuildShuffledIndices(empties.Length);

        for (int i = 0; i < prefabs.Length; i++)
        {
            GameObject obj = Instantiate(prefabs[i], empties[indices[i]]);
            obj.transform.localPosition = Vector3.zero;

            // Assign Script Holder to DropPlaceScript
            DropPlaceScript dropPlace = obj.GetComponent<DropPlaceScript>();
            if (dropPlace != null)
            {
                dropPlace.objScript = objectScriptHolder;
            }

            // If placeholder also needs ObjectScript, assign it here
            ObjectScript objScript = obj.GetComponent<ObjectScript>();
            if (objScript != null)
            {
                objScript = objectScriptHolder; // assign holder
            }
        }
    }

    void SpawnCars(GameObject[] prefabs, Transform[] empties)
    {
        List<int> indices = BuildShuffledIndices(empties.Length);

        for (int i = 0; i < prefabs.Length; i++)
        {
            GameObject obj = Instantiate(prefabs[i], empties[indices[i]]);
            obj.transform.localPosition = Vector3.zero;

            // Assign DragAndDropScript references
            DragAndDropScript drag = obj.GetComponent<DragAndDropScript>();
            if (drag != null)
            {
                drag.objectScr = objectScriptHolder;
                drag.screenBou = screenBoundriesHolder;
            }

            // Dynamically add spawned car to ObjectScript vehicles
            List<GameObject> vehicleList = new List<GameObject>(objectScriptHolder.vehicles ?? new GameObject[0]);
            vehicleList.Add(obj);
            objectScriptHolder.vehicles = vehicleList.ToArray();

            List<Vector2> startList = new List<Vector2>(objectScriptHolder.startCoordinates ?? new Vector2[0]);
            startList.Add(obj.GetComponent<RectTransform>().anchoredPosition);
            objectScriptHolder.startCoordinates = startList.ToArray();
        }
    }

    List<int> BuildShuffledIndices(int length)
    {
        List<int> indices = new List<int>();
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
