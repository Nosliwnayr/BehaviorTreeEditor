namespace BehaviorTree
{
    public abstract class Decorator : Node
    {
        public Node child;

        public override Node Clone()
        {
            Decorator clone = Instantiate(this);
            if (child != null)
            {
                clone.child = child.Clone();
            }
            return clone;
        }
    }
}
