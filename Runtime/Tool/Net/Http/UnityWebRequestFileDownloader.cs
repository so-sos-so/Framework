﻿/*
/*
 * MIT License
 *
 * Copyright (c) 2018 Clark Yang
 *
 * Permission is hereby granted, free of charge, to any person obtaining a copy of 
 * this software and associated documentation files (the "Software"), to deal in 
 * the Software without restriction, including without limitation the rights to 
 * use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies 
 * of the Software, and to permit persons to whom the Software is furnished to do so, 
 * subject to the following conditions:
 *
 * The above copyright notice and this permission notice shall be included in all 
 * copies or substantial portions of the Software.
 *
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR 
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE 
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER 
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, 
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE 
 * SOFTWARE.
 #1#
*/
using System;
using System.IO;
using System.Collections;
using UnityEngine.Networking;
using System.Collections.Generic;
using Framework.Asynchronous;
using UnityEngine;

namespace Framework.Net
{
    public class UnityWebRequestFileDownloader : FileDownloaderBase
    {
        public override IProgressResult<ProgressInfo, FileInfo> DownloadFileAsync(string path, FileInfo fileInfo)
        {
            return Execution.Executors.RunOnCoroutine<ProgressInfo, FileInfo>((promise) =>
                DoDownloadFileAsync(path, fileInfo, promise));
        }

        protected virtual IEnumerator DoDownloadFileAsync(string path, FileInfo fileInfo,
            IProgressPromise<ProgressInfo> promise)
        {
            if (fileInfo.Directory != null && !fileInfo.Directory.Exists)
                fileInfo.Directory.Create();
            ProgressInfo progressInfo = new ProgressInfo {TotalCount = 1};
            using (UnityWebRequest www = new UnityWebRequest(path))
            {
                www.downloadHandler = new DownloadFileHandler(fileInfo);
                www.SendWebRequest();
                while (!www.isDone)
                {
                    if (www.downloadProgress >= 0)
                    {
                        if (progressInfo.TotalSize <= 0)
                            progressInfo.TotalSize = (long) (www.downloadedBytes / www.downloadProgress);
                        progressInfo.CompletedSize = (long) www.downloadedBytes;
                        promise.UpdateProgress(progressInfo);
                    }

                    yield return null;
                }

                if(www.isNetworkError || www.isHttpError)
                {
                    promise.SetException(www.error);
                    yield break;
                }
                
                progressInfo.CompletedCount = 1;
                progressInfo.CompletedSize = progressInfo.TotalSize;
                promise.UpdateProgress(progressInfo);
                promise.SetResult(fileInfo);
            }
        }

        public override IProgressResult<ProgressInfo, ResourceInfo[]> DownloadFileAsync(ResourceInfo[] infos)
        {
            return Execution.Executors.RunOnCoroutine<ProgressInfo, ResourceInfo[]>((promise) =>
                DoDownloadFileAsync(infos, promise));
        }

        protected virtual IEnumerator DoDownloadFileAsync(ResourceInfo[] infos, IProgressPromise<ProgressInfo> promise)
        {
            long totalSize = 0;
            long downloadedSize = 0;
            List<ResourceInfo> downloadList = new List<ResourceInfo>();
            foreach (var info in infos)
            {
                var fileInfo = info.FileInfo;

                if (info.FileSize < 0)
                {
                    if (fileInfo.Exists)
                    {
                        info.FileSize = fileInfo.Length;
                    }
                    else
                    {
                        using (UnityWebRequest www = UnityWebRequest.Head(info.Path))
                        {
                            yield return www.SendWebRequest();
                            string contentLength = www.GetResponseHeader("Content-Length");
                            if (!string.IsNullOrEmpty(contentLength))
                                info.FileSize = long.Parse(contentLength);
                        }
                    }
                }

                totalSize += info.FileSize;
                if (fileInfo.Exists)
                    downloadedSize += info.FileSize;
                else
                    downloadList.Add(info);
            }

            ProgressInfo progressInfo = new ProgressInfo
            {
                TotalCount = infos.Length,
                CompletedCount = infos.Length - downloadList.Count,
                TotalSize = totalSize,
                CompletedSize = downloadedSize
            };

            yield return null;

            List<KeyValuePair<ResourceInfo, UnityWebRequest>> tasks =
                new List<KeyValuePair<ResourceInfo, UnityWebRequest>>();
            for (int i = 0; i < downloadList.Count; i++)
            {
                ResourceInfo info = downloadList[i];
                var path = info.Path;
                FileInfo fileInfo = info.FileInfo;
                if (fileInfo.Directory != null && !fileInfo.Directory.Exists)
                    fileInfo.Directory.Create();

                UnityWebRequest www = new UnityWebRequest(path);
                www.downloadHandler = new DownloadFileHandler(fileInfo);

                www.SendWebRequest();
                tasks.Add(new KeyValuePair<ResourceInfo, UnityWebRequest>(info, www));

                while (tasks.Count >= this.MaxTaskCount || (i == downloadList.Count - 1 && tasks.Count > 0))
                {
                    long tmpSize = 0;
                    for (int j = tasks.Count - 1; j >= 0; j--)
                    {
                        var task = tasks[j];
                        ResourceInfo _info = task.Key;
                        UnityWebRequest _www = task.Value;

                        if (!_www.isDone)
                        {
                            tmpSize += (long) Math.Max(0,
                                _www.downloadedBytes); //the UnityWebRequest.downloadedProgress has a bug in android platform
                            continue;
                        }

                        progressInfo.CompletedCount += 1;
                        tasks.RemoveAt(j);
                        downloadedSize += _info.FileSize;
                        if (_www.isNetworkError)
                        {
                            promise.SetException(new Exception(_www.error));
                            Log.Error(
                                $"Downloads file '{_info.FileInfo.FullName}' failure from the address '{(_info.Path)}'.Reason:{_www.error}");
                            _www.Dispose();

                            try
                            {
                                foreach (var kv in tasks)
                                {
                                    kv.Value.Dispose();
                                }
                            }
                            catch (Exception)
                            {
                            }

                            yield break;
                        }

                        _www.Dispose();
                    }

                    progressInfo.CompletedSize = downloadedSize + tmpSize;
                    promise.UpdateProgress(progressInfo);
                    yield return null;
                }
            }

            promise.SetResult(infos);
        }
    }
}
