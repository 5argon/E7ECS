using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// Welcome to the "I have put a bool into an IComponentData before" club.
/// </summary>
[Serializable]
public struct Bool
{
	[SerializeField] private byte Value;

    public static implicit operator Bool(bool b) => new Bool() { Value = Convert.ToByte(b) };
    public static implicit operator bool(Bool b) => b.Value == 1;

    public override string ToString() => Value == 1 ? "true" : "false";
	public override int GetHashCode() => Value.GetHashCode();
}

#if UNITY_EDITOR
[CustomPropertyDrawer(typeof(Bool))]
public class BlittableBoolDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
	{
        EditorGUI.BeginProperty(position, label, property);
        property.Next(true);
        property.intValue = (EditorGUI.Toggle(position, label, property.intValue == 1 ? true : false) ? 1 : 0);
        EditorGUI.EndProperty();
    }
}
#endif