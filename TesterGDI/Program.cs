using System;
using System.IO;
using System.Text;
using System.Windows.Forms;
namespace Tester {
    class Program {
        static byte[] strEV;
        public static string CreateMD5(byte[] input) {
            // Use input string to calculate MD5 hash
            using (System.Security.Cryptography.MD5 md5 = System.Security.Cryptography.MD5.Create()) {
                byte[] inputBytes = input;
                byte[] hashBytes = md5.ComputeHash(inputBytes);

                // Convert the byte array to hexadecimal string
                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < hashBytes.Length; i++) {
                    sb.Append(hashBytes[i].ToString("X2"));
                }
                return sb.ToString();
            }
        }

        public static byte[] FileToByteArray(string fileName) {
            return File.ReadAllBytes(fileName);
        }
        public static string FilePath() {
            OpenFileDialog openFileDialog1 = new OpenFileDialog();
            DialogResult result = openFileDialog1.ShowDialog();
            if (result == DialogResult.OK) {
                return openFileDialog1.FileName;
            } else {
                return null;
            }
        }
        public static byte[] ImageToByteArray(System.Drawing.Image imageIn) {
            using (var ms = new MemoryStream()) {
                imageIn.Save(ms, System.Drawing.Imaging.ImageFormat.Gif);
                return ms.ToArray();
            }
        }
        [STAThread]
        static void Main(string[] args) {
            byte[] a = new byte[] { 16, 15, 14, 255, 254 };
            byte[] s;
            BytePartition nn = new BytePartition();
            nn.CreatePartition(a);
            nn.EncodePartition(out s);
            Console.WriteLine(string.Join(", ", s));
            Console.ReadLine();
        }
    }
}