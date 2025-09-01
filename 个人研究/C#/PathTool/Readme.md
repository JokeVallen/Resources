# PathTool 工具库

## 功能简介

PathTool 用于对文件或文件夹路径字符串进行校验，判断其是否符合路径规范。

## 主要功能

- **Windows**

- 校验字符串是否为合法的文件或文件夹路径
- 兼容 Win32 长路径（`\\?\` 前缀）
- 支持常见路径分隔符（`\` 和 `/`）
- 判断路径类型（绝对路径、相对路径、驱动器路径等）

## 支持平台

- **Windows**

> ⚠️ 目前主要针对 Windows 路径格式设计，部分功能对 Linux/macOS 等其他平台支持有限。如需跨平台支持，请根据实际需求扩展相关处理逻辑。

## 文件说明

- `PathTool.cs`         路径处理主工具类
- `FilePathTool.cs`     文件路径专用工具
- `FolderPathTool.cs`   文件夹路径专用工具

## 使用示例

```csharp
using GameAssistant.Core.IO;

string path = @"C:\Users\Example\file.txt";
bool isFolderPath = PathTool.IsFolderPath(path);
bool isFilePath = PathTool.IsFilePath(path);
```

## 可扩展

可以基于IFilePathRule或IFolderPathRule接口扩展跨平台的路径检测规则，具体可参考WindowsFilePathRule和WindowsFolderPathRule。

## 许可证

本工具库仅供学习与个人项目使用，禁止用于商业用途。
