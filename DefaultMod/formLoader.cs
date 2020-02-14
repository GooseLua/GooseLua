using MoonSharp.Interpreter;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Net;
using System.Text;
using System.Windows.Forms;

namespace GooseLua {
    public partial class formLoader : MetroFramework.Forms.MetroForm {
        public formLoader() {
            InitializeComponent();
            Analytics.StartSession();
            Util.MsgC(this, Script.GetBanner("Goose Lua"));
        }

        private void formLoader_Load(object sender, EventArgs e) {
            if (!Directory.Exists(_G.path)) Directory.CreateDirectory(_G.path);
            _G.LuaState.Globals["MsgC"] = new CallbackFunction((ScriptExecutionContext context, CallbackArguments arguments) => {
                List<dynamic> args = new List<dynamic>();
                for (int i = 0; i < arguments.Count; i++) {
                    DynValue arg = arguments.RawGet(i, true);
                    if (arg == DynValue.Nil) continue;
                    if (arg.Table.MetaTable["Color"] != null) {
                        int r = (int)((Table)arg.Table.MetaTable["Color"])["r"];
                        int g = (int)((Table)arg.Table.MetaTable["Color"])["g"];
                        int b = (int)((Table)arg.Table.MetaTable["Color"])["b"];
                        int a = (int)((Table)arg.Table.MetaTable["Color"])["a"];
                        args.Add(Color.FromArgb(r, g, b, a));
                    }
                    args.Add(arg);
                }
                Util.MsgC(this, args);
                return DynValue.Nil;
            });

            _G.LuaState.Globals["Msg"] = new CallbackFunction((ScriptExecutionContext context, CallbackArguments arguments) => {
                List<dynamic> args = new List<dynamic>();
                for (int i = 0; i < arguments.Count; i++) {
                    DynValue arg = arguments.RawGet(i, true);
                    if (arg == DynValue.Nil) continue;
                    args.Add(arg);
                }
                Util.MsgC(this, args + "\r\n");
                return DynValue.Nil;
            });

            _G.LuaState.Options.DebugPrint = s => {
                console.AppendText(s + "\r\n");
            };

            string[] files = Directory.GetFiles(_G.path, "*.lua");
            foreach (string mod in files) {
                string modFile = Path.GetFileName(mod);
                int len = modFile.Length;
                modList.Items.Add(modFile.Substring(0, len - 4));
                try {
                    _G.luaQueue.Add(File.ReadAllText(mod));
                } catch (ScriptRuntimeException ex) {
                    Util.MsgC(this, Color.FromArgb(255, 0, 0), string.Format("Doh! An error occured! {0}", ex.DecoratedMessage), "\r\n");
                }
            }
        }

        private void formLoader_FormClosing(object sender, FormClosingEventArgs e) {
            e.Cancel = e.CloseReason == CloseReason.UserClosing;
            if(!e.Cancel) {
                Analytics.EndSession();
            } if (e.CloseReason != CloseReason.UserClosing || MessageBox.Show("Are you sure you want to exit?", "Desktop Goose", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes) {
                Application.Exit();
            }
        }

        private void metroTextBox1_KeyDown(object sender, KeyEventArgs e) {
            if(e.KeyCode == Keys.Enter) {
                string safe = metroTextBox1.Text.Replace("\"", "\\\"");
                List<string> args = new List<string>(safe.Split(' '));
                safe = args[0];
                args.RemoveAt(0);
                StringBuilder argstr = new StringBuilder("{");
                foreach(string arg in args) {
                    argstr.Append('"' + arg + '"' + ", ");
                }
                string argstable = argstr.ToString();
                int len = argstable.Length;
                if (args.Count > 0) argstable = argstable.Substring(0, len - 2);
                argstable += '}';
                _G.luaQueue.Add("print(\"] " + safe + " " + string.Join(" ", args) + "\") concommand.Run(\"" + safe + "\", " + argstable + ")");
                metroTextBox1.Clear();
            }
        }

        public RichTextBox console {
            get {
                return _console;
            }
        }

        private void timer1_Tick(object sender, EventArgs e) {
            string users = "0";
            try {
                using(WebClient wc = new WebClient()) {
                    users = wc.DownloadString(_G.ApiURL + "users");
                }
            } catch {
                users = "API Seems to be down.";
            }
            DateTime date = DateTime.Now;
            string ap = "A";
            int twelveHour = date.Hour;
            if(twelveHour > 12) { ap = "P"; twelveHour -= 12; }
            string h = twelveHour.ToString().PadLeft(2, '0');
            string m = date.Minute.ToString().PadLeft(2, '0');
            string time = $"{h}:{m} {ap}M";
            metroLabel1.Text = string.Format("[{0}] Active Users: {1}", time, users);
        }
    }
}
