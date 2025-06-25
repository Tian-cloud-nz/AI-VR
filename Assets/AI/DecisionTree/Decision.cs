using UnityEngine;

public class Decision : DecisionTreeNode
{
    public DecisionTreeNode trueNode;
    public DecisionTreeNode falseNode;
    public virtual bool TestValue() { return false; }

    public override DecisionTreeNode MakeDecision()
    {
        return TestValue() ? trueNode.MakeDecision() : falseNode.MakeDecision();
    }
}