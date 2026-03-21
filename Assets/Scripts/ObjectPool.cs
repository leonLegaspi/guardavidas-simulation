using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Pool generico reutilizable para cualquier componente de Unity.
/// Evita Instantiate/Destroy en runtime, mejorando la performance.
/// </summary>
public class ObjectPool<T> where T : Component
{
    private readonly T prefab;
    private readonly Transform parent;
    private readonly Queue<T> available = new();

    public ObjectPool(T prefab, Transform parent, int initialSize = 0)
    {
        this.prefab = prefab;
        this.parent = parent;

        // Pre-calentar el pool
        for (int i = 0; i < initialSize; i++)
        {
            T instance = Create();
            instance.gameObject.SetActive(false);
            available.Enqueue(instance);
        }
    }

    /// <summary>
    /// Obtiene un objeto del pool. Si no hay disponibles, crea uno nuevo.
    /// </summary>
    public T Get(Vector3 position, Quaternion rotation)
    {
        T instance = available.Count > 0 ? available.Dequeue() : Create();

        instance.transform.SetPositionAndRotation(position, rotation);
        instance.gameObject.SetActive(true);

        return instance;
    }

    /// <summary>
    /// Devuelve un objeto al pool para reutilizarlo.
    /// </summary>
    public void Return(T instance)
    {
        instance.gameObject.SetActive(false);
        available.Enqueue(instance);
    }

    public int AvailableCount => available.Count;

    private T Create()
    {
        T instance = Object.Instantiate(prefab, parent);
        instance.gameObject.SetActive(false);
        return instance;
    }
}
