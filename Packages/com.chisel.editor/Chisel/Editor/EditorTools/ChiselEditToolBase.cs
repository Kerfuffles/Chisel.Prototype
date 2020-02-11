﻿using Chisel.Components;
using System;
using System.Linq;
using UnityEditor;
using UnityEditor.EditorTools;
using UnityEngine;
using UnityObject = UnityEngine.Object;
 
namespace Chisel.Editors
{
    abstract class ChiselEditToolBase : EditorTool, IChiselToolMode
    {
        // Serialize this value to set a default value in the Inspector.
        [SerializeField] internal Texture2D m_ToolIcon = null;

        public abstract string ToolName { get; }

        public override GUIContent toolbarIcon { get { return m_IconContent; } }
        GUIContent m_IconContent;

        public void OnEnable()
        {
            m_IconContent = new GUIContent()
            {
                image   = m_ToolIcon,
                text    = $"Chisel {ToolName} Tool",
                tooltip = $"Chisel {ToolName} Tool"
            };
            ChiselOptionsOverlay.Register(this); 
        }

        public abstract void OnSceneSettingsGUI(UnityEngine.Object target, SceneView sceneView);


        static bool haveNodeSelection = false;

        public static void OnSelectionChanged()
        {
            haveNodeSelection = (Selection.GetFiltered<ChiselNode>(SelectionMode.Deep | SelectionMode.Editable).Length > 0);
        }

        public static void ShowDefaultOverlay()
        {
            if (!haveNodeSelection)
                return;
            ChiselOptionsOverlay.Show();
            ChiselGridOptionsOverlay.Show();
        }

        public override void OnToolGUI(EditorWindow window)
        {
            var sceneView = window as SceneView;
            var dragArea = sceneView.position;
            dragArea.position = Vector2.zero;
            ChiselGeneratorManager.ActivateTool(this);

            ChiselOptionsOverlay.AdditionalSettings = null;
            ChiselOptionsOverlay.SetTitle(ToolName);
            OnSceneGUI(sceneView, dragArea);

            ChiselOptionsOverlay.Show();
            ChiselGridOptionsOverlay.Show();
        }

        public virtual void OnActivate() { }

        public virtual void OnDeactivate() { }

        public abstract void OnSceneGUI(SceneView sceneView, Rect dragArea);
    }
}
