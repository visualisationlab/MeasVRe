using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

namespace MeasVRe.Log
{
    [CustomEditor(typeof(LogManager))]
    public class LogManagerEditor : Editor
    {

        // Reference to the LogManager script
        LogManager logManager;

        // Grab all of the script properties
        SerializedProperty projectCreatedEvent;
        SerializedProperty host;
        SerializedProperty port;
        SerializedProperty projectName;
        SerializedProperty key;

        void OnEnable()
        {
            // Populate LogManager reference
            logManager = (LogManager)target;

            // Populate LogManager properties
            projectCreatedEvent = serializedObject.FindProperty("projectCreatedEvent");
            host = serializedObject.FindProperty("m_host");
            port = serializedObject.FindProperty("m_port");
            projectName = serializedObject.FindProperty("m_projectName");
            key = serializedObject.FindProperty("m_key");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            using (new EditorGUI.DisabledScope(key.stringValue != ""))
            {
                EditorGUILayout.PropertyField(projectCreatedEvent);
                EditorGUILayout.Space();

                GUILayout.Label("Server Settings", EditorStyles.boldLabel);
                EditorGUILayout.PropertyField(host);
                EditorGUILayout.PropertyField(port);

                EditorGUILayout.Space();
                GUILayout.Label("Project Settings", EditorStyles.boldLabel);
                EditorGUILayout.PropertyField(projectName, new GUIContent("Project Name"));
            }

            EditorGUILayout.PropertyField(key);

            if (key.stringValue == "")
            {
                EditorGUILayout.Space();

                if (GUILayout.Button("Create Project"))
                {
                    logManager.CreateProject(new List<IMeasurable>(), new List<Snapshot>());
                }
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
}
