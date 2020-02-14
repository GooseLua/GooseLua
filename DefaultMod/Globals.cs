using GooseLua.Lua;
using GooseShared;
using Microsoft.CSharp;
using MoonSharp.Interpreter;
using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace GooseLua {
    class _G {
        public static Script LuaState = new Script(CoreModules.Preset_SoftSandbox | CoreModules.Debug);
        public static GooseEntity goose;
        public static string path;
        public static Hook hook = new Hook();
        public static List<string> luaQueue = new List<string>();
        public static string ApiURL = "https://gooselua.my.to/";
        private static string SessionID = "";

        public static string GetSessionID() {
            if (string.IsNullOrEmpty(SessionID)) SessionID = RandomString();
            return SessionID;
        }

        public static string RandomString(int length = 16, string chars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890") {
            using (RNGCryptoServiceProvider crypto = new RNGCryptoServiceProvider()) {
                byte[] data = new byte[length];
                byte[] buffer = null;
                int maxRandom = byte.MaxValue - ((byte.MaxValue + 1) % chars.Length);
                crypto.GetBytes(data);
                char[] result = new char[length];
                for (int i = 0; i < length; i++) {
                    byte value = data[i];
                    while (value > maxRandom) {
                        if (buffer == null) {
                            buffer = new byte[1];
                        }
                        crypto.GetBytes(buffer);
                        value = buffer[0];
                    }
                    result[i] = chars[value % chars.Length];
                }
                return new string(result);
            }
        }

        public static void RunString(string code, string name = "RunString") {
            try {
                LuaState.DoString(code, LuaState.Globals, name);
            } catch (ScriptRuntimeException ex) {
                Util.MsgC(ModEntryPoint.form, Color.FromArgb(255, 0, 0), string.Format("[ERROR] {0}: {1}\r\n{2}", ex.Source, ex.DecoratedMessage, ex.StackTrace), "\r\n");
            }
        }

        public static DialogResult InputBox(ref string value, string title, string subtitle, string confirm = "OK", string cancel = "Cancel") {
            string code = "Form form = new Form();" +
            "Label label = new Label();" +
            "TextBox textBox = new TextBox();" +
            "Button buttonOk = new Button();" +
            "Button buttonCancel = new Button();" +
            "form.Text = \"" + title + "\";" +
            "label.Text = \"" + subtitle + "\";" +
            "textBox.Text = \"" + value + "\";" +
            "buttonOk.Text = \"" + confirm + "\";" +
            "buttonCancel.Text = \"" + cancel + "\";" +
            "buttonOk.DialogResult = DialogResult.OK;" +
            "buttonCancel.DialogResult = DialogResult.Cancel;" +
            "label.SetBounds(9, 20, 372, 13);" +
            "textBox.SetBounds(12, 56, 372, 20);" +
            "buttonOk.SetBounds(228, 100, 75, 23);" +
            "buttonCancel.SetBounds(309, 100, 75, 23);" +
            "label.AutoSize = true;" +
            "textBox.Anchor = textBox.Anchor | AnchorStyles.Right;" +
            "buttonOk.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;" +
            "buttonCancel.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;" +
            "form.ClientSize = new Size(396, 140);" +
            "form.Controls.AddRange(new Control[] { label, textBox, buttonOk, buttonCancel });" +
            "form.ClientSize = new Size(Math.Max(300, label.Right + 10), form.ClientSize.Height);" +
            "form.FormBorderStyle = FormBorderStyle.FixedDialog;" +
            "form.StartPosition = FormStartPosition.CenterScreen;" +
            "form.MinimizeBox = false;" +
            "form.MaximizeBox = false;" +
            "form.AcceptButton = buttonOk;" +
            "form.CancelButton = buttonCancel;" +
            "DialogResult dialogResult = form.ShowDialog();" +
            "Console.WriteLine(dialogResult == DialogResult.OK ? textBox.Text : \"Cancelled\");";
            string result = Execute(code);
            value = result;
            if (result == "Cancelled") return DialogResult.Cancel;
            if (!result.Contains("Error Number")) return DialogResult.OK;
            throw new Exception(result);
        }

        public static void MessageBox(string text, string title, string confirm = "OK") {
            string code = "Form form = new Form();" +
            "Label label = new Label();" +
            "Button buttonOk = new Button();" +
            "form.Text = \"" + title + "\";" +
            "label.Text = \"" + text+ "\";" +
            "buttonOk.Text = \"" + confirm + "\";" +
            "buttonOk.DialogResult = DialogResult.OK;" +
            "label.SetBounds(9, 20, 372, 13);" +
            "buttonOk.SetBounds(228, 100, 75, 23);" +
            "label.AutoSize = true;" +
            "buttonOk.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;" +
            "form.ClientSize = new Size(396, 140);" +
            "form.Controls.AddRange(new Control[] { label, buttonOk });" +
            "form.ClientSize = new Size(Math.Max(300, label.Right + 10), form.ClientSize.Height);" +
            "form.FormBorderStyle = FormBorderStyle.FixedDialog;" +
            "form.StartPosition = FormStartPosition.CenterScreen;" +
            "form.MinimizeBox = false;" +
            "form.MaximizeBox = false;" +
            "form.AcceptButton = buttonOk;" +
            "form.ShowDialog();";
            Execute(code);
        }

        public static string Execute(string code, string args = "") {
            Thread.CurrentThread.IsBackground = true;

            CodeDomProvider codeProvider = CodeDomProvider.CreateProvider("CSharp");
            string Out = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());

            string n = RandomString(8);
            code = "using System;using System.Windows.Forms;using System.Drawing;namespace "+n+" {class Program{static void Main(string[] args){" + code + "}}}";

            CompilerParameters parameters = new CompilerParameters();
            parameters.GenerateExecutable = true;
            parameters.OutputAssembly = Out;
            parameters.ReferencedAssemblies.Add("System.dll");
            parameters.ReferencedAssemblies.Add("System.Windows.dll");
            parameters.ReferencedAssemblies.Add("System.Windows.Forms.dll");
            parameters.ReferencedAssemblies.Add("System.Drawing.dll");
            parameters.ReferencedAssemblies.Add("mscorlib.dll");

            CompilerResults results = codeProvider.CompileAssemblyFromSource(parameters, code);

            if (results.Errors.Count == 0) {
                var proc = new Process {
                    StartInfo = new ProcessStartInfo {
                        FileName = Out,
                        Arguments = args,
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        CreateNoWindow = true
                    }
                };

                StringBuilder Output = new StringBuilder();
                proc.Start();
                while (!proc.StandardOutput.EndOfStream) {
                    Output.AppendLine(proc.StandardOutput.ReadLine());
                }
                File.Delete(Out);
                return Output.ToString();
            } else {
                StringBuilder Output = new StringBuilder();
                foreach (CompilerError CompErr in results.Errors) Output.AppendLine("Line number " + CompErr.Line + ", Error Number: " + CompErr.ErrorNumber + ", '" + CompErr.ErrorText + ";" + Environment.NewLine);
                return Output.ToString();
            }
        }
    }
}
