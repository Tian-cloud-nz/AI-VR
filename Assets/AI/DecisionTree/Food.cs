using UnityEngine;

public class Food : MonoBehaviour
{
    public float nutritionalValue = 0.3f;

    void Start()
    {
        // 随机大小和颜色
        transform.localScale = Vector3.one * Random.Range(0.5f, 1.5f);
        GetComponent<Renderer>().material.color =
            new Color(Random.value, Random.value, Random.value);
    }
}