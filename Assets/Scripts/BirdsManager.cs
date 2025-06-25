using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BirdsManager : MonoBehaviour
{
    [Header("Spawn Settings")]
    public GameObject[] birdPrefabs; // Array of different bird types
    public int birdCount = 30;
    public float spawnRadius = 10f;
    public float boundsRadius = 30f;

    [Header("Behavior Settings")]
    public float cohesionRadius = 3f;
    public float cohesionWeight = 0.5f;
    public float separationRadius = 3f;
    public float separationWeight = 3f;
    public float alignmentRadius = 3f;
    public float alignmentWeight = 0.8f;

    [Header("Spawn Animation")]
    public float spawnDuration = 5f;      // 入场总时间
    public float spawnInterval = 0.3f;    // 每只鸟生成间隔
    private int spawnedCount = 0;         // 已生成数量

    [Header("Boid Settings")]
    public float minSpeed = 2f;
    public float maxSpeed = 5f;

    [Header("Flight Settings")]
    public float minFlightHeight = 5f;
    public float maxFlightHeight = 8f;
    public float heightAdjustForce = 5f;


    private List<BirdController> birds = new List<BirdController>();

    IEnumerator SpawnBirdsOverTime()
    {
        float startTime = Time.time;

        while (spawnedCount < birdCount)
        {
            // 计算生成位置（从边界外飞入）
            // 生成点计算（确保从外围飞入）
            float angle = Random.Range(0, Mathf.PI * 2);
            Vector3 spawnDir = new Vector3(Mathf.Cos(angle), 0.3f, Mathf.Sin(angle));
            Vector3 spawnPos = transform.position + spawnDir.normalized * boundsRadius * 1.5f;

            // 确保初始高度在范围内
            spawnPos.y = Random.Range(minFlightHeight, maxFlightHeight);

            /* Vector3 spawnDir = Random.onUnitSphere;
             spawnDir.y = Mathf.Clamp(spawnDir.y, 0.2f, 1f); // 确保有一定高度

             Vector3 spawnPos = transform.position + spawnDir.normalized * boundsRadius * 1.2f;
 */
            // 生成鸟
            GameObject prefab = birdPrefabs[Random.Range(0, birdPrefabs.Length)];
            GameObject birdObj = Instantiate(prefab, spawnPos, Quaternion.LookRotation(-spawnDir));
            birdObj.transform.parent = transform;

            // 设置初始速度（飞向中心）
            Rigidbody rb = birdObj.GetComponent<Rigidbody>();
            rb.linearVelocity = -spawnDir.normalized * Random.Range(minSpeed, maxSpeed);

            // 添加到列表
            BirdController bird = birdObj.GetComponent<BirdController>();
            birds.Add(bird);

            spawnedCount++;
            yield return new WaitForSeconds(spawnInterval);
        }
    }

    void Start()
    {
        if (birdPrefabs == null || birdPrefabs.Length == 0)
        {
            Debug.LogError("Bird prefabs not assigned!");
            return;
        }
        StartCoroutine(SpawnBirdsOverTime());
    }

    void Update()
    {
        // Pre-calculate all positions and velocities
        Vector3[] positions = new Vector3[birds.Count];
        Vector3[] velocities = new Vector3[birds.Count];

        for (int i = 0; i < birds.Count; i++)
        {
            positions[i] = birds[i].transform.position;
            velocities[i] = birds[i].GetComponent<Rigidbody>().linearVelocity;
        }

        // Process each bird
        for (int i = 0; i < birds.Count; i++)
        {
            Vector3 cohesion = Vector3.zero;
            Vector3 separation = Vector3.zero;
            Vector3 alignment = Vector3.zero;
            int cohesionNeighbors = 0;
            int separationNeighbors = 0;
            int alignmentNeighbors = 0;

            for (int j = 0; j < birds.Count; j++)
            {
                if (i == j) continue;

                float dist = Vector3.Distance(positions[i], positions[j]);

                // Cohesion calculation
                if (dist < cohesionRadius)
                {
                    cohesion += positions[j];
                    cohesionNeighbors++;
                }

                // Separation calculation
                if (dist < separationRadius)
                {
                    separation += (positions[i] - positions[j]) / (dist + 0.0001f);
                    separationNeighbors++;
                }

                // Alignment calculation
                if (dist < alignmentRadius)
                {
                    alignment += velocities[j];
                    alignmentNeighbors++;
                }
            }

            // Calculate averages
            if (cohesionNeighbors > 0)
            {
                cohesion = (cohesion / cohesionNeighbors) - positions[i];
            }

            if (separationNeighbors > 0)
            {
                separation = separation / separationNeighbors;
            }

            if (alignmentNeighbors > 0)
            {
                alignment = (alignment / alignmentNeighbors).normalized * birds[i].maxSpeed;
                alignment -= velocities[i];
            }

            // 修改boundsAvoidance计算部分：
            Vector3 boundsAvoidance = Vector3.zero;
            float distToCenter = Vector3.Distance(positions[i], transform.position);

            // 渐进式边界控制（离中心越远拉力越强）
            if (distToCenter > boundsRadius * 0.7f)
            {
                float pullFactor = Mathf.Pow((distToCenter - boundsRadius * 0.7f) / (boundsRadius * 0.3f), 2);
                boundsAvoidance = (transform.position - positions[i]).normalized *
                                 (birds[i].boundsWeight * (1 + pullFactor * 3));

                // 90%边界外强制减速
                if (distToCenter > boundsRadius * 0.9f)
                {
                    birds[i].GetComponent<Rigidbody>().linearVelocity *= 0.95f;
                }
            }

            // Update bird
            birds[i].UpdateBoid(cohesion * cohesionWeight,
                   separation * separationWeight,
                   alignment * alignmentWeight, // 新增权重
                   boundsAvoidance);
        }
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, boundsRadius);
    }
}