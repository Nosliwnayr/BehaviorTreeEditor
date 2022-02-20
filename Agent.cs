using UnityEngine;

namespace BehaviorTree
{
    public class Agent : MonoBehaviour
    {
        public BehaviorTree tree;

        void Start()
        {
            tree = tree.Clone(this);
        }

        void Update()
        {
            tree.Update();
        }
    }
}