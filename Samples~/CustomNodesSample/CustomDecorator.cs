using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BehaviorTree;

public class CustomDecorator : Decorator
{
    public override string Description => "Custom Decorator";

    protected override void OnStart()
    {
        Debug.Log("Custom Decorator Starting");
    }

    protected override State OnUpdate()
    {
        Debug.Log("Custom Decorator Updating");
        return child.Update();
    }

    protected override void OnStop()
    {
        Debug.Log("Custom Decorator Stopping");
    }
}
