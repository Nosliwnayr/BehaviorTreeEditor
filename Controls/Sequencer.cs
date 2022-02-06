namespace BehaviorTree
{
    public class Sequencer : Control
    {
        int currentChild;

        protected override void OnStart()
        {
            currentChild = 0;
        }

        protected override void OnStop()
        {

        }

        protected override State OnUpdate()
        {
            // Execute current child
            State childState = children[currentChild].Update();

            // increment current child if node succeeded
            if (childState == State.Success)
            {
                currentChild++;

                // if this node successfully executed all its children: return success, otherwise keep running
                return currentChild == children.Count ? State.Success : State.Running;
            }

            return childState;
        }
    }
}
