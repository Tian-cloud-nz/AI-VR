using UnityEngine;

public class FoodNearbyDecision : Decision
{
    private BirdController boid;

    void Start()
    {
        boid = GetComponent<BirdController>();
    }

    public override bool TestValue()
    {
        return boid.HasFoodNearby();
    }
}