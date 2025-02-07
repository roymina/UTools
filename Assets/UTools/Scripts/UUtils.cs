//-----------------------------------------------------------------------
// <copyright file="UUtils.cs" company="DxTech Co. Ltd.">
//     Copyright (c) DxTech Co. Ltd.. All rights reserved.
// </copyright>
// <author>Roy</author>
// <date>2025-02-07</date>
// <summary>
//     Static helper method class.
// </summary>
//-----------------------------------------------------------------------

using System;
using System.Linq;
using UnityEngine;

public static class UUtils
{
    /// <summary>
    /// get all children & decendents, includs deactived
    /// </summary>
    /// <returns></returns>
    public static GameObject[] GetAllChildren(this GameObject self)
    {
        Transform[] childTransforms = self.GetComponentsInChildren<Transform>(true);
        return Array.FindAll(childTransforms, transform => transform.gameObject != self)
                    .Select(transform => transform.gameObject)
                    .ToArray();
    }
}
