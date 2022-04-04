namespace BehaviorTree
{
    public class Repeat : Decorator
    {
        public override string Description => "Repeat child execution forever";

        protected override void OnStart()
        {

        }

        protected override void OnStop()
        {

        }

        protected override State OnUpdate()
        {
            child.Update();
            return State.Running;
        }
    }
}
