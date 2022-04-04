namespace BehaviorTree
{
    public class NullNode : Node
    {
        public override string Description => "Internal only null node";

        public static Node instance = CreateInstance<NullNode>();
    }
}
