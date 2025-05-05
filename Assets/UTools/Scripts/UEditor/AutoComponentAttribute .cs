//-----------------------------------------------------------------------
// <copyright file="AutoComponentAttribute.cs" company="DxTech Co. Ltd.">
//     Copyright (c) DxTech Co. Ltd. All rights reserved.
// </copyright>
// <author>Roy</author>
// <date>2025-02-07</date>
// <summary>
// Attribute for automatically fetching components, supporting one or multiple component types.
// </summary>
//-----------------------------------------------------------------------

using UnityEngine;
using System;

namespace UTools
{
    /// <summary>
    /// Attribute for automatically fetching components.
    /// </summary>
    /// <remarks>Fields with this attribute will automatically fetch the specified component types at runtime.</remarks>
    /// <example>Example: <code>[AutoComponent(typeof(Rigidbody))]</code></example>
    /// <example>Example: <code>[AutoComponent(typeof(Rigidbody), typeof(Collider))]</code></example>
    /// <example>Example: <code>[AutoComponent(typeof(Rigidbody), typeof(Collider), typeof(MeshRenderer))]</code></example>
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
    public class AutoComponentAttribute : PropertyAttribute
    {
        public Type[] ComponentTypes { get; private set; }

        // Supports a single component type
        public AutoComponentAttribute(Type componentType)
        {
            ComponentTypes = new Type[] { componentType };
        }

        // Supports multiple component types
        public AutoComponentAttribute(params Type[] componentTypes)
        {
            ComponentTypes = componentTypes;
        }
    }
}