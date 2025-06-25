using UnityEngine;

public class IsHungryDecision : Decision
{
    private BirdController boid;

    void Start()
    {
        boid = GetComponent<BirdController>();
    }

    public override bool TestValue()
    {
        return boid.IsHungry();
    }
}