using UnityEngine;
namespace XRToolkit
{
    public class CameraLocation
    {
        /// <summary>
        ///位置
        /// <summary>
        public SerializableVector3 Position { get; set; }
        /// <summary>
        ///旋转
        /// <summary>
        public SerializableVector3 Rotation { get; set; }
        /// <summary>
        ///距离
        /// <summary>
        public float Distance { get; set; }
        /// <summary>
        ///水平旋转
        /// <summary>
        public float HorizontalRotation { get; set; }
        /// <summary>
        /// 垂直旋转
        /// </summary>
        public float VerticalRotation { get; set; }
        /// <summary>
        ///Pivot
        /// <summary>
        public SerializableVector3 Pivot { get; set; }
    }

    public class SerializableVector3
    {
        public float x;
        public float y;
        public float z;

        public SerializableVector3(Vector3 vector)
        {
            x = vector.x;
            y = vector.y;
            z = vector.z;
        }

        public Vector3 ToVector3()
        {
            return new Vector3(x, y, z);
        }
    }

    

}

