namespace BehaviorTree
{
    public class NullNode : Node
    {
        public static Node instance = CreateInstance<NullNode>();
    }
}
