using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace IHI.Server.Useful.Collections
{
    public class NestedSetDictionary<TKey, TValue> : IDictionary<TKey, TValue>
    {
        /// <summary>
        /// Stores all the data including the left and right values.
        /// </summary>
        private readonly Dictionary<TKey, NestedSetData<TValue>> _values;

        /// <summary>
        /// Caches the key for each position in the nested set.
        /// </summary>
        private TKey[] _positionCache;

        public NestedSetDictionary(TKey rootKey, TValue rootValue)
        {
            _values = new Dictionary<TKey, NestedSetData<TValue>>()
                          {
                              {
                                  rootKey, new NestedSetData<TValue>
                                               {
                                                   Left = 0,
                                                   Right = 1,
                                                   Value = rootValue
                                               }
                                  }
                          };
            Rebuild();
        }

        #region NestedSet Methods
        /// <summary>
        /// Rebuilds fInternalArray. This is called automatically after each change.
        /// </summary>
        private void Rebuild()
        {
            _positionCache = new TKey[_values.Count * 2];

            foreach (KeyValuePair<TKey, NestedSetData<TValue>> value in _values)
            {
                _positionCache[value.Value.Left] = value.Key;
                _positionCache[value.Value.Right] = value.Key;
            }
        }

        #region ToNameLaterBecauseICantThinkRightNow
        public ICollection<TValue> GetChildren(TKey key)
        {
            NestedSetData<TValue> parentData = _values[key];

            List<TValue> children = new List<TValue>();
            for(int i = parentData.Left+1; i < parentData.Right; i++)
            {
                children.Add(_values[_positionCache[i]].Value);
            }
            return children;
        }
        #endregion

        #region Add

        /// <summary>
        /// Determines whether the <see cref="T:System.Collections.Generic.IDictionary`2"/> contains an element with the specified key.
        /// </summary>
        /// <returns>
        /// true if the <see cref="T:System.Collections.Generic.IDictionary`2"/> contains an element with the key; otherwise, false.
        /// </returns>
        /// <param name="key">The key to locate in the <see cref="T:System.Collections.Generic.IDictionary`2"/>.</param><exception cref="T:System.ArgumentNullException"><paramref name="key"/> is null.</exception>
        public bool ContainsKey(TKey key)
        {
            return _positionCache.Contains(key);
        }

        /// <summary>
        /// Adds the value after the last child of first value.
        /// </summary>
        public void Add(TKey key, TValue value)
        {
            AddAsChildOf(key, value, _positionCache[0]);
        }
        /// <summary>
        /// Adds the value after the last child of parent.
        /// </summary>
        public void AddAsChildOf(TKey key, TValue value, TKey parentKey)
        {
            NestedSetData<TValue> parentData = _values[parentKey];

            NestedSetData<TValue> newData = new NestedSetData<TValue>
            {
                Left = parentData.Right,
                Right = parentData.Right + 1,
                Value = value
            };

            for (int i = parentData.Right + 1; i < _positionCache.Length; i++)
            {
                NestedSetData<TValue> workingData = _values[_positionCache[i]];
                workingData.Left += 2;
                workingData.Right += 2;

                // Replace the value
                _values.Remove(_positionCache[i]);
                _values.Add(_positionCache[i], workingData);
            }

            // Update the parent
            parentData.Right += 2;
            _values.Remove(parentKey);
            _values.Add(parentKey, parentData);

            _values.Add(key, newData);
            Rebuild();
        }

        public void AddLeftOf(TKey key, TValue value, TKey siblingKey)
        {
            NestedSetData<TValue> newData = new NestedSetData<TValue>
                                   {
                                       Left = _values[siblingKey].Left - 2,
                                       Right = _values[siblingKey].Left - 1,
                                       Value = value
                                   };

            for (int i = _values[siblingKey].Left; i < _positionCache.Length; i++)
            {
                NestedSetData<TValue> workingData = _values[_positionCache[i]];
                workingData.Left += 2;
                workingData.Right += 2;

                // Replace the value
                _values.Remove(_positionCache[i]);
                _values.Add(_positionCache[i], workingData);
            }
            _values.Add(key, newData);
            Rebuild();
        }

        public void AddRightOf(TKey key, TValue value, TKey siblingKey)
        {
            NestedSetData<TValue> newData = new NestedSetData<TValue>
            {
                Left = _values[siblingKey].Right + 1,
                Right = _values[siblingKey].Right + 2,
                Value = value
            }; 
            
            for (int i = _values[siblingKey].Right + 1; i < _positionCache.Length; i++)
            {
                NestedSetData<TValue> workingData = _values[_positionCache[i]];
                workingData.Left += 2;
                workingData.Right += 2;

                // Replace the value
                _values.Remove(_positionCache[i]);
                _values.Add(_positionCache[i], workingData);
            }
            _values.Add(key, newData);
            Rebuild();
        }
        #endregion

        #region Remove
        public bool Remove(TKey key)
        {
            return Remove(key, NestedSetRemoveChildAction.RecursiveDelete);
        }

        /// <summary>
        /// Gets the value associated with the specified key.
        /// </summary>
        /// <returns>
        /// true if the object that implements <see cref="T:System.Collections.Generic.IDictionary`2"/> contains an element with the specified key; otherwise, false.
        /// </returns>
        /// <param name="key">The key whose value to get.</param><param name="value">When this method returns, the value associated with the specified key, if the key is found; otherwise, the default value for the type of the <paramref name="value"/> parameter. This parameter is passed uninitialized.</param><exception cref="T:System.ArgumentNullException"><paramref name="key"/> is null.</exception>
        public bool TryGetValue(TKey key, out TValue value)
        {
            NestedSetData<TValue> tempValue;
            if (_values.TryGetValue(key, out tempValue))
            {
                value = tempValue.Value;
                return true;
            }
            value = default(TValue);
            return false;
        }

        /// <summary>
        /// Gets or sets the element with the specified key.
        /// </summary>
        /// <returns>
        /// The element with the specified key.
        /// </returns>
        /// <param name="key">The key of the element to get or set.</param><exception cref="T:System.ArgumentNullException"><paramref name="key"/> is null.</exception><exception cref="T:System.Collections.Generic.KeyNotFoundException">The property is retrieved and <paramref name="key"/> is not found.</exception><exception cref="T:System.NotSupportedException">The property is set and the <see cref="T:System.Collections.Generic.IDictionary`2"/> is read-only.</exception>
        public TValue this[TKey key]
        {
            get
            {
                return _values[key].Value;
            }
            set
            {
                if (!_values.ContainsKey(key))
                    throw new NotSupportedException("Adding values is not supported using indexers. Use the supplied methods instead. Existing values may be changed with indexers however.");
                NestedSetData<TValue> tempValue = _values[key];
                tempValue.Value = value;
                _values[key] = tempValue;
            }
        }

        /// <summary>
        /// Gets an <see cref="T:System.Collections.Generic.ICollection`1"/> containing the keys of the <see cref="T:System.Collections.Generic.IDictionary`2"/>.
        /// </summary>
        /// <returns>
        /// An <see cref="T:System.Collections.Generic.ICollection`1"/> containing the keys of the object that implements <see cref="T:System.Collections.Generic.IDictionary`2"/>.
        /// </returns>
        public ICollection<TKey> Keys
        {
            get { return _values.Keys; }
        }

        /// <summary>
        /// Gets an <see cref="T:System.Collections.Generic.ICollection`1"/> containing the values in the <see cref="T:System.Collections.Generic.IDictionary`2"/>.
        /// </summary>
        /// <returns>
        /// An <see cref="T:System.Collections.Generic.ICollection`1"/> containing the values in the object that implements <see cref="T:System.Collections.Generic.IDictionary`2"/>.
        /// </returns>
        public ICollection<TValue> Values
        {
            get
            {
                List<TValue> values = new List<TValue>(_values.Count);
                foreach(NestedSetData<TValue> valueData in _values.Values)
                {
                    values.Add(valueData.Value);
                }
                return values;
            }
        }

        public bool Remove(TKey key, NestedSetRemoveChildAction childAction)
        {
            // Does the item have child items?
            int correctionRequired = -2;
            if (_values[key].Left + 1 < _values[key].Right)
            {
                // Yes. What do we do?

                // Throw an exception?
                if (childAction == NestedSetRemoveChildAction.ThrowException)
                    throw new NotSupportedException("Unable to remove item due to child items.");

                // Do nothing and return false?
                if (childAction == NestedSetRemoveChildAction.ReturnFalse)
                    return false;

                // Remove them too?
                if (childAction == NestedSetRemoveChildAction.RecursiveDelete)
                {
                    correctionRequired = _values[key].Left - _values[key].Right - 1;

                    for (int i = _values[key].Left + 1; i < _values[key].Right; i++)
                    {
                        _values.Remove(_positionCache[i]);
                    }
                }

                // Move them up a generation?
                else if (childAction == NestedSetRemoveChildAction.MoveUpGeneration)
                {
                    for (int i = _values[key].Left + 1; i < _values[key].Right; i++)
                    {
                        NestedSetData<TValue> workingData = _values[_positionCache[i]];
                        workingData.Left -= 1;
                        workingData.Right -= 1;

                        // Replace the value
                        _values.Remove(_positionCache[i]);
                        _values.Add(_positionCache[i], workingData);
                    }
                }
            }

            for (int i = _values[key].Right+1; i < _positionCache.Length - 1; i++)
            {
                NestedSetData<TValue> workingData = _values[_positionCache[i]];
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
        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            foreach(KeyValuePair<TKey, NestedSetData<TValue>> valueData in _values)
            {
                yield return new KeyValuePair<TKey, TValue>(valueData.Key, valueData.Value.Value);
            }
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

        #region Implementation of ICollection<KeyValuePair<TKey,TValue>>

        /// <summary>
        /// Adds an item to the <see cref="T:System.Collections.Generic.ICollection`1"/>.
        /// </summary>
        /// <param name="item">The object to add to the <see cref="T:System.Collections.Generic.ICollection`1"/>.</param><exception cref="T:System.NotSupportedException">The <see cref="T:System.Collections.Generic.ICollection`1"/> is read-only.</exception>
        public void Add(KeyValuePair<TKey, TValue> item)
        {
            Add(item.Key, item.Value);
        }

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
        public bool Contains(KeyValuePair<TKey, TValue> item)
        {
            if (!_values.ContainsKey(item.Key))
                return false;

            return _values[item.Key].Value.Equals(item.Value);
        }

        /// <summary>
        /// Copies the elements of the <see cref="T:System.Collections.Generic.ICollection`1"/> to an <see cref="T:System.Array"/>, starting at a particular <see cref="T:System.Array"/> index.
        /// </summary>
        /// <param name="array">The one-dimensional <see cref="T:System.Array"/> that is the destination of the elements copied from <see cref="T:System.Collections.Generic.ICollection`1"/>. The <see cref="T:System.Array"/> must have zero-based indexing.</param><param name="arrayIndex">The zero-based index in <paramref name="array"/> at which copying begins.</param><exception cref="T:System.ArgumentNullException"><paramref name="array"/> is null.</exception><exception cref="T:System.ArgumentOutOfRangeException"><paramref name="arrayIndex"/> is less than 0.</exception><exception cref="T:System.ArgumentException"><paramref name="array"/> is multidimensional.-or-The number of elements in the source <see cref="T:System.Collections.Generic.ICollection`1"/> is greater than the available space from <paramref name="arrayIndex"/> to the end of the destination <paramref name="array"/>.-or-Type <paramref name="T"/> cannot be cast automatically to the type of the destination <paramref name="array"/>.</exception>
        public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
        {
            foreach(KeyValuePair<TKey, TValue> keyValue in this)
            {
                array[arrayIndex++] = keyValue;
            }
        }

        /// <summary>
        /// Removes the first occurrence of a specific object from the <see cref="T:System.Collections.Generic.ICollection`1"/>.
        /// </summary>
        /// <returns>
        /// true if <paramref name="item"/> was successfully removed from the <see cref="T:System.Collections.Generic.ICollection`1"/>; otherwise, false. This method also returns false if <paramref name="item"/> is not found in the original <see cref="T:System.Collections.Generic.ICollection`1"/>.
        /// </returns>
        /// <param name="item">The object to remove from the <see cref="T:System.Collections.Generic.ICollection`1"/>.</param><exception cref="T:System.NotSupportedException">The <see cref="T:System.Collections.Generic.ICollection`1"/> is read-only.</exception>
        public bool Remove(KeyValuePair<TKey, TValue> item)
        {
            if (!_values.ContainsKey(item.Key))
                return false;
            if (!_values[item.Key].Equals(item.Value))
                return false;
            Remove(item.Key);
            return true;
        }

        /// <summary>
        /// Gets the number of elements contained in the <see cref="T:System.Collections.Generic.ICollection`1"/>.
        /// </summary>
        /// <returns>
        /// The number of elements contained in the <see cref="T:System.Collections.Generic.ICollection`1"/>.
        /// </returns>
        public int Count
        {
            get { return _values.Count; }
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