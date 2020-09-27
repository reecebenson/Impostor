﻿using System;
using System.Net;
using Agones;
using Microsoft.Extensions.Caching.Distributed;

namespace Impostor.Server.Net.Redirector
{
    public class NodeLocatorRedis : INodeLocator
    {
        private readonly IDistributedCache _cache;
        private readonly AgonesSDK _agones;
        
        public NodeLocatorRedis(IDistributedCache cache, AgonesSDK agones)
        {
            _cache = cache;
            _agones = agones;
        }

        public IPEndPoint Find(string gameCode)
        {
            var entry = _cache.GetString(gameCode);
            if (entry == null)
            {
                return null;
            }
            
            return IPEndPoint.Parse(entry);
        }

        public void Save(string gameCode, IPEndPoint endPoint)
        {
            _cache.SetString(gameCode, endPoint.ToString(), new DistributedCacheEntryOptions
            {
                SlidingExpiration = TimeSpan.FromHours(1)
            });
        }

        public void Remove(string gameCode)
        {
            _cache.Remove(gameCode);
            var shutdown = _agones.ReadyAsync().Result;
            Console.WriteLine("Shutdown Result " + shutdown);
        }
    }
}