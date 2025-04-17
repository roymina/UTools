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
