using System;
using System.IO;
using UnityEngine;

internal class DownloaderItem
{
    public readonly string PackageName;
    public int Progress { get; private set; }
    public HttpWebRequestDownload DownloadClient { get; }

    public DownloaderItem(string packageName , string version)
    {
        this.PackageName = packageName;
        if (AlreadyDownloaded(packageName, version))
        {
            Progress = 100;
        }
        else
        {
            DownloadClient = new HttpWebRequestDownload();
            DownloadClient.DownloadProgressChanged += ProgressChanged;
            DownloadClient.DownloadFileCompleted += DownloadCompleted;
        }
    }

    private static bool AlreadyDownloaded(string packageName , string version)
    {
        var path =
            $"{Application.dataPath.Replace("/Assets", "")}/Packages/Local/GameGC/{packageName}f/{packageName}-{version}.tgz";
        return File.Exists(path);
    }

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