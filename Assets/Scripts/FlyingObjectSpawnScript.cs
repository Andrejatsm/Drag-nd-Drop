using UnityEngine;

public class FlyingObjectSpawnScript : MonoBehaviour
{
    ScreenBoundries screenBoundriesScript;
    public GameObject[] cludsPrefabs;
    public GameObject[] objectPrefabs;
    public Transform spawnPoint;

    public float cloudSpawnInterval = 2f;
    public float objectSpawnInterval = 3f;
    private float minY, maxY;
    public float cloudMinSpeed = 1.5f;
    public float cloudMaxSpeed = 150f;
    public float objectMinSpeed = 2f;
    public float objectMaxSpeed = 200f;

    void Start()
    {
        screenBoundriesScript = FindAnyObjectByType<ScreenBoundries>();
        if (screenBoundriesScript != null)
        {
            var wb = screenBoundriesScript.worldBounds;
            minY = wb.yMin;
            maxY = wb.yMax;
        }
        else
        {
            // Fallback to a sensible vertical range if bounds are missing
            minY = -540f;
            maxY = 540f;
        }
        InvokeRepeating(nameof(SpawnCloud), 0f, cloudSpawnInterval);
        InvokeRepeating(nameof(SpawnObject), 0f, objectSpawnInterval);
    }

    void SpawnCloud()
    {
        if (cludsPrefabs.Length == 0 || spawnPoint == null)
            return;

        GameObject cloudPrefab = cludsPrefabs[Random.Range(0, cludsPrefabs.Length)];
        float y = Random.Range(minY, maxY);
        Vector3 spawnPosition = new Vector3(spawnPoint.position.x, y, spawnPoint.position.z);
        GameObject cloud =
            Instantiate(cloudPrefab, spawnPosition, Quaternion.identity, spawnPoint.parent);
        cloud.transform.SetAsLastSibling();
        float movementSpeed = Random.Range(cloudMinSpeed, cloudMaxSpeed);
        FlyingObjectsControllerScript controller =
            cloud.GetComponent<FlyingObjectsControllerScript>();
        if (controller != null)
            controller.speed = movementSpeed; // positive speed => move left in controller
    }

    void SpawnObject()
    {
        if (objectPrefabs.Length == 0 || spawnPoint == null)
            return;

        GameObject objectPrefab = objectPrefabs[Random.Range(0, objectPrefabs.Length)];
        float y = Random.Range(minY, maxY);

        Vector3 spawnPosition = new Vector3(-spawnPoint.position.x, y, spawnPoint.position.z);

        GameObject flyingObject =
            Instantiate(objectPrefab, spawnPosition, Quaternion.identity, spawnPoint.parent);
        flyingObject.transform.SetAsLastSibling();
        float movementSpeed = Random.Range(objectMinSpeed, objectMaxSpeed);
        FlyingObjectsControllerScript controller =
            flyingObject.GetComponent<FlyingObjectsControllerScript>();
        if (controller != null)
            controller.speed = -movementSpeed; // negative speed => move right in controller
    }
}
