using UnityEngine;

namespace Assets.Scripts.UI.Utils
{
    /// <summary>
    /// Меняет размер у приципленной камеры ориентируясь на размер другой камеры
    /// </summary>
    [RequireComponent(typeof(Camera))]
    public class OrthographicCamScaler : MonoBehaviour
    {
        private Camera cam;
        [SerializeField] private Camera targetCam;
        void Start()
        {
            cam = GetComponent<Camera>();
        }
        void Update()
        {
            cam.orthographicSize = targetCam.orthographicSize;
        }
    }
}
