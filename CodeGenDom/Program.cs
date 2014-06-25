//Sony Computer Entertainment Confidential

using System;
using System.IO;
using System.Text;

using Sce.Atf.Dom;

namespace DomGen
{
    static class Program
    {
        private static void WriteLine(StringBuilder sb, string s, params object[] p)
        {
            sb.Append(string.Format(s, p));
            sb.Append(Environment.NewLine);
        }

        // usage: CodeDomGen schemaFile outputFile classNamespace
        [STAThread]
        static void Main(string[] args)
        {            
            if (args.Length < 3)
            {                
                Console.WriteLine("usage:\r\nCodeDomGen schemaFile outputFile codeNamespace");
                return;
            }

            string inputFile = args[0];            
            string outputFile = args[1];
            string codeNamespace = args[2];

            var typeLoader = new XmlSchemaTypeLoader();
            typeLoader.Load(inputFile);
            UTF8Encoding encoding = new UTF8Encoding();
            FileStream strm = File.Open(outputFile, FileMode.Create);

            // TODO: NativeCodeGen will use NativeSchemaInfo to generate code.
            NativeSchemaInfo schemaInfo = new NativeSchemaInfo(typeLoader);
            NativeCodeGen codeGen = new NativeCodeGen();

            //string s = SchemaGen.Generate(typeLoader, codeNamespace, inputFile);
            string s = codeGen.Generate(schemaInfo, codeNamespace, inputFile);
            byte[] bytes = encoding.GetBytes(s);
            strm.Write(bytes, 0, bytes.Length);
        }
    }
}
