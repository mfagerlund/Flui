using Flui;
using Flui.Bootstrap;
using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(ColoredClass))]
public class ColoredClassDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        // Begin property drawing
        EditorGUI.BeginProperty(position, label, property);

        // Set up the label width
        EditorGUIUtility.labelWidth = 60;

        // Define property fields
        var enabledProp = property.FindPropertyRelative("enabled");
        var classNameProp = property.FindPropertyRelative("className");
        var colorProp = property.FindPropertyRelative("color");

        // Check for null properties
        if (enabledProp == null || classNameProp == null || colorProp == null)
        {
            EditorGUI.LabelField(position, "Property not found");
            EditorGUI.EndProperty();
            return;
        }

        // Calculate rects for each field
        Rect enabledRect = new Rect(position.x, position.y, 30, position.height);
        Rect nameRect = new Rect(position.x + 35, position.y, position.width - 140, position.height);
        Rect colorRect = new Rect(position.x + position.width - 100, position.y, 100, position.height);

        // Draw fields
        EditorGUI.PropertyField(enabledRect, enabledProp, GUIContent.none);
        EditorGUI.PropertyField(nameRect, classNameProp, GUIContent.none);
        EditorGUI.PropertyField(colorRect, colorProp, GUIContent.none);

        // End property drawing
        EditorGUI.EndProperty();
    }
}
