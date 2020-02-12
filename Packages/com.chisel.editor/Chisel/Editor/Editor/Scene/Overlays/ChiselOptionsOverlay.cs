using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;
using Chisel.Core;
using Chisel.Components;
using UnitySceneExtensions;
using UnityEditor.EditorTools;

namespace Chisel.Editors
{
    // TODO: add tooltips
    public static class ChiselOptionsOverlay
    {
        const string kRebuildIconName   = "rebuild";
        const string kRebuildTooltip    = "Rebuild all generated meshes";
        public static void Rebuild()
        {
            var startTime = EditorApplication.timeSinceStartup;
            ChiselNodeHierarchyManager.Rebuild();
            var csg_endTime = EditorApplication.timeSinceStartup;
            Debug.Log($"Full CSG rebuild done in {((csg_endTime - startTime) * 1000)} ms. ");
        }

        public static ChiselOverlay.WindowFunction AdditionalSettings;


        const int kPrimaryOrder = int.MaxValue;
        
        static readonly GUIContent      kOverlayTitle   = new GUIContent("Chisel");
        static readonly ChiselOverlay   kOverlay        = new ChiselOverlay(kOverlayTitle, DisplayControls, kPrimaryOrder);

        static GUIContent kRebuildButton;

        static SortedList<string, ChiselEditToolBase> editModes = new SortedList<string, ChiselEditToolBase>();

        [InitializeOnLoadMethod]
        static void Initialize()
        {
            kRebuildButton = ChiselEditorResources.GetIconContent(kRebuildIconName, kRebuildTooltip)[0];
        }

        // TODO: move to dedicated manager
        internal static void Register(ChiselEditToolBase editMode)
        {
            editModes[editMode.ToolName] = editMode;
        }

        public static void UpdateCreateToolIcon()
        {
            if (!editModes.TryGetValue(ChiselCreateTool.kToolName, out ChiselEditToolBase toolBase))
                return;
            toolBase.UpdateIcon();
        }

        const float kButtonSize = 32;
        static GUILayoutOption[] buttonOptions = new[]
        {
            GUILayout.Width(kButtonSize),
            GUILayout.Height(kButtonSize)
        };


        static bool Toggle(ChiselEditToolBase editMode, Type editModeType)
        {
            var content = editMode.m_ToolIcon;
            var selected = EditorTools.activeToolType == editModeType;
            return GUILayout.Toggle(selected, content, GUI.skin.button, buttonOptions);
        }

        static void EditModeButton(ChiselEditToolBase editMode, bool enabled)
        {
            var editModeType = editMode.GetType();
            using (new EditorGUI.DisabledScope(!enabled))
            {
                EditorGUI.BeginChangeCheck();
                var value = Toggle(editMode, editModeType);
                if (EditorGUI.EndChangeCheck() && value)
                {
                    EditorTools.SetActiveTool(editModeType);
                    ChiselEditorSettings.Save();
                }
            }
        }

        static void DisplayControls(UnityEngine.Object target, SceneView sceneView)
        {
            if (!sceneView)
                return;

            EditorGUI.BeginChangeCheck();
            {
                AdditionalSettings?.Invoke(target, sceneView);

                var enabled = Selection.GetFiltered<ChiselNode>(SelectionMode.OnlyUserModifiable).Length > 0;

                if (editModes.Count > 0 && enabled)
                {
                    GUILayout.BeginHorizontal(ChiselOverlay.kMinWidthLayout);

                    foreach (var editMode in editModes.Values)
                    {
                        if (editMode.GetType() == typeof(ChiselCreateTool))
                            continue;
                        EditModeButton(editMode, enabled);
                    }
                    GUILayout.FlexibleSpace();

                    // TODO: assign hotkey to rebuild, and possibly move it elsewhere to avoid it seemingly like a necessary action.

                    if (GUILayout.Toggle(false, kRebuildButton, GUI.skin.button, buttonOptions))
                    {
                        Rebuild();
                    }
                    GUILayout.EndHorizontal();
                }
                
                ChiselPlacementToolsSelectionWindow.RenderCreationTools();
            }
            if (EditorGUI.EndChangeCheck())
                ChiselEditorSettings.Save();
        }

        public static void SetTitle(string title)
        {
            kOverlayTitle.text = title;
        }

        public static void Show()
        {
            kOverlay.Show();
        }
    }
}