using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEditor;

namespace BehaviorTree
{
    /// <summary>
    /// Behavior Tree data
    /// </summary>
    [CreateAssetMenu(fileName = "BehaviorTree", menuName = "BehaviorTree", order = int.MaxValue)]
    public class BehaviorTree : ScriptableObject
    {
        /// <summary>
        /// Root node of the behavior tree
        /// </summary>
        public Node rootNode;

        /// <summary>
        /// State of the tree as a whole (root node state)
        /// </summary>
        public Node.State treeState { get => rootNode.state; }

        /// <summary>
        /// What nodes belong to this tree. A tree could have detached nodes in the editor
        /// </summary>
        public List<Node> nodes = new List<Node>();


        /// <summary>
        /// Tree level blackboard. All agents that use this tree will share this blackboard instance
        /// </summary>
        static public Dictionary<string, dynamic> TreeBlackboard = new Dictionary<string, dynamic> { };

        /// <summary>
        /// Tree private blackboard
        /// </summary>
        static private Dictionary<string, dynamic> Blackboard = new Dictionary<string, dynamic> { };

        /// <summary>
        /// Execute the behavior tree (root node)
        /// </summary>
        /// <returns>State of the behavior tree (root node)</returns>
        public Node.State Update()
        {
            if (treeState == Node.State.Running)
            {
                rootNode.Update();
            }
            return treeState;
        }

        /// <summary>
        /// Editor create a node in this tree
        /// </summary>
        /// <param name="type">Type of node to create</param>
        /// <returns>The new node</returns>
        public Node CreateNode(Type type)
        {
            Node node = ScriptableObject.CreateInstance(type) as Node;
            node.name = type.Name;
            node.guid = GUID.Generate().ToString();

            Undo.RecordObject(this, "Behavior Tree (Create Node)");
            nodes.Add(node);

            if (!Application.isPlaying)
            {
                AssetDatabase.AddObjectToAsset(node, this);
            }

            Undo.RegisterCreatedObjectUndo(node, "Behavior Tree (Create Node)");

            AssetDatabase.SaveAssets();
            return node;
        }

        /// <summary>
        /// Editor delete node
        /// </summary>
        /// <param name="node">The node to delete</param>
        public void DeleteNode(Node node)
        {
            Undo.RecordObject(this, "Behavior Tree (Delete Node)");
            nodes.Remove(node);

            Undo.DestroyObjectImmediate(node);
            AssetDatabase.SaveAssets();
        }

        /// <summary>
        /// Editor create parent child relationship
        /// </summary>
        /// <param name="parent">The parent node</param>
        /// <param name="child">The child node</param>
        public void AddChild(Node parent, Node child)
        {
            if (parent is Decorator)
            {
                Decorator decorator = parent as Decorator;
                Undo.RecordObject(decorator, "Behavior Tree (Add Child)");
                decorator.child = child;
                EditorUtility.SetDirty(decorator);
            }
            else if (parent is Control)
            {
                Control control = parent as Control;
                Undo.RecordObject(control, "Behavior Tree (Add Child)");
                control.children.Add(child);
                EditorUtility.SetDirty(control);
            }
            else if (parent is RootNode)
            {
                RootNode rootNode = parent as RootNode;
                Undo.RecordObject(rootNode, "Behavior Tree (Add Child)");
                rootNode.child = child;
                EditorUtility.SetDirty(rootNode);
            }
        }

        /// <summary>
        /// Editor remove parent child relationship
        /// </summary>
        /// <param name="parent">The parent node</param>
        /// <param name="child">The child node</param>
        public void RemoveChild(Node parent, Node child)
        {
            if (parent is Decorator)
            {
                Decorator decorator = parent as Decorator;
                Undo.RecordObject(decorator, "Behavior Tree (Remove Child)");
                decorator.child = null;
                EditorUtility.SetDirty(decorator);
            }
            else if (parent is Control)
            {
                Control control = parent as Control;
                Undo.RecordObject(control, "Behavior Tree (Remove Child)");
                control.children.Remove(child);
                EditorUtility.SetDirty(control);
            }
            else if (parent is RootNode)
            {
                RootNode rootNode = parent as RootNode;
                Undo.RecordObject(rootNode, "Behavior Tree (Remove Child)");
                rootNode.child = null;
                EditorUtility.SetDirty(rootNode);
            }
        }

        /// <summary>
        /// Get the children of a node
        /// </summary>
        /// <param name="parent">The parent node</param>
        /// <returns>The children of the node</returns>
        public List<Node> GetChildren(Node parent)
        {
            List<Node> children = new List<Node>();

            if (parent is Decorator)
            {
                Decorator decorator = parent as Decorator;

                if (decorator.child)
                {
                    children.Add(decorator.child);
                }
            }
            else if (parent is RootNode)
            {
                RootNode rootNode = parent as RootNode;

                if (rootNode.child)
                {
                    children.Add(rootNode.child);
                }
            }
            else if (parent is Control)
            {
                Control control = parent as Control;
                children = control.children;
            }

            return children;
        }

        /// <summary>
        /// Traverse the node tree calling the visitor action on each node
        /// </summary>
        /// <param name="node">root node to traverse</param>
        /// <param name="visitor">action to call on each node</param>
        private void Traverse(Node node, Action<Node> visitor)
        {
            if (!node)
            {
                return;
            }

            visitor.Invoke(node);
            var children = GetChildren(node);
            children.ForEach((n) => Traverse(n, visitor));
        }

        /// <summary>
        /// Runtime clone tree
        /// </summary>
        /// <param name="parent">The owner of the new clone</param>
        /// <returns>The cloned tree</returns>
        public BehaviorTree Clone(object parent)
        {
            BehaviorTree tree = Instantiate(this);
            tree.rootNode = rootNode.Clone();
            tree.nodes = new List<Node>();
            Traverse(tree.rootNode, (n) =>
            {
                tree.nodes.Add(n);
                if (n is Leaf)
                {
                    Leaf l = n as Leaf;
                    l.SetParent(parent);
                }
            });
            // TODO: optimize this from N^2 if it's an issue
            foreach (Node node in nodes)
            {
                Node clone = tree.nodes.Find((clone) => node.guid == clone.guid);
                if (clone == null)
                {
                    tree.nodes.Add(node.Clone());
                }
            }
            return tree;
        }
    }
}
