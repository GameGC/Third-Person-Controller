using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace RedOwl.UIX.Engine
{
    /*
    public void Example()
    {
        using(new Timer("Name.Of.Timer.Maker"))
        {
            ... Something you want to time ...
        }
    }
    */
    public struct Timer : IDisposable
    {
        private static readonly Dictionary<string, Stopwatch> Watches = new Dictionary<string, Stopwatch>();
        
        private readonly string _name;

        public Timer(string name)
        {
            _name = name;
            if (Watches.TryGetValue(_name, out var w))
                w.Restart();
            else
            {
                var watch = new Stopwatch();
                Watches.Add(_name, watch);
                watch.Start();
            }
        }

        public void Dispose()
        {
            if (Watches.TryGetValue(_name, out var w))
            {
                w.Stop();
                UnityEngine.Debug.Log($"[{_name}] RunTime: {w.Elapsed}");
            }
        }
        
        public static void Slow(int seconds = 10)
        {
            var end = DateTime.Now + TimeSpan.FromSeconds(seconds);
            while (DateTime.Now < end) {}
        }
    }
}