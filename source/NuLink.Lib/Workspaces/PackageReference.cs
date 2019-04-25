using System;
using Semver;

namespace NuLink.Lib.Workspaces
{
    public class PackageReference : IEquatable<PackageReference>
    {
        public PackageReference(string id, SemVersion version)
        {
            Id = id;
            Version = version;
        }

        public bool Equals(PackageReference other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return string.Equals(Id, other.Id) && Equals(Version, other.Version);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((PackageReference) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((Id != null ? Id.GetHashCode() : 0) * 397) ^ (Version != null ? Version.GetHashCode() : 0);
            }
        }

        public string Id { get; }
        public SemVersion Version { get; }
    }
}
