using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;

[CreateAssetMenu(fileName = "NoiseGeneration", menuName = "Algorithms/NoiseGeneration")]
public class NoiseGeneration : AlgorithmBase
{
    [Header("Noise settings")]
    // The more octaves, the longer generation will take
    public int Octaves; 
    [Range(0, 1)]
    public float Persistance;
    public float Lacunarity;
    public float NoiseScale;
    public Vector2 Offset;
    public bool ApplyIslandGradient;

    [Serializable]
    class NoiseValues
    {
        [Range(0f, 1f)]
        public float Height;
        public GroundTileType GroundTile;
    }

    [SerializeField]
    private NoiseValues[] TileTypes;

    public override void Apply(TilemapGeneration tilemap)
    {
        // Make sure that TileTypes are ordered from small to high height
        TileTypes = TileTypes.OrderBy(a => a.Height).ToArray();

        // Pass along our parameters to generate our noise
        var noiseMap = PNoise.GenerateNoiseMap(tilemap.Width, tilemap.Height, tilemap.Grid.Seed, NoiseScale, Octaves, Persistance, Lacunarity, Offset);

        // if Island Gradient applied
        if (ApplyIslandGradient)
        {
            var islandGradient = AccidentalNois.GenerateIslandGradientMap(tilemap.Width, tilemap.Height);
            for (int x = 0, y; x < tilemap.Width; x++)
            {
                for (y = 0; y < tilemap.Height; y++)
                {
                    // Subtract the islandGradient value from the noiseMap value
                    float subtractedValue = noiseMap[y * tilemap.Width + x] - islandGradient[y * tilemap.Width + x];

                    // Apply it into the map, but make sure we clamp it between 0f and 1f
                    noiseMap[y * tilemap.Width + x] = Mathf.Clamp01(subtractedValue);
                }
            }
        }

        for (int x=0; x < tilemap.Width; x++)
        {
            for (int y = 0; y < tilemap.Height; y++)
            {
                // Get height at this position
                var height = noiseMap[y * tilemap.Width + x];

                // Loop over our configured tile types
                for (int i = 0; i < TileTypes.Length; i++)
                {
                    // If the height is smaller or equal then use this tiletype
                    if (height <= TileTypes[i].Height)
                    {
                        tilemap.SetTile(x, y, (int)TileTypes[i].GroundTile);
                        break;
                    }
                }
            }
        }
    }
}