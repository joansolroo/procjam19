using UnityEngine;
using UnityEngine.UIElements;

using UnityEditor;


[CustomPropertyDrawer(typeof(GraphDense))]
public class CustomGraphDrawer : PropertyDrawer
{
    public override VisualElement CreatePropertyGUI(SerializedProperty property)
    {
        return base.CreatePropertyGUI(property);
    }
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        label = EditorGUI.BeginProperty(position, label, property);
        EditorGUI.LabelField(position, "GRAPH", EditorStyles.boldLabel);
        EditorGUI.BeginChangeCheck();
        position.x += 10;
        Vector2 cellSize = new Vector2(40, 20);
        EditorGUI.PrefixLabel(position, label);
        Rect newPosition = position;

        newPosition.width = cellSize.y;
        newPosition.height = cellSize.y;
        newPosition.y += cellSize.y;

        GUI.enabled = false;
        SerializedProperty nodecount = property.FindPropertyRelative("nodeCount");
        newPosition.width = 80;
        EditorGUI.PrefixLabel(newPosition, new GUIContent("Node count"));
        newPosition.x += newPosition.width;
        newPosition.width = 40;
        nodecount.intValue = EditorGUI.IntField(newPosition, nodecount.intValue);
        GUI.enabled = true;

        newPosition.x += newPosition.width+10;
        SerializedProperty entry = property.FindPropertyRelative("entry");
        newPosition.width = 40;
        EditorGUI.PrefixLabel(newPosition, new GUIContent("Entry"));
        newPosition.x += newPosition.width;
        newPosition.width = 40;
        entry.intValue = EditorGUI.IntField(newPosition, entry.intValue);

        newPosition.x += newPosition.width+10;
        SerializedProperty exit = property.FindPropertyRelative("exit");
        newPosition.width = 40;
        EditorGUI.PrefixLabel(newPosition, new GUIContent("Exit"));
        newPosition.x += newPosition.width;
        newPosition.width = 40;
        exit.intValue = EditorGUI.IntField(newPosition, exit.intValue);
        newPosition.y += cellSize.y;


        int LengthOfArray = nodecount.intValue;
        

        SerializedProperty transitions = property.FindPropertyRelative("transitions");
        SerializedProperty options = property.FindPropertyRelative("nodeData");

        if (options.arraySize != LengthOfArray)
        {
            options.arraySize = LengthOfArray;
        }

        newPosition.x = position.x;
        newPosition.width = 160;
        EditorGUI.LabelField(newPosition, "NODE", EditorStyles.boldLabel);
        newPosition.width = cellSize.x;
        newPosition.x = position.x + 160;
        for (int i = 0; i < LengthOfArray; ++i)
        {

            EditorGUI.LabelField(newPosition, "#" + i.ToString("  0 "), EditorStyles.boldLabel);
            newPosition.x += cellSize.x;

        }
        newPosition.y += cellSize.y;

        newPosition.x = position.x;
        for (int i = 0; i < LengthOfArray; i++)
        {
            if (transitions.arraySize != LengthOfArray)
            {
                transitions.arraySize = LengthOfArray;
            }
            {
                SerializedProperty probabilities = transitions.GetArrayElementAtIndex(i).FindPropertyRelative("probabilities");
                newPosition.height = cellSize.y;

                if (probabilities.arraySize != LengthOfArray)
                {
                    probabilities.arraySize = LengthOfArray;
                }

                newPosition.width = cellSize.x;


                for (int j = -1; j < LengthOfArray; j++)
                {
                    if (j == -1)
                    {
                        GUI.enabled = false;
                        newPosition.width = 160;
                        EditorGUI.PropertyField(newPosition, options.GetArrayElementAtIndex(i), GUIContent.none);
                        newPosition.x += newPosition.width;
                        newPosition.width = cellSize.x;
                        GUI.enabled = true;
                    }

                    else
                    {
                        newPosition.width = cellSize.x;
                        EditorGUI.PropertyField(newPosition, probabilities.GetArrayElementAtIndex(j), GUIContent.none);
                        newPosition.x += newPosition.width;
                    }
                }
                EditorGUI.LabelField(newPosition, "#" + i.ToString("  0 "), EditorStyles.boldLabel);
            }
            newPosition.x = position.x;
            newPosition.y += cellSize.y;
        }
        EditorGUI.PrefixLabel(newPosition, label);
        EditorGUI.EndProperty();
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        return 20 * 9;
    }
}