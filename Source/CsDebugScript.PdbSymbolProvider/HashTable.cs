using CsDebugScript.Engine.Utility;
using CsDebugScript.PdbSymbolProvider.Utility;
using System;
using System.Collections.Generic;

namespace CsDebugScript.PdbSymbolProvider
{
    /// <summary>
    /// Represents PDB stream hash table. Since we are using it as readonly, it can easily be converted to <see cref="HashTable.Dictionary"/>.
    /// </summary>
    public class HashTable
    {
        /// <summary>
        /// Buckets array. Since it is readonly, every nonempty entry in this array corresponds to present element.
        /// </summary>
        private Tuple<uint, uint>[] buckets;

        /// <summary>
        /// Bit array of present entries.
        /// </summary>
        private uint[] present;

        /// <summary>
        /// Bit array of deleted entries.
        /// </summary>
        private uint[] deleted;

        /// <summary>
        /// Cache for <see cref="Dictionary"/>.
        /// </summary>
        private SimpleCacheStruct<Dictionary<uint, uint>> dictionaryCache;

        /// <summary>
        /// Initializes a new instance of the <see cref="HashTable"/> class.
        /// </summary>
        /// <param name="reader">Stream binary reader.</param>
        public HashTable(IBinaryReader reader)
        {
            Size = reader.ReadUint();
            Capacity = reader.ReadUint();
            if (Capacity == 0)
                throw new Exception("Invalid Hash Table Capacity");
            if (Size > MaxLoad(Capacity))
                throw new Exception("Invalid Hash Table Size");
            buckets = new Tuple<uint, uint>[Capacity];
            present = ReadSparseBitArray(reader);
            if (CountBits(present) != Size)
                throw new Exception("Present bit vector does not match size!");
            deleted = ReadSparseBitArray(reader);
            if (BitsIntersect(present, deleted))
                throw new Exception("Present bit vector interesects deleted!");
            for (int i = 0, index = 0; i < present.Length; i++)
                for (uint j = 0, bit = 1; j < 32; j++, bit <<= 1, index++)
                    if ((present[i] & bit) != 0)
                        buckets[index] = Tuple.Create(reader.ReadUint(), reader.ReadUint());
            dictionaryCache = SimpleCache.CreateStruct(() =>
            {
                Dictionary<uint, uint> dictionary = new Dictionary<uint, uint>();

                foreach (var tuple in buckets)
                    if (tuple != null)
                        dictionary.Add(tuple.Item1, tuple.Item2);
                return dictionary;
            });
        }

        /// <summary>
        /// Gets the number of entries in the dictionary.
        /// </summary>
        public uint Size { get; private set; }

        /// <summary>
        /// Gets the capacity of buckets array of this dictionary.
        /// </summary>
        public uint Capacity { get; private set; }

        /// <summary>
        /// Converts hash table to .NET dictionary.
        /// </summary>
        public Dictionary<uint, uint> Dictionary => dictionaryCache.Value;

        /// <summary>
        /// Reads bit array from the specified stream.
        /// </summary>
        /// <param name="reader">Stream binary reader.</param>
        private static uint[] ReadSparseBitArray(IBinaryReader reader)
        {
            uint entries = reader.ReadUint();

            return reader.ReadUintArray((int)entries);
        }

        /// <summary>
        /// Counts number of set bits in bit array.
        /// </summary>
        /// <param name="bitArray">Bit array.</param>
        private static int CountBits(uint[] bitArray)
        {
            int count = 0;

            for (int i = 0; i < bitArray.Length; i++)
                for (uint j = 0, bit = 1; j < 32; j++, bit <<= 1)
                    if ((bitArray[i] & bit) != 0)
                        count++;
            return count;
        }

        /// <summary>
        /// Checks whether two bit arrays intersect.
        /// </summary>
        /// <param name="array1">First bit array.</param>
        /// <param name="array2">Second bit array.</param>
        private static bool BitsIntersect(uint[] array1, uint[] array2)
        {
            for (int i = 0; i < array1.Length; i++)
                for (uint j = 0, bit = 1; j < 32; j++, bit <<= 1)
                    if ((array1[i] & bit) != 0 && (array2[i] & bit) != 0)
                        return true;
            return false;
        }

        /// <summary>
        /// Calculates maximum number of entries that hash table can have in the buckets array.
        /// </summary>
        /// <param name="capacity">Buckets array size.</param>
        private static uint MaxLoad(uint capacity)
        {
            return capacity * 2 / 3 + 1;
        }
    }
}
