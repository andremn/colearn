using System;
using System.IO;

namespace FinalProject.Helpers
{
    public static class FileSystemHelper
    {
        public static string BuildPath(params string[] paths)
        {
            return Path.Combine(paths);
        }

        public static void CreateFolder(string path)
        {
            Directory.CreateDirectory(path);
        }

        public static FileStream CreateFileForWrite(string path)
        {
            return File.Create(path);
        }

        public static FileStream CreateOrOpenFileForWrite(string path)
        {
            return File.OpenWrite(path);
        }

        public static FileStream CreateOrOpenFileToAppend(string path)
        {
            return !FileExists(path)
                ? File.Create(path)
                : File.Open(path, FileMode.Append, FileAccess.Write);
        }

        public static StreamWriter CreateTextFile(string path, bool createNew)
        {
            if (createNew && FileExists(path))
            {
                DeleteFile(path);
            }

            return File.CreateText(path);
        }

        public static bool DeleteFile(string path)
        {
            try
            {
                File.Delete(path);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public static bool FileExists(string path)
        {
            return File.Exists(path);
        }

        public static bool FolderExists(string path)
        {
            return Directory.Exists(path);
        }

        public static string GetAbsolutePath(string relativePath)
        {
            if (relativePath.StartsWith("/"))
            {
                relativePath = relativePath.Remove(0, 1);
            }
            else if (relativePath.StartsWith("//"))
            {
                relativePath = relativePath.Remove(0, 2);
            }

            return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, relativePath);
        }

        public static string[] GetFilesNameWithinFolder(string folderPath, string searchPattern)
        {
            return Directory.GetFiles(folderPath, searchPattern, SearchOption.TopDirectoryOnly);
        }

        public static FileStream OpenFileForRead(string path)
        {
            return File.OpenRead(path);
        }

        public static TextReader OpenInternalText(string relativePath)
        {
            return OpenText(GetAbsolutePath(relativePath));
        }

        public static TextReader OpenText(string path)
        {
            return File.OpenText(path);
        }
    }
}