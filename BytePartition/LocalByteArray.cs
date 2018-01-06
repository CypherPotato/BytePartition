using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Collections;
using System.Linq;

namespace System
{
    public class LocalByteArray : IEnumerable<byte>, ICollection<byte>, IDisposable {
        internal string localFile = "";
        internal FileStream localStream;
        public LocalByteArray() {
            string tempFile = Path.GetTempFileName();
            localFile = tempFile;
            localStream = File.Open(localFile, FileMode.Open);
        }

        #region "Widening operators"
        // byte[] => local byte array
        public static implicit operator LocalByteArray(byte[] Intro) {
            LocalByteArray r = new LocalByteArray();
            r.localStream.Write(Intro, 0, Intro.Length);
            return r;
        }
        // local byte array => byte[]
        public static implicit operator byte[](LocalByteArray ar) {
            return ar.ToByteArray();
        }
        #endregion

        public bool EnableMemoryCleanup { get; set; }

        public int Length => Count;
        public int Count {
            get {
                return (int)localStream.Length;
            }
        }

        public bool IsReadOnly => false;

        public byte this[int index] {
            get {
                if ((index >= 0 && index <= localStream.Length) == false) throw new IndexOutOfRangeException();
                localStream.Position = index;
                return (byte)localStream.ReadByte();
            }
            set {
                if ((index >= 0 && index <= localStream.Length) == false) throw new IndexOutOfRangeException();
                localStream.Position = index;
                localStream.WriteByte(value);
            }
        }

        public void Insert(int index, byte item) {
            // TODO: IMPLEMENT
        }

        public void Add(byte item) {
            localStream.Position = localStream.Length;
            localStream.WriteByte(item);
        }

        public void Clear() {
            // closes the stream
            localStream.Close();
            // clear the file
            File.WriteAllBytes(localFile, new byte[] { });
            // reopen the stream
            localStream = File.Open(localFile, FileMode.Open);
        }
        public bool Contains(byte item) {
            localStream.Position = 0;
            int l = (int)localStream.Length;
            for (int i = 0; i < l; i++) {
                if ((byte)localStream.ReadByte() == item) return true;
            }
            return false;
        }
        public void CopyTo(byte[] array, int arrayIndex) {
            byte[] l = ReadFully(localStream);
            l.CopyTo(array, arrayIndex);
            System.Array.Clear(l, 0, l.Length);
        }

        public IEnumerator<byte> GetEnumerator() {
            localStream.Position = 0;
            int max = (int)localStream.Length;
            for(int i = 0; i < max; i++) {
                byte l = (byte)localStream.ReadByte();
                yield return l;
            }
        }

        public bool Remove(byte item) {
            // remove the item from the file
            byte[] newArray = ReadFully(localStream).Where(b => b != item).ToArray();
            // closes the stream
            localStream.Close();
            File.WriteAllBytes(localFile, newArray);
            // reopen the stream
            localStream = File.Open(localFile, FileMode.Open);

            // clear memory
            if(EnableMemoryCleanup) {
                Array.Clear(newArray, 0, newArray.Length);
                newArray = null;
                GC.Collect(); GC.WaitForPendingFinalizers();
            }

            return true;
        }
        public bool RemoveAt(int index) {
            // remove the item from the file
            byte[] newArray = ReadFully(localStream);
            newArray = newArray.Where(w => w != newArray[index]).ToArray();
            // closes the stream
            localStream.Close();
            File.WriteAllBytes(localFile, newArray);
            // reopen the stream
            localStream = File.Open(localFile, FileMode.Open);

            // clear memory
            if (EnableMemoryCleanup) {
                Array.Clear(newArray, 0, newArray.Length);
                newArray = null;
                GC.Collect(); GC.WaitForPendingFinalizers();
            }

            return true;
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public byte[] ToByteArray() {
            return ReadFully(localStream);
        }
        
        internal static byte[] ReadFully(Stream input) {
            long max = input.Length;
            byte[] buffer = new byte[max];
            input.Position = 0;
            for(long i = 0; i < max; i++) {
                buffer[i] = (byte)input.ReadByte();
            }
            return buffer;
        }

        public virtual void Dispose() {
            localStream.Close();
            File.Delete(localFile);
            GC.Collect();
            GC.WaitForPendingFinalizers();
        }
    }
}
