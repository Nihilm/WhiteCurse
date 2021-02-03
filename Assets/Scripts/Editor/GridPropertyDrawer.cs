// using System.Collections;
// using System.Collections.Generic;
// using UnityEngine;
// using UnityEditor;
// using UnityEditor.Experimental.GraphView;
// using UnityEditor.UIElements;
// using UnityEngine.UIElements;

// namespace UnityEditor.Custom {
//     [CustomPropertyDrawer(typeof(TileGrid),true)]
//     public class GridPropertyDrawer : PropertyDrawer {
//         public override void OnGUI(Rect position, SerializedProperty property, GUIContent label){
//             SerializedProperty columnsProperty = property.serializedObject.FindProperty($"{property.propertyPath}.columns");
//             SerializedProperty rowsProperty = property.serializedObject.FindProperty($"{property.propertyPath}.rows");
//             SerializedProperty tilesProperty = property.serializedObject.FindProperty($"{property.propertyPath}.tiles");
//             int columns = columnsProperty.intValue;
//             int rows = rowsProperty.intValue;
//             tilesProperty.arraySize = columns * rows;

//             EditorGUI.BeginProperty(position, label, property);
//             Rect offset = position;
//             EditorGUI.PrefixLabel(position,label);
//             EditorGUI.PropertyField(
//                 new Rect(position.x, position.y + EditorGUIUtility.singleLineHeight, position.width, EditorGUIUtility.singleLineHeight),
//                 columnsProperty, new GUIContent("Columns"));
//             EditorGUI.PropertyField(
//                 new Rect(position.x, position.y + 2 * EditorGUIUtility.singleLineHeight, position.width, EditorGUIUtility.singleLineHeight),
//                 rowsProperty, new GUIContent("Rows"));
//             float offsetY = position.y + 3 * EditorGUIUtility.singleLineHeight;
//             float height = position.height - 3 * EditorGUIUtility.singleLineHeight;

//             for(int c = 0; c < columns; c++)
//             for(int r = 0; r < rows; r++){
//                 int i = c * rows + r;
//                 EditorGUI.PropertyField(new Rect(
//                     position.x + r * position.width / rows, offsetY + c * height / columns,
//                     position.width / rows, height / columns
//                 ), tilesProperty.GetArrayElementAtIndex(i), GUIContent.none);
//             }
//             EditorGUI.EndProperty();
//         }
//         public override float GetPropertyHeight(SerializedProperty property, GUIContent label){
//             int columns = property.serializedObject.FindProperty($"{property.propertyPath}.columns").intValue;
//             SerializedProperty tilesProperty = property.serializedObject.FindProperty($"{property.propertyPath}.tiles");
//             float tileHeight = tilesProperty.arraySize > 0 ? base.GetPropertyHeight(tilesProperty.GetArrayElementAtIndex(0), GUIContent.none) : 0;
//             return 3 * EditorGUIUtility.singleLineHeight + tileHeight * columns;
//         }
//     }
// }