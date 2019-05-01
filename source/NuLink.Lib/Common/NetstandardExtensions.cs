using System;
using System.IO;
using System.Linq;

namespace NuLink.Lib.Common
{
    public static class NetstandardExtensions
    {
        public static T ToEnum<T>(this string s) where T : struct
        {
            return (T) Enum.Parse(typeof(T), s, ignoreCase: true);
        }
        
        public static string ToCamelCase(this string s)
        {
            if (s?.Length > 0)
            {
                if (s.All(ch => (!char.IsLetter(ch)) || char.IsUpper(ch)))
                {
                    return s.ToLower();
                }

                return char.ToLower(s[0]) + s.Substring(1);
            }

            return s;
        }
        
        public static string GetPathRelativeTo(this FileInfo absolutePath, FileInfo relativeTo)
        {
            return GetPathRelativeTo(absolutePath.FullName, relativeTo.FullName);
        }        

        public static string GetPathRelativeTo(this string absolutePath, string relativeTo)
        {
            var relativePath = new Uri(relativeTo)
                .MakeRelativeUri(new Uri(absolutePath)).ToString().Replace("/", "" + Path.DirectorySeparatorChar);
            
            return relativePath;
        }

        public static FileInfo WithPathSuffix(this FileInfo fileInfo, string path)
        {
            return new FileInfo(Path.Combine(fileInfo.FullName, path));
        }

        public static FileInfo WithFileExtension(this FileInfo fileInfo, string extension)
        {
            return new FileInfo(fileInfo.FullName + "." + extension.TrimStart('.'));
        }
    }
}