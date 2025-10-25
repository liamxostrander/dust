using UnityEngine;

[RequireComponent(typeof(Camera))]
public class ForceRenderTextureClear : MonoBehaviour
{
    private Camera cam;

    void Awake()
    {
        cam = GetComponent<Camera>();
        cam.forceIntoRenderTexture = true;
    }

    void OnPreRender()
    {
        GL.Clear(true, true, Color.black);
    }
}
