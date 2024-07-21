using UnityEditor;
using UnityEngine;

public enum ClothingSetType
{
    One,
    Two,
    None,
}

[CreateAssetMenu(fileName = "CustomItem", menuName = "ScriptableObjects/CharacterCustomItem", order = 1)]
public class CharacterCustomItem : ScriptableObject
{
    public string itemName;
    public string skeletonPartName;
    public Sprite itemSprite;
    public CategoryType type;
    public ClothingSetType setType = ClothingSetType.None;

    private void OnValidate()
    {
        string fileName = "";
        switch (type)
        {
            case CategoryType.Body:
                fileName += "body_";
                break;
            case CategoryType.Hair:
                fileName += "hair_";
                break;
            case CategoryType.Accessories:
                fileName += "acc_";
                break;
            default:
                break;
        }
        fileName += RemoveSpacesFromString(itemName);
        AssetDatabase.RenameAsset(AssetDatabase.GetAssetPath(this), fileName);
    }

    private string RemoveSpacesFromString(string aString)
    {
        return aString.Replace(" ", "");
    }
}
