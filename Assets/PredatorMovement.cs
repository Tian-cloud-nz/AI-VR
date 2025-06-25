using UnityEngine;

public class PredatorMovement : MonoBehaviour
{
    public float moveSpeed = 5f;
    public float directionChangeInterval = 3f;
    public float chaseDistance = 20f;

    private Vector3 moveDirection;
    private float directionChangeTime;
    private Transform[] boids;

    void Start()
    {
        // 获取所有鸟的引用
        GameObject[] boidObjects = GameObject.FindGameObjectsWithTag("Boid");
        boids = new Transform[boidObjects.Length];
        for (int i = 0; i < boidObjects.Length; i++)
        {
            boids[i] = boidObjects[i].transform;
        }

        ChangeDirection();
    }

    void Update()
    {
        // 检查是否有鸟在追击范围内
        Transform closestBoid = FindClosestBoid();
        if (closestBoid != null &&
            Vector3.Distance(transform.position, closestBoid.position) < chaseDistance)
        {
            // 追击最近的鸟
            moveDirection = (closestBoid.position - transform.position).normalized;
        }
        else if (Time.time > directionChangeTime)
        {
            ChangeDirection();
        }

        // 移动捕食者
        transform.position += moveDirection * moveSpeed * Time.deltaTime;

        // 面向移动方向
        if (moveDirection != Vector3.zero)
        {
            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                Quaternion.LookRotation(moveDirection),
                Time.deltaTime * 5f);
        }
    }

    void ChangeDirection()
    {
        moveDirection = new Vector3(
            Random.Range(-1f, 1f),
            0,
            Random.Range(-1f, 1f)).normalized;

        directionChangeTime = Time.time + directionChangeInterval;
    }

    Transform FindClosestBoid()
    {
        Transform closest = null;
        float closestDistance = float.MaxValue;

        foreach (Transform boid in boids)
        {
            float distance = Vector3.Distance(transform.position, boid.position);
            if (distance < closestDistance)
            {
                closestDistance = distance;
                closest = boid;
            }
        }

        return closestDistance < chaseDistance ? closest : null;
    }
}