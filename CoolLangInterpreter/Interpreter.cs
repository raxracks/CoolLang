using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoolLangInterpreter
{
    class Interpreter
    {
        static int tableWidth = 73;

        StringToFormula stf = new StringToFormula();

        public Dictionary<string, object> ram = new Dictionary<string, object>();

        public string[] operators = { "^", "/", "*", "+", "-" };

        public List<string> SplitAndKeep(string s, string[] delims)
        {
            var rows = new List<string>() { s };
            foreach (string delim in delims)
            {
                for (int i = 0; i < rows.Count; i++)
                {
                    int index = rows[i].IndexOf(delim);
                    if (index > -1
                        && rows[i].Length > index + 1)
                    {
                        bool nextToOperator = false;

                        if (index > 0)
                        {
                            foreach (string op in operators)
                            {
                                if (s[index - 1] == op.ToCharArray()[0]) { nextToOperator = true; }
                            }
                        }

                        string leftPart;

                        if (nextToOperator)
                        {
                            leftPart = rows[i].Substring(0, index + delim.Length);
                        } else
                        {
                            leftPart = rows[i].Substring(0, index);
                        }
                        string rightPart = rows[i].Substring(index + delim.Length);
                        rows[i] = leftPart;
                        if (!nextToOperator)
                        {
                            rows.Insert(i + 1, delim);
                        }
                        rows.Insert(i + 2, rightPart);
                    }
                }
            }
            return rows;
        }

        public bool TryParseEquation(string input, out double o)
        {
            input = String.Join("", input.Split(' '));

            bool includesOperator = false;

            foreach(string mathOperator in operators) {
                if (input.Contains(mathOperator)) includesOperator = true;
            }

            if(includesOperator)
            {
                List<string> operations = new List<string>();

                foreach (string mathOperator in operators)
                {
                    List<string> split = SplitAndKeep(input, new string[] { mathOperator.ToCharArray()[0].ToString() });

                    if (split.Count > 1)
                    {
                        foreach (string splitItem in split)
                        {
                            operations.Add(splitItem.ToString().Trim());
                        }
                    }
                }

                double sum = 0;

                for(int i = 0; i < operations.Count; i += 3)
                {
                    double left = double.Parse(operations[i]);
                    double right = double.Parse(operations[i + 2]);
                    string operation = operations[i + 1];
                    double n = 0;
                    
                    switch(operation)
                    {
                        case "+":
                            n = left + right;
                            break;

                        case "-":
                            n = left - right;
                            break;

                        case "/":
                            n = left / right;
                            break;

                        case "*":
                            n = left * right;
                            break;

                        case "^":
                            n = Math.Pow(left, right);
                            break;

                        default:
                            n = 0;
                            break;
                    }

                    sum += n;
                }

                o = sum;
                return true;
            } else
            {
                o = 0;
                return false;
            }
        }

        public object Parse(string input)
        {
            string inp = String.Join("", input.Split(' '));

            string full = "";

            if (input.Split(' ').Length > 1)
            {
                for (int i = 0; i < operators.Length; i++)
                {
                    string[] sp = inp.Split(operators[i].ToCharArray()[0]);

                    if (sp.Length > 1)
                    {
                        for (int b = 0; b < sp.Length; b++)
                        {
                            full += Parse(sp[b]).ToString() + operators[i];
                        }
                    }
                }
            }

            for (int i = 0; i < operators.Length; i++)
            {
                while(full.EndsWith(operators[i]))
                {
                    full = full.Substring(0, full.Length - 1);
                }
            }

            string o;

            if (string.IsNullOrEmpty(full)) {
                o = input;
            } else
            {
                o = Parse(full).ToString();
            }

            int valueInt;
            bool valueBool;
            double valueDouble;
            float valueFloat;
            object valueVariable;
            double valueExpression;
            bool isInt = int.TryParse(o, out valueInt);
            bool isBool = bool.TryParse(o, out valueBool);
            bool isDouble = double.TryParse(o, out valueDouble);
            bool isFloat = float.TryParse(o, out valueFloat);
            bool isVariable = ram.TryGetValue(o, out valueVariable);
            bool isString = o.StartsWith("\"") && o.EndsWith("\"") || o.StartsWith("'") && o.EndsWith("'");
            bool isExpression = TryParseEquation(o, out valueExpression);

            if (isInt)
            {
                return valueInt;
            }
            else if (isBool)
            {
                return valueBool;
            }
            else if (isDouble)
            {
                return valueDouble;
            }
            else if (isFloat)
            {
                return valueFloat;
            }
            else if (isVariable)
            {
                return GetMemory(input);
            } 
            else if(isExpression)
            {
                return valueExpression;
            }
            else if (isString)
            {
                return String.Join("", String.Join("", input.Split('"')).Split('\'')).ToString();
            }
            else
            {
                Exception exception = new Exception(input + " is undefined.");
                throw exception;
            }
        }

        public void DebugOut(object obj)
        {
            Console.WriteLine("[Debug]: " + obj);
        }

        static void PrintLine()
        {
            Console.WriteLine(new string('-', tableWidth));
        }

        static void PrintRow(params string[] columns)
        {
            int width = (tableWidth - columns.Length) / columns.Length;
            string row = "|";

            foreach (string column in columns)
            {
                row += AlignCentre(column, width) + "|";
            }

            Console.WriteLine(row);
        }

        static string AlignCentre(string text, int width)
        {
            text = text.Length > width ? text.Substring(0, width - 3) + "..." : text;

            if (string.IsNullOrEmpty(text))
            {
                return new string(' ', width);
            }
            else
            {
                return text.PadRight(width - (width - text.Length) / 2).PadLeft(width);
            }
        }

        public void SetMemory(string key, object value)
        {
            ram[key] = value;
        }

        public object GetMemory(string key)
        {
            return ram[key];
        }

        public void ClearMemory(string key)
        {
            ram.Remove(key);
        }

        public void ExecuteLine(string[] split, string line, int i, string[] lines, int currentLine)
        {
            if (split[i] == "define" && split[i + 2] == "as")
            {
                SetMemory(split[i + 1], Parse(line.Substring(split[i].Length + " ".Length + split[i + 1].Length + " ".Length + split[i + 2].Length + " ".Length)));
            }
            else if (split[i] == "print")
            {
                var output = line.Substring(split[i].Length + 1);
                Console.WriteLine(Parse(output));
            }
            else if (split[i] == "func" && line.EndsWith("{"))
            {
                List<string> funcLines = new List<string>();
                for (int index = currentLine; index < lines.Length; index++)
                {
                    if (lines[index].EndsWith("}"))
                    {
                        break;
                    }
                    funcLines.Add(lines[index]);
                }

                DebugOut(String.Join("\n", funcLines) + "\n\n\n\n");

                SetMemory(split[i + 1], funcLines);
            }
            else if (split[i] == "execfunc")
            {
                List<string> funcLines = (List<string>)GetMemory(split[i + 1]);

                foreach (string funcLine in funcLines)
                {
                    var funcSplit = funcLine.Split(' ');
                    for (int index = 0; index < funcSplit.Length; index++)
                    {
                        ExecuteLine(funcSplit, funcLine, index, funcLines.ToArray(), currentLine);
                    }
                }
            }
            else if (split[i] == "mempeek")
            {
                if (ram.Count > 0)
                {
                    //Console.Clear();
                    PrintLine();
                    PrintRow("Address", "Name", "Value", "Type");
                    PrintLine();

                    int memI = 0;
                    foreach (KeyValuePair<string, object> pointer in ram)
                    {
                        PrintRow("0x" + memI.ToString("X2"), pointer.Key.ToString(), pointer.Value.ToString(), pointer.Value.GetType().Name);
                        memI++;
                    }

                    PrintLine();
                } else
                {
                    Console.WriteLine("Nothing in memory.");
                }
            }
            else if (split[i] == "for" && line.EndsWith("{"))
            {
                var output = line.Substring(split[i].Length + 1);
                string[] parameters = output.Split(',');
                string name = parameters[0].Split(' ')[0];
                int value = (int)Parse(parameters[1].Split(' ')[0]);
                int max = (int)Parse(parameters[2].Split(' ')[0]);

                List<string> forLines = new List<string>();

                for (int index = currentLine; index < lines.Length; index++)
                {
                    if (lines[index].EndsWith("}"))
                    {
                        break;
                    }

                    forLines.Add(lines[index]);
                }

                SetMemory(name, value);

                for(int index = value; index < max; index++)
                {
                    SetMemory(name, index);
                    foreach(string forLine in forLines)
                    {
                        var forSplit = forLine.Split(' ');
                        for (int index2 = 0; index2 < forSplit.Length; index2++)
                        {
                            ExecuteLine(forSplit, forLine, index2, forLines.ToArray(), currentLine);
                        }
                    }
                }

                ClearMemory(name);
            }
        }

        public void Execute(string code)
        {
            string[] lines = code.Split('\n');
            int currentLine = 1;
            foreach (string line in lines)
            {
                var split = line.Split(' ');
                for (int i = 0; i < split.Length; i++)
                {
                    try
                    {
                        ExecuteLine(split, line, i, lines, currentLine);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.ToString());
                    }
                }

                currentLine++;
            }
        }
    }
}
