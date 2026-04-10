//-----------------------------------------------------------------------
// <copyright file="ChildrenAttribute.cs" company="DxTech Co. Ltd.">
//     Copyright (c) DxTech Co. Ltd.. All rights reserved.
// </copyright>
// <author>Roy</author>
// <date>2026-04-10</date>
// <summary>
//     Label identifying the parent object whose children will be collected.
// </summary>
//-----------------------------------------------------------------------
using System;

namespace UTools
{
    [AttributeUsage(AttributeTargets.Field, Inherited = false)]
    public class ChildrenAttribute : Attribute
    {
        public string parentName;
        public bool includeDecendents = false;
        public bool includeInactive = true;

        public ChildrenAttribute()
        {
        }

        public ChildrenAttribute(string parentName)
        {
            this.parentName = parentName;
        }
    }
}
