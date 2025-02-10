//-----------------------------------------------------------------------
// <copyright file="ChildAttribute.cs" company="DxTech Co. Ltd.">
//     Copyright (c) DxTech Co. Ltd.. All rights reserved.
// </copyright>
// <author>Roy</author>
// <date>2025-02-07</date>
// <summary>
//     Label identifying the child object.
// </summary>
//-----------------------------------------------------------------------
using System;


namespace UTools
{
    [AttributeUsage(AttributeTargets.Field, Inherited = false)]
    public class ChildAttribute : Attribute
    {
        public readonly string Name;
        public ChildAttribute()
        {

        }
        public ChildAttribute(string name) => Name = name;
    }
}