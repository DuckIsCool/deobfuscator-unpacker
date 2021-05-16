using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net;
using WindowsFormsApp12.Properties;
using dnlib.DotNet;
using System.IO;
using dnlib.DotNet.Emit;
using dnlib.DotNet.Writer;

namespace WindowsFormsApp12
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }
        private void Form1_Load(object sender, EventArgs e)
        {

        }
        private void button1_Click(object sender, EventArgs e)
        {
            
            string path = textBox1.Text;
            string path2 = path.Replace("\"", "");
            ModuleContext modu = ModuleDef.CreateModuleContext();
            var modules = ModuleDefMD.Load(textBox1.Text, modu);
            if (checkBox1.Checked)
            {
                Deobfuscate(modules);
                richTextBox1.AppendText(String.Format("{0}{1}", "Patched AntiDe4Dot", Environment.NewLine));
            }
            if (checkBox2.Checked)
            {
                Deobfuscate2(modules);
                richTextBox1.AppendText(String.Format("{0}{1}", "Patched Anti-Decompiler", Environment.NewLine));
                
            }
            if (checkBox3.Checked)
            {
                Deobfuscate3(modules);
                richTextBox1.AppendText(String.Format("{0}{1}", "Patched Proxy-Calls", Environment.NewLine));

            }
            var text2 = Path.GetDirectoryName(textBox1.Text);
            if (text2 != null && !text2.EndsWith("\\"))
            {
                text2 += "\\";
            }
            var pathez = $"{text2}{Path.GetFileNameWithoutExtension(textBox1.Text)}.ducked{Path.GetExtension(textBox1.Text)}";
            modules.Write(pathez,
                         new ModuleWriterOptions(modules)
                         { PEHeadersOptions = { NumberOfRvaAndSizes = 13 }, Logger = DummyLogger.NoThrowInstance });
            richTextBox1.AppendText(String.Format("{0}{1}", $"[+] Saved To {text2}", Environment.NewLine));

        }

        private void button2_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog dialog = new OpenFileDialog())
            {
                if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    textBox1.Text = dialog.FileName;
                }
            }
        }
        public static int Deobfuscate3(ModuleDefMD module)
        {
            int num = 0;
            foreach (TypeDef typeDef in module.Types)
            {
                foreach (MethodDef methodDef in typeDef.Methods)
                {
                    bool flag = !methodDef.HasBody;
                    bool flag10 = !flag;
                    if (flag10)
                    {
                        for (int i = 0; i < methodDef.Body.Instructions.Count; i++)
                        {
                            bool flag2 = methodDef.Body.Instructions[i].OpCode == OpCodes.Call;
                            bool flag11 = flag2;
                            if (flag11)
                            {
                                try
                                {
                                    MethodDef methodDef2 = methodDef.Body.Instructions[i].Operand as MethodDef;
                                    bool flag3 = methodDef2 == null;
                                    bool flag4 = !flag3;
                                    bool flag12 = flag4;
                                    if (flag12)
                                    {
                                        bool flag5 = !methodDef2.IsStatic || !typeDef.Methods.Contains(methodDef2);
                                        bool flag6 = !flag5;
                                        bool flag13 = flag6;
                                        if (flag13)
                                        {
                                            OpCode opCode;
                                            object proxyValues = GetProxyValues(methodDef2, out opCode);
                                            bool flag7 = opCode == null || proxyValues == null;
                                            bool flag8 = !flag7;
                                            bool flag14 = flag8;
                                            if (flag14)
                                            {
                                                methodDef.Body.Instructions[i].OpCode = opCode;
                                                methodDef.Body.Instructions[i].Operand = proxyValues;
                                                num++;
                                                bool flag9 = !junkMethods.Contains(methodDef2);
                                                bool flag15 = flag9;
                                                if (flag15)
                                                {
                                                    junkMethods.Add(methodDef2);
                                                }
                                            }
                                        }
                                    }
                                }
                                catch
                                {
                                }
                            }
                        }
                    }
                }
            }
            RemoveJunkMethods(module);
            return num;
        }
        private static object GetProxyValues(MethodDef method, out OpCode opCode)
        {
            result = null;
            opCode = null;
            int i = 0;
            while (i < method.Body.Instructions.Count)
            {
                bool flag = method.Body.Instructions.Count <= 10;
                bool flag5 = flag;
                object obj;
                if (flag5)
                {
                    bool flag2 = method.Body.Instructions[i].OpCode == OpCodes.Call;
                    bool flag6 = flag2;
                    if (flag6)
                    {
                        opCode = OpCodes.Call;
                        result = method.Body.Instructions[i].Operand;
                        obj = result;
                    }
                    else
                    {
                        bool flag3 = method.Body.Instructions[i].OpCode == OpCodes.Newobj;
                        bool flag7 = flag3;
                        if (flag7)
                        {
                            opCode = OpCodes.Newobj;
                            result = method.Body.Instructions[i].Operand;
                            obj = result;
                        }
                        else
                        {
                            bool flag4 = method.Body.Instructions[i].OpCode == OpCodes.Callvirt;
                            bool flag8 = !flag4;
                            if (flag8)
                            {
                                opCode = null;
                                result = null;
                                i++;
                                continue;
                            }
                            opCode = OpCodes.Callvirt;
                            result = method.Body.Instructions[i].Operand;
                            obj = result;
                        }
                    }
                }
                else
                {
                    obj = null;
                }
                return obj;
            }
            return result;
        }
        private static object result;
        private static void RemoveJunkMethods(ModuleDefMD module)
        {
            int num = 0;
            foreach (TypeDef typeDef in module.GetTypes())
            {
                List<MethodDef> list = new List<MethodDef>();
                foreach (MethodDef item in typeDef.Methods)
                {
                    bool flag = junkMethods.Contains(item);
                    bool flag2 = flag;
                    bool flag4 = flag2;
                    if (flag4)
                    {
                        list.Add(item);
                    }
                }
                int num2;
                for (int i = 0; i < list.Count; i = num2 + 1)
                {
                    typeDef.Methods.Remove(list[i]);
                    num2 = num;
                    num = num2 + 1;
                    num2 = i;
                }
                list.Clear();
            }

            junkMethods.Clear();
            bool flag3 = num > 0;
        }
        private static List<MethodDef> junkMethods = new List<MethodDef>();
        public static void Deobfuscate2(ModuleDef module)
        {
            foreach (TypeDef type in module.Types)
            {
                foreach (MethodDef method in type.Methods)
                {
                    bool flag = !method.HasBody;
                    if (!flag)
                    {
                        bool hasBody = method.HasBody;
                        if (hasBody)
                        {
                            bool flag2 = !method.Body.HasInstructions;
                            if (flag2)
                            {
                                continue;
                            }
                        }
                        CilBody body = method.Body;
                        for (int i = 0; i < body.ExceptionHandlers.Count; i++)
                        {
                            bool flag3 = body.ExceptionHandlers[i].HandlerType == ExceptionHandlerType.Finally;
                            if (flag3)
                            {
                                for (int j = 0; j < body.Instructions.Count; j++)
                                {
                                    bool flag4 = body.Instructions[j].OpCode == OpCodes.Calli && body.Instructions[j].Operand == null && body.Instructions[j + 1].OpCode == OpCodes.Sizeof;
                                    if (flag4)
                                    {
                                        body.ExceptionHandlers.RemoveAt(i);
                                        body.Instructions.RemoveAt(j);
                                        body.Instructions.RemoveAt(j + 1);
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
        public static void Deobfuscate(ModuleDefMD module)
        {
            for (int i = 0; i < module.Types.Count; i++)
            {
                TypeDef type = module.Types[i];
                bool hasInterfaces = type.HasInterfaces;
                if (hasInterfaces)
                {
                    for (int j = 0; j < type.Interfaces.Count; j++)
                    {
                        bool flag = type.Interfaces[j].Interface.Name.Contains(type.Name) || type.Name.Contains(type.Interfaces[j].Interface.Name);
                        if (flag)
                        {

                            module.Types.RemoveAt(i);
                        }
                    }
                }
            }
        }
    }
}
