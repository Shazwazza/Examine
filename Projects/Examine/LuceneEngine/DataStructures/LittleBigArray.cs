using System;
using System.Collections;
using System.Collections.Generic;

namespace Examine.LuceneEngine.DataStructures
{
    /// <summary>
    /// This array dodges the LOH heap, is sparse, and fast to iterate when non zero items a few
    /// </summary>
    internal class LittleBigArray : IEnumerable<KeyValuePair<int, int>>
    {
        //@nielskuhnel: This code is originally from http://blogs.msdn.com/b/joshwil/archive/2005/08/10/450202.aspx but changed so that it will not be allocated on the Large object heap (LOH) (if less than 40M items). 
        //And also improved with respect to iteration performance

        //private OpenBitSet _mainArraysIndicies;
        //private OpenBitSet[] _subArraysIndices;

        internal const int BlockSize = 4096;
        internal const int Mask = BlockSize - 1;
        internal const int BlockSizeLog2 = 12;

        int[][] _elements;
        int _length;

        // maximum BigArray size = BLOCK_SIZE * Int.MaxValue
        public LittleBigArray(int size)
        {
            int numBlocks = (size / BlockSize);
            if ((numBlocks * BlockSize) < size)
            {
                numBlocks += 1;
            }

            //_mainArraysIndicies = new OpenBitSet(numBlocks);
            //_subArraysIndices = new OpenBitSet[numBlocks];

            _length = size;
            _elements = new int[numBlocks][];

            //for (int i = 0; i < numBlocks; i++)
            //{
            //    _subArraysIndices[i] = new OpenBitSet(BlockSize);
            //    _elements[i] = new T[BlockSize];
            //}
        }

        public int Length
        {
            get
            {
                return _length;
            }
        }

        public int this[int elementNumber]
        {
            get
            {
                var subArray = _elements[elementNumber >> BlockSizeLog2];
                return subArray != null ? subArray[elementNumber & Mask] : 0;
            }
            set
            {
                int major = elementNumber >> BlockSizeLog2, minor = elementNumber & Mask;
                //_mainArraysIndicies.FastSet(major);
                if( _elements[major] == null )
                {
                    //_subArraysIndices[major] = new OpenBitSet(BlockSize);
                    _elements[major] = new int[BlockSize];
                }
                //_subArraysIndices[major].FastSet(minor);
                _elements[major][minor] = value;
            }
        }

        public void Reset()
        {
            //_mainArraysIndicies.Clear(0, _length);
            for (var i = 0; i < _elements.Length; i++)
            {
                if (_elements[i] != null)
                {
              //      _subArraysIndices[i].Clear(0, BlockSize);
                    Array.Clear(_elements[i], 0, BlockSize);
                }
            }
        }


        public IEnumerator<KeyValuePair<int, int>> GetEnumerator()
        {
            for (int i = 0, n = _elements.Length; i < n; i++  )
            {
                var sub = _elements[i];
                if( sub != null )
                {
                    for( int j = 0, n_j = sub.Length; j < n_j; j++)
                    {
                        if (sub[j] > 0)
                        {
                            yield return new KeyValuePair<int, int>((i << BlockSizeLog2) + j, _elements[i][j]);
                        }
                    }
                }
            }
                //foreach (var m in _mainArraysIndicies.GetDocIds())
                //{
                //    foreach (var i in _subArraysIndices[m].GetDocIds())
                //    {
                //        yield return new KeyValuePair<int, T>((m << BlockSizeLog2) + i, _elements[m][i]);
                //    }
                //}
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
        
