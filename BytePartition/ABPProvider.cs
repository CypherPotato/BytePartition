using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace System
{
    /// <summary>
    /// Allocated Byte Partitions code provider.
    /// </summary>
    public class ABPProvider
    {
        public List<byte[]> InternalAlloc = new List<byte[]>();

        #region "Constructors"
        /// <summary>
        /// Creates a new empty ABP instance.
        /// </summary>
        public ABPProvider() { }
        /// <summary>
        /// Creates a new ABP instance loading an existing buffer array.
        /// </summary>
        /// <param name="deconstruct">The ABP encoded byte array.</param>
        public ABPProvider(byte[] deconstruct) {
            this.DecodeInput(deconstruct);
        }
        #endregion

        #region "IO functions"
        /// <summary>
        /// Creates an new empty partition with specified allocated size.
        /// </summary>
        /// <param name="size">The partition size in bytes.</param>
        /// <returns></returns>
        public int CreatePartition(int size) { InternalAlloc.Add(new byte[size]); return InternalAlloc.Count; }
        /// <summary>
        /// Creates an new partition with an existing buffer.
        /// </summary>
        /// <param name="buffer">The existing byte buffer.</param>
        /// <returns></returns>
        public int CreatePartition(byte[] buffer) { InternalAlloc.Add(buffer); return InternalAlloc.Count; }

        /// <summary>
        /// Gets an entire partition byte-buffer.
        /// </summary>
        /// <param name="partition">The partition's index.</param>
        /// <returns></returns>
        public byte[] GetPartitionBuffer(int partition) { return InternalAlloc[partition]; }
        /// <summary>
        /// Gets an partition buffer with specified index and length.
        /// </summary>
        /// <param name="partition">The partition's index.</param>
        /// <param name="startIndex">The buffer start offset.</param>
        /// <param name="length">Number of bytes to get after the offset.</param>
        /// <returns></returns>
        public byte[] GetPartitionBuffer(int partition, int startIndex, int length) { return InternalAlloc[partition].Skip(startIndex).Take(length).ToArray(); }

        /// <summary>
        /// Returns an partition's total size.
        /// </summary>
        /// <param name="partition">The partition's index.</param>
        /// <returns></returns>
        public long GetPartitionSize(int partition) { return InternalAlloc[partition].Length; }
        
        public void ModifyPartition(int partition, long offset, byte newValue) { InternalAlloc[partition][offset] = newValue; }
        public void ModifyPartition(int partition, byte[] newBuffer) { InternalAlloc[partition] = newBuffer; }
        public void ErasePartition(int partition, int startIndex, int length) { Array.Clear(InternalAlloc[partition], startIndex, length); }
        public void FormatPartition(int partition, int newSize) {
            byte[] n = InternalAlloc[partition];
            InternalAlloc[partition] = new byte[0];
            Array.Resize(ref n, newSize);
            InternalAlloc[partition] = n;
        }
        public void RemovePartition(int partition) { InternalAlloc.RemoveAt(partition); }

        public long TotalSize {
            get {
                var calc = PerformCalcs();
                return calc.bufferSize + calc.postPosition;
            }
        }
        public int PartitionCount => InternalAlloc.Count;

        #endregion

        #region "Calculations"
        
        internal byte[] TrimEnd(byte[] array) {
            int lastIndex = Array.FindLastIndex(array, b => b != 0);

            Array.Resize(ref array, lastIndex + 1);

            return array;
        }
        
        internal (byte PartitionCount, int postPosition, long[] offsets, long bufferSize) PerformCalcs() {

            int biggestCount = 0;
            int counts = 0;

            if (InternalAlloc.Count > 255)
                throw new InvalidOperationException("Out of memory allocation.");

            // total of partitions
            counts = (byte)InternalAlloc.Count;
            
            // get offsets
            long[] offsets = new long[counts];
            int pos = 0; long hem = 0;
            foreach(byte[] mn in InternalAlloc) {
                offsets[pos] = (hem + mn.Length); // offset of the current array
                hem += mn.Length; // increase the total allocated size for others array
                ++pos;
            }

            // calc offsets size
            foreach(long ofst in offsets) {
                var arr = TrimEnd(BitConverter.GetBytes(ofst));
                int siz = arr.Length; // marshal of the size
                if (siz > biggestCount) biggestCount = siz;
            }

            return ((byte)counts,           // total of partitions
                biggestCount * counts,  // offset of starting data array
                offsets,                 // array of numeric positions of partitions
                hem
                );
        }

        #endregion

        #region "Encoders"
        internal void DecodeInput(byte[] buffer) {
            (byte pCount, byte allocSize) info;
            info.allocSize = buffer[1];
            info.pCount = buffer[0];
            
            int reservedValues = 2 + info.allocSize * info.pCount;
            long[] partitionSplitters = new long[info.pCount];
            {
                // calc partitions offsets
                IEnumerable<byte> readBuffer = buffer.Skip(2).Take(reservedValues);
                int i = 0, j = 0;
                var query = from s in readBuffer
                            let num = i++
                            group s by num / info.allocSize into g
                            select g.ToArray();
                var results = query.ToArray().Take(info.pCount);
                foreach (byte[] l in results) {
                    long calc = BitConverter.ToInt64(l, 0);
                    partitionSplitters[j] = calc;
                    j++;
                }
            }
            
            List<byte[]> lists = new List<byte[]>();
            {
                // calc bytes
                int j = 0;
                List<byte> casting = new List<byte>();
                bool firstItem = false;
                for(int i = reservedValues - 1; i < buffer.Length; i++) {
                    if(firstItem == false) { firstItem = true; j++; continue; } // an brazilian gambiarra. don't disable it.
                    casting.Add(buffer[i]);
                    if (partitionSplitters.Contains(j)) {
                        // split partition
                        lists.Add(casting.ToArray());
                        casting.Clear();
                    }
                    j++;
                }
            }

            // convert temporary output to class instance
            InternalAlloc.Clear(); foreach(byte[] k in lists) {
                this.InternalAlloc.Add(k.ToArray());
            }
            // done!
        }

        public void EncodeOutput(out byte[] buffer) {
            var calcResults = PerformCalcs();

            var allocSize = 8;

            List<byte> _buffer = new List<byte>();

            // PRE BYTES
            {
                _buffer.Add(calcResults.PartitionCount); // 0
                _buffer.Add((byte)allocSize); // 1
            }

            // PARTITION NODES
            foreach(long pos in calcResults.offsets) {
                byte[] s = BitConverter.GetBytes(pos);
                _buffer.AddRange(s);
            }

            // REST OF BYTES
            foreach(byte[] i in InternalAlloc) {
                foreach(byte j in i) {
                    _buffer.Add(j);
                }
            }

            buffer = _buffer.ToArray();
            _buffer.Clear();
        }
        #endregion
    }
}
