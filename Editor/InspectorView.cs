using BehaviorTree;
using UnityEngine.UIElements;
using UnityEditor;

public class InspectorView : VisualElement
{
    private new class UxmlFactory : UxmlFactory<InspectorView, UxmlTraits> { }
    Editor editor;

    public InspectorView()
    {

    }

    internal void UpdateSelection(NodeView nodeView)
    {
        // remove any previous children
        Clear();

        UnityEngine.Object.DestroyImmediate(editor);

        editor = Editor.CreateEditor(nodeView.node);
        IMGUIContainer container = new(() =>
        {
            if (editor.target)
            {
                editor.OnInspectorGUI();
            }
        });

        Add(container);
    }
}