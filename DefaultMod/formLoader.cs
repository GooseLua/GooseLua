using MoonSharp.Interpreter;
using System;
using System.IO;
using System.Windows.Forms;

namespace GooseLua {
    public partial class formLoader : MetroFramework.Forms.MetroForm {
        public formLoader() {
            InitializeComponent();
        }

        private void formLoader_Load(object sender, EventArgs e) {
            if (!Directory.Exists(_G.path)) Directory.CreateDirectory(_G.path);

            _G.LuaState.Globals["print"] = new CallbackFunction((ScriptExecutionContext context, CallbackArguments arguments) => {
                for(int i = 0;i<arguments.Count;i++) {
                    richTextBox1.Text += arguments.AsStringUsingMeta(context, i, "print");
                    if (i + 1 < arguments.Count) richTextBox1.Text += "   ";
                }
                richTextBox1.AppendText("\r\n");
                return DynValue.Nil;
            });

            string[] files = Directory.GetFiles(_G.path, "*.lua");
            foreach (string mod in files) {
                modList.Items.Add(mod);
                _G.LuaState.DoFile(mod);
            }
        }

        private void formLoader_FormClosing(object sender, FormClosingEventArgs e) {
            e.Cancel = e.CloseReason == CloseReason.UserClosing;
            if (e.CloseReason == CloseReason.UserClosing && MessageBox.Show("Are you sure you want to exit?", "Desktop Goose", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes) {
                Application.Exit();
            }
        }
    }
}
