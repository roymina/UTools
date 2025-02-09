//-----------------------------------------------------------------------
// <copyright file="InjectAttribute.cs" company="DxTech Co. Ltd.">
//     Copyright (c) DxTech Co. Ltd. All rights reserved.
// </copyright>
// <author>Roy</author>
// <date>2025-02-07</date>
// <summary>
// 
// </summary>
//-----------------------------------------------------------------------

using System;

namespace UTools
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Method)]
    public class InjectAttribute : Attribute
    {
    }
}
