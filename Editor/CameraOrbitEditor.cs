using Newtonsoft.Json;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace XRToolkit
{
    [CustomEditor(typeof(CameraOrbit))]
    public class CameraOrbitEditor : Editor
    {
        private CameraOrbit t;
        // Serialized properties
        private SerializedProperty m_target;
        private SerializedProperty m_rotationSpeedX;
        private SerializedProperty m_rotationSpeedY;
        private SerializedProperty m_zoomSpeed;
        private SerializedProperty m_zoomSpeedCurve;
        private SerializedProperty m_minVerticalAngle;
        private SerializedProperty m_maxVerticalAngle;
        private SerializedProperty m_distance;
        private SerializedProperty m_minDistance;
        private SerializedProperty m_maxDistance;
        private SerializedProperty m_dampingFactor;
        private SerializedProperty m_useDamping;
        private SerializedProperty m_enableDragging;
        private SerializedProperty m_allowTargetVerticalMovement;
        private SerializedProperty m_targetMoveSpeed;
        private SerializedProperty m_targetMinHeight;
        private SerializedProperty m_targetMaxHeight;
        private SerializedProperty m_groundLayer;
        private SerializedProperty m_groundOffset;

        private string dataName = "CameraLocation";

        private void OnEnable()
        {
            t = (CameraOrbit)target;

            // Find the serialized properties
            m_target = serializedObject.FindProperty("target");
            m_rotationSpeedX = serializedObject.FindProperty("rotationSpeedX");
            m_rotationSpeedY = serializedObject.FindProperty("rotationSpeedY");
            m_zoomSpeed = serializedObject.FindProperty("zoomSpeed");
            m_zoomSpeedCurve = serializedObject.FindProperty("zoomSpeedCurve");
            m_minVerticalAngle = serializedObject.FindProperty("minVerticalAngle");
            m_maxVerticalAngle = serializedObject.FindProperty("maxVerticalAngle");
            m_distance = serializedObject.FindProperty("distance");
            m_minDistance = serializedObject.FindProperty("minDistance");
            m_maxDistance = serializedObject.FindProperty("maxDistance");
            m_useDamping = serializedObject.FindProperty("useDamping");
            m_dampingFactor = serializedObject.FindProperty("dampingFactor");
            m_enableDragging = serializedObject.FindProperty("enableDragging");
            m_allowTargetVerticalMovement = serializedObject.FindProperty("allowTargetVerticalMovement");
            m_targetMoveSpeed = serializedObject.FindProperty("targetMoveSpeed");
            m_targetMinHeight = serializedObject.FindProperty("targetMinHeight");
            m_targetMaxHeight = serializedObject.FindProperty("targetMaxHeight");
            m_groundLayer = serializedObject.FindProperty("groundLayer");
            m_groundOffset = serializedObject.FindProperty("groundOffset");
        }

        public override void OnInspectorGUI()
        {
            // Start custom inspector
            serializedObject.Update();
            EditorGUI.BeginChangeCheck();

            GUILayout.BeginVertical("Box");
            //拾取目标点
            GUILayout.BeginHorizontal();
            EditorGUILayout.PropertyField(m_target, new GUIContent("目标点"));
            if (GUILayout.Button("创建"))
            {
                var target = GameObject.Find("Target");
                if (target == null)
                    target = new GameObject("Target");
                t.target = target.transform;
            }
            GUILayout.EndHorizontal();
            EditorGUILayout.PropertyField(m_enableDragging, new GUIContent("启用视角水平移动"));
            EditorGUILayout.PropertyField(m_allowTargetVerticalMovement, new GUIContent("启用视角垂直移动"));
            if (t.allowTargetVerticalMovement)
            {
                GUILayout.BeginHorizontal();

                GUILayout.BeginHorizontal();
                GUILayout.Label("速度：");
                EditorGUILayout.PropertyField(m_targetMoveSpeed, new GUIContent(""));
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                GUILayout.Label("最低：");
                EditorGUILayout.PropertyField(m_targetMinHeight, new GUIContent(""));
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                GUILayout.Label("最高：");
                EditorGUILayout.PropertyField(m_targetMaxHeight, new GUIContent(""));
                GUILayout.EndHorizontal();

               

                GUILayout.EndHorizontal();
            }

            GUILayout.EndVertical();


            GUILayout.BeginVertical("Box");
            EditorGUILayout.PropertyField(m_rotationSpeedX, new GUIContent("水平旋转速度"));
            EditorGUILayout.PropertyField(m_rotationSpeedY, new GUIContent("垂直旋转速度"));
            EditorGUILayout.PropertyField(m_minVerticalAngle, new GUIContent("垂直旋转最低角度"));
            EditorGUILayout.PropertyField(m_maxVerticalAngle, new GUIContent("垂直旋转最高角度"));
            EditorGUILayout.PropertyField(m_zoomSpeed, new GUIContent("视角缩放速度"));
            EditorGUILayout.PropertyField(m_zoomSpeedCurve, new GUIContent("视角缩放速度曲线"));
            EditorGUILayout.PropertyField(m_distance, new GUIContent("视角距离"));
            EditorGUILayout.PropertyField(m_minDistance, new GUIContent("视角最小距离"));
            EditorGUILayout.PropertyField(m_maxDistance, new GUIContent("视角最大距离"));

            GUILayout.EndVertical();

            GUILayout.BeginVertical("Box");
            EditorGUILayout.PropertyField(m_groundLayer, new GUIContent("地面层"));
            EditorGUILayout.PropertyField(m_groundOffset, new GUIContent("地面偏移"));
            GUILayout.EndVertical();


            GUILayout.BeginVertical("Box");
            EditorGUILayout.PropertyField(m_useDamping, new GUIContent("启用阻尼"));
            if (t.useDamping)
                EditorGUILayout.PropertyField(m_dampingFactor, new GUIContent("阻尼"));

            GUILayout.EndVertical();


            GUILayout.BeginVertical("Box");
            GUILayout.Label("保存数据");
            GUILayout.BeginHorizontal();
            dataName = EditorGUILayout.TextField("数据名称", dataName);
            if (GUILayout.Button("保存"))
            {
                var path = Application.streamingAssetsPath + "/CameraData";
                if (!Directory.Exists(path))
                    Directory.CreateDirectory(path);
                var data = new CameraLocation();
                data.Position = new SerializableVector3(t.transform.position);
                data.Rotation = new SerializableVector3(t.transform.rotation.eulerAngles);
                data.Distance = t.distance;
                data.HorizontalRotation = t.horizontalRotation;
                data.VerticalRotation = t.verticalRotation;
                data.Pivot = new SerializableVector3(t.target.transform.position);
                //保存数据
                var s = JsonConvert.SerializeObject(data, Formatting.Indented);
                File.WriteAllText($"{path}/{dataName}.json", s);
            }

            if (GUILayout.Button("加载"))
            {
                var url = $"{Application.streamingAssetsPath}/CameraData/{dataName}.json";
                if (!File.Exists(url))
                    Debug.Log("数据不存在");
                else
                {
                    var json = File.ReadAllText(url);
                    var data = JsonConvert.DeserializeObject<CameraLocation>(json);
                    t.transform.position = data.Position.ToVector3();
                    t.transform.rotation = Quaternion.Euler(data.Rotation.ToVector3());
                    t.distance = data.Distance;
                    t.horizontalRotation = data.HorizontalRotation;
                    t.verticalRotation = data.VerticalRotation;
                    t.target.position = data.Pivot.ToVector3();
                }

            }

            GUILayout.EndHorizontal();
            GUILayout.EndVertical();




            if (EditorGUI.EndChangeCheck())
            {
                serializedObject.ApplyModifiedProperties();
            }

        }


    }
}