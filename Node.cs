using UnityEngine;
using System;
using System.Collections.Generic;

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

        public Action updateHeat;

        public Dictionary<string, dynamic> TreeBlackboard;

        /// <summary>
        /// Tree private blackboard
        /// </summary>
        public Dictionary<string, dynamic> Blackboard;

        public dynamic BlackboardGet(string key)
        {
            if (Blackboard.ContainsKey(key))
            {
                return Blackboard[key];
            }
            return TreeBlackboard[key];
        }

        public void BlackboardSet(string key, dynamic value)
        {
            if (Debug.isDebugBuild && TreeBlackboard.ContainsKey(key))
            {
                Debug.LogWarning($"Shadowing Tree Blackboard value at {key}");
            }
            Blackboard[key] = value;
        }

        public void TreeBlackboardSet(string key, dynamic value)
        {
            if (Debug.isDebugBuild && Blackboard.ContainsKey(key))
            {
                Debug.LogWarning($"Tree Blackboard value at {key} shadowed by private blackboard value");
            }
            TreeBlackboard[key] = value;
        }

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
