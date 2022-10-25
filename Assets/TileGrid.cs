using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class TileGrid : MonoBehaviour
{
    public int Width, Height;
    public int TileSize, Seed;
    public Dictionary<int, Tile> Tiles { get; private set; }

    [Serializable]
    public class GroundTiles
    {   
        public GroundTileType TileType;
        public Texture2D Texture;
        public Color Color;
    }

    /*
    // new2
    // First choose biome type 
    // Then choose biome tiles
    [Serializable]
    public class BiomeType
    {
        public BiomeTileType BiomeTilesType;
        public GroundTiles[] TilesInBiome;
    }
    // new2
    */

    [Serializable]
    class ObjectTiles
    {
        public ObjectTileType TileType;
        public Texture2D Texture;
        public Color Color;
    }

    /*
    //new 2
    [SerializeField]
    private BiomeType[] BiomeTileTypes;
    //new 2
    */

    [SerializeField]
    private GroundTiles[] GroundTileTypes;

    [SerializeField]
    private ObjectTiles[] ObjectTileTypes;

    public Dictionary<TilemapType, TilemapGeneration> Tilemaps;

    private void Awake()
    {
        Tiles = InitializeTiles();

        Tilemaps = new Dictionary<TilemapType, TilemapGeneration>();

        // Add all our tilemaps by name to collection, so we can access them easily.
        foreach (Transform child in transform)
        {
            var tilemap = child.GetComponent<TilemapGeneration>();
            if (tilemap == null) continue;
            if (Tilemaps.ContainsKey(tilemap.Type))
            {
                throw new Exception("Duplicate tilemap type: " + tilemap.Type);
            }
            Tilemaps.Add(tilemap.Type, tilemap);
        }

        // Let's initialize our tilemaps now that they are in the collection.
        foreach (var tilemap in Tilemaps.Values)
        {
            tilemap.Initialize();
        }
    }

    private Dictionary<int, Tile> InitializeTiles() 
    {
        var dictionary = new Dictionary<int, Tile>();

        // Create a Tile for each GroundTileType
        foreach (var tiletype in GroundTileTypes)
        {
            // Don't make a tile for empty
            if (tiletype.TileType == 0) continue;
            // Create tile scriptable object
            var tile = CreateTile(tiletype.Color, tiletype.Texture);
            // Add to dictionary by key int value, value Tile
            dictionary.Add((int)tiletype.TileType, tile);
        }

        //new2
        /*
        for (int i = 0; i < BiomeTileTypes.Length; i++)
        {
            var currentBiome = BiomeTileTypes[i];

            for (int j = 0; j < currentBiome.TilesInBiome.Length; j++)
            {
               // var currentTileInBiome = currentBiome.TilesInBiome[j];

                foreach (var tiletype in currentBiome.TilesInBiome)
                {
                    // Don't make a tile for empty
                    if (tiletype.TileType == 0) continue;

                    // Create tile scriptable object
                    var tile = CreateTile(tiletype.Color, tiletype.Texture);

                    // Add to dictionary by key int value, value Tile
                    dictionary.Add((int)tiletype.TileType, tile);
                }
            }
        }
        */
        //new2       

        // Create a Tile for each ObjectTileType
        foreach (var tiletype in ObjectTileTypes)
        {
            // Don't make a tile for empty
            if (tiletype.TileType == 0) continue;
            // Create tile scriptable object
            var tile = CreateTile(tiletype.Color, tiletype.Texture);
            // Add to dictionary by key int value, value Tile
            dictionary.Add((int)tiletype.TileType, tile);
        }

        return dictionary;
    }

    private Tile CreateTile(Color color, Texture2D texture)
    {
        // If we haven't specified one, we just create an empty one for the color instead
        bool setColor = false;
        if (texture == null)
        {
            setColor = true;
            texture = new Texture2D(TileSize, TileSize);
        }

        // We should be using Point mode, to get the most quality out of our tiles
        texture.filterMode = FilterMode.Point;

        // Create our sprite with the texture passed along
        var sprite = Sprite.Create(texture, new Rect(0, 0, TileSize, TileSize), new Vector2(0.5f, 0.5f), TileSize);

        // Create a scriptable object instance of type Tile (inherits from TileBase)
        var tile = ScriptableObject.CreateInstance<Tile>();

        if (setColor)
        {
            // Make sure color is not transparant
            color.a = 1;
            // Set the tile color
            tile.color = color;
        }

        // Assign the sprite we created earlier to our tiles
        tile.sprite = sprite;

        return tile;
    }
}