using System.Collections.Generic;

namespace BehaviorTree
{
    public abstract class Control : Node
    {
        public List<Node> children = new List<Node>();

        public override Node Clone()
        {
            Control clone = Instantiate(this);
            clone.children = CloneChildren();
            return clone;
        }

        private List<Node> CloneChildren()
        {
            List<Node> clones = new List<Node>();

            foreach (Node child in children)
            {
                if (child == null)
                {
                    clones.Add(NullNode.instance);
                }
                else
                {
                    clones.Add(child.Clone());
                }
            }

            return clones;
        }
    }
}
