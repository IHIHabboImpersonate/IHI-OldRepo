namespace IHI.Server.Useful.Collections
{
    /// <remarks>
    /// Used for NestedSetList.
    /// </remarks>
    internal struct NestedSetData
    {
        internal int Left
        {
            get;
            set;
        }
        internal int Right
        {
            get;
            set;
        }
    }
    /// <remarks>
    /// Used for NestedSetDictionary.
    /// </remarks>
    internal struct NestedSetData<T>
    {
        internal T Value
        {
            get;
            set;
        }
        internal int Left
        {
            get;
            set;
        }
        internal int Right
        {
            get;
            set;
        }
    }
}