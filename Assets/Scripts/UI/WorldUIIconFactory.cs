using UnityEngine;
[CreateAssetMenu(menuName = "ScriptableObjects/WorldIcon")]
public class WorldUIIconFactory : ScriptableObject
{
    public Sprite icon;
    public Vector3 worldOffset;
    public Vector2 screenOffset;
    public WorldUIManager.WorldUIIcon Create(WorldUIManager.IconManager iconManager,Transform target)
    {
        GameObject iconClone = Instantiate(iconManager.iconPrefab, iconManager.ui.canvas.transform);

        FlexibileUIListItem uiListItem = iconClone.GetComponent<FlexibileUIListItem>();
        if (uiListItem == null)
            Debug.LogError("ICON PREFAB DOES NOT HAVE FLEXIBLE UI ON IT");
            
        uiListItem.GetSprite(0).sprite = icon;
            
        return new WorldUIManager.WorldUIIcon(this, iconClone, target);
    }
}
