using UnityEngine;
using System;
using System.Linq;
using AccidentalNoise;

[CreateAssetMenu(fileName = "AccidentalNoiseGeneration", menuName = "Algorithms/AccidentalNoiseGeneration")]
public class AccidentalNoiseGeneration : AlgorithmBase
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

    [Header("Moisure Settings")]
    public int MoisureOctaves;
    public float MoisurePersistance;
    public float MoisureLacunarity;
    public float MoisureNoiseScale;
    public Vector2 MoisureOffset;

    //new 2
    [Serializable]
    class MoisureValues
    {   
        [Range(0f, 1f)]
        public float MoisureHeight;
        public GroundTileType GroundTile;
    }

    private MoisureValues[] MoisureTypes;
    //new 2

    [Serializable]
    class NoiseValues
    {   
        [Range(0f, 1f)]
        public float Height;
        public MoisureValues[] MoisureValues;
      
    }

    [SerializeField]
    private NoiseValues[] TileTypes;

    public override void Apply(TilemapGeneration tilemap)
    {
        // Make sure that TileTypes are ordered from small to high height
        TileTypes = TileTypes.OrderBy(a => a.Height).ToArray();

        //new 2
        MoisureTypes = MoisureTypes.OrderBy(a => a.MoisureHeight).ToArray();
        //new 2

        // Pass along our parameters to generate our noise
        var noiseMap = AccidentalNois.GenerateNoiseMap(tilemap.Width, tilemap.Height, tilemap.Grid.Seed, NoiseScale, Octaves, Persistance, Lacunarity, Offset);

        //new 2
        var moisureMap = MoisureNoise.GenerateMoisureMap(tilemap.Width, tilemap.Height, tilemap.Grid.Seed, MoisureNoiseScale, MoisureOctaves, MoisurePersistance, MoisureLacunarity, MoisureOffset);
        //new 2

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

                    //new 2
                    float subtractedValue2 = moisureMap[y * tilemap.Width + x] - islandGradient[y * tilemap.Width + x];
                    moisureMap[y * tilemap.Width + x] = Mathf.Clamp01(subtractedValue2);
                    //new 2
                }
            }
        }
        
        for (int x = 0; x < tilemap.Width; x++)
        {
            for (int y = 0; y < tilemap.Height; y++)
            {
                // Get height at this position
                var height = noiseMap[y * tilemap.Width + x];

                //new 2
                var moisureHeight = moisureMap[y * tilemap.Width + x];
                //new 2
                
                // Loop over our configured tile types
                for (int i = 0; i < TileTypes.Length; i++)
                {   
                    var TileType = TileTypes[i];

                    for (var j = 0; j < TileType.MoisureValues.Length; j++)
                    {                          
                        // If the height is smaller or equal then use this tiletype
                        if(height <= TileType.Height)
                        {
                            if (moisureHeight <= TileType.MoisureValues[j].MoisureHeight)
                            {                
                                tilemap.SetTile(x, y, (int)TileType.MoisureValues[j].GroundTile);
                                break;     
                            } 
                        }                                                           
                    }             
                }

                /*
                foreach (var c in TileTypes)
                {   
                    if (height <= c.Height)
                    {
                        foreach(var c2 in c.MoisureValues)
                        {                                                                
                            if (moisureHeight <= c2.MoisureHeight)
                            {                
                                tilemap.SetTile(x, y, (int)c2.GroundTile);
                                break;     
                            }                                                                                              
                        }
                    }                            
                }
                */
                
            }
        }
    }
    
}
