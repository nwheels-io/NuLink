using System;
using System.Collections.Generic;
using System.IO;
using Buildalyzer;
using Murphy.SymbolicLink;
using NuLink.Cli.ProjectStyles;

namespace NuLink.Cli
{
    public class PackageReferenceInfo : IEquatable<PackageReferenceInfo>
    {
        public PackageReferenceInfo(string packageId, string version, string rootFolderPath, string libSubfolderPath)
        {
            PackageId = packageId;
            Version = version;
            RootFolderPath = rootFolderPath;

            //StatusFilePath = Path.Combine(rootFolderPath, "nulink-status.txt"); 
            LibFolderPath = Path.Combine(rootFolderPath, libSubfolderPath);
            LibBackupFolderPath = Path.Combine(rootFolderPath, "nulink-backup.lib");
        }

        public PackageStatusInfo CheckStatus()
        {
            if (Directory.Exists(LibFolderPath))
            {
                //var statusFileContents = File.Exists(StatusFilePath) ? File.ReadAllText(StatusFilePath) : null;
                var targetPath = TryGetTargetPath(LibFolderPath); 
                
                return new PackageStatusInfo(
                    libFolderExists: true, 
                    isLibFolderLinked: targetPath != null, 
                    libFolderLinkTargetPath: targetPath,
                    libBackupFolderExists: Directory.Exists(LibBackupFolderPath));
            }
            
            return new PackageStatusInfo(
                libFolderExists: false, 
                isLibFolderLinked: false, 
                libFolderLinkTargetPath: null,
                libBackupFolderExists: false);
        }

        public string PackageId { get; }
        public string Version { get; }
        public string RootFolderPath { get; }
        public string LibFolderPath { get; }
        //public string StatusFilePath { get; }
        public string LibBackupFolderPath { get; }

        public bool Equals(PackageReferenceInfo other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return ( 
                string.Equals(PackageId, other.PackageId) && 
                string.Equals(Version, other.Version) && 
                string.Equals(RootFolderPath, other.RootFolderPath));
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((PackageReferenceInfo) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = PackageId.GetHashCode();
                hashCode = (hashCode * 397) ^ Version.GetHashCode();
                hashCode = (hashCode * 397) ^ RootFolderPath.GetHashCode();
                return hashCode;
            }
        }

        private string TryGetTargetPath(string linkPath)
        {
            var targetPath = SymbolicLinkWithDiagnostics.Resolve(linkPath);
            return (targetPath != linkPath ? targetPath : null);
        }
    }
    
    public class PackageStatusInfo
    {
        public PackageStatusInfo(bool libFolderExists, bool isLibFolderLinked, string libFolderLinkTargetPath, bool libBackupFolderExists)
        {
            LibFolderExists = libFolderExists;
            IsLibFolderLinked = isLibFolderLinked;
            LibFolderLinkTargetPath = libFolderLinkTargetPath;
            LibBackupFolderExists = libBackupFolderExists;
        }

        public bool LibFolderExists { get; }
        public bool IsLibFolderLinked { get; }
        public string LibFolderLinkTargetPath { get; }
        public bool LibBackupFolderExists { get; }
        public bool IsLinkable => LibFolderExists;
        public bool IsCorrupt => LibBackupFolderExists != IsLibFolderLinked;
    }
}
