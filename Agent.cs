using UnityEngine;

namespace BehaviorTree
{
    public class Agent : MonoBehaviour
    {
        public BehaviorTree tree;

        // Start is called before the first frame update
        void Start()
        {
            tree = tree.Clone(this);
        }

        // Update is called once per frame
        void Update()
        {
            tree.Update();
        }
    }
}