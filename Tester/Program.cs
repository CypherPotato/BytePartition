using System;
using System.Threading;
using System.Text;
using System.Linq;
namespace Tester
{
    class Program
    {
        static byte[] strEV;
        static void Main(string[] args)
        {
            {
                // testar a array temp
                LocalByteArray k = new LocalByteArray() { 12, 52, 73 };
                k.Add(255);
                k.Remove((byte)52);
                k.Add(31);
                foreach (byte n in k) {
                    Console.Write(n + ", ");
                }
                Console.WriteLine();
                Console.WriteLine(k.Contains(12)); // true
                Console.WriteLine(k.Contains(1)); // false
                Console.WriteLine(k.Count); // 4
                Console.WriteLine(string.Join(", ", k.ToArray()));
                Console.ReadLine();
                Console.Clear();
            }
            // textos padrões
            string str1 = "Olá, mundo!";
            string str2 = "Eu sou outra string nada a ver com a outra";
            // bytes dos textos acima
            byte[] str1b = Encoding.UTF8.GetBytes(str1);
            byte[] str2b = Encoding.UTF8.GetBytes(str2);

            // onde vão ficar armazenado as variaveis
            byte[] str1o;
            byte[] str2o;

            // bytes que serão encodados

            // cria o gerenciador
            BytePartition part = new BytePartition();
            // cria as partições
            part.CreatePartition(str1b);
            part.CreatePartition(str2b);

            // manda tudo para os encoders em Async
            //                                       //Thread Method = new Thread(() => part.EncodePartition(out strEV));
            // O programa termina antes do thread ~> //Method.IsBackground = true;
            //                                       //Method.Start();

            part.EncodePartition(out strEV);

            // vamos ver como ficou
            {
                Console.WriteLine("String 1 -> " + string.Join(" ", str1b));
                Console.WriteLine("String 2 -> " + string.Join(" ", str2b));
                Console.WriteLine("String T -> " + string.Join(" ", strEV));
            }
            
            // agora vamos decodar em duas variáveis
            BytePartition part2 = new BytePartition(strEV);
            
            str1o = part2.GetPartitionBuffer(0); // primeira string
            str2o = part2.GetPartitionBuffer(1); // segunda string

            // vamos ver o resultado
            Console.WriteLine("-----------------------------");
            Console.WriteLine(Encoding.UTF8.GetString(str1o));
            Console.WriteLine(Encoding.UTF8.GetString(str2o));

            Console.ReadLine();
        }
    }
}
