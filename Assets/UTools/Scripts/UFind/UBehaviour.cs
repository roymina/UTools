//-----------------------------------------------------------------------
// <copyright file="UBehaviour.cs" company="DxTech Co. Ltd.">
//     Copyright (c) DxTech Co. Ltd.. All rights reserved.
// </copyright>
// <author>Roy</author>
// <date>2025-02-07</date>
// <summary>
//     The core class that implements the search of child objects and components. Other classes must inherit this class to use the search function.
// </summary>
//-----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace UTools
{
    public class UBehaviour : MonoBehaviour
    {
        protected GameObject[] children;
        protected virtual void Awake()
        {
            GetComp();
            FindChildren();
            GetResources();
        }

        private void GetResources()
        {
            var fields = GetType().GetFields(
                           BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance
                           ).Where(filed => Attribute.IsDefined(filed, typeof(ResourceAttribute)));
            if (fields.Any())
            {
                foreach (var field in fields)
                {
                    var resourceAttr = field.GetCustomAttribute<ResourceAttribute>();
                    if (resourceAttr != null)
                    {
                        string path = resourceAttr.Path;
                        string fieldName = field.Name;
                        string fullPath = $"{path}/{fieldName}";
                        var resource = Resources.Load(fullPath, field.FieldType);
                        if (resource != null)
                        {
                            field.SetValue(this, resource);
                        }
                        else
                        {
                            Debug.LogWarning($"Failed to load resource at {fullPath}");
                        }
                    }
                }
            }
        }

        private void GetComp()
        {
            var fields = GetType().GetFields(
                           BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance
                           ).Where(filed => Attribute.IsDefined(filed, typeof(CompAttribute)));
            if (fields?.Count() > 0)
            {
                foreach (var field in fields)
                {
                    //get target field's type
                    Type fieldType = Nullable.GetUnderlyingType(field.FieldType) ?? field.FieldType;
                    //try to get target field's value, if null , assign value to it 
                    var fieldValue = field.GetValue(this);
                    // null check is different between unity's components and custom scripts                  
                    if (fieldValue == null || fieldValue.ToString() == "null")
                    {
                        try
                        {
                            fieldValue = GetComponent(fieldType);
                            if (fieldValue != null)
                            {
                                object safeValue = Convert.ChangeType(fieldValue, fieldType);
                                field.SetValue(this, safeValue);
                            }
                        }
                        catch
                        {
                            Debug.Log($"{gameObject.name} cannot find the {fieldValue.ToString()} component");
                        }
                    }
                }
            }
        }

        private void FindChildren()
        {

            var fields = GetType().GetFields(
                BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance
                ).Where(filed => Attribute.IsDefined(filed, typeof(ChildAttribute)));
            if (!fields.Any()) return;

            if (transform.childCount == 0) return;

            if (children is null || children?.Length == 0)
            {
                children = UUtils.GetAllChildren(gameObject);

            }

            foreach (var field in fields)
            {
                //get target field's type
                Type fieldType = Nullable.GetUnderlyingType(field.FieldType) ?? field.FieldType;
                //try to get target field's value, if null , assign value to it 
                var fieldValue = field.GetValue(this);
                // null check is different between unity's components and custom scripts
                bool fieldIsNull = false;
                if (fieldValue == null)
                {
                    fieldIsNull = true;
                }
                else if (fieldValue.ToString() == "null")
                {
                    fieldIsNull = true;
                }
                if (fieldIsNull)
                {
                    //gameobjects can't be picked by getcomponent method
                    if (fieldType.Equals(typeof(GameObject)))
                    {
                        IEnumerable<GameObject> targets;

                        string attributeNameField = field.GetCustomAttribute<ChildAttribute>().Name;
                        if (string.IsNullOrEmpty(attributeNameField))
                        {
                            targets = children.Where(x => x.name.ToLower() == field.Name.ToLower());
                        }
                        else
                        {
                            targets = children.Where(x => x.name.ToLower() == attributeNameField);
                        }

                        GameObject target = null;
                        if (targets?.Count() == 0)
                        {
                            Debug.LogError($"Unable to find GameObject:{field.Name} under {gameObject.name}, please check whether the variable name and object name are consistent");
                            continue;
                        }
                        else if (targets?.Count() > 1)
                        {
                            Debug.LogWarning($"{targets?.Count()} GameObjects:{field.Name} are found under {gameObject.name}, and the first one will be selected");
                        }
                        target = targets.ToArray()[0];
                        field.SetValue(this, target.gameObject);
                    }
                    else
                    {
                        var children = GetComponentsInChildren(fieldType, true);
                        if (children?.Length > 0)
                        {
                            string attributeNameField = field.GetCustomAttribute<ChildAttribute>().Name;

                            if (string.IsNullOrEmpty(attributeNameField))
                            {
                                fieldValue = children.FirstOrDefault(x => x.gameObject.name == field.Name);
                            }
                            else
                            {
                                fieldValue = children.FirstOrDefault(x => x.gameObject.name == attributeNameField);
                            }
                            if (fieldValue != null)
                            {
                                try
                                {
                                    object safeValue = (fieldValue == null) ? null : Convert.ChangeType(fieldValue, fieldType);
                                    field.SetValue(this, safeValue);
                                }
                                catch
                                {
                                    Debug.Log(gameObject.name + "can not find：" + fieldValue.ToString());
                                }

                            }
                            else
                            {
                                Debug.LogError($"Unable to find {field.Name} instance under {gameObject.name}, please check whether the variable name and object name are consistent");
                            }
                        }
                        else
                        {
                            Debug.LogError($"Unable to find {field.Name} instance under {gameObject.name}, please check whether the variable name and object name are consistent");
                        }
                    }
                }
            }
        }
    }
}

