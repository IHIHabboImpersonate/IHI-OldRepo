using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace IHI.Server.Useful.Collections
{
    public class NestedSetList<T> : ICollection<T>
    {
        /// <summary>
        /// Stores all the data including the left and right values.
        /// </summary>
        private Dictionary<T, NestedSetData> _values;

        /// <summary>
        /// Caches the key for each position in the nested set.
        /// </summary>
        private T[] _positionCache;

        #region NestedSet Methods
        /// <summary>
        /// Rebuilds fInternalArray. This is called automatically after each change.
        /// </summary>
        private void Rebuild()
        {
            _positionCache = new T[_values.Count * 2];

            foreach (KeyValuePair<T, NestedSetData> value in _values)
            {
                _positionCache[value.Value.Left] = value.Key;
                _positionCache[value.Value.Right] = value.Key;
            }
        }

        #region ToNameLaterBecauseICantThinkRightNow
        public IEnumerable<T> GetChildren(T key)
        {
            NestedSetData parentData = _values[key];

            for(int i = parentData.Left+1; i < parentData.Right; i++)
            {
                yield return _positionCache[i];
            }
        }
        #endregion

        #region Add
        /// <summary>
        /// Adds the value after the last child of first value.
        /// </summary>
        public void Add(T value)
        {
            AddAsChildOf(value, _positionCache[0]);
        }
        /// <summary>
        /// Adds the value after the last child of parent.
        /// </summary>
        public void AddAsChildOf(T value, T parent)
        {
            for (int i = _values[parent].Right; i < _positionCache.Length - 1; i++)
            {
                NestedSetData workingData = _values[_positionCache[i]];
                workingData.Left += 2;
                workingData.Right += 2;

                // Replace the value
                _values.Remove(_positionCache[i]);
                _values.Add(_positionCache[i], workingData);
            }
            _values.Add(value, new NestedSetData
            {
                Left = _values[parent].Right - 2,
                Right = _values[parent].Right -1
            });
            Rebuild();
        }

        public void AddLeftOf(T value, T sibling)
        {
            for (int i = _values[sibling].Left; i < _positionCache.Length - 1; i++)
            {
                NestedSetData workingData = _values[_positionCache[i]];
                workingData.Left += 2;
                workingData.Right += 2;

                // Replace the value
                _values.Remove(_positionCache[i]);
                _values.Add(_positionCache[i], workingData);
            }
            _values.Add(value, new NestedSetData
                                   {
                                       Left = _values[sibling].Right + 1,
                                       Right = _values[sibling].Right + 2
                                   });
            Rebuild();
        }

        public void AddRightOf(T value, T sibling)
        {
            for (int i = _values[sibling].Right + 1; i < _positionCache.Length - 1; i++)
            {
                NestedSetData workingData = _values[_positionCache[i]];
                workingData.Left += 2;
                workingData.Right += 2;

                // Replace the value
                _values.Remove(_positionCache[i]);
                _values.Add(_positionCache[i], workingData);
            }
            _values.Add(value, new NestedSetData
                                   {
                                       Left = _values[sibling].Right + 1,
                                       Right = _values[sibling].Right + 2
                                   });
            Rebuild();
        }
        #endregion

        #region Remove
        public bool Remove(T item)
        {
            return Remove(item, NestedSetRemoveChildAction.RecursiveDelete);
        }

        public bool Remove(T item, NestedSetRemoveChildAction childAction)
        {
            // Does the item have child items?
            int correctionRequired = -2;
            if(_values[item].Left+1 < _values[item].Right)
            {
                // Yes. What do we do?

                if (childAction == NestedSetRemoveChildAction.ThrowException)
                    throw new NotSupportedException("Unable to remove item due to child items.");
                if (childAction == NestedSetRemoveChildAction.ReturnFalse)
                    return false;

                if (childAction == NestedSetRemoveChildAction.RecursiveDelete)
                {
                    correctionRequired = _values[item].Left - _values[item].Right - 1;

                    for (int i = _values[item].Left + 1; i < _values[item].Right; i++)
                    {
                        _values.Remove(_positionCache[i]);
                    }
                }
                else if (childAction == NestedSetRemoveChildAction.MoveUpGeneration)
                {
                    for (int i = _values[item].Left + 1; i < _values[item].Right; i++)
                    {
                        NestedSetData workingData = _values[_positionCache[i]];
                        workingData.Left -= 1;
                        workingData.Right -= 1;

                        // Replace the value
                        _values.Remove(_positionCache[i]);
                        _values.Add(_positionCache[i], workingData);
                    }
                }
            }

            for (int i = _values[item].Right+1; i < _positionCache.Length - 1; i++)
            {
                NestedSetData workingData = _values[_positionCache[i]];
                workingData.Left += correctionRequired;
                workingData.Right += correctionRequired;

                // Replace the value
                _values.Remove(_positionCache[i]);
                _values.Add(_positionCache[i], workingData);
            }
            Rebuild();

            return true;
        }
        #endregion

        #endregion

        #region Implementation of IEnumerable
        /// <summary>
        /// Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.Collections.Generic.IEnumerator`1"/> that can be used to iterate through the collection.
        /// </returns>
        /// <filterpriority>1</filterpriority>
        public IEnumerator<T> GetEnumerator()
        {
            return _values.Keys.GetEnumerator();
        }

        /// <summary>
        /// Returns an enumerator that iterates through a collection.
        /// </summary>
        /// <returns>
        /// An <see cref="T:System.Collections.IEnumerator"/> object that can be used to iterate through the collection.
        /// </returns>
        /// <filterpriority>2</filterpriority>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion

        #region Implementation of ICollection<T>
        /// <summary>
        /// Removes all items from the <see cref="T:System.Collections.Generic.ICollection`1"/>.
        /// </summary>
        /// <exception cref="T:System.NotSupportedException">The <see cref="T:System.Collections.Generic.ICollection`1"/> is read-only. </exception>
        public void Clear()
        {
            _values.Clear();
            Rebuild();
        }

        /// <summary>
        /// Determines whether the <see cref="T:System.Collections.Generic.ICollection`1"/> contains a specific value.
        /// </summary>
        /// <returns>
        /// true if <paramref name="item"/> is found in the <see cref="T:System.Collections.Generic.ICollection`1"/>; otherwise, false.
        /// </returns>
        /// <param name="item">The object to locate in the <see cref="T:System.Collections.Generic.ICollection`1"/>.</param>
        public bool Contains(T item)
        {
            return _values.ContainsKey(item);
        }

        /// <summary>
        /// Copies the elements of the <see cref="T:System.Collections.Generic.ICollection`1"/> to an <see cref="T:System.Array"/>, starting at a particular <see cref="T:System.Array"/> index.
        /// </summary>
        /// <param name="array">The one-dimensional <see cref="T:System.Array"/> that is the destination of the elements copied from <see cref="T:System.Collections.Generic.ICollection`1"/>. The <see cref="T:System.Array"/> must have zero-based indexing.</param><param name="arrayIndex">The zero-based index in <paramref name="array"/> at which copying begins.</param><exception cref="T:System.ArgumentNullException"><paramref name="array"/> is null.</exception><exception cref="T:System.ArgumentOutOfRangeException"><paramref name="arrayIndex"/> is less than 0.</exception><exception cref="T:System.ArgumentException"><paramref name="array"/> is multidimensional.-or-The number of elements in the source <see cref="T:System.Collections.Generic.ICollection`1"/> is greater than the available space from <paramref name="arrayIndex"/> to the end of the destination <paramref name="array"/>.-or-Type <paramref name="T"/> cannot be cast automatically to the type of the destination <paramref name="array"/>.</exception>
        public void CopyTo(T[] array, int arrayIndex)
        {
            _values.Keys.CopyTo(array, arrayIndex);
        }

        /// <summary>
        /// Gets the number of elements contained in the <see cref="T:System.Collections.Generic.ICollection`1"/>.
        /// </summary>
        /// <returns>
        /// The number of elements contained in the <see cref="T:System.Collections.Generic.ICollection`1"/>.
        /// </returns>
        public int Count
        {
            get
            {
                return _values.Count;
            }
        }

        /// <summary>
        /// Gets a value indicating whether the <see cref="T:System.Collections.Generic.ICollection`1"/> is read-only.
        /// </summary>
        /// <returns>
        /// true if the <see cref="T:System.Collections.Generic.ICollection`1"/> is read-only; otherwise, false.
        /// </returns>
        public bool IsReadOnly
        {
            get { return false; }
        }

        #endregion
    }
}
