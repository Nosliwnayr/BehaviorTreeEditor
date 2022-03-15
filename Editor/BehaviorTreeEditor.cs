using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.Callbacks;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BehaviorTree
{
    public class BehaviorTreeEditor : EditorWindow
    {
        List<Type> nodeTypes;
        int leaves;
        int controls;
        ListView library;
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

            PopulateLibrary();

            treeView = root.Q<BehaviorTreeView>();
            inspectorView = root.Q<InspectorView>();

            treeView.OnNodeSelected = OnNodeSelectionChanged;

            OnSelectionChange();
        }

        private Button MakeLibraryEntry()
        {
            var result = new Button();

            return result;
        }

        private void BindLibraryEntry(VisualElement e, int i)
        {
            var button = e as Button;
            button.text = nodeTypes[i].Name;
            button.AddToClassList("LibraryEntry");
            Action onClick = () =>
            {
                treeView.CreateNode(nodeTypes[i]);
            };
            button.userData = onClick;
            button.clicked += onClick;
            if (i < leaves)
            {
                button.style.backgroundColor = new Color(244 / 255.0f, 65 / 255.0f, 42 / 255.0f);
            }
            else if (i < leaves + controls)
            {
                button.style.backgroundColor = new Color(166 / 255.0f, 201 / 255.0f, 84 / 255.0f);
            }
            else
            {
                button.style.backgroundColor = new Color(10 / 255.0f, 255 / 255.0f, 255 / 255.0f);
            }
        }

        private void UnbindLibraryEntry(VisualElement e, int i)
        {
            var button = e as Button;
            button.clicked -= button.userData as Action;
        }

        private void PopulateLibrary()
        {
            var leafTypes = TypeCache.GetTypesDerivedFrom<Leaf>();
            leaves = leafTypes.Count();
            var controlTypes = TypeCache.GetTypesDerivedFrom<Control>();
            controls = controlTypes.Count();
            var decoratorTypes = TypeCache.GetTypesDerivedFrom<Decorator>();

            nodeTypes = leafTypes.Union(controlTypes).Union(decoratorTypes).ToList();

            const int itemHeight = 32;

            library = rootVisualElement.Q<ListView>("library");

            library.itemsSource = nodeTypes;
            library.fixedItemHeight = itemHeight;
            library.makeItem = MakeLibraryEntry;
            library.bindItem = BindLibraryEntry;
            library.unbindItem = UnbindLibraryEntry;

            library.selectionType = SelectionType.Single;
            library.onItemsChosen += objects => Debug.Log(objects);
            library.onSelectionChange += objects => Debug.Log(objects);
            library.style.flexGrow = 1.0f;
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
