using UnityEngine;

public class FlyingObjectManager : MonoBehaviour
{
    public void DestroyAllFlyingObjects()
    {
        FlyingObjectsControllerScript[] flyingObjects = Object.FindObjectsByType<FlyingObjectsControllerScript>(
            FindObjectsInactive.Exclude, FindObjectsSortMode.None);

        foreach (FlyingObjectsControllerScript obj  in flyingObjects)
        {
            if(obj == null)
                continue;

            if (obj.CompareTag("bomb"))
            {
                obj.TriggerExplosion();
            }
            else
            {
                obj.StartDestroy(Color.cyan);
            }

        }
    }
}
