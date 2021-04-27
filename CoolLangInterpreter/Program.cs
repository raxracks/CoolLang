using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoolLangInterpreter
{
    class Program
    {
        static void Main(string[] args)
        {
            Interpreter interpreter = new Interpreter();

            //interpreter.Execute("func hi\nprint \"hello\"\n}");

            string code = "";
            bool multiline = false;

            while (true)
            {
                Console.Write(">");
                string c = Console.ReadLine();
                if(c == "multiline")
                {
                    multiline = true;
                    while (multiline)
                    {
                        Console.Write(">>");
                        string mc = Console.ReadLine();
                        if (mc == "exit")
                        {
                            interpreter.Execute(code);
                            code = "";
                            multiline = false;
                        } else
                        {
                            code += mc + "\n";
                        }
                    }
                } else
                {
                    interpreter.Execute(c);
                }
            }
        }
    }
}
