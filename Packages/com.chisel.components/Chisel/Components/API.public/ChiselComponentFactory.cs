﻿using System;
using System.Linq;
using Chisel.Core;
using Vector2 = UnityEngine.Vector2;
using Vector3 = UnityEngine.Vector3;
using Vector4 = UnityEngine.Vector4;
using Quaternion = UnityEngine.Quaternion;
using Matrix4x4 = UnityEngine.Matrix4x4;
using Mathf = UnityEngine.Mathf;
using Plane = UnityEngine.Plane;
using Debug = UnityEngine.Debug;
using System.Collections.Generic;

namespace Chisel.Components
{
    // TODO: rename
    public sealed class ChiselComponentFactory
    {
        public static T AddComponent<T>(UnityEngine.GameObject gameObject) where T : ChiselNode
        {
            // TODO: ensure we're creating this in the active scene
            // TODO: handle scene being locked by version control

            if (!gameObject)
                return null;

            bool prevActive = gameObject.activeSelf;
            if (prevActive)
                gameObject.SetActive(false);
            try
            {
                var brushTransform = gameObject.transform;
#if UNITY_EDITOR
                return UnityEditor.Undo.AddComponent<T>(gameObject);
#else
                return gameObject.AddComponent<T>();
#endif
            }
            finally
            {
                if (prevActive)
                    gameObject.SetActive(prevActive);
            }
        }

        public static void SetTransform<T>(T component, UnityEngine.Transform parent, Matrix4x4 trsMatrix) where T : ChiselNode
        {
            if (!component)
                return;
            SetTransform(component.transform, parent, trsMatrix);
        }

        public static void SetTransform<T>(T component, Matrix4x4 trsMatrix) where T : ChiselNode
        {
            if (!component)
                return;
            SetTransform(component.transform, trsMatrix);
        }

        public static void SetTransform(UnityEngine.Transform transform, Matrix4x4 trsMatrix)
        {
            if (!transform)
                return;
            SetTransform(transform, (transform == null) ? null : transform.parent, trsMatrix);
        }

        public static void SetTransform(UnityEngine.Transform transform, UnityEngine.Transform parent, Matrix4x4 trsMatrix)
        {
            if (!transform)
                return;

#if UNITY_EDITOR
            UnityEditor.Undo.RecordObject(transform, "Move child node to given position");
#endif
            if (parent)
                transform.SetLocal(parent.worldToLocalMatrix * trsMatrix);
            else
                transform.SetLocal(trsMatrix);
        }


        public static T Create<T>(string name, UnityEngine.Transform parent, Matrix4x4 trsMatrix) where T : ChiselNode
        {
            // TODO: ensure we're creating this in the active scene
            // TODO: handle scene being locked by version control

            if (string.IsNullOrEmpty(name))
            {
#if UNITY_EDITOR
                name = UnityEditor.GameObjectUtility.GetUniqueNameForSibling(parent, typeof(T).Name);
#else
                name = typeof(T).Name;
#endif
            }

            var newGameObject = new UnityEngine.GameObject(name);
#if UNITY_EDITOR
            UnityEditor.Undo.RegisterCreatedObjectUndo(newGameObject, "Created " + name);
#endif
            newGameObject.SetActive(false);
            try
            {
                var brushTransform = newGameObject.transform;
#if UNITY_EDITOR
                if (parent)
                    UnityEditor.Undo.SetTransformParent(brushTransform, parent, "Move child node underneath parent composite");
                UnityEditor.Undo.RecordObject(brushTransform, "Move child node to given position");
#else
                if (parent)
                    brushTransform.SetParent(parent, false);
#endif
                if (parent)
                    brushTransform.SetLocal(parent.worldToLocalMatrix * trsMatrix);
                else
                    brushTransform.SetLocal(trsMatrix);

#if UNITY_EDITOR
                return UnityEditor.Undo.AddComponent<T>(newGameObject);
#else
                return newGameObject.AddComponent<T>();
#endif
            }
            finally
            {
                newGameObject.SetActive(true);
            }
        }
         
        
        public static UnityEngine.Component Create(Type type, string name, UnityEngine.Transform parent, Matrix4x4 trsMatrix)
        {
            // TODO: ensure we're creating this in the active scene
            // TODO: handle scene being locked by version control

            if (string.IsNullOrEmpty(name))
            {
#if UNITY_EDITOR
                name = UnityEditor.GameObjectUtility.GetUniqueNameForSibling(parent, type.Name);
#else
                name = type.Name;
#endif
            }

            var newGameObject = new UnityEngine.GameObject(name);
#if UNITY_EDITOR
            UnityEditor.Undo.RegisterCreatedObjectUndo(newGameObject, "Created " + name);
#endif
            newGameObject.SetActive(false);
            try
            {
                var brushTransform = newGameObject.transform;
#if UNITY_EDITOR
                if (parent)
                    UnityEditor.Undo.SetTransformParent(brushTransform, parent, "Move child node underneath parent composite");
                UnityEditor.Undo.RecordObject(brushTransform, "Move child node to given position");
#else
                if (parent)
                    brushTransform.SetParent(parent, false);
#endif
                if (parent)
                    brushTransform.SetLocal(parent.worldToLocalMatrix * trsMatrix);
                else
                    brushTransform.SetLocal(trsMatrix);

#if UNITY_EDITOR
                return UnityEditor.Undo.AddComponent(newGameObject, type);
#else
                return newGameObject.AddComponent(type);
#endif
            }
            finally
            {
                newGameObject.SetActive(true);
            }
        }
         
        public static T Create<T>(string name, UnityEngine.Transform parent, Vector3 position, Quaternion rotation, Vector3 scale) where T : ChiselNode
        {
            return Create<T>(name, parent, Matrix4x4.TRS(position, rotation, scale));
        }

        public static T Create<T>(string name, ChiselModel model) where T : ChiselNode { return Create<T>(name, model ? model.transform : null, Vector3.zero, Quaternion.identity, Vector3.one); }
        public static T Create<T>(string name, UnityEngine.Transform parent = null) where T : ChiselNode { return Create<T>(name, parent, Vector3.zero, Quaternion.identity, Vector3.one); }
        public static T Create<T>(UnityEngine.Transform parent, Vector3 position, Quaternion rotation, Vector3 scale) where T : ChiselNode { return Create<T>(null, parent, position, rotation, scale); }
        public static T Create<T>(UnityEngine.Transform parent, Matrix4x4 trsMatrix) where T : ChiselNode { return Create<T>(null, parent, trsMatrix); }
        public static T Create<T>(UnityEngine.Transform parent = null) where T : ChiselNode { return Create<T>(null, parent, Vector3.zero, Quaternion.identity, Vector3.one); }
        public static T Create<T>(Vector3 position, Quaternion rotation, Vector3 scale) where T : ChiselNode { return Create<T>(null, (UnityEngine.Transform)null, position, rotation, scale); }
        public static T Create<T>(Matrix4x4 trsMatrix) where T : ChiselNode { return Create<T>(null, (UnityEngine.Transform)null, trsMatrix); }
        public static T Create<T>(ChiselModel model, Vector3 position, Quaternion rotation, Vector3 scale) where T : ChiselNode { return Create<T>(null, model ? model.transform : null, position, rotation, scale); }
        public static T Create<T>(ChiselModel model, Matrix4x4 trsMatrix) where T : ChiselNode { return Create<T>(null, model ? model.transform : null, trsMatrix); }
        public static T Create<T>(ChiselModel model) where T : ChiselNode { return Create<T>(null, model ? model.transform : null, Vector3.zero, Quaternion.identity, Vector3.one); }
        public static T Create<T>(string name, ChiselModel model, Matrix4x4 trsMatrix) where T : ChiselNode { return Create<T>(name, model ? model.transform : null, trsMatrix); }
        public static T Create<T>(string name, ChiselModel model, Vector3 position, Quaternion rotation, Vector3 scale) where T : ChiselNode { return Create<T>(name, model ? model.transform : null, position, rotation, scale); }
    }
}
