using UnityEditor;
using UnityEngine;

namespace Machi.EditorTools
{
    /// <summary>Import settings for town models, the palette texture and 2D item art.</summary>
    public class MachiImportRules : AssetPostprocessor
    {
        const string TownDir = "Assets/Machi/Resources/Town/";
        const string ItemsDir = "Assets/Machi/Resources/Items/";

        void OnPreprocessModel()
        {
            if (!assetPath.StartsWith(TownDir)) return;
            var m = (ModelImporter)assetImporter;
            m.globalScale = 1f;
            m.importAnimation = false;
            m.importCameras = false;
            m.importLights = false;
            m.importBlendShapes = false;
            m.materialImportMode = ModelImporterMaterialImportMode.ImportViaMaterialDescription;
            m.isReadable = false;
        }

        void OnPreprocessTexture()
        {
            var t = (TextureImporter)assetImporter;
            if (assetPath.StartsWith(TownDir))
            {
                // The palette is a grid of flat colour swatches: no filtering, mips or compression.
                t.filterMode = FilterMode.Point;
                t.mipmapEnabled = false;
                t.textureCompression = TextureImporterCompression.Uncompressed;
                t.wrapMode = TextureWrapMode.Clamp;
            }
            else if (assetPath.StartsWith(ItemsDir))
            {
                t.textureType = TextureImporterType.Sprite;
                t.spriteImportMode = SpriteImportMode.Single;
                t.alphaIsTransparency = true;
                t.mipmapEnabled = false;
                t.maxTextureSize = 512;
            }
        }
    }
}
