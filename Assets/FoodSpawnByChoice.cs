// FoodSpawnByChoice.cs
using UnityEngine;

public class FoodSpawnByChoice : MonoBehaviour
{
    [Header("Prefabs in index order (0,1,...)")]
    public GameObject[] foodPrefabs;   // [0]=Burger, [1]=Soup (match button order)

    [Header("Where to spawn")]
    public Transform spawnPoint;       // optional, defaults to this transform

    [Header("PlayerPrefs key (must match ChoiceUI)")]
    public string saveKey = "ChosenFoodIndex";

    [Header("Options")]
    public bool destroyOldChildrenAtStart = true; // helpful if scene reloads
    public bool spawnOnStart = true;

    void Start()
    {
        if (spawnOnStart) SpawnFromSavedChoice();
    }

    public void SpawnFromSavedChoice()
    {
        int index = PlayerPrefs.GetInt(saveKey, -1);
        if (index < 0 || foodPrefefsInvalid(index))
        {
            Debug.LogWarning($"[FoodSpawnByChoice] No valid choice for '{saveKey}' (value={index}).");
            return;
        }

        var parent = spawnPoint != null ? spawnPoint : transform;

        if (destroyOldChildrenAtStart)
        {
            for (int i = parent.childCount - 1; i >= 0; i--)
                Destroy(parent.GetChild(i).gameObject);
        }

        Instantiate(foodPrefabs[index], parent.position, parent.rotation, parent);
        Debug.Log($"[FoodSpawnByChoice] Spawned '{foodPrefabs[index].name}' for choice {index}.");
    }

    bool foodPrefefsInvalid(int index)
    {
        return foodPrefabs == null || index >= foodPrefabs.Length || foodPrefabs[index] == null;
    }
}
