// ParallaxController.cs
using UnityEngine;

public class ParallaxController : MonoBehaviour
{
    [System.Serializable]
    public class Layer {
        public Transform target;
        [Range(0f, 2f)] public float strength;
        public bool followY = false;
    }

    public Transform cam;
    public Layer[] layers;

    private Vector3 _prevCamPos;

    void Awake() {
        if (!cam) cam = Camera.main.transform;
        _prevCamPos = cam.position;
    }

    void LateUpdate() {
        Vector3 delta = cam.position - _prevCamPos;

        foreach (var l in layers) {
            if (!l.target) continue;
            float dx = delta.x * l.strength;
            float dy = l.followY ? delta.y * l.strength : 0f;
            l.target.position += new Vector3(dx, dy, 0f);
        }

        _prevCamPos = cam.position;
    }
}
