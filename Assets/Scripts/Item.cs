//This class is designed for scriptable objects

using UnityEngine;

[CreateAssetMenu(menuName = "Match-3/Item")]
public  class Item : ScriptableObject
{
    public int value;
    public Sprite sprite;
}
