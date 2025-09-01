namespace GameAssistant.Core.IO
{
    public interface IFolderPathRule
    {
        /// <summary>
        /// 指定的字符串所指示路径是否为文件夹路径
        /// </summary>
        /// <param name="path">路径字符串</param>
        public bool IsFolderPath(string path);
    }
}