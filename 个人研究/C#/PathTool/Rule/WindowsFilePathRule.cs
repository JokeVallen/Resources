using System;
using System.Collections.Generic;
using System.Linq;

namespace GameAssistant.Core.IO
{
    internal class WindowsFilePathRule : IFilePathRule
    {
        private static Dictionary<string, int>[] s_FileStateTable; // 文件状态表(反向)
        private static Dictionary<string, int>[] s_AltFileStateTable; // 文件状态表(正向)
        private const int END_STATE_UNIT_INDEX = 3; // 结束状态单元对应文件状态表(反向分隔符)中的索引
        private const int END_STATE_ALT_UNIT_INDEX = 2; // 结束状态单元对应文件状态表(正向分隔符)中的索引

        static WindowsFilePathRule()
        {
            // 初始化文件状态表(反向分隔符)
            // 1.驱动器名:\ 2..\ 3...\ 4.\?\ 5.有效文件夹名称\ 6.空字符串("")--结束状态
            s_FileStateTable = new Dictionary<string, int>[]
            {
                new Dictionary<string, int>{{@"driveName:\", 2},{@".\", 2},{@"..\", 2},{@"\?\", 1}},
                new Dictionary<string, int>{{@"driveName:\", 2}},
                new Dictionary<string, int>{{@"folderName\", 2}},
                new Dictionary<string, int>{{string.Empty, 3}}
            };

            // 初始化文件状态表(正向分隔符)
            // 1.驱动器名:/ 2.有效文件夹名称/ 3.空字符串("")--结束状态
            s_AltFileStateTable = new Dictionary<string, int>[]
            {
                new Dictionary<string, int>{{"driveName:/", 1}},
                new Dictionary<string, int>{{"folderName/", 1}},
                new Dictionary<string, int>{{string.Empty, 2}}
            };

            /*
                fileStateTable和altFileStateTable都是状态表，而状态表中的每个元素称为状态单元，每个状态单元中
                包括一到多个状态键值对，状态键值对的Key记录状态，而Value则记录下一个状态单元索引。
            */
        }

        public bool IsFilePath(string path)
        {
            if (string.IsNullOrEmpty(path)) return false;
            return DoIsAltFilePath(path) || DoIsFilePath(path);
        }

        // 执行逻辑：是否为文件路径(反向分隔符)
        private static bool DoIsFilePath(string path)
        {
            string v_path = path;
            int v_stateUnitIndex = 0; // 状态单元索引，默认为0(对应状态表初始状态单元的索引)
            string v_state = v_path.StartsWith("\\?\\") ? @"\?\" : string.Empty;

            // 以反向路径分隔符作为字符串分隔符，分隔待检测的字符串
            string[] v_pathStrs = v_path.Split('\\');

            // 根据分隔后的字符串数组的首元素确立初始状态
            if (string.IsNullOrEmpty(v_state) && v_pathStrs.Length > 1)
            {
                if (Array.Exists(v_pathStrs, s => string.IsNullOrEmpty(s))) return false;

                if (v_pathStrs[0].Equals("."))
                    v_state = @".\";
                else if (v_pathStrs[0].Equals(".."))
                    v_state = @"..\";
                else if (WindowsPathSharedData.IsDrive(v_pathStrs[0]))
                    v_state = @"driveName:\";
            }

            // 初始状态为Null或空字符串代表不合法
            if (string.IsNullOrEmpty(v_state)) return false;

            // 根据当前状态单元索引和初始状态读取下一个状态单元索引
            v_stateUnitIndex = s_FileStateTable[v_stateUnitIndex][v_state];

            int v_start = v_state == "\\?\\" ? 2 : 1;
            // 从第2个元素开始至倒数第2个元素解析分隔后的字符串数组(有限自动机)
            for (int i = v_start; i < v_pathStrs.Length - 1; i++)
            {
                if (WindowsPathSharedData.IsDrive(v_pathStrs[i]))
                    v_state = @"driveName:\";
                else if (v_pathStrs[i].All(c => WindowsPathSharedData.SharedNameChars.Contains(c)
                || WindowsPathSharedData.FileOrFolderSpecialNameChars.Contains(c)))
                    v_state = @"folderName\";
                else return false;

                if (!s_FileStateTable[v_stateUnitIndex].TryGetValue(v_state, out v_stateUnitIndex))
                    return false;
            }

            // 分隔后的字符串数组的最后一个元素可能是带有扩展的文件名，所以单独检测其规范性
            if (DoFileWithExtensionCheck(v_pathStrs[v_pathStrs.Length - 1]))
                v_stateUnitIndex = END_STATE_UNIT_INDEX;

            // 若最终状态单元索引为endStateUnitIndex则表示正常退出，就判定为属于文件路径
            if (v_stateUnitIndex == END_STATE_UNIT_INDEX) return true;
            return false;
        }

        // 执行逻辑：是否为文件路径(正向分隔符)，逻辑与上述反向分隔符判断大同小异
        private static bool DoIsAltFilePath(string path)
        {
            string v_path = path;
            int v_stateUnitIndex = 0;
            string v_state = string.Empty;

            string[] v_pathStrs = v_path.Split('/');
            if (Array.Exists(v_pathStrs, s => string.IsNullOrEmpty(s))) return false;

            if (v_pathStrs.Length > 0)
            {
                if (Array.Exists(v_pathStrs, s => string.IsNullOrEmpty(s))) return false;

                if (WindowsPathSharedData.IsDrive(v_pathStrs[0]))
                    v_state = "driveName:/";
            }

            if (string.IsNullOrEmpty(v_state)) return false;

            v_stateUnitIndex = s_AltFileStateTable[v_stateUnitIndex][v_state];

            for (int i = 1; i < v_pathStrs.Length - 1; i++)
            {
                if (v_pathStrs[i].All(c => WindowsPathSharedData.SharedNameChars.Contains(c)
                || WindowsPathSharedData.FileOrFolderSpecialNameChars.Contains(c)))
                    v_state = "folderName/";
                else return false;

                if (!s_AltFileStateTable[v_stateUnitIndex].TryGetValue(v_state, out v_stateUnitIndex))
                    return false;
            }

            if (DoFileWithExtensionCheck(v_pathStrs[v_pathStrs.Length - 1]))
                v_stateUnitIndex = END_STATE_ALT_UNIT_INDEX;

            if (v_stateUnitIndex == END_STATE_ALT_UNIT_INDEX) return true;
            return false;
        }

        // 执行逻辑：检测指示带有扩展的文件名的字符串是否符合规范
        private static bool DoFileWithExtensionCheck(string str)
        {
            // Null或空字符串检测
            if (string.IsNullOrEmpty(str)) return false;

            // 找到最后一个'.'的索引
            int v_lastDotIndex = Array.FindLastIndex(str.ToCharArray(), c => c == '.');

            // 未找到'.'
            if (v_lastDotIndex == -1) return false;

            // 获取文件名
            string v_fileName = str.Substring(0, v_lastDotIndex);

            // 文件名规范性检测
            if (v_fileName.All(c => WindowsPathSharedData.SharedNameChars.Contains(c)
            || WindowsPathSharedData.FileOrFolderSpecialNameChars.Contains(c)))
            {
                // 获取文件扩展名
                string v_extensionName = str.Substring(v_lastDotIndex + 1, str.Length - v_lastDotIndex - 1);

                // 文件扩展名规范性检测
                if (v_extensionName.All(c => WindowsPathSharedData.SharedNameChars.Contains(c)
                || WindowsPathSharedData.FileExtensionSpecialNameChars.Contains(c)))
                    return true;
            }

            return false;
        }
    }
}