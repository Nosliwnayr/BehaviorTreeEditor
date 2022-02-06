using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace BehaviorTree
{
    /// <summary>
    /// Behavior Tree data
    /// </summary>
    [CreateAssetMenu(fileName = "BehaviorTree", menuName = "BehaviorTreeEditor/BehaviorTree")]
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

        public Node CreateNode(System.Type type)
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

        public void DeleteNode(Node node)
        {
            Undo.RecordObject(this, "Behavior Tree (Delete Node)");
            nodes.Remove(node);

            //AssetDatabase.RemoveObjectFromAsset(node);
            // superceded by this:
            Undo.DestroyObjectImmediate(node);
            AssetDatabase.SaveAssets();
        }

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

        private void Traverse(Node node, System.Action<Node> visitor)
        {
            if (!node)
            {
                return;
            }

            visitor.Invoke(node);
            var children = GetChildren(node);
            children.ForEach((n) => Traverse(n, visitor));
        }

        public BehaviorTree Clone()
        {
            BehaviorTree tree = Instantiate(this);
            tree.rootNode = rootNode.Clone();
            tree.nodes = new List<Node>();
            Traverse(tree.rootNode, (n) =>
            {
                tree.nodes.Add(n);
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
