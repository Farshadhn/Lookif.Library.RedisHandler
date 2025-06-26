using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Text.Json;

namespace Lookif.Library.RedisHandler
{
    public class RedisHandlerService
    {
        private readonly IDatabase _db;

        public RedisHandlerService(IConnectionMultiplexer multiplexer, int db = -1)
        {
            _db = multiplexer.GetDatabase(db);
        }

        // 1. Add new Key or Keys
        public async Task AddAsync(string key, string value, int? ttlMinutes = null)
        {
            await _db.StringSetAsync(key, value, ttlMinutes.HasValue ? TimeSpan.FromMinutes(ttlMinutes.Value) : (TimeSpan?)null);
        }

        public async Task AddAsync(Dictionary<string, string> keyValues, int? ttlMinutes = null)
        {
            foreach (var kv in keyValues)
            {
                await _db.StringSetAsync(kv.Key, kv.Value, ttlMinutes.HasValue ? TimeSpan.FromMinutes(ttlMinutes.Value) : (TimeSpan?)null);
            }
        }

        // Generic AddKeyAsync<T>
        public async Task AddAsync<T>(string key, T value, int? ttlMinutes = null)
        {
            var json = JsonSerializer.Serialize(value);
            await _db.StringSetAsync(key, json, ttlMinutes.HasValue ? TimeSpan.FromMinutes(ttlMinutes.Value) : (TimeSpan?)null);
        }
        public async Task AddAsync<T>(string key, List<T> value, int? ttlMinutes = null)
        {
            var json = JsonSerializer.Serialize(value);
            await _db.StringSetAsync(key, json, ttlMinutes.HasValue ? TimeSpan.FromMinutes(ttlMinutes.Value) : (TimeSpan?)null);
        }
        // Generic AddKeysAsync<T>
        public async Task AddAsync<T>(Dictionary<string, T> keyValues, int? ttlMinutes = null)
        {
            foreach (var kv in keyValues)
            {
                var json = JsonSerializer.Serialize(kv.Value);
                await _db.StringSetAsync(kv.Key, json, ttlMinutes.HasValue ? TimeSpan.FromMinutes(ttlMinutes.Value) : (TimeSpan?)null);
            }
        }

        // 2. Remove Key Or Keys
        public async Task RemoveAsync(string key)
        {
            await _db.KeyDeleteAsync(key);
        }

        public async Task RemoveAsync(IEnumerable<string> keys)
        {
            var redisKeys = keys.Select(k => (RedisKey)k).ToArray();
            await _db.KeyDeleteAsync(redisKeys);
        }

        // 3. Remove Key by regex
        public async Task RemoveByRegexAsync(string pattern)
        {
            var endpoints = _db.Multiplexer.GetEndPoints();
            var server = _db.Multiplexer.GetServer(endpoints.First());
            var allKeys = server.Keys(_db.Database).Select(k => (string)k);
            var regex = new Regex(pattern);
            var matchedKeys = allKeys.Where(k => regex.IsMatch(k)).ToArray();
            if (matchedKeys.Length > 0)
            {
                await RemoveAsync(matchedKeys);
            }
        }
        public async Task RemoveByPrefixAsync(string prefix)
        {
            var endpoints = _db.Multiplexer.GetEndPoints();
            var server = _db.Multiplexer.GetServer(endpoints.First());

            // Use the SCAN pattern to avoid loading all keys at once
            var allKeys = server.Keys(_db.Database, pattern: $"{prefix}*");

            var keyList = allKeys.Select(k => (string)k).ToArray();

            if (keyList.Length > 0)
            {
                await RemoveAsync(keyList);
            }
        }
        // 4. Read Key or Keys
        public async Task<string?> ReadAsync(string key)
        {
            return await _db.StringGetAsync(key);
        }

        public async Task<Dictionary<string, string>> ReadAsync(IEnumerable<string> keys)
        {
            var redisKeys = keys.Select(k => (RedisKey)k).ToArray();
            var values = await _db.StringGetAsync(redisKeys);
            var result = new Dictionary<string, string>();
            for (int i = 0; i < redisKeys.Length; i++)
            {
                if (values[i].HasValue)
                    result[(string)redisKeys[i]] = values[i];
            }
            return result;
        }

        // Generic ReadKeyAsync<T>
        public async Task<T> ReadAsync<T>(string key)
        {
            var value = await _db.StringGetAsync(key);
            if (!value.HasValue) return default;
            return JsonSerializer.Deserialize<T>(value);
        }
        public async Task<List<T>> ReadListAsync<T>(string key)
        {
            var value = await _db.StringGetAsync(key);
            if (!value.HasValue) return Enumerable.Empty<T>().ToList();
            return JsonSerializer.Deserialize<List<T>>(value);
        }
        // Generic ReadKeysAsync<T>
        public async Task<Dictionary<string, T>> ReadAsync<T>(IEnumerable<string> keys)
        {
            var redisKeys = keys.Select(k => (RedisKey)k).ToArray();
            var values = await _db.StringGetAsync(redisKeys);
            var result = new Dictionary<string, T>();
            for (int i = 0; i < redisKeys.Length; i++)
            {
                if (values[i].HasValue)
                {
                    var obj = JsonSerializer.Deserialize<T>(values[i]);
                    if (obj != null)
                        result[(string)redisKeys[i]] = obj;
                }
            }
            return result;
        }

        // 5. Read Keys using regex
        public async Task<Dictionary<string, string>> ReadByRegexAsync(string pattern)
        {
            var endpoints = _db.Multiplexer.GetEndPoints();
            var server = _db.Multiplexer.GetServer(endpoints.First());
            var allKeys = server.Keys(_db.Database).Select(k => (string)k);
            var regex = new Regex(pattern);
            var matchedKeys = allKeys.Where(k => regex.IsMatch(k)).ToArray();
            var result = new Dictionary<string, string>();
            foreach (var key in matchedKeys)
            {
                var value = await _db.StringGetAsync(key);
                if (value.HasValue)
                    result[key] = value;
            }
            return result;
        }

        // Generic ReadKeysByRegexAsync<T>
        public async Task<Dictionary<string, T>> ReadByRegexAsync<T>(string pattern)
        {
            var endpoints = _db.Multiplexer.GetEndPoints();
            var server = _db.Multiplexer.GetServer(endpoints.First());
            var allKeys = server.Keys(_db.Database).Select(k => (string)k);
            var regex = new Regex(pattern);
            var matchedKeys = allKeys.Where(k => regex.IsMatch(k)).ToArray();
            var result = new Dictionary<string, T>();
            foreach (var key in matchedKeys)
            {
                var value = await _db.StringGetAsync(key);
                if (value.HasValue)
                {
                    var obj = JsonSerializer.Deserialize<T>(value);
                    if (obj != null)
                        result[key] = obj;
                }
            }
            return result;
        }
    }
} 