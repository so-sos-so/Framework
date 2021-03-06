using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace Framework
{
    /// <summary>
    /// 职责类似Unity的Application
    /// </summary>
    public static class FApplication
    {
        #region 路径相关

        static  FApplication()
        {
            Init();
        }

        private static void Init()
        {
            ProjectRoot = Application.dataPath.Replace("/Assets", "");
            AssetsRoot = Application.dataPath;
            Library = ProjectRoot + "/Library";
        }
        /// <summary>
        /// 项目根目录
        /// Assets的上层目录
        /// </summary>
        public static string ProjectRoot { get; private set; }
        public static string AssetsRoot { get; private set; }
        /// <summary>
        /// Library
        /// </summary>
        public static string Library { get; private set; }

        /// <summary>
        /// 平台资源的父路径
        /// </summary>
        public static string GetPlatformPath(RuntimePlatform platform)
        {
            switch (platform)
            {
                case RuntimePlatform.OSXEditor:
                case RuntimePlatform.OSXPlayer:
                    return "OSX";
                case RuntimePlatform.WindowsEditor:
                case RuntimePlatform.WindowsPlayer:
                    return "Windows";
                case RuntimePlatform.Android:
                    return "Android";
                case RuntimePlatform.IPhonePlayer:
                    return "iOS";
            }

            return "";
        }
        #endregion

        /// <summary>
        /// UnityWebRequest加载
        /// StreamingAssets 下， mac要加file:///  安卓不能加
        /// persistent下 都要加file:///
        ///
        /// File加载
        /// StreamingAssets是打进压缩包，不能使用
        /// persistent按正常文件加载
        /// </summary>
        public static string PathPrefix = "file:///";
        
        public static string streamingAssetsPath
        {
            get
            {
                if (Application.platform == RuntimePlatform.Android ||
                    Application.platform == RuntimePlatform.IPhonePlayer)
                {
                    return Application.streamingAssetsPath;
                }
                if (Application.platform == RuntimePlatform.WindowsEditor ||
                    Application.platform == RuntimePlatform.OSXEditor)
                {
                    return PathPrefix + Application.streamingAssetsPath;
                }
                return Application.streamingAssetsPath;
            }
        }
        
        public static string persistentDataPath
        {
            get
            {
                if (Application.platform == RuntimePlatform.Android ||
                    Application.platform == RuntimePlatform.IPhonePlayer ||
                    Application.platform == RuntimePlatform.WindowsEditor ||
                    Application.platform == RuntimePlatform.OSXEditor)
                {
                    return PathPrefix + Application.persistentDataPath;
                }
                return Application.persistentDataPath;
            }
        }

        public static bool IsMobile => Application.platform == RuntimePlatform.Android ||
                                       Application.platform == RuntimePlatform.IPhonePlayer;

        public static bool IsEditor => Application.platform == RuntimePlatform.WindowsEditor ||
                                       Application.platform == RuntimePlatform.LinuxEditor ||
                                       Application.platform == RuntimePlatform.OSXEditor;
    }
}