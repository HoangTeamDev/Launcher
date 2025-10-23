using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceLocations;

public class AddressablesManager : MonoBehaviour
{
    public static AddressablesManager Instance { get; private set; }

    private readonly Dictionary<string, AsyncOperationHandle> _handleCache = new();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// Load 1 asset bất đồng bộ.
    /// </summary>
    public async UniTask<T> LoadAssetAsync<T>(string key, CancellationToken cancellationToken = default)
    {
        if (_handleCache.TryGetValue(key, out var existingHandle))
        {
            return (T)existingHandle.Result;
        }

        var handle = Addressables.LoadAssetAsync<T>(key);
        await handle.WithCancellation(cancellationToken);

        if (handle.Status == AsyncOperationStatus.Succeeded)
        {
            _handleCache[key] = handle;
            return handle.Result;
        }
        else
        {
            MainLog.LogError($"Failed to load asset:",$" {key}",ReadColor.Red);
            return default;
        }
    }

    /// <summary>
    /// Release 1 asset.
    /// </summary>
    public void ReleaseAsset(string key)
    {
        if (_handleCache.TryGetValue(key, out var handle))
        {
            Addressables.Release(handle);
            _handleCache.Remove(key);
        }
    }

    /// <summary>
    /// Load danh sách resource location theo label.
    /// </summary>
    public async UniTask<IList<IResourceLocation>> LoadLocationsAsync(string label, CancellationToken cancellationToken = default)
    {
        var handle = Addressables.LoadResourceLocationsAsync(label);
        await handle.WithCancellation(cancellationToken);

        if (handle.Status == AsyncOperationStatus.Succeeded)
        {
            return handle.Result;
        }
        else
        {
            MainLog.LogError($"Failed to load resource locations for label:", $" {label}", ReadColor.Red);
           
            return null;
        }
    }

    /// <summary>
    /// Load nhiều asset theo label.
    /// </summary>
    public async UniTask<List<T>> LoadAssetsByLabelAsync<T>(string label, CancellationToken cancellationToken = default)
    {
        var results = new List<T>();
        var locations = await LoadLocationsAsync(label, cancellationToken);
        if (locations == null) return results;

        foreach (var loc in locations)
        {
            var obj = await LoadAssetAsync<T>(loc.PrimaryKey, cancellationToken);
            results.Add(obj);
        }
        return results;
    }

    /// <summary>
    /// Release toàn bộ các asset đã load.
    /// </summary>
    public void ReleaseAll()
    {
        foreach (var handle in _handleCache.Values)
        {
            Addressables.Release(handle);
        }
        _handleCache.Clear();
    }
}
