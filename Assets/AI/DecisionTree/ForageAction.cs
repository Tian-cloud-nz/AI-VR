using UnityEngine;

// 确保这是项目中唯一的ForageAction类定义
public class ForageAction : ActionNode
{
    private BirdController boid;

    void Start()
    {
        boid = GetComponent<BirdController>();
    }

    public override void PerformAction()
    {
        boid.SeekFood();
    }
}