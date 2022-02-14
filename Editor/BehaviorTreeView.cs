using UnityEngine.UIElements;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BehaviorTree
{
    public class BehaviorTreeView : GraphView
    {
        public Action<NodeView> OnNodeSelected;

        private new class UxmlFactory : UxmlFactory<BehaviorTreeView, GraphView.UxmlTraits> { }
        BehaviorTree tree;

        public BehaviorTreeView()
        {
            Insert(0, new GridBackground());

            this.AddManipulator(new ContentZoomer());
            this.AddManipulator(new ContentDragger());
            this.AddManipulator(new SelectionDragger());
            this.AddManipulator(new RectangleSelector());

            var styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>(AssetDatabase.GUIDToAssetPath("2d662e239946309de9e9eea7ad151345"));
            styleSheets.Add(styleSheet);

            Undo.undoRedoPerformed += OnUndoRedo;
        }

        private void OnUndoRedo()
        {
            PopulateView(tree);
            AssetDatabase.SaveAssets();
        }

        private NodeView FindNodeView(Node node)
        {
            return GetNodeByGuid(node.guid) as NodeView;
        }

        internal void PopulateView(BehaviorTree behaviorTree)
        {
            tree = behaviorTree;

            graphViewChanged -= OnGraphViewChanged;
            // clear any existing elements
            DeleteElements(graphElements);
            graphViewChanged += OnGraphViewChanged;

            if (tree.rootNode == null)
            {
                tree.rootNode = tree.CreateNode(typeof(RootNode)) as RootNode;
                EditorUtility.SetDirty(tree);
                AssetDatabase.SaveAssets();
            }

            // populate node views for existing nodes
            tree.nodes.ForEach(n => CreateNodeView(n));

            // populate edge views for existing edges
            tree.nodes.ForEach(n =>
            {
                if (n is NullNode)
                {
                    return;
                }

                NodeView parentView = FindNodeView(n);
                var children = tree.GetChildren(n);
                for (int i = 0; i < children.Count; i++)
                {
                    if (children[i] is NullNode || children[i] == null)
                    {
                        continue;
                    }

                    NodeView childView = FindNodeView(children[i]);

                    Edge edge = parentView.outputs[i].ConnectTo(childView.input);
                    AddElement(edge);
                }
            });
        }

        private GraphViewChange OnGraphViewChanged(GraphViewChange graphViewChange)
        {
            if (graphViewChange.elementsToRemove != null)
            {
                graphViewChange.elementsToRemove.ForEach(elem =>
                {
                    if (elem is NodeView)
                    {
                        NodeView nodeView = elem as NodeView;
                        tree.DeleteNode(nodeView.node);
                    }
                    else if (elem is Edge)
                    {
                        Edge edge = elem as Edge;
                        NodeView parentView = edge.output.node as NodeView;
                        NodeView childView = edge.input.node as NodeView;
                        tree.RemoveChild(parentView.node, childView.node);
                    }
                });
            }

            if (graphViewChange.edgesToCreate != null)
            {
                graphViewChange.edgesToCreate.ForEach(edge =>
                {
                    NodeView parentView = edge.output.node as NodeView;
                    NodeView childView = edge.input.node as NodeView;

                    tree.AddChild(parentView.node, childView.node);
                }
                );
            }

            return graphViewChange;
        }

        private void CreateNodeView(Node node)
        {
            if (node is NullNode)
            {
                return;
            }

            NodeView nodeView = new NodeView(node);
            nodeView.OnNodeSelected = OnNodeSelected;
            AddElement(nodeView);
        }

        private void CreateNode(System.Type type)
        {
            Node node = tree.CreateNode(type);

            CreateNodeView(node);
        }

        public override List<Port> GetCompatiblePorts(Port startPort, NodeAdapter nodeAdapter)
        {
            // return the list of ports that can connect to the start port
            return ports.ToList().Where(endPort =>
                endPort.direction != startPort.direction && endPort.node != startPort.node
            ).ToList();
        }

        public override void BuildContextualMenu(ContextualMenuPopulateEvent evt)
        {
            evt.menu.AppendSeparator("Leaves");
            var leafTypes = TypeCache.GetTypesDerivedFrom<Leaf>();
            foreach (var type in leafTypes)
            {
                evt.menu.AppendAction(type.Name, (a) => CreateNode(type));
            }

            evt.menu.AppendSeparator("Decorators");
            var decoratorTypes = TypeCache.GetTypesDerivedFrom<Decorator>();
            foreach (var type in decoratorTypes)
            {
                evt.menu.AppendAction(type.Name, (a) => CreateNode(type));
            }

            evt.menu.AppendSeparator("Controllers");
            var controlTypes = TypeCache.GetTypesDerivedFrom<Control>();
            foreach (var type in controlTypes)
            {
                evt.menu.AppendAction(type.Name, (a) => CreateNode(type));
            }
        }
    }
}
