namespace Advize_Armoire;

using System.Collections.Generic;
using TMPro;
using UnityEngine;

public static class UIResources
{
    public static readonly ResourceCache<Sprite> SpriteCache = new();
    public static readonly ResourceCache<Material> MaterialCache = new();
    public static readonly ResourceCache<Texture2D> TextureCache = new();

    public static readonly Dictionary<string, Font> FontCache = [];
    public static readonly Dictionary<string, TMP_FontAsset> FontAssetCache = [];

    public static Sprite GetSprite(string spriteName) => SpriteCache.GetResource(spriteName);
    public static Material GetMaterial(string materialName) => MaterialCache.GetResource(materialName);
    public static Texture2D GetTexture(string textureName) => TextureCache.GetResource(textureName);

    public static Sprite GetItemIcon(string prefabName, int variant = 0)
    {
        string cacheKey = $"{prefabName}.{variant.ToString()}";
        Sprite cachedSprite = SpriteCache.GetResource(cacheKey);
        if (cachedSprite) return cachedSprite;

        GameObject prefab = ZNetScene.instance.GetPrefab(prefabName);

        if (prefab && prefab.TryGetComponent(out ItemDrop itemDrop))
        {
            if (itemDrop.m_itemData?.m_shared?.m_icons?.Length > variant)
            {
                Sprite prefabIcon = itemDrop.m_itemData.m_shared.m_icons[variant];
                if (prefabIcon)
                {
                    SpriteCache.SetResource(cacheKey, prefabIcon);
                    return prefabIcon;
                }
            }
        }

        return null;
    }

    public static TMP_FontAsset GetFontAsset(string fontName)
    {
        if (!FontAssetCache.TryGetValue(fontName, out TMP_FontAsset fontAsset))
        {
            foreach (TMP_FontAsset candidate in Resources.FindObjectsOfTypeAll<TMP_FontAsset>())
            {
                if (candidate.name == fontName)
                {
                    fontAsset = candidate;
                    break;
                }
            }
            FontAssetCache[fontName] = fontAsset;
        }

        return fontAsset;
    }
}

public sealed class ResourceCache<T> where T : UnityEngine.Object
{
    readonly Dictionary<string, T> _cache = [];

    public T GetResource(string resourceName)
    {
        if (!_cache.TryGetValue(resourceName, out T cachedResource))
        {
            foreach (T candidate in Resources.FindObjectsOfTypeAll<T>())
            {
                if (candidate.name == resourceName)
                {
                    cachedResource = candidate;
                    break;
                }
            }
            _cache[resourceName] = cachedResource;
        }

        return cachedResource;
    }

    public void SetResource(string resourceName, T resource) => _cache[resourceName] = resource;
}
