using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.Callbacks;

namespace BehaviorTree
{
    public class BehaviorTreeEditor : EditorWindow
    {
        BehaviorTreeView treeView;
        InspectorView inspectorView;

        [MenuItem("Tools/BehaviorTreeEditor")]
        public static void OpenEditor()
        {
            BehaviorTreeEditor wnd = GetWindow<BehaviorTreeEditor>();
            wnd.titleContent = new GUIContent("BehaviorTreeEditor");
        }

        [OnOpenAsset]
        public static bool OnOpenAsset(int instanceId, int line)
        {
            if (Selection.activeObject is not BehaviorTree)
            {
                return false;
            }

            OpenEditor();
            return true;
        }

        public void CreateGUI()
        {
            VisualElement root = rootVisualElement;

            // Import UXML
            var visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(AssetDatabase.GUIDToAssetPath("87974c575fcf127b1a89339bc51b2576"));
            visualTree.CloneTree(root);

            var styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>(AssetDatabase.GUIDToAssetPath("2d662e239946309de9e9eea7ad151345"));
            root.styleSheets.Add(styleSheet);

            treeView = root.Q<BehaviorTreeView>();
            inspectorView = root.Q<InspectorView>();

            treeView.OnNodeSelected = OnNodeSelectionChanged;

            OnSelectionChange();
        }

        private void OnEnable()
        {
            EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
        }

        private void OnDisable()
        {
            EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
        }

        private void OnPlayModeStateChanged(PlayModeStateChange newState)
        {
            if (newState == PlayModeStateChange.EnteredEditMode || newState == PlayModeStateChange.EnteredPlayMode)
            {
                OnSelectionChange();
            }
        }

        private void OnSelectionChange()
        {
            if (treeView == null)
            {
                return;
            }

            if (Selection.activeObject is not BehaviorTree)
            {
                if (Selection.activeGameObject && Selection.activeGameObject.TryGetComponent<Agent>(out Agent agent))
                {
                    treeView.PopulateView(agent.tree);
                }
                return;
            }

            // clear graph contents and create new ones for the currently edited tree
            if (Application.isPlaying || AssetDatabase.CanOpenAssetInEditor(Selection.activeObject.GetInstanceID()))
            {
                treeView.PopulateView(Selection.activeObject as BehaviorTree);
            }
        }

        void OnNodeSelectionChanged(NodeView nodeView)
        {
            inspectorView.UpdateSelection(nodeView);
        }
    }
}
