namespace GameAssistant.Core.IO
{
    /// <summary>
    /// 路径工具类
    /// </summary>
    public static class PathTool
    {
        /// <summary>
        /// 传递的字符串所指示路径是否为文件夹路径
        /// <para>path：指示为路径的字符串</para>
        /// </summary>
        public static bool IsFolderPath(string path)
        {
            return FolderPathTool.IsFolderPath(path);
        }

        /// <summary>
        /// 传递的字符串所指示路径是否为文件路径
        /// <para>path：指示为路径的字符串</para>
        /// </summary>
        public static bool IsFilePath(string path)
        {
            return FilePathTool.IsFilePath(path);
        }
    }
}