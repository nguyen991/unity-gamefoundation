using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.U2D;
using System.Linq;
using System;
using Cysharp.Threading.Tasks;

namespace GameFoundation.Addressable
{
    public class SpriteLoader : Utilities.SingletonBehaviour<SpriteLoader>
    {
        public List<AssetLabelReference> spriteAtlasLabels;

        public bool Initialized { get; private set; } = false;

        private IList<SpriteAtlas> spriteAtlas = null;

        public async UniTask Init()
        {
            if (Initialized)
            {
                return;
            }

            try
            {
                if (spriteAtlasLabels.Count > 0)
                {
                    spriteAtlas = await Addressables.LoadAssetsAsync<SpriteAtlas>(spriteAtlasLabels.Select(label => label.labelString), null, Addressables.MergeMode.Union);
                }
            }
            catch (Exception)
            {
                Debug.Log($"SpriteLoader: load sprite atlas failed");
            }
            finally
            {
                Initialized = true;
            }
        }

        public Sprite GetSpriteFromAtlas(string name, string atlasName = null)
        {
            if (spriteAtlas == null)
            {
                return null;
            }
            if (atlasName == null)
            {
                return spriteAtlas.FirstOrDefault(atlas => atlas.GetSprite(name) != null)?.GetSprite(name);
            }
            else
            {
                return spriteAtlas.FirstOrDefault(atlas => atlas.name == atlasName)?.GetSprite(name);
            }
        }

        public async UniTask<Sprite> GetSprite(string name)
        {
            try
            {
                return await Addressables.LoadAssetAsync<Sprite>(name);
            }
            catch (Exception ex)
            {
                Debug.Log($"AtlasLoader: {ex.Message}");
            }
            return null;
        }
    }
}