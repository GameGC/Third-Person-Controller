using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Mime;
using System.Security.Cryptography;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

internal static class CustomWebClient
{
    const string TestCookieSign = "aes.js";

    public static async Task<string> Get(string url)
    {
        var address = new Uri(url);
        var cookieContainer = new CookieContainer();
        using (var handler = new HttpClientHandler() { CookieContainer = cookieContainer })
        using (var client = new HttpClient(handler))
        {
            var content = (await client.GetAsync(address)).Content;
            var script = await content.ReadAsStringAsync();

            if (!script.Contains(TestCookieSign))
            {
                return await content.ReadAsStringAsync();
            }

            var test = Decrypt(script);
            cookieContainer.Add(new Cookie("__test", test) { Domain = address.Host });

            var wait = await client.GetAsync(address);
            content = wait.Content;
               
            if ((await content.ReadAsStringAsync()).Contains(TestCookieSign))
            {
                throw new Exception();
            }

            Debug.Log((await content.ReadAsStringAsync()));
            return await content.ReadAsStringAsync();
        }
    }

    public static byte[] Post(string url, byte[] data)
    {
        var address = new Uri(url);
        var cookieContainer = new CookieContainer();
        using (var handler = new HttpClientHandler() { CookieContainer = cookieContainer })
        using (var client = new HttpClient(handler))
        using (var post = new ByteArrayContent(data))
        {
            var content = client.PostAsync(address, post).WaitResult().Content;
            var script = content.ReadAsStringAsync().WaitResult();

            if (!script.Contains(TestCookieSign))
            {
                return content.ReadAsByteArrayAsync().WaitResult();
            }

            var test = Decrypt(script);
            cookieContainer.Add(new Cookie("__test", test) { Domain = address.Host });

            content = client.PostAsync(address, post).WaitResult().Content;
            if (content.ReadAsStringAsync().WaitResult().Contains(TestCookieSign))
            {
                throw new Exception();
            }

            return content.ReadAsByteArrayAsync().WaitResult();
        }
    }

    static string Decrypt(string script)
    {
        var split = script.Split(new[] { "toNumbers(\"", "\")" }, StringSplitOptions.RemoveEmptyEntries)
            .Where(s => s.Length == 32)
            .ToArray();

        if (split.Length != 3)
            throw new Exception();

        var key = StringToByteArray(split[0]);
        var iv = StringToByteArray(split[1]);
        var bytesIn = StringToByteArray(split[2]);

        var aes = Aes.Create();
        aes.Padding = PaddingMode.None;
        aes.Mode = CipherMode.CBC;
        aes.BlockSize = 128;
        aes.KeySize = 128;
        aes.Key = key;
        aes.IV = iv;

        var decrypter = aes.CreateDecryptor();
        var decrypted = decrypter.TransformFinalBlock(bytesIn, 0, bytesIn.Length);

        decrypter.Dispose();
        aes.Dispose();

        return BitConverter.ToString(decrypted).Replace("-", "").ToLower();
    }

    static byte[] StringToByteArray(string hex) // Taken from https://stackoverflow.com/a/321404/9248173
    {
        return Enumerable.Range(0, hex.Length)
            .Where(x => x % 2 == 0)
            .Select(x => Convert.ToByte(hex.Substring(x, 2), 16))
            .ToArray();
    }
    
}

public class HttpWebRequestDownload
{
    private long _totalBytesLength = 0;
    private long _transferredBytes = 0;
    private int _transferredPercents => (int) ((100 * _transferredBytes) / _totalBytesLength);
    private string _defaultDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
    public string downloadedFilePath = String.Empty;

    public HttpWebRequestDownload()
    {
        //Windows 7 fix
        ServicePointManager.SecurityProtocol =
            SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;
    }

    public void DownloadFile(string url, string destinationDirectory = default)
    {
        string filename = "";
        if (destinationDirectory == default)
            destinationDirectory = _defaultDirectory;

        try
        {
            HttpWebRequest request = (HttpWebRequest) WebRequest.Create(url);
            request.Headers.Add("Cache-Control", "no-cache");
            request.Headers.Add("Cache-Control", "no-store");
            request.Headers.Add("Cache-Control", "max-age=1");
            request.Headers.Add("Cache-Control", "s-maxage=1");
            request.Headers.Add("Pragma", "no-cache");
            request.Headers.Add("Expires", "-1");

            if (!Directory.Exists(destinationDirectory))
            {
                Directory.CreateDirectory(destinationDirectory);
            }

            using (HttpWebResponse response = (HttpWebResponse) request.GetResponseAsync().Result)
            {
                _totalBytesLength = response.ContentLength;

                string path = response.Headers["Content-Disposition"];
                if (string.IsNullOrWhiteSpace(path))
                {
                    var uri = new Uri(url);
                    filename = Path.GetFileName(uri.LocalPath);
                }
                else
                {
                    ContentDisposition contentDisposition = new ContentDisposition(path);
                    filename = contentDisposition.FileName;
                }

                if(File.Exists(System.IO.Path.Combine(destinationDirectory, filename)))
                    File.Delete(System.IO.Path.Combine(destinationDirectory, filename));
                using (Stream responseStream = response.GetResponseStream())
                using (FileStream fileStream = File.Create(System.IO.Path.Combine(destinationDirectory, filename)))
                {
                    byte[] buffer = new byte[1024 * 1024]; // 1MB buffer
                    ProgressEventArgs eventArgs = new ProgressEventArgs(_totalBytesLength);

                    int size = responseStream.Read(buffer, 0, buffer.Length);
                    while (size > 0)
                    {
                        fileStream.Write(buffer, 0, size);
                        _transferredBytes += size;

                        size = responseStream.Read(buffer, 0, buffer.Length);

                        eventArgs.UpdateData(_transferredBytes, _transferredPercents);
                        OnDownloadProgressChanged(eventArgs);
                    }
                }
            }

            downloadedFilePath = Path.Combine(destinationDirectory, filename);
            OnDownloadFileCompleted(EventArgs.Empty);
        }
        catch (Exception e)
        {
            OnError($"{e.Message} => {e?.InnerException?.Message}");
        }
    }

    //events
    public event EventHandler<ProgressEventArgs> DownloadProgressChanged;
    public event EventHandler DownloadFileCompleted;
    public event EventHandler<string> Error;

    public class ProgressEventArgs : EventArgs
    {
        public long TransferredBytes { get; set; }
        public int TransferredPercents { get; set; }
        public long TotalBytesLength { get; set; }

        public ProgressEventArgs(long transferredBytes, int transferredPercents, long totalBytesLength)
        {
            TransferredBytes = transferredBytes;
            TransferredPercents = transferredPercents;
            TotalBytesLength = totalBytesLength;
        }

        public ProgressEventArgs(long totalBytesLength)
        {
            this.TotalBytesLength = totalBytesLength;
        }

        public void UpdateData(long transferredBytes, int transferredPercents)
        {
            TransferredBytes = transferredBytes;
            TransferredPercents = transferredPercents;
        }
    }

    protected virtual void OnDownloadProgressChanged(ProgressEventArgs e)
    {
        DownloadProgressChanged?.Invoke(this, e);
    }

    protected virtual void OnDownloadFileCompleted(EventArgs e)
    {
        DownloadFileCompleted?.Invoke(this, e);
    }

    protected virtual void OnError(string errorMessage)
    {
        Error?.Invoke(this, errorMessage);
    }
}


static class ExtensionMethods
{
    public static T WaitResult<T>(this Task<T> task)
    {
        task.Wait();
        return task.Result;
    }
}