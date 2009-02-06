using System;
using System.Collections;
using System.Text;

namespace Be.Windows.Forms
{
    internal class DataMap : ICollection, IEnumerable
    {
        readonly object _syncRoot = new object();
        internal int _count;
        internal DataBlock _firstBlock;
        internal int _version;

        public DataMap()
        {
        }

        public DataBlock FirstBlock
        {
            get
            {
                return _firstBlock;
            }
        }

        public void AddAfter(DataBlock block, DataBlock newBlock)
        {
            AddAfterInternal(block, newBlock);
        }

        public void AddBefore(DataBlock block, DataBlock newBlock)
        {
            AddBeforeInternal(block, newBlock);
        }

        public void AddFirst(DataBlock block)
        {
            if (_firstBlock == null)
            {
                AddBlockToEmptyMap(block);
            }
            else
            {
                AddBeforeInternal(_firstBlock, block);
            }
        }

        public void Remove(DataBlock block)
        {
            RemoveInternal(block);
        }

		public DataBlock Replace(DataBlock block, DataBlock newBlock)
		{
			AddAfterInternal(block, newBlock);
			RemoveInternal(block);
			return newBlock;
		}

        void AddAfterInternal(DataBlock block, DataBlock newBlock)
        {
            newBlock._previousBlock = block;
            newBlock._nextBlock = block._nextBlock;
            newBlock._map = this;

            if (block._nextBlock != null)
            {
                block._nextBlock._previousBlock = newBlock;
            }
            block._nextBlock = newBlock;

            this._version++;
            this._count++;
        }

        void AddBeforeInternal(DataBlock block, DataBlock newBlock)
        {
            newBlock._nextBlock = block;
            newBlock._previousBlock = block._previousBlock;
            newBlock._map = this;

            if (block._previousBlock != null)
            {
                block._previousBlock._nextBlock = newBlock;
            }
            block._previousBlock = newBlock;

            if (_firstBlock == block)
            {
                _firstBlock = newBlock;
            }
            this._version++;
            this._count++;
        }

        void RemoveInternal(DataBlock block)
        {
            DataBlock previousBlock = block._previousBlock;
            DataBlock nextBlock = block._nextBlock;

            if (previousBlock != null)
            {
                previousBlock._nextBlock = nextBlock;
            }

            if (nextBlock != null)
            {
                nextBlock._previousBlock = previousBlock;
            }

            if (_firstBlock == block)
            {
                _firstBlock = nextBlock;
            }

            InvalidateBlock(block);

            _count--;
            _version++;
        }

        static void InvalidateBlock(DataBlock block)
        {
            block._map = null;
            block._nextBlock = null;
            block._previousBlock = null;
        }

        void AddBlockToEmptyMap(DataBlock block)
        {
            block._map = this;
            block._nextBlock = null;
            block._previousBlock = null;

            _firstBlock = block;
            _version++;
            _count++;
        }

        #region ICollection Members
        public void CopyTo(Array array, int index)
        {
            DataBlock[] blockArray = array as DataBlock[];
            for (DataBlock block = FirstBlock; block != null; block = block.NextBlock)
            {
                blockArray[index++] = block;
            }
        }

        public int Count
        {
            get
            {
                return _count;
            }
        }

        public bool IsSynchronized
        {
            get
            {
                return false;
            }
        }

        public object SyncRoot
        {
            get
            {
                return _syncRoot;
            }
        }
        #endregion

        #region IEnumerable Members
        public IEnumerator GetEnumerator()
        {
            return new Enumerator(this);
        }
        #endregion

        #region Enumerator Nested Type
        internal class Enumerator : IEnumerator, IDisposable
        {
            DataMap _map;
            DataBlock _current;
            int _index;
            int _version;

            internal Enumerator(DataMap map)
            {
                _map = map;
                _version = map._version;
                _index = -1;
            }

            object IEnumerator.Current
            {
                get
                {
                    if (_index < 0 || _index > _map.Count)
                    {
                        throw new InvalidOperationException("Enumerator is positioned before the first element or after the last element of the collection.");
                    }
                    return _current;
                }
            }

            public bool MoveNext()
            {
                if (this._version != _map._version)
                {
                    throw new InvalidOperationException("Collection was modified after the enumerator was instantiated.");
                }

                if (_index >= _map.Count)
                {
                    return false;
                }

                if (++_index == 0)
                {
                    _current = _map.FirstBlock;
                }
                else
                {
                    _current = _current.NextBlock;
                }

                return (_index < _map.Count);
            }

            void IEnumerator.Reset()
            {
                if (this._version != this._map._version)
                {
                    throw new InvalidOperationException("Collection was modified after the enumerator was instantiated.");
                }

                this._index = -1;
                this._current = null;
            }

            public void Dispose()
            {
            }
        }
        #endregion
    }
}
