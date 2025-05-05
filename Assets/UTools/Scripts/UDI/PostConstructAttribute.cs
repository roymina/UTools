//-----------------------------------------------------------------------
// <copyright file="PostConstructAttribute.cs" company="DxTech Co. Ltd.">
//     Copyright (c) DxTech Co. Ltd. All rights reserved.
// </copyright>
// <author>Roy</author>
// <date>2025-02-07</date>
// <summary>
// Attribute to mark methods for execution after dependency injection is complete.
// </summary>
//-----------------------------------------------------------------------

using System;

namespace UTools
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class PostConstructAttribute : Attribute
    {
    }
}