using UnityEngine;

public class FoodSpawner : MonoBehaviour
{
    public GameObject foodPrefab;
    public int maxFood = 20;
    public float spawnRadius = 15f;
    public float spawnInterval = 5f;

    private float timer;

    void Start()
    {
        for (int i = 0; i < maxFood / 2; i++)
        {
            SpawnFood();
        }
    }

    void Update()
    {
        timer += Time.deltaTime;
        if (timer >= spawnInterval)
        {
            timer = 0;
            if (GameObject.FindGameObjectsWithTag("Food").Length < maxFood)
            {
                SpawnFood();
            }
        }
    }

    void SpawnFood()
    {
        Vector3 spawnPos = transform.position + Random.insideUnitSphere * spawnRadius;
        spawnPos.y = 0.5f; // 确保食物在地面上
        GameObject food = Instantiate(foodPrefab, spawnPos, Quaternion.identity);
        food.tag = "Food";
    }
}