using System;
using System.Collections.Generic;

namespace cAlgo
{
    /// <summary>
    /// Generic LRU (Least Recently Used) Cache implementation
    /// </summary>
    /// <typeparam name="TKey">Type of the cache key</typeparam>
    /// <typeparam name="TValue">Type of the cached value</typeparam>
    public class LRUCache<TKey, TValue>
    {
        private readonly int _capacity;
        private readonly Dictionary<TKey, LinkedListNode<CacheItem>> _cache;
        private readonly LinkedList<CacheItem> _lruList;

        /// <summary>
        /// Structure to hold both the key and value in the linked list
        /// </summary>
        private class CacheItem
        {
            public TKey Key { get; }
            public TValue Value { get; }

            public CacheItem(TKey key, TValue value)
            {
                Key = key;
                Value = value;
            }
        }

        /// <summary>
        /// Creates a new LRU cache with the specified capacity
        /// </summary>
        /// <param name="capacity">Maximum number of items to store in the cache</param>
        public LRUCache(int capacity)
        {
            if (capacity <= 0)
                throw new ArgumentException("Capacity must be positive", nameof(capacity));

            _capacity = capacity;
            _cache = new Dictionary<TKey, LinkedListNode<CacheItem>>(capacity);
            _lruList = new LinkedList<CacheItem>();
        }

        /// <summary>
        /// Gets the number of items in the cache
        /// </summary>
        public int Count => _cache.Count;

        /// <summary>
        /// Tries to get a value from the cache
        /// </summary>
        /// <param name="key">Key to look up</param>
        /// <param name="value">Output value if found</param>
        /// <returns>True if the key was found, false otherwise</returns>
        public bool TryGetValue(TKey key, out TValue value)
        {
            if (_cache.TryGetValue(key, out LinkedListNode<CacheItem> node))
            {
                // Move the accessed node to the front of the list (most recently used)
                _lruList.Remove(node);
                _lruList.AddFirst(node);
                
                value = node.Value.Value;
                return true;
            }

            value = default;
            return false;
        }

        /// <summary>
        /// Adds or updates a value in the cache
        /// </summary>
        /// <param name="key">Key to add or update</param>
        /// <param name="value">Value to store</param>
        public void Set(TKey key, TValue value)
        {
            // If the key already exists, remove it first
            if (_cache.TryGetValue(key, out LinkedListNode<CacheItem> existingNode))
            {
                _lruList.Remove(existingNode);
                _cache.Remove(key);
            }
            // If at capacity, remove the least recently used item
            else if (_cache.Count >= _capacity)
            {
                LinkedListNode<CacheItem> lastNode = _lruList.Last;
                _lruList.RemoveLast();
                _cache.Remove(lastNode.Value.Key);
            }

            // Add the new item to the front of the list
            CacheItem cacheItem = new CacheItem(key, value);
            LinkedListNode<CacheItem> newNode = _lruList.AddFirst(cacheItem);
            _cache.Add(key, newNode);
        }

        /// <summary>
        /// Determines whether the cache contains the specified key
        /// </summary>
        /// <param name="key">Key to check</param>
        /// <returns>True if the key exists, false otherwise</returns>
        public bool ContainsKey(TKey key)
        {
            return _cache.ContainsKey(key);
        }

        /// <summary>
        /// Removes a key from the cache
        /// </summary>
        /// <param name="key">Key to remove</param>
        /// <returns>True if the key was removed, false if it didn't exist</returns>
        public bool Remove(TKey key)
        {
            if (_cache.TryGetValue(key, out LinkedListNode<CacheItem> node))
            {
                _lruList.Remove(node);
                return _cache.Remove(key);
            }
            return false;
        }

        /// <summary>
        /// Clears all items from the cache
        /// </summary>
        public void Clear()
        {
            _cache.Clear();
            _lruList.Clear();
        }
    }
}
