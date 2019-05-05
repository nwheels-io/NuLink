using System;
using System.Collections.Generic;

namespace NuLink.Cli
{
    public interface IPackageReferenceInfo : IEquatable<IPackageReferenceInfo>
    {
        string PackageId { get; }
        string Version { get; }
        string PackageFolder { get; }
        bool IsLinked { get; }
        bool LinkedFolderPath { get; }
    }
    
    public class PackageReferenceInfo : IPackageReferenceInfo, IEquatable<PackageReferenceInfo>, IEquatable<IPackageReferenceInfo>
    {
        public string PackageId { get; set; }
        public string Version { get; set; }
        public string PackageFolder { get; set; }
        public bool IsLinked { get; set; }
        public bool LinkedFolderPath { get; set; }

        public bool Equals(PackageReferenceInfo other)
        {
            return string.Equals(PackageId, other.PackageId) && string.Equals(Version, other.Version);
        }

        bool IEquatable<IPackageReferenceInfo>.Equals(IPackageReferenceInfo other)
        {
            return this.Equals(other);
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
                return ((PackageId != null ? PackageId.GetHashCode() : 0) * 397) ^ (Version != null ? Version.GetHashCode() : 0);
            }
        }
    }
}