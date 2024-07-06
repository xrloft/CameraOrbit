using UnityEngine;
using UnityEngine.EventSystems;

namespace XRToolkit
{
    /// <summary>
    /// 摄影机控制
    /// </summary>
    public class CameraOrbit : MonoBehaviour
    {
        #region Variables
        //目标点
        public Transform target;
        //水平旋转速度
        public float rotationSpeedX = 200f;
        //垂直旋转速度
        public float rotationSpeedY = 200f;
        //视角缩放速度
        public float zoomSpeed = 100f;
        public AnimationCurve zoomSpeedCurve = AnimationCurve.Linear(0.0f, 0.0f, 1.0f, 1.0f);
        //垂直旋转最小角度
        public float minVerticalAngle = 0f;
        //垂直旋转最高角度
        public float maxVerticalAngle = 90f;
        //视角距离
        public float distance = 100f;
        //视角最小距离
        public float minDistance = 0f;
        //视角最大距离
        public float maxDistance = 200f;
        //是否开启阻尼
        public bool useDamping = true;
        //阻尼值
        public float dampingFactor = 5.0f;
        //是否开启拖拽
        public bool enableDragging=true;
        //是否开启目标点垂直移动
        public bool allowTargetVerticalMovement;
        //目标点移动速度
        public float targetMoveSpeed = 10f;
        //目标点最小高度
        public float targetMinHeight = -10f;
        //目标点最大高度
        public float targetMaxHeight = 40f;
        // 水平旋转角度
        public float horizontalRotation = 0.0f;
        // 垂直旋转角度
        public float verticalRotation = 0.0f;
        private Vector3 previousMousePosition = Vector3.zero;

        private float initialHorizontalRotation;
        private float initialVerticalRotation;
        private float initialDistance;
        private Vector3 initialTargetPosition;
        private Vector3 initialCameraPosition;
        private Quaternion initialCameraRotation;
       
        //地面层
        public float groundOffset = 1f;
        public LayerMask groundLayer;
        #endregion
        private void Start()
        {
            if (target == null)
            {
                Debug.LogError("未设置目标点");
                return;
            }

            Vector3 angles = transform.eulerAngles;
            horizontalRotation = angles.y;
            verticalRotation = angles.x;
            //记录相机初始位置
            SaveInitialCameraPosition();
        }
        private void Update()
        {
            HandleRotation();
            HandleZoom();
            if (enableDragging) HandleDragging();
            if (allowTargetVerticalMovement) HandleTargetVerticalMovement();
            CheckGroundCollision(transform.position);
        }
        #region Private Function
        /// <summary>
        /// 旋转
        /// </summary>
        private void HandleRotation()
        {
            if (Input.GetMouseButton(1))
            {
                horizontalRotation += Input.GetAxis("Mouse X") * rotationSpeedX * 0.02f;
                verticalRotation -= Input.GetAxis("Mouse Y") * rotationSpeedY * 0.02f;
                verticalRotation = ClampAngle(verticalRotation, minVerticalAngle, maxVerticalAngle);
            }

            var rotation = Quaternion.Euler(verticalRotation, horizontalRotation, 0.0f);
            Vector3 disVector = new Vector3(0.0f, 0.0f, -distance);
            Vector3 position = rotation * disVector + target.position;
            if (useDamping)
            {
                transform.rotation = Quaternion.Lerp(transform.rotation, rotation, Time.deltaTime * dampingFactor);
                transform.position = Vector3.Lerp(transform.position, position, Time.deltaTime * dampingFactor);
            }
            else
            {
                transform.SetPositionAndRotation(position, rotation);
            }
        }

        private void CheckGroundCollision(Vector3 position)
        {
            RaycastHit hit;
            if (Physics.Raycast(transform.position, -Vector3.up, out hit, Mathf.Infinity, groundLayer))
            {
                if (hit.distance < groundOffset)
                {
                    transform.position = new Vector3(transform.position.x, hit.point.y + groundOffset, transform.position.z);
                }
            }
        }

        /// <summary>
        /// 推拉
        /// </summary>
        private void HandleZoom()
        {
            // 将距离归一化到0到1之间
            float normalizedDistance = distance / maxDistance;
            float s = zoomSpeed * zoomSpeedCurve.Evaluate(normalizedDistance);
            if (!EventSystem.current.IsPointerOverGameObject())
                distance -= Input.GetAxis("Mouse ScrollWheel") * s;
            distance = Mathf.Clamp(distance, minDistance, maxDistance);
        }
        /// <summary>
        /// 拖拽
        /// </summary>
        private void HandleDragging()
        {
            if (Input.GetMouseButtonDown(2))
            {
                previousMousePosition = GetMouseWorldPosition(target.position);
            }

            if (Input.GetMouseButtonUp(2))
            {
                previousMousePosition = Vector3.zero;
            }

            if (!Input.GetMouseButton(2) || EventSystem.current.IsPointerOverGameObject()) return;

            Vector3 currentMousePosition = GetMouseWorldPosition(target.position);
            Vector3 offset = currentMousePosition - previousMousePosition;
            offset.y = 0;
            target.position -= offset;
            previousMousePosition = currentMousePosition;
        }
        private Vector3 GetMouseWorldPosition(Vector3 referencePosition)
        {
            var screenPosition = GetComponent<Camera>().WorldToScreenPoint(referencePosition);
            return GetComponent<Camera>().ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, screenPosition.z));
        }
        /// <summary>
        /// 目标点垂直移动
        /// </summary>
        private void HandleTargetVerticalMovement()
        {
            if (Input.GetKey(KeyCode.UpArrow))
                target.Translate(0, Time.deltaTime * targetMoveSpeed, 0);
            else if (Input.GetKey(KeyCode.DownArrow))
                target.Translate(0, Time.deltaTime * targetMoveSpeed * -1, 0);

            target.position = new Vector3(target.position.x, Mathf.Clamp(target.position.y, targetMinHeight, targetMaxHeight), target.position.z);
        }

        /// <summary>
        /// 记录相机初始位置
        /// </summary>
        private void SaveInitialCameraPosition()
        {
            initialCameraPosition = this.transform.position;
            initialCameraRotation = this.transform.rotation;
            initialTargetPosition = target.position;
            initialHorizontalRotation = horizontalRotation;
            initialVerticalRotation = verticalRotation;
            initialDistance = distance;
        }
        /// <summary>
        /// 视角限制
        /// </summary>
        /// <param name="angle"></param>
        /// <param name="min"></param>
        /// <param name="max"></param>
        /// <returns></returns>
        private float ClampAngle(float angle, float min, float max)
        {
            if (angle < -360)
                angle += 360;
            if (angle > 360)
                angle -= 360;
            return Mathf.Clamp(angle, min, max);
        }
        #endregion

        #region Public Function
        /// <summary>
        /// 重置相机视角
        /// </summary>
        public void ResetCamera()
        {
            transform.SetPositionAndRotation(initialCameraPosition, initialCameraRotation);
            target.position = initialTargetPosition;
            horizontalRotation = initialHorizontalRotation;
            verticalRotation = initialVerticalRotation;
            distance = initialDistance;
        }
        /// <summary>
        /// 设置视角
        /// </summary>
        /// <param name="data"></param>
        public void SetView(CameraLocation data)
        {
            var t = this.transform;
            t.SetPositionAndRotation(data.Position.ToVector3(), Quaternion.Euler(data.Rotation.ToVector3()));
            target.position = data.Pivot.ToVector3();
            horizontalRotation = data.HorizontalRotation;
            verticalRotation = data.VerticalRotation;
            distance = data.Distance;
        }
        /// <summary>
        /// 设置视角
        /// </summary>
        /// <param name="pivot">目标点位置</param>
        /// <param name="distance">距离</param>
        public void SetView(Vector3 pivot,float distance)
        {
            target.position = pivot;
            this.distance = distance;
        }
        /// <summary>
        /// 设置视角
        /// </summary>
        /// <param name="pivot">目标点</param>
        /// <param name="distance">距离</param>
        /// <param name="horizontalRotation">水平旋转角度</param>
        /// <param name="verticalRotation">垂直旋转角度</param>
        public void SetView(Vector3 pivot, float distance, float horizontalRotation, float verticalRotation)
        {
            this.horizontalRotation = horizontalRotation;
            this.verticalRotation = verticalRotation;
            this.distance = distance;
            target.position = pivot;
        }
        #endregion
    }

   
}