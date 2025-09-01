namespace GameAssistant.Core.IO
{
    public interface IFilePathRule
    {
        /// <summary>
        /// 指定的字符串所指示路径是否为文件路径
        /// </summary>
        /// <param name="path">路径字符串</param>
        public bool IsFilePath(string path);
    }
}