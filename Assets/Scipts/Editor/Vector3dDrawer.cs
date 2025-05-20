using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(Vector3d))]
public class Vector3dDrawerUIE : PropertyDrawer
{
    // Draw the property inside the given rect
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        // Using BeginProperty / EndProperty on the parent property means that
        // prefab override logic works on the entire property.
        EditorGUI.BeginProperty(position, label, property);

        // Draw label
        position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);

        // Calculate rects
        float gap = 4;
        float varWidth = position.width / 3 - gap;
        var xRect = new Rect(position.x, position.y, varWidth, position.height);
        var yRect = new Rect(position.x + varWidth + gap, position.y, varWidth, position.height);
        var zRect = new Rect(position.x + varWidth * 2 + gap * 2, position.y, varWidth, position.height);

        // Draw fields - passs GUIContent.none to each so they are drawn without labels
        EditorGUI.PropertyField(xRect, property.FindPropertyRelative("x"), GUIContent.none);
        EditorGUI.PropertyField(yRect, property.FindPropertyRelative("y"), GUIContent.none);
        EditorGUI.PropertyField(zRect, property.FindPropertyRelative("z"), GUIContent.none);

        EditorGUI.EndProperty();
    }
}