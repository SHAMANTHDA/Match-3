using DG.Tweening;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using Random = UnityEngine.Random;

public  class Board : MonoBehaviour
{
    public static Board Instance { get; private set; } //Singleton

    //two properties which should be assgined in unity editor
    [SerializeField] private AudioClip collectSound;
    [SerializeField] private AudioSource audioSource;

    public Row[] rows; //Creates an instance of row as array

    public Tile[,] Tiles { get; private set; } //Instance of slass tiles of 2D array 

    public int width => Tiles.GetLength(dimension: 0); //first diemnsion of array
    public int height => Tiles.GetLength(dimension: 1); //second dimension of array

    private readonly List<Tile> _selection = new List<Tile>(); //initializes the _selection field with a new instance

    private const float TweenDuration = 0.25f;// duration of animation

    private void Awake()
    {
        Instance = this;
        //By assigning this to Instance, the class ensures that there is only one instance of itself throughout the
        //game's execution, and other parts of the code can access this instance through the Instance variable.
    }

    private void Start()
    {
        Tiles = new Tile[rows.Max(row => row.tiles.Length), rows.Length]; //finds the length of the longest row in terms of tiles and uses that as the width of the array.

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                var tile = rows[y].tiles[x];

                tile.x = x;
                tile.y = y;

                tile.Item = ItemDataBase.Items[Random.Range(0, ItemDataBase.Items.Length)];

                Tiles[x, y] = tile;
            }
        }
        pop();
    }

    public void Select(Tile tile)
    {
        // If the tile is not already in the selection, add it to the selection.
        if (!_selection.Contains(tile))
        {
            // If there are already two tiles in the selection, clear the selection and add this tile.
            if (_selection.Count >= 2)
            {
                _selection.Clear();
                _selection.Add(tile);
            }
            else
            {
                // If there is one tile in the selection, check if it's adjacent to the new tile.
                if (_selection.Count == 1 && IsAdjacent(_selection[0], tile))
                {
                    _selection.Add(tile);
                    // Perform the swap.
                    PerformSwap();
                }
                else
                {
                    // If the new tile is not adjacent to the tile in the selection, clear the selection and add the new tile.
                    _selection.Clear();
                    _selection.Add(tile);
                }
            }
        }
    }


    private async void PerformSwap()
    {
        if (_selection.Count != 2) return;

        var tile1 = _selection[0];
        var tile2 = _selection[1];

        await Swap(tile1, tile2);

        if (canPop())
        {
            pop();
        }
        else
        {
            await Swap(tile1, tile2);
        }

        _selection.Clear();
    }

    private bool CanSwapBack(Tile tile1, Tile tile2)
    {
        // Temporarily swap the tiles
        Swap(tile1, tile2).Wait();

        // Check if any of the swapped tiles can create a match
        var canPop = HasAnyMatches();

        // Swap back the tiles to their original positions
        Swap(tile1, tile2).Wait();

        return canPop;
    }

    private bool HasAnyMatches()
    {
        // Check if there are any valid moves by iterating through all tiles on the board
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                // Check if swapping this tile with its adjacent tiles creates a match
                var tile = Tiles[x, y];

                // Check if swapping with right neighbor creates a match
                if (x + 1 < width && CanSwapBack(tile, Tiles[x + 1, y]))
                    return true;

                // Check if swapping with bottom neighbor creates a match
                if (y + 1 < height && CanSwapBack(tile, Tiles[x, y + 1]))
                    return true;
            }
        }

        // No valid moves found
        return false;
    }

    private bool IsAdjacent(Tile tile1, Tile tile2)
    {
        // Check if two tiles are adjacent by comparing their positions.
        return Mathf.Abs(tile1.x - tile2.x) + Mathf.Abs(tile1.y - tile2.y) == 1;
    }

    public async Task Swap(Tile tile1, Tile tile2)
    {
        var icon1 = tile1.icon;
        var icon2 = tile2.icon;

        var icon1transfomr = icon1.transform;
        var icon2transfomr = icon2.transform;

        var sequence = DOTween.Sequence();

        sequence.Join(icon1transfomr.DOMove(icon2transfomr.position, TweenDuration))
                .Join(icon2transfomr.DOMove(icon1transfomr.position, TweenDuration));

        await sequence.Play().AsyncWaitForCompletion();

        icon1transfomr.SetParent(tile2.transform);
        icon2transfomr.SetParent(tile1.transform);

        tile1.icon = icon2;
        tile2.icon = icon1;

        var tile1Item = tile1.Item;

        tile1.Item = tile2.Item;
        tile2.Item = tile1Item;
    }

    private bool canPop()
    {
        for (var y = 0; y < height; y++)
        {
            for (var x = 0; x < width; x++)
            {
                if (Tiles[x, y].GetConnectedTiles().Skip(1).Count() >= 2)
                {
                    return true;
                }
            }
        }
        return false;
    }

    private async void pop()
    {
        for (var y = 0; y < height; y++)
        {
            for (var x = 0; x < width; x++)
            {
                var tile = Tiles[x, y];

                var connectedTiles = tile.GetConnectedTiles();

                if (connectedTiles.Skip(1).Count() < 2) continue;

                var deflatsequence = DOTween.Sequence();


                foreach (var connectedTile in connectedTiles)
                {
                    deflatsequence.Join(connectedTile.icon.transform.DOScale(Vector3.zero, TweenDuration));
                }

                audioSource.PlayOneShot(collectSound);

                ScoreCounter.Instance.Score += tile.Item.value * connectedTiles.Count;

                await deflatsequence.Play().AsyncWaitForCompletion();


                var inflatesequence = DOTween.Sequence();

                foreach (var connectedTile in connectedTiles)
                {
                    connectedTile.Item = ItemDataBase.Items[Random.Range(0, ItemDataBase.Items.Length)];

                    inflatesequence.Join(connectedTile.icon.transform.DOScale(Vector3.one, TweenDuration));
                }

                await inflatesequence.Play().AsyncWaitForCompletion();

                x = 0;
                y = 0;
            }
        }
    }
}
