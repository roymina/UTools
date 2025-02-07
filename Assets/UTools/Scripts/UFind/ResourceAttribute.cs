//-----------------------------------------------------------------------
// <copyright file="ResourceAttribute.cs" company="DxTech Co. Ltd.">
//     Copyright (c) DxTech Co. Ltd. All rights reserved.
// </copyright>
// <author>Roy</author>
// <date>2025-02-07</date>
// <summary>
//     Label identifying the resource.
// </summary>
//-----------------------------------------------------------------------

using System;

namespace UTools
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = false)]
    public class ResourceAttribute : Attribute
    {
        public string Path { get; }

        public ResourceAttribute(string path)
        {
            Path = path;
        }
    }

}
