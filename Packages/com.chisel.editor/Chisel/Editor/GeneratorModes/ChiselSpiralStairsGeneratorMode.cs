﻿using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Chisel.Core;
using Chisel.Components;
using UnitySceneExtensions;
using UnityEditor.ShortcutManagement;

namespace Chisel.Editors
{
    public sealed class ChiselSpiralStairsGeneratorMode : ChiselGeneratorMode
    {
        const string kToolName = ChiselSpiralStairs.kNodeTypeName;
        public override string ToolName => kToolName;

        #region Keyboard Shortcut
        const string kToolShotcutName = ChiselKeyboardDefaults.ShortCutCreateBase + kToolName;
        [Shortcut(kToolShotcutName, ChiselKeyboardDefaults.SpiralStairsBuilderModeKey, ChiselKeyboardDefaults.SpiralStairsBuilderModeModifiers, displayName = kToolShotcutName)]
        public static void StartGeneratorMode() { ChiselGeneratorManager.GeneratorType = typeof(ChiselSpiralStairsGeneratorMode); }
        #endregion
        
        // TODO: Handle forcing operation types
        CSGOperationType? forceOperation = null; // TODO: WHY??

        // TODO: Ability to modify default settings
        // TODO: Store/retrieve default settings
        bool			generateFromCenterXZ    = true;
        int             outerSegments           = ChiselSpiralStairsDefinition.kDefaultOuterSegments;
        float           stepHeight              = ChiselSpiralStairsDefinition.kDefaultStepHeight;

        ChiselSpiralStairs spiralStairs;
        public override void OnSceneSettingsGUI()
        {
            // TODO: implement
            GUILayout.BeginVertical();
            generateFromCenterXZ = EditorGUILayout.Toggle(EditorGUIUtility.TrTextContent("Generate from Center"), generateFromCenterXZ);
            ChiselOperationGUI.ChooseGeneratorOperation(ref forceOperation);
            GUILayout.EndVertical();
        }

        public override void OnSceneGUI(SceneView sceneView, Rect dragArea)
        {
            var flags = BoxExtrusionFlags.AlwaysFaceUp |
                        BoxExtrusionFlags.IsSymmetricalXZ |
                        (generateFromCenterXZ ? BoxExtrusionFlags.GenerateFromCenterXZ : BoxExtrusionFlags.None);

            switch (BoxExtrusionHandle.Do(dragArea, out Bounds bounds, out float height, out ChiselModel modelBeneathCursor, out Matrix4x4 transformation, flags, Axis.Y, snappingSteps: stepHeight))
            {
                case BoxExtrusionState.Create:
                {
                    spiralStairs = ChiselComponentFactory.Create<ChiselSpiralStairs>(ChiselSpiralStairs.kNodeTypeName,
                                                                        ChiselModelManager.GetActiveModelOrCreate(modelBeneathCursor),
                                                                        transformation);
                    spiralStairs.definition.Reset();
                    spiralStairs.Operation		= forceOperation ?? CSGOperationType.Additive;
                    spiralStairs.StepHeight     = stepHeight;
                    spiralStairs.Height         = height;
                    spiralStairs.OuterDiameter  = bounds.size[(int)Axis.X];
                    spiralStairs.OuterSegments  = outerSegments;
                    spiralStairs.UpdateGenerator();
                    break;
                }

                case BoxExtrusionState.Modified:
                {
                    spiralStairs.Operation      = forceOperation ?? 
                                                  ((height < 0 && modelBeneathCursor) ? 
                                                    CSGOperationType.Subtractive : 
                                                    CSGOperationType.Additive);
                    spiralStairs.Height			= bounds.size.y;
                    spiralStairs.OuterDiameter	= bounds.size[(int)Axis.X];
                    break;
                }
                
                case BoxExtrusionState.Commit:      { Commit(spiralStairs.gameObject); break; }
                case BoxExtrusionState.Cancel:      { Cancel(); break; }
                case BoxExtrusionState.BoxMode:
                case BoxExtrusionState.SquareMode:	{ ChiselOutlineRenderer.VisualizationMode = VisualizationMode.SimpleOutline; break; }
                case BoxExtrusionState.HoverMode:	{ ChiselOutlineRenderer.VisualizationMode = VisualizationMode.Outline; break; }
            }

            HandleRendering.RenderCylinder(transformation, bounds, (spiralStairs) ? spiralStairs.OuterSegments : outerSegments);
        }
    }
}