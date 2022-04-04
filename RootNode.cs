namespace BehaviorTree
{
    public class RootNode : Node
    {
        public override string Description => "Tree Execution Start";

        public Node child;

        public override Node Clone()
        {
            RootNode clone = Instantiate(this);
            clone.child = child.Clone();
            return clone;
        }

        protected override void OnStart()
        {
        }

        protected override void OnStop()
        {
        }

        protected override State OnUpdate()
        {
            return child.Update();
        }
    }
}
