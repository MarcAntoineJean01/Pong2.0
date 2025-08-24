using UnityEngine;
namespace PongGame.CameraLocker
{
    [System.Serializable]
    public class CameraMasks
    {
        public LayerMask mainDefault;
        public LayerMask mainFreeMove;
        public LayerMask overlay;
        public LayerMask leftPad;
        public LayerMask rightPad;
    }
}
