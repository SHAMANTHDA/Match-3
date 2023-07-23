using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public  class Tile : MonoBehaviour
{
    //x & y co-oridinates
    public int x;
    public int y;

    //Instance of an Item class
    private Item _item;

    public Item Item //item is assgined to respective value and sprite
    {
        get => _item;

        set
        {
            if (_item == value) return;
            {
                _item = value;

                icon.sprite = _item.sprite;
            }
        }
    }

    //two properties which should be assgined in unity editor
    public Image icon;
    public Button button;



    //These properties provide a convenient way to access the neighboring tiles &
    //If the neighboring tiles are within the bounds of the grid, the properties return the respective Tile object others return to null
    public Tile Left => x > 0 ? Board.Instance.Tiles[x - 1, y] : null; //represents the tile to the left of the current Tile
    public Tile Top => y > 0 ? Board.Instance.Tiles[x, y - 1] : null; //represents the tile above the current Tile
    public Tile Right => x < Board.Instance.width - 1 ? Board.Instance.Tiles[x + 1, y] : null; //represents the tile to the right of the current Tile
    public Tile Bottom => y < Board.Instance.height - 1 ? Board.Instance.Tiles[x, y + 1] : null; //represents the tile below the current Tile

    public Tile[] Neighbours => new[] // New array initalizer  
    {
        Left,
        Top,
        Right,
        Bottom,
    };

    //This acts as button and includes select method from Board class
    private void Start() => button.onClick.AddListener(call: () => Board.Instance.Select(this));


    /*The GetConnectedTiles method is a recursive function that finds and returns a list of connected Tile objects
    that have the same Item value.*/
    public List<Tile> GetConnectedTiles(List<Tile> exclude = null) 
    {
        var result = new List<Tile> { this, }; //The list will contain all connected tiles that have the same Item value.

        if (exclude == null)
        {
            exclude = new List<Tile> { this, };
        }
        else
        {
            exclude.Add(this);
        }

        //This iterates over the neighboring tiles of the current tile
        foreach (var neighbour in Neighbours)
        {
            if (neighbour == null || exclude.Contains(neighbour) || neighbour.Item != Item) continue;
            {
                result.AddRange(collection: neighbour.GetConnectedTiles(exclude));
            }
        }

        return result;
    }

}
