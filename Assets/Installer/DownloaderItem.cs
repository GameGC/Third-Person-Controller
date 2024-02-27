using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json.Linq;
using UnityEngine;

internal class DownloaderItem
{
    public readonly string PackageName;
    public readonly string Version;

    public int Progress
    {
        get => _progress;
        private set
        {
            _progress = value;
            ChangedSinceRepaint = true;
        }
    }
    public static bool ChangedSinceRepaint { get;  set; }

    
    public HttpWebRequestDownload DownloadClient => _downloadClient;

    public string DownloadPath =>
        $"{Application.dataPath.Replace("/Assets", "")}/Packages/Local/GameGC/{PackageName}f/{PackageName}-{Version}.tgz";

    
    private int _progress;
    private readonly HttpWebRequestDownload _downloadClient;

    public DownloaderItem(string packageName , string version)
    {
        this.Version = version;
        this.PackageName = packageName;
        if (IsDownloaded())
        {
            _progress = 100;
        }
        else
        {
            _downloadClient = new HttpWebRequestDownload();
            DownloadClient.DownloadProgressChanged += ProgressChanged;
            DownloadClient.DownloadFileCompleted += DownloadCompleted;
        }
    }

    private bool IsDownloaded() => File.Exists(DownloadPath);
    

    public bool IsInstalled(IDictionary<string, string> packageList)
    {
        if (!packageList.TryGetValue(PackageName, out var packageVersion)) return false;

        if (packageVersion.StartsWith("file:"))
        {
            int cIndex = packageVersion.LastIndexOf('-');
            packageVersion = packageVersion.Substring(cIndex + 1, packageVersion.LastIndexOf('.') - cIndex - 1);
        }

        return packageVersion == Version;
    }
    
    public bool IsInstalled()
    {
        if (!File.Exists("Packages/manifest.json"))
            return false;

        string jsonText = File.ReadAllText("Packages/manifest.json");
        var json = JObject.Parse(jsonText);
        var values = json["dependencies"].ToObject<Dictionary<string, string>>();
        
        if (!values.TryGetValue(PackageName, out var packageVersion)) return false;

        if (packageVersion.StartsWith("file:"))
        {
            int cIndex = packageVersion.LastIndexOf('-');
            packageVersion = packageVersion.Substring(cIndex + 1, packageVersion.LastIndexOf('.') - cIndex - 1);
        }

        return packageVersion == Version;
    }

    //public bool IsInstalled(ListRequest request)
    //{
    //    var package = request.Result.
    //        FirstOrDefault(packageInfo => packageInfo.packageId == PackageName);
    //    
    //    return (package !=null && package.version == version);
    //}
    //public async Task<bool> IsInstalled()
    //{
    //    var list = Client.List();
    //    while (!list.IsCompleted)
    //        await Task.Yield();
    //    
    //    var package = list.Result.
    //        FirstOrDefault(packageInfo => packageInfo.packageId == PackageName);
    //    
    //    return (package !=null && package.version == version);
    //}

    private void DownloadCompleted(object sender, EventArgs e)
    {
        Progress = 100;
    }

    private void ProgressChanged(object sender, HttpWebRequestDownload.ProgressEventArgs e)
    {
        Progress = e.TransferredPercents;
    }

    ~DownloaderItem()
    {
        DownloadClient.DownloadProgressChanged -= ProgressChanged;
        DownloadClient.DownloadFileCompleted -= DownloadCompleted;
    }
}