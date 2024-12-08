using UnityEngine;
using UnityUtils;
public class ThumbnailGenerator : Singleton<ThumbnailGenerator>
{

    public Camera cam;

    public Sprite GenerateThumbnail(GameObject obj, float boundsPadding = 0.1f, Vector3 cameraOffset = default)
    {
        //Vector3 posOffset = Vector3.down * 1000; // Ensure out of view for thumbnail
        //obj.transform.position += posOffset;

        // Calculate bounds
        Bounds bounds = Utils.CalculateBounds(obj);
        cam.transform.position = bounds.center + (cameraOffset == default ? Vector3.back * 10 : cameraOffset);
        cam.orthographicSize = Mathf.Max(bounds.extents.x, bounds.extents.y) + boundsPadding;

        // Render texture
        RenderTexture renderTexture = cam.targetTexture;
        RenderTexture.active = renderTexture;
        cam.Render();

        Texture2D texture = new Texture2D(renderTexture.width, renderTexture.height, TextureFormat.RGBA32, false);
        texture.ReadPixels(new Rect(0, 0, renderTexture.width, renderTexture.height), 0, 0);
        texture.Apply();

        texture = Utils.MakeColorTransparent(texture, Color.white);

        RenderTexture.active = null;

        obj.SetActive(false);
        GameObject.Destroy(obj);

        return Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
    }
    
}
