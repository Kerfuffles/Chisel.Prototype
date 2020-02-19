﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

namespace Chisel.Core
{
    // TODO: create real Asset that contains this so we can drag & drop a complete "material" onto a brush
    [Serializable]
    public sealed class ChiselBrushMaterial : IDisposable
    {
        // This ensures names remain identical, or a compile error occurs.
        public const string kLayerUsageFieldName      = nameof(layerUsage);
        public const string kRenderMaterialFieldName  = nameof(renderMaterial);
        public const string kPhysicsMaterialFieldName = nameof(physicsMaterial);

        public ChiselBrushMaterial()    { ChiselBrushMaterialManager.Register(this); }
        public void OnDestroy()         { Dispose(); }
        public void Dispose()           { ChiselBrushMaterialManager.Unregister(this); }
        ~ChiselBrushMaterial()          { Dispose(); }
        

        [SerializeField] LayerUsageFlags  layerUsage = LayerUsageFlags.RenderReceiveCastShadows | LayerUsageFlags.Collidable;
        [SerializeField] Material         renderMaterial;
        [SerializeField] PhysicMaterial   physicsMaterial;

        public LayerUsageFlags	LayerUsage		            { get { return layerUsage;      } set { if (layerUsage      == value) return; var prevValue = layerUsage;      layerUsage      = value; ChiselBrushMaterialManager.OnLayerUsageFlagsChanged(this, prevValue, value); } }
        public Material			RenderMaterial	            { get { return renderMaterial;  } set { if (renderMaterial  == value) return; var prevValue = renderMaterial;  renderMaterial  = value; ChiselBrushMaterialManager.OnRenderMaterialChanged(this, prevValue, value); } }
        public PhysicMaterial	PhysicsMaterial             { get { return physicsMaterial; } set { if (physicsMaterial == value) return; var prevValue = physicsMaterial; physicsMaterial = value; ChiselBrushMaterialManager.OnPhysicsMaterialChanged(this, prevValue, value); } }
        public int				RenderMaterialInstanceID	{ get { return renderMaterial  ? renderMaterial .GetInstanceID() : 0; } }
        public int				PhysicsMaterialInstanceID	{ get { return physicsMaterial ? physicsMaterial.GetInstanceID() : 0; } }
        

        public void SetDirty()
        {
            ChiselBrushMaterialManager.OnSetDirty(this);
        }

        public static ChiselBrushMaterial CreateInstance(ChiselBrushMaterial other)
        {
            return CreateInstance(other.renderMaterial, other.physicsMaterial, other.layerUsage);
        }

  
        
        public static ChiselBrushMaterial CreateInstance(LayerUsageFlags layerUsage = LayerUsageFlags.None)
        {
            return CreateInstance(null, null, layerUsage);
        }

        public static ChiselBrushMaterial CreateInstance(Material renderMaterial, LayerUsageFlags layerUsage = LayerUsageFlags.RenderReceiveCastShadows)
        {
            return CreateInstance(renderMaterial, null, layerUsage);
        }

        public static ChiselBrushMaterial CreateInstance(PhysicMaterial physicsMaterial, LayerUsageFlags layerUsage = LayerUsageFlags.Collidable)
        {
            return CreateInstance(null, physicsMaterial, layerUsage);
        }

        public static ChiselBrushMaterial CreateInstance(Material renderMaterial, PhysicMaterial physicsMaterial, LayerUsageFlags layerUsage = LayerUsageFlags.RenderReceiveCastShadows | LayerUsageFlags.Collidable)
        {
            return new ChiselBrushMaterial
            {
                LayerUsage = layerUsage,
                RenderMaterial = renderMaterial,
                PhysicsMaterial = physicsMaterial
            };
        }
    }
}
