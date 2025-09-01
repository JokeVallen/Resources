namespace GameAssistant.Core.IO
{
    /// <summary>
    /// 文件路径工具类
    /// </summary>
    internal static class FilePathTool
    {
        private static readonly IFilePathRule s_Rule;

        static FilePathTool()
        {
#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
            s_Rule = new WindowsFilePathRule();
#endif
        }

        /// <summary>
        /// 指定的字符串所指示路径是否为文件路径
        /// </summary>
        /// <param name="path">路径字符串</param>
        public static bool IsFilePath(string path)
        {
            if (string.IsNullOrEmpty(path)) return false;
            return s_Rule.IsFilePath(path);
        }
    }
}