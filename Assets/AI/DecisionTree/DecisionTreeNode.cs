using UnityEngine;

public abstract class DecisionTreeNode : MonoBehaviour
{
    public abstract DecisionTreeNode MakeDecision();
}