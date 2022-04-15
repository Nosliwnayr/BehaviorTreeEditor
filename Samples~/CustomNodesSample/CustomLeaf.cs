using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BehaviorTree;

public class CustomLeaf : Leaf
{
    public override string Description => "Example custom leaf";

    protected override void OnStart()
    {
        Debug.Log("Custom leaf Starting");
    }

    protected override State OnUpdate()
    {
        Debug.Log("Custom leaf Updating");
        return State.Success;
    }

    protected override void OnStop()
    {
        Debug.Log("Custom Leaf Stopping");
    }
}
