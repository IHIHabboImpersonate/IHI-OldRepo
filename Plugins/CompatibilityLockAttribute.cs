using System;

namespace IHI.Server.Plugins
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public class CompatibilityLockAttribute : Attribute
    {
        public int Release
        {
            get;
            private set;
        }
        public int SubRelease
        {
            get;
            private set;
        }

        public CompatibilityLockAttribute(int release) : this(release, -1){}

        public CompatibilityLockAttribute(int release, int subRelease)
        {
            Release = release;
            SubRelease = subRelease;
        }
    }
}
