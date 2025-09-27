using System;
using System.Collections.Generic;
using UnityEngine;

public class FoodButton : MonoBehaviour
{
    [Header("Food Setup")]
    public string foodName;              // e.g. "Apple"
    public GameObject foodPrefab;        // Prefab to spawn
    public Transform fridgeContainer;    // Parent inside fridge

    // Shared state (accessible anywhere)
    public static List<string> storedFoods = new List<string>();
    private static Dictionary<string, GameObject> spawnedFoods = new Dictionary<string, GameObject>();

    // Event so UI can update when list changes
    public static Action OnFoodListChanged;

    // Called by the UI Button OnClick()
    public void OnFoodButtonClick()
    {
        if (string.IsNullOrEmpty(foodName) || foodPrefab == null || fridgeContainer == null)
        {
            Debug.LogWarning("FoodButton not set up fully. Please assign foodName, prefab and fridgeContainer in the inspector.");
            return;
        }

        if (spawnedFoods.ContainsKey(foodName))
            RemoveFood();
        else
            AddFood();
    }

    private void AddFood()
    {
        // Instantiate as child of fridge container
        GameObject spawned = Instantiate(foodPrefab, fridgeContainer);
        // reset local transform so it sits nicely in the container (tweak for your layout)
        spawned.transform.localPosition = Vector3.zero;
        spawned.transform.localRotation = Quaternion.identity;
        spawned.transform.localScale = Vector3.one;

        // Track
        spawnedFoods[foodName] = spawned;
        storedFoods.Add(foodName);

        // Notify UI
        OnFoodListChanged?.Invoke();

        Debug.Log($"Added {foodName} to fridge and list.");
    }

    private void RemoveFood()
    {
        // Destroy the spawned gameobject
        if (spawnedFoods.TryGetValue(foodName, out GameObject spawned))
        {
            Destroy(spawned);
            spawnedFoods.Remove(foodName);
        }

        // Remove name from the stored list (removes first occurrence)
        storedFoods.Remove(foodName);

        // Notify UI
        OnFoodListChanged?.Invoke();

        Debug.Log($"Removed {foodName} from fridge and list.");
    }
}
