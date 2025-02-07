//-----------------------------------------------------------------------
// <copyright file="CompAttribute.cs" company="DxTech Co. Ltd.">
//     Copyright (c) DxTech Co. Ltd.. All rights reserved.
// </copyright>
// <author>Roy</author>
// <date>2025-02-07</date>
// <summary>
//     Label identifying the component.
// </summary>
//-----------------------------------------------------------------------

using System;

namespace UTools
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = false)]
    public class CompAttribute : Attribute
    {

    }

}
