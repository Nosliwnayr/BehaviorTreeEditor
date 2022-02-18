namespace BehaviorTree
{
    public abstract class Leaf : Node
    {
        protected object parent;

        public void SetParent(object parent)
        {
            this.parent = parent;
        }
    }
}