using System;
using ContentManager.Model.Enums;

namespace ContentManager.Model
{
    public abstract class ContentBase
    {
        #region Members

        public abstract string Name { get; protected set; }

        public abstract string CreationTime { get; }

        public abstract string ModificationTime { get; }

        public abstract FolderContentType Type { get; protected set; }

        public abstract long Size { get; }

        public abstract string RelativePath { get; protected set; }

        #endregion

        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType())
            {
                return false;
            }

            return this.Equals((ContentBase)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = (Name != null ? Name.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (RelativePath != null ? RelativePath.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ Type.GetHashCode();
                return hashCode;
            }
        }

        protected bool Equals(ContentBase other)
        {
            return Name.Equals(other.Name, StringComparison.OrdinalIgnoreCase) &&
                   RelativePath.Equals(other.RelativePath, StringComparison.OrdinalIgnoreCase) &&
                   Type.Equals(other.Type);
        }
    }
}
