using UnityEditor;
using UnityEngine;

// Crisp pixel-art import for everything under Assets/Art: point filter, no
// compression, 32 px per Unity unit so 1 tile == 1 world unit.
public sealed class PixelArtImportPostprocessor : AssetPostprocessor
{
    private const string ArtRoot = "Assets/Resources/";
    private const int PixelsPerUnit = 32;

    private void OnPreprocessTexture()
    {
        if (!assetPath.StartsWith(ArtRoot))
        {
            return;
        }

        var importer = (TextureImporter)assetImporter;
        importer.textureType = TextureImporterType.Sprite;
        importer.spritePixelsPerUnit = PixelsPerUnit;
        importer.filterMode = FilterMode.Point;
        importer.textureCompression = TextureImporterCompression.Uncompressed;
        importer.mipmapEnabled = false;
        importer.alphaIsTransparency = true;
        importer.wrapMode = TextureWrapMode.Clamp;
    }
}
