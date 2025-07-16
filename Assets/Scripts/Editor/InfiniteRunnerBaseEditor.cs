using UnityEngine;
using UnityEditor;

/// <summary>
/// Custom editor for InfiniteRunnerBase and its child classes.
/// Shows forwardSpeed only for the base InfiniteRunnerBase class, hides it from child classes.
/// </summary>
[CustomEditor(typeof(InfiniteRunnerBase), true)]
public class InfiniteRunnerBaseEditor : Editor
{
    public override void OnInspectorGUI()
    {
        // Get the target object (the script being inspected)
        InfiniteRunnerBase targetScript = (InfiniteRunnerBase)target;
        
        // Get the actual type of the script
        System.Type scriptType = targetScript.GetType();
        
        // Check if this is the BASE InfiniteRunnerBase class (not a child class)
        bool isBaseClass = scriptType == typeof(InfiniteRunnerBase);
        
        // Start checking for changes
        EditorGUI.BeginChangeCheck();
        
        // Manually draw all properties
        SerializedProperty prop = serializedObject.GetIterator();
        bool enterChildren = true;
        
        while (prop.NextVisible(enterChildren))
        {
            enterChildren = false;
            
            // Always skip the script field (Unity handles this automatically)
            if (prop.name == "m_Script")
                continue;
            
            // Only show forwardSpeed if this is the base class
            if (prop.name == "forwardSpeed")
            {
                if (isBaseClass)
                {
                    EditorGUILayout.PropertyField(prop, true);
                }
                // Skip drawing forwardSpeed for child classes
                continue;
            }
            
            // Draw all other properties normally
            EditorGUILayout.PropertyField(prop, true);
        }
        
        // Apply changes if any were made
        if (EditorGUI.EndChangeCheck())
        {
            serializedObject.ApplyModifiedProperties();
        }
        
        // Add helpful info boxes
        EditorGUILayout.Space();
        if (isBaseClass)
        {
            EditorGUILayout.HelpBox("This is the base InfiniteRunnerBase class. The Forward Speed setting here controls the speed for all child scripts.", MessageType.Info);
        }
       
    }
} 