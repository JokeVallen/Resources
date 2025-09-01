using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;

namespace GameAssistant.Core.IO
{
    /// <summary>
    /// 路径共享数据
    /// </summary>
    internal static class WindowsPathSharedData
    {
        /// <summary>
        /// 共享字符集，包括大小写字母和数字
        /// </summary>
        public static ReadOnlyCollection<char> SharedNameChars => Array.AsReadOnly(s_SharedNameChars);

        /// <summary>
        /// 文件或文件夹名称的特殊字符集
        /// </summary>
        public static ReadOnlyCollection<char> FileOrFolderSpecialNameChars
        => Array.AsReadOnly(s_FileOrFolderSpecialNameChars);

        /// <summary>
        /// 驱动器名称的特殊字符集
        /// </summary>
        public static ReadOnlyCollection<char> DriveSpecialNameChars
        => Array.AsReadOnly(s_DriveSpecialNameChars);

        /// <summary>
        /// 文件扩展名的特殊字符集
        /// </summary>
        public static ReadOnlyCollection<char> FileExtensionSpecialNameChars
        => Array.AsReadOnly(s_FileExtensionSpecialNameChars);

        /// <summary>
        /// 驱动器名称合集
        /// </summary>
        public static ReadOnlyCollection<string> DriveNames => Array.AsReadOnly(s_DriveNames);

        /// <summary>
        /// 驱动器标识符
        /// </summary>
        public const char DRIVE_CHAR = ':';

        /// <summary>
        /// Win32超长路径标识符
        /// </summary>
        public const string WIN32_LONG_PATH_HEAD = "\\?\\";

        private static readonly char[] s_SharedNameChars; // 共享字符集，包括大小写字母和数字
        private static readonly char[] s_FileOrFolderSpecialNameChars; // 文件或文件夹名称的特殊字符集
        private static readonly char[] s_DriveSpecialNameChars; // 驱动器名称的特殊字符集
        private static readonly char[] s_FileExtensionSpecialNameChars; // 文件扩展名的特殊字符集
        private static readonly string[] s_DriveNames; // 驱动器名称合集

        static WindowsPathSharedData()
        {
            // 初始化共享字符集
            s_SharedNameChars = new char[62];
            for (int i = 0; i < 62; i++)
            {
                if (i <= 25) s_SharedNameChars[i] = (char)('a' + i);
                else if (i > 25 && i <= 51) s_SharedNameChars[i] = (char)('A' + (i - 26));
                else s_SharedNameChars[i] = (char)('0' + (i - 52));
            }

            // 初始化文件或文件夹名称的特殊字符集 !#$%&'()-@^_`~{}+,.;=
            s_FileOrFolderSpecialNameChars = new char[22]
            {
                '\u0020', '\u0021', '\u0023', '\u0024', '\u0025', '\u0026', '\u0027',
                '\u0028','\u0029','\u002D','\u0040','\u005E','\u005F','\u0060','\u007E',
                '\u007B','\u007D','\u002B','\u002C','\u002E','\u003B','\u003D'
            };

            // 初始化驱动器名称的特殊字符集 -_
            s_DriveSpecialNameChars = new char[3] { '\u0020', '\u002D', '\u005F' };

            // 初始化文件扩展名的特殊字符集-_
            s_FileExtensionSpecialNameChars = new char[2] { '\u002D', '\u005F' };

            // 初始化驱动器名称合集
            List<string> v_driveNames = new List<string>();

            DriveInfo[] v_drives = DriveInfo.GetDrives();
            foreach (DriveInfo driveInfo in v_drives)
            {
                v_driveNames.Add(driveInfo.Name.Replace("\\", ""));
            }
            s_DriveNames = v_driveNames.ToArray();
        }

        public static bool Exists<T>(ReadOnlyCollection<T> collections, Func<T, bool> match)
        {
            if (collections == null || collections.Count == 0 || match == null) return false;

            for (int i = 0; i < collections.Count; i++)
            {
                if (match(collections[i])) return true;
            }
            return false;
        }

        public static bool IsDrive(string value)
        {
            if (string.IsNullOrEmpty(value)) return false;

            int v_len = value.Length;
            for (int i = 0; i < v_len; i++)
            {
                if (i < v_len - 1)
                {
                    if (!Array.Exists(s_SharedNameChars, v => v == value[i])
                    && !Array.Exists(s_DriveSpecialNameChars, v => v == value[i])) return false;
                }
                else
                {
                    if (value[i] != DRIVE_CHAR)
                        return false;
                }
            }
            return true;
        }
    }
}