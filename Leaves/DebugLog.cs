using UnityEngine;

namespace BehaviorTree
{
    public class DebugLog : Leaf
    {
        public override string Description => "Print a log message";

        public enum DebugLevel
        {
            Log,
            Warning,
            Error
        }

        public DebugLevel debugLevel = DebugLevel.Log;

        public string text = "";

        protected override State OnUpdate()
        {
            switch (debugLevel)
            {
                case DebugLevel.Log:
                    Debug.Log(text);
                    break;
                case DebugLevel.Warning:
                    Debug.LogWarning(text);
                    break;
                case DebugLevel.Error:
                    Debug.LogError(text);
                    break;
            }

            return State.Success;
        }
    }
}
