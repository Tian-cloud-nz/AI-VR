using UnityEngine;

public class ActionNode : DecisionTreeNode
{
    public override DecisionTreeNode MakeDecision()
    {
        return this;
    }

    public virtual void PerformAction() { }
}