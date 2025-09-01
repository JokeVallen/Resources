namespace GameAssistant.Core.IO
{
    /// <summary>
    /// 文件夹路径工具类
    /// </summary>
    internal static class FolderPathTool
    {
        private static readonly IFolderPathRule s_Rule;

        static FolderPathTool()
        {
#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
            s_Rule = new WindowsFolderPathRule();
#endif
        }

        /// <summary>
        /// 指定的字符串所指示路径是否为文件夹路径
        /// </summary>
        /// <param name="path">路径字符串</param>
        public static bool IsFolderPath(string path)
        {
            if (string.IsNullOrEmpty(path)) return false;
            return s_Rule.IsFolderPath(path);
        }
    }
}