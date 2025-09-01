using System;
using System.Collections.Generic;
using System.Linq;

namespace GameAssistant.Core.IO
{
    internal class WindowsFolderPathRule : IFolderPathRule
    {
        private static Dictionary<string, int>[] s_FolderStateTable; // 文件夹状态表(反向)
        private static Dictionary<string, int>[] s_AltFolderStateTable; // 文件夹状态表(正向)
        private const int END_STATE_UNIT_INDEX = 3; // 结束状态单元对应文件夹状态表(反向分隔符)中的索引
        private const int END_STATE_ALT_UNIT_INDEX = 3; // 结束状态单元对应文件夹状态表(正向分隔符)中的索引

        static WindowsFolderPathRule()
        {
            // 初始化文件夹状态表(反向分隔符)
            // 1.驱动器名:\ 2..\ 3...\ 4.有效文件夹名称\ 5.有效文件夹名称 6.空字符串("")--结束状态
            s_FolderStateTable = new Dictionary<string, int>[]
            {
                new Dictionary<string,int>{{@"driveName:\", 1},{@".\", 1},{@"..\", 1},{@"\?\", 2}},
                new Dictionary<string,int>{{@"folderName\", 1},{@"folderName", 3}},
                new Dictionary<string,int>{{@"driveName:\", 1}},
                new Dictionary<string,int>{{string.Empty, 3}}
            };

            // 初始化文件夹状态表(正向分隔符)
            // 1.驱动器名:/ 2.有效文件夹名称/ 3.有效文件夹名称 4.空字符串("")--结束状态
            s_AltFolderStateTable = new Dictionary<string, int>[]
            {
                new Dictionary<string, int>{{"driveName:/", 1}},
                new Dictionary<string, int>{{string.Empty, 3},{"folderName/", 2},{"folderName", 3}},
                new Dictionary<string, int>{{"folderName/", 2},{"folderName", 3}},
                new Dictionary<string, int>{{string.Empty, 3}}
            };

            /*
                folderStateTable和altFolderStateTable都是状态表，而状态表中的每个元素称为状态单元，每个状态单元中
                包括一到多个状态键值对，状态键值对的Key记录状态，而Value则记录下一个状态单元索引。
            */
        }

        public bool IsFolderPath(string path)
        {
            if (string.IsNullOrEmpty(path)) return false;
            return DoIsAltFolderPath(path) || DoIsFolderPath(path);
        }

        // 执行逻辑：是否为文件夹路径(反向分隔符)
        private static bool DoIsFolderPath(string path)
        {
            string v_path = path;
            int v_stateUnitIndex = 0; // 状态单元索引，默认为0(对应状态表初始状态单元的索引)
            string v_state = v_path.StartsWith("\\?\\") ? @"\?\" : string.Empty;

            // 以路径分隔符作为字符串分隔符，分隔待检测的字符串
            string[] v_pathStrs = v_path.Split('\\');

            // 根据分隔后的字符串数组的首元素确立起始状态
            if (string.IsNullOrEmpty(v_state) && v_pathStrs.Length > 1)
            {
                if (v_pathStrs[0].Equals("."))
                    v_state = @".\";
                else if (v_pathStrs[0].Equals(".."))
                    v_state = @"..\";
                else if (WindowsPathSharedData.IsDrive(v_pathStrs[0]))
                {
                    v_state = @"driveName:\";
                    if (v_pathStrs.Length == 2 && string.IsNullOrEmpty(v_pathStrs[1]))
                        return true;
                }

                if (Array.Exists(v_pathStrs, s => string.IsNullOrEmpty(s)))
                    return false;
            }

            // 起始状态为Null或空字符串代表不合法
            if (string.IsNullOrEmpty(v_state)) return false;

            // 根据当前状态单元索引和起始状态读取下一个状态单元索引
            v_stateUnitIndex = s_FolderStateTable[v_stateUnitIndex][v_state];

            int v_start = v_state == "\\?\\" ? 2 : 1;
            // 开始解析分隔后的字符串数组中的其它元素 (有限自动机)
            for (int i = v_start; i < v_pathStrs.Length; i++)
            {
                if (WindowsPathSharedData.IsDrive(v_pathStrs[i]))
                    v_state = @"driveName:\";
                else if (v_pathStrs[i].All(c => WindowsPathSharedData.SharedNameChars.Contains(c)
                || WindowsPathSharedData.FileOrFolderSpecialNameChars.Contains(c)))
                    v_state = i == v_pathStrs.Length - 1 ? @"folderName" : @"folderName\";
                else return false;

                if (!s_FolderStateTable[v_stateUnitIndex].TryGetValue(v_state, out v_stateUnitIndex))
                    return false;
            }

            // 若状态单元索引为最终状态单元索引则表示正常退出，就判定为属于文件夹路径
            if (v_stateUnitIndex == END_STATE_UNIT_INDEX) return true;
            return false;
        }

        // 执行逻辑：是否为文件夹路径(正向分隔符)，逻辑与上述反向分隔符判断大同小异
        private static bool DoIsAltFolderPath(string path)
        {
            string v_path = path;
            int v_stateUnitIndex = 0;
            string v_state = string.Empty;

            string[] v_pathStrs = v_path.Split('/');

            if (v_pathStrs.Length > 0)
            {
                if (WindowsPathSharedData.IsDrive(v_pathStrs[0]))
                {
                    v_state = "driveName:/";
                    if (v_pathStrs.Length == 2 && string.IsNullOrEmpty(v_pathStrs[1]))
                        return true;
                }

                if (Array.Exists(v_pathStrs, s => string.IsNullOrEmpty(s))) return false;
            }

            if (string.IsNullOrEmpty(v_state)) return false;
            v_stateUnitIndex = s_AltFolderStateTable[v_stateUnitIndex][v_state];

            for (int i = 1; i < v_pathStrs.Length; i++)
            {
                if (v_pathStrs[i].All(c => WindowsPathSharedData.SharedNameChars.Contains(c)
                || WindowsPathSharedData.FileOrFolderSpecialNameChars.Contains(c)))
                    v_state = i == v_pathStrs.Length - 1 ? @"folderName" : @"folderName/";
                else return false;

                if (!s_AltFolderStateTable[v_stateUnitIndex].TryGetValue(v_state, out v_stateUnitIndex))
                    return false;
            }

            if (v_stateUnitIndex == END_STATE_ALT_UNIT_INDEX) return true;
            return false;
        }
    }
}