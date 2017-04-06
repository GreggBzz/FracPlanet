using UnityEngine;

public class ScreenFadeControl : MonoBehaviour
{
    public Material fadeMaterial = null;

    void OnPostRender()
    {
        fadeMaterial.SetPass(0);
        GL.PushMatrix();
        GL.LoadOrtho();
        GL.Color(fadeMaterial.color);
        GL.Begin(GL.QUADS);
        GL.Vertex3(0f, 0f, -12f);
        GL.Vertex3(0f, 1f, -12f);
        GL.Vertex3(1f, 1f, -12f);
        GL.Vertex3(1f, 0f, -12f);
        GL.End();
        GL.PopMatrix();
    }
}
