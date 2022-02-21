using UnityEngine;

namespace BehaviorTree
{
    /// <summary>
    /// Base node for behavior tree nodes
    /// </summary>
    public abstract class Node : ScriptableObject
    {
        /// <summary>
        /// Possible node states
        /// </summary>
        public enum State
        {
            Running,
            Failure,
            Success
        }

        public State state { get; protected set; }
        public bool running { get; private set; } = false;
        public string guid;

        public System.Action updateHeat;

        /// <summary>
        /// Editor position
        /// </summary>
        public Vector2 position;

        /// <summary>
        /// Execute this nodes logic callbacks
        /// </summary>
        /// <returns>The state of the node after execution</returns>
        public State Update()
        {
            if (!running)
            {
                OnStart();
                running = true;
            }

            state = OnUpdate();
            updateHeat?.Invoke();

            if (state == State.Failure || state == State.Success)
            {
                OnStop();
                running = false;
            }

            return state;
        }

        public virtual Node Clone()
        {
            return Instantiate(this);
        }

        /// <summary>
        /// Called when the node begins executing
        /// </summary>
        protected virtual void OnStart() { }
        /// <summary>
        /// Called when the node finishes execution (state becomes Success or Failure)
        /// </summary>
        protected virtual void OnStop() { }
        /// <summary>
        /// Called once per node execution
        /// </summary>
        /// <returns>The state of the node after execution</returns>
        protected virtual State OnUpdate() { return State.Success; }
    }
}
