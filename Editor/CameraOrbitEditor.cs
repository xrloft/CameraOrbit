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
        private SerializedProperty m_speedX;
        private SerializedProperty m_speedY;
        private SerializedProperty m_speed;
        private SerializedProperty m_speedCurve;
        private SerializedProperty m_minY;
        private SerializedProperty m_maxY;
        private SerializedProperty m_distance;
        private SerializedProperty m_minDistance;
        private SerializedProperty m_maxDistance;
        private SerializedProperty m_damping;
        private SerializedProperty m_isDamping;
        private SerializedProperty m_isDrag;
        private SerializedProperty m_isTargetMove;
        private SerializedProperty m_targetMoveSpeed;
        private SerializedProperty m_targetMinHeight;
        private SerializedProperty m_targetMaxHeight;

        private string dataName = "CameraLocation";

        private void OnEnable()
        {
            t = (CameraOrbit)target;

            // Find the serialized properties
            m_target = serializedObject.FindProperty("target");
            m_speedX = serializedObject.FindProperty("speedX");
            m_speedY = serializedObject.FindProperty("speedY");
            m_speed = serializedObject.FindProperty("speed");
            m_speedCurve = serializedObject.FindProperty("speedCurve");
            m_minY = serializedObject.FindProperty("minY");
            m_maxY = serializedObject.FindProperty("maxY");
            m_distance = serializedObject.FindProperty("distance");
            m_minDistance = serializedObject.FindProperty("minDistance");
            m_maxDistance = serializedObject.FindProperty("maxDistance");
            m_isDamping = serializedObject.FindProperty("isDamping");
            m_damping = serializedObject.FindProperty("damping");
            m_isDrag = serializedObject.FindProperty("isDrag");
            m_isTargetMove = serializedObject.FindProperty("isTargetMove");
            m_targetMoveSpeed = serializedObject.FindProperty("targetMoveSpeed");
            m_targetMinHeight = serializedObject.FindProperty("targetMinHeight");
            m_targetMaxHeight = serializedObject.FindProperty("targetMaxHeight");
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
            EditorGUILayout.PropertyField(m_isDrag, new GUIContent("启用视角水平移动"));
            EditorGUILayout.PropertyField(m_isTargetMove, new GUIContent("启用视角垂直移动"));
            if (t.isTargetMove)
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
            EditorGUILayout.PropertyField(m_speedX, new GUIContent("水平旋转速度"));
            EditorGUILayout.PropertyField(m_speedY, new GUIContent("垂直旋转速度"));
            EditorGUILayout.PropertyField(m_minY, new GUIContent("垂直旋转最低角度"));
            EditorGUILayout.PropertyField(m_maxY, new GUIContent("垂直旋转最高角度"));
            EditorGUILayout.PropertyField(m_speed, new GUIContent("视角缩放速度"));
            EditorGUILayout.PropertyField(m_speedCurve, new GUIContent("视角缩放速度曲线"));
            EditorGUILayout.PropertyField(m_distance, new GUIContent("视角距离"));
            EditorGUILayout.PropertyField(m_minDistance, new GUIContent("视角最小距离"));
            EditorGUILayout.PropertyField(m_maxDistance, new GUIContent("视角最大距离"));

            GUILayout.EndVertical();

            GUILayout.BeginVertical("Box");
            EditorGUILayout.PropertyField(m_isDamping, new GUIContent("启用阻尼"));
            if (t.isDamping)
                EditorGUILayout.PropertyField(m_damping, new GUIContent("阻尼"));

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
                data.Position = t.transform.position;
                data.Rotation = t.transform.rotation.eulerAngles;
                data.Distance = t.distance;
                data.Xy = new Vector2(t.x, t.y);
                data.Pivot = t.target.transform.position;
                //保存数据
                var settings = new JsonSerializerSettings
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                };
                var s = JsonConvert.SerializeObject(data, Formatting.Indented, settings);
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
                    t.transform.position = data.Position;
                    t.transform.rotation = Quaternion.Euler(data.Rotation);
                    t.distance = data.Distance;
                    t.x = data.Xy.x;
                    t.y = data.Xy.y;
                    t.target.position = data.Pivot;
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