using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectScript : MonoBehaviour
{
    public GameObject[] vehicles;
    [HideInInspector] public Vector2[] startCoordinates;

    public Canvas can;
    public AudioSource effects;
    public AudioClip[] audioCli;

    [HideInInspector] public bool rightPlace = false;
    public static GameObject lastDragged = null;
    public static bool drag = false;

    
    private int carsAlive;

    void Awake()
    {
        startCoordinates = new Vector2[vehicles.Length];
        carsAlive = vehicles.Length; // start with all cars alive

        for (int i = 0; i < vehicles.Length; i++)
        {
            if (vehicles[i] == null) continue;
            startCoordinates[i] = vehicles[i].GetComponent<RectTransform>().localPosition;
        }
    }

    public void OnCarPlacedCorrectly()
    {
        rightPlace = true;
        GameManager.Instance.CarPlacedCorrectly(); // +100 points
    }

  
    public void OnCarDestroyed(GameObject car)
    {
        if (car != null)
            Destroy(car);

        carsAlive--;

        // If any car is destroyed, game ends immediately
        GameManager.Instance.CarDestroyed();
    }
}
