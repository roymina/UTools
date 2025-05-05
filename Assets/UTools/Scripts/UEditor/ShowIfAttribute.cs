//-----------------------------------------------------------------------
// <copyright file="ShowIfAttribute.cs" company="DxTech Co. Ltd.">
//     Copyright (c) DxTech Co. Ltd. All rights reserved.
// </copyright>
// <author>Roy</author>
// <date>2025-02-07</date>
// <summary>
// Attribute to conditionally show fields in the Unity Inspector based on a condition.
// </summary>
//-----------------------------------------------------------------------

using UnityEngine;
using System;

namespace UTools
{
    [AttributeUsage(AttributeTargets.Field, Inherited = false)]
    public class ShowIfAttribute : PropertyAttribute
    {
        public readonly string ConditionField;
        public readonly bool Inverse;

        public ShowIfAttribute(string conditionField, bool inverse = true)
        {
            ConditionField = conditionField;
            Inverse = inverse;
        }
    }
}

