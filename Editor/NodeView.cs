using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.Experimental.GraphView;
using UnityEditor;

namespace BehaviorTree
{
    public class NodeView : UnityEditor.Experimental.GraphView.Node
    {
        const int HEAT_MAX = 5;

        public Action<NodeView> OnNodeSelected;
        public Node node;
        public Port input;
        public List<Port> outputs = new();
        private VisualElement heatDisplay;
        private float heat;

        public NodeView(Node node) : base(AssetDatabase.GUIDToAssetPath("c713e1ec1c7833b70b77f7005aaa3880"))
        {
            this.node = node;
            title = node.name;

            // metadata to retrieve node from graph view
            viewDataKey = node.guid;

            style.left = node.position.x;
            style.top = node.position.y;

            CreateInputPorts();
            CreateOutputPorts();
            SetupUXMLClasses();

            TextElement description = contentContainer.Q<TextElement>("description");
            description.text = node.Description;

            // store heat display so we can use it for heatmap information
            heatDisplay = contentContainer.Q<VisualElement>("heat-display");
            node.updateHeat = UpdateHeat;
        }

        ~NodeView()
        {
            node.updateHeat = null;
        }

        private void SetupUXMLClasses()
        {
            if (node is Leaf)
            {
                AddToClassList("leaf");
            }
            else if (node is Control)
            {
                AddToClassList("control");
            }
            else if (node is Decorator)
            {
                AddToClassList("decorator");
            }
        }

        private void CreateInputPorts()
        {
            if (node is RootNode)
            {
                return;
            }

            input = InstantiatePort(Orientation.Horizontal, Direction.Input, Port.Capacity.Single, typeof(bool));

            // clear autogenerated name
            inputContainer.Add(input);
        }

        private void CreateOutputPorts()
        {
            if (node is Leaf)
            {
                return;
            }
            else if (node is Control)
            {
                Control control = node as Control;
                outputContainer.Add(new Button(() =>
                {
                    outputContainer.Add(InstantiatePort(Orientation.Horizontal, Direction.Output, Port.Capacity.Single, typeof(bool)));
                }));
                for (int i = 0; i < control.children.Count; i++)
                {
                    Port port = InstantiatePort(Orientation.Horizontal, Direction.Output, Port.Capacity.Single, typeof(bool));
                    outputs.Add(port);
                    outputContainer.Add(port);
                }
            }
            else
            {
                Port port = InstantiatePort(Orientation.Horizontal, Direction.Output, Port.Capacity.Single, typeof(bool));
                outputs.Add(port);
                outputContainer.Add(port);
            }

        }

        public override void SetPosition(Rect newPos)
        {
            base.SetPosition(newPos);
            Undo.RecordObject(node, "Behavior Tree (Change Position)");
            node.position.x = newPos.xMin;
            node.position.y = newPos.yMin;
            EditorUtility.SetDirty(node);
        }

        public override void OnSelected()
        {
            base.OnSelected();
            if (OnNodeSelected != null)
            {
                OnNodeSelected.Invoke(this);
            }
        }

        public void UpdateHeat()
        {
            if (node.running)
            {
                heat += Time.deltaTime;
            }
            else
            {
                heat -= Time.deltaTime / 10;
            }
            heat = Math.Clamp(heat, 0, HEAT_MAX);

            UpdateHeatDisplay();
        }

        private void UpdateHeatDisplay()
        {
            heatDisplay.style.borderTopWidth = heat;
            heatDisplay.style.borderRightWidth = heat;
            heatDisplay.style.borderBottomWidth = heat;
            heatDisplay.style.borderLeftWidth = heat;
            heatDisplay.style.borderTopColor = Color.Lerp(Color.green, Color.red, heat / HEAT_MAX);
            heatDisplay.style.borderRightColor = Color.Lerp(Color.green, Color.red, heat / HEAT_MAX);
            heatDisplay.style.borderBottomColor = Color.Lerp(Color.green, Color.red, heat / HEAT_MAX);
            heatDisplay.style.borderLeftColor = Color.Lerp(Color.green, Color.red, heat / HEAT_MAX);
        }
    }
}