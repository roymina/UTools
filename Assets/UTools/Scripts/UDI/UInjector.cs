//-----------------------------------------------------------------------
// <copyright file="UInjector.cs" company="DxTech Co. Ltd.">
//     Copyright (c) DxTech Co. Ltd. All rights reserved.
// </copyright>
// <author>Roy</author>
// <date>2025-02-07</date>
// <summary>
// Dependency Injector
// </summary>
//-----------------------------------------------------------------------


using System.Reflection;

namespace UTools
{
    public class UInjector
    {
        public static void Inject(object target)
        {
            var fields = target.GetType().GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            foreach (var field in fields)
            {
                var injectAttribute = field.GetCustomAttribute<InjectAttribute>();
                if (injectAttribute != null)
                {
                    var fieldType = field.FieldType;
                    var instance = UDependencyContainer.Instance.Resolve(fieldType);
                    field.SetValue(target, instance);
                }
            }
        }
    }
}
