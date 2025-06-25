using UnityEngine;

public class FlockAction : ActionNode
{
    private BirdController boid;

    void Start()
    {
        boid = GetComponent<BirdController>();
    }

    public override void PerformAction()
    {
        boid.NormalFlocking();
    }
}
