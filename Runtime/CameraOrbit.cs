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
        public float speedX = 200f;
        //垂直旋转速度
        public float speedY = 200f;
        //视角缩放速度
        public float speed = 100f;
        public AnimationCurve speedCurve = AnimationCurve.Linear(0.0f, 0.0f, 1.0f, 1.0f);
        //垂直旋转最小角度
        public float minY = 0f;
        //垂直旋转最高角度
        public float maxY = 90f;
        //视角距离
        public float distance = 100f;
        //视角最小距离
        public float minDistance = 0f;
        //视角最大距离
        public float maxDistance = 200f;
        //是否开启阻尼
        public bool isDamping = true;
        //阻尼值
        public float damping = 5.0f;
        //是否开启拖拽
        public bool isDrag=true;
        //是否开启目标点垂直移动
        public bool isTargetMove;
        //目标点移动速度
        public float targetMoveSpeed = 10f;
        //目标点最小高度
        public float targetMinHeight = -10f;
        //目标点最大高度
        public float targetMaxHeight = 40f;


        public float x = 0.0f;
        public float y = 0.0f;
        private Vector3 prevPos = Vector3.zero;

        private float originalX;
        private float originalY;
        private float originalDistance;
        private Vector3 originalTargetPosition;
        private Vector3 originalCameraPosition;
        private Quaternion originalCameraRotation;
        // 地面的高度
        public float groundHeight = 0f;
        // 离地面最近的距离
        public float groundOffset = 1f; 
        #endregion
        private void Start()
        {
            if (target == null)
            {
                Debug.LogError("未设置目标点");
                return;
            }

            Vector3 angles = transform.eulerAngles;
            x = angles.y;
            y = angles.x;
            //记录相机初始位置
            SetOriginalLocation();
        }
        private void Update()
        {
            OnRotate();
            PushAndPull();
            if (isDrag)
                OnDrag();
            if(isTargetMove)
                OnTargetMove();
        }
        #region Private Function
        /// <summary>
        /// 旋转
        /// </summary>
        private void OnRotate()
        {
            if (Input.GetMouseButton(1))
            {
                x += Input.GetAxis("Mouse X") * speedX * 0.02f;
                y -= Input.GetAxis("Mouse Y") * speedY * 0.02f;
                y = ClampAngle(y, minY, maxY);
            }

            var rotation = Quaternion.Euler(y, x, 0.0f);
            Vector3 disVector = new Vector3(0.0f, 0.0f, -distance);
            Vector3 position = rotation * disVector + target.position;

            // 检测地面
            if (position.y < groundHeight + groundOffset)
            {
                position.y = groundHeight + groundOffset;
            }

            if (isDamping)
            {
                transform.rotation = Quaternion.Lerp(transform.rotation, rotation, Time.deltaTime * damping);
                transform.position = Vector3.Lerp(transform.position, position, Time.deltaTime * damping);
            }
            else
            {
                transform.SetPositionAndRotation(position, rotation);
            }
        }
        /// <summary>
        /// 推拉
        /// </summary>
        private void PushAndPull()
        {
            // 将距离归一化到0到1之间
            float normalizedDistance = distance / maxDistance;
            float s = speed * speedCurve.Evaluate(normalizedDistance);
            if (!EventSystem.current.IsPointerOverGameObject())
                distance -= Input.GetAxis("Mouse ScrollWheel") * s;
            distance = Mathf.Clamp(distance, minDistance, maxDistance);
        }
        /// <summary>
        /// 拖拽
        /// </summary>
        private void OnDrag()
        {
            if (Input.GetMouseButtonDown(2))
            {
                var screen = GetComponent<Camera>().WorldToScreenPoint(target.position);
                prevPos = new Vector3(Input.mousePosition.x, Input.mousePosition.y, screen.z);
            }
            if (Input.GetMouseButtonUp(2))
                prevPos = Vector3.zero;
            if (!Input.GetMouseButton(2)) return;
            if (EventSystem.current.IsPointerOverGameObject()) return;
            var position = target.position;
            var screenSpace = GetComponent<Camera>().WorldToScreenPoint(position);
            var currPos = new Vector3(Input.mousePosition.x, Input.mousePosition.y, screenSpace.z);
            var prev = GetComponent<Camera>().ScreenToWorldPoint(prevPos);
            var curr = GetComponent<Camera>().ScreenToWorldPoint(currPos);
            var offset = curr - prev;
            offset.y = 0;
            position += -offset;
            target.position = position;
            prevPos = currPos;
        }
        /// <summary>
        /// 目标点垂直移动
        /// </summary>
        private void OnTargetMove()
        {
            if (Input.GetKey(KeyCode.UpArrow))
                target.transform.Translate(0, Time.deltaTime * targetMoveSpeed, 0);
            else if (Input.GetKey(KeyCode.DownArrow))
                target.transform.Translate(0, Time.deltaTime * targetMoveSpeed * -1, 0);

            var pos = target.transform.position;
            if (pos.y <= targetMinHeight)
                target.transform.position = new Vector3(pos.x, targetMinHeight, pos.z);
            if (pos.y >= targetMaxHeight)
                target.transform.position = new Vector3(pos.x, targetMaxHeight, pos.z);
        }
        /// <summary>
        /// 记录相机初始位置
        /// </summary>
        private void SetOriginalLocation()
        {
            originalCameraPosition = this.transform.position;
            originalCameraRotation = this.transform.rotation;
            originalTargetPosition = target.position;
            originalX = x;
            originalY = y;
            originalDistance = distance;
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
            transform.SetPositionAndRotation(originalCameraPosition, originalCameraRotation);
            target.position = originalTargetPosition;
            x = originalX;
            y = originalY;
            distance = originalDistance;
        }
        /// <summary>
        /// 设置视角
        /// </summary>
        /// <param name="data"></param>
        public void SetView(CameraLocation data)
        {
            var t = this.transform;
            t.SetPositionAndRotation(data.Position, Quaternion.Euler(data.Rotation));
            target.position = data.Pivot;
            x = data.Xy.x;
            y = data.Xy.y;
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
        /// <param name="pivot"></param>
        /// <param name="distance"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        public void SetView(Vector3 pivot, float distance, float x, float y)
        {
            this.x = x;
            this.y = y;
            this.distance = distance;
            target.position = pivot;
        }
        #endregion
    }

   
}