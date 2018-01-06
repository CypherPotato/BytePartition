
//#define USE_OPTIMIZATIONS

using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using System.Security.Cryptography;

namespace System
{
    /// <summary>
    /// Stores and encodes multiple byte arrays for an encoded one.
    /// </summary>
    public class BytePartition
    {
        #region "Encoders"
        internal void DecodePartition(byte[] buffer) {
            List<byte[]> partitions = new List<byte[]>();
            List<byte> tmp_Crt = new List<byte>();
            for(int pos = 0; pos < buffer.Length; pos++) {
                byte by = buffer[pos];
                
                // end of stream
                if(pos == buffer.Length - 1) {
                    partitions.Add(tmp_Crt.ToArray());
                    Console.WriteLine("Closing...");
                    break;
                }

                if(by == 255) { // partition divisor
                    partitions.Add(tmp_Crt.ToArray());
                    tmp_Crt.Clear();
                    tmp_Crt = new List<byte>();
#if USE_OPTIMIZATIONS
                    GC.Collect();
                    GC.WaitForPendingFinalizers(); // wait for memory clear
#endif
                    continue;

                } else {
                    byte n = Format128UnsignedTo255Byte(by);
                    tmp_Crt.Add(n);
                    continue;

                }
            }
            Console.WriteLine(partitions.Count);
            this.PartitionsDecoded.AddRange(partitions);
            partitions.Clear();
            partitions = null;
            tmp_Crt.Clear();
            tmp_Crt = null;
            GC.Collect();
        }
        /// <summary>
        /// Creates and encodes all the partitions to an byte array.
        /// </summary>
        /// <param name="buffer">Output byte array.</param>
        public void EncodePartition(out byte[] buffer) {
            long alloc = 0;
            long[] endOffsets = new long[PartitionsDecoded.Count];
            for (int k = 0; k < PartitionsDecoded.Count; ++k) {
                endOffsets[k] = PartitionsDecoded[k].Length;
                alloc += PartitionsDecoded[k].Length;
                alloc++;
            }

            byte[] output = new byte[alloc];

            int pos = -1;
            foreach (byte[] P in PartitionsDecoded) {
                foreach (byte p in P) {
                    pos++;
                    output[pos] = Format255ByteTo128Unsigned(p);
                }
                pos++;
                output[pos] = 255;
            }

            buffer = output;
        }
        #endregion
        /// <summary>
        /// Creates an new <seealso cref="BytePartition"/> class instance decoding an existing BytePartition encoded array.
        /// </summary>
        /// <param name="encodedData">The encoded byte array to decode in this class.</param>
        public BytePartition(byte[] encodedData) {
            this.DecodePartition(encodedData);
        }
        /// <summary>
        /// Creates a new <seealso cref="BytePartition"/> class instance without decoding an existing array.
        /// </summary>
        public BytePartition() { }
        
        internal List<byte[]> PartitionsDecoded = new List<byte[]>();

        #region "Functions"
        
        /// <summary>
        /// Returns the total of byte partitions in this instance.
        /// </summary>
        public int PartitionCount => PartitionsDecoded.Count;

        /// <summary>
        /// Gets the total size of all partitions.
        /// </summary>
        public long TotalSize { get {
                long n = 0;
                foreach(byte[] k in PartitionsDecoded) {
                    n += k.Length;
                    ++n; // partition
                }
                return n;
            } }

        #endregion

        #region "Hashing"
        /// <summary>
        /// Get an partition hash code using an specific hash algorithm.
        /// </summary>
        /// <param name="position">The partition position.</param>
        /// <param name="hashing">An <seealso cref="HashAlgorithm"/> implemented class to calculate the hash.</param>
        /// <returns></returns>
        public byte[] GetPartitionHash(int position, HashAlgorithm hashing) {
            return hashing.ComputeHash(PartitionsDecoded[position]);
        }
        /// <summary>
        /// Gets an partition hash code using SHA-256.
        /// </summary>
        /// <param name="position">The partition position.</param>
        /// <returns></returns>
        public byte[] GetPartitionHash(int position) {
            // Use SHA-256 in default
            return GetPartitionHash(position, new SHA256Managed());
        }
        #endregion

        #region "Methods"

        /// <summary>
        /// Creates an partition with an specified size.
        /// </summary>
        /// <param name="size">The fixed size of the partition.</param>
        /// <returns>The created partition position.</returns>
        public int CreatePartition(int size) {
            byte[] prt = new byte[size];
            return CreatePartition(prt);
        }

        /// <summary>
        /// Creates an partition with an existing byte buffer.
        /// </summary>
        /// <param name="buffer">The existing byte array that will be stored on the partition.</param>
        /// <returns>The created partition position.</returns>
        public int CreatePartition(byte[] buffer) {
            PartitionsDecoded.Add(buffer);
            return PartitionsDecoded.Count - 1;
        }

        /// <summary>
        /// Gets the entire partition byte buffer.
        /// </summary>
        /// <param name="partition">The partition position.</param>
        /// <returns></returns>
        public byte[] GetPartitionBuffer(int partition) {
            return PartitionsDecoded[partition];
        }

        /// <summary>
        /// Gets an partition allocated size.
        /// </summary>
        /// <param name="partition">The partition position.</param>
        /// <returns></returns>
        public long GetPartitionSize(int partition) {
            return PartitionsDecoded[partition].LongLength;
        }

        /// <summary>
        /// Changes the bytes stored in a partition.
        /// </summary>
        /// <param name="partition">The partition position.</param>
        /// <param name="offset">The byte position.</param>
        /// <param name="value">The new value for the byte.</param>
        public void ModifyPartition(int partition, int offset, byte value) {
            PartitionsDecoded[partition][offset] = value;
        }

        /// <summary>
        /// Replaces the entire partition buffer by another one. This changes the fixed size.
        /// </summary>
        /// <param name="partition">The partition position.</param>
        /// <param name="entireBuffer">The replacing byte buffer.</param>
        public void ModifyPartition(int partition, byte[] entireBuffer) {
            PartitionsDecoded[partition] = entireBuffer;
        }

        /// <summary>
        /// Clears an specified range in an partition.
        /// </summary>
        /// <param name="partition">The partition position.</param>
        /// <param name="offset">Starting byte position.</param>
        /// <param name="length">The length that will be cleaned from the offset.</param>
        public void ErasePartition(int partition, int offset, int length) {
            Array.Clear(PartitionsDecoded[partition], offset, length);
        }

        /// <summary>
        /// Erases and deletes an partition. All partitions positions is adjusted when removing an partition.
        /// </summary>
        /// <param name="partition">The partition position.</param>
        public void RemovePartition(int partition) {
            PartitionsDecoded.RemoveAt(partition);
        }
        
        /// <summary>
        /// Changes an partition's size.
        /// </summary>
        /// <param name="partition">The partition position.</param>
        /// <param name="newSize">The new partition's size. Everything that remains out of the range of the partition will be deleted.</param>
        public void FormatPartition(int partition, int newSize) {
            byte[] n = PartitionsDecoded[partition];
            PartitionsDecoded[partition] = new byte[0];
            Array.Resize(ref n, newSize);
            PartitionsDecoded[partition] = n;
        }
        #endregion

        #region "Formatters"
        int clearBit(int value, int bit) => value & ~(1 << (bit - 1));
        int setBit(int value, int bit) => value | (1 << (bit - 1));

        internal byte Format255ByteTo128Unsigned(byte originalByte) {
            if (originalByte == 0) return originalByte;
            if (originalByte == 254) return originalByte;
            bool bit = (originalByte & (1 << 1 - 1)) != 0;
            byte k = originalByte;
            if(bit == true) {
                k = (byte)clearBit(k, 1);
            } else {
                k = (byte)setBit(k, 1);
            }
            return k;
        }

        internal byte Format128UnsignedTo255Byte(byte signedByte) {
            if (signedByte == 0) return signedByte;
            if (signedByte == 254) return signedByte;
            bool bit = (signedByte & (1 << 1 - 1)) != 0;
            byte k = signedByte;
            if (bit == false) {
                k = (byte)setBit(k, 1);
            } else {
                k = (byte)clearBit(k, 1);
            }
            return k;
        }
        #endregion
    }
}
