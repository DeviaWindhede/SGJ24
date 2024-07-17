using UnityEngine;

[CreateAssetMenu(fileName = "CustomItem", menuName = "ScriptableObjects/CharacterCustomItem", order = 1)]
public class CharacterCustomItem : ScriptableObject
{
    public string itemName;
    public int index;
    public Sprite itemSprite;
    public CategoryType type;
}
