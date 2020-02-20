using GooseShared;
using MoonSharp.Interpreter;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace GooseLua {
    public partial class formLoader : MetroFramework.Forms.MetroForm {
        AutoCompleteStringCollection commands = new AutoCompleteStringCollection();

        public formLoader() {
            InitializeComponent();
            Analytics.StartSession();
            Util.MsgC(this, Script.GetBanner("Goose Lua"));
            label1.Text = string.Format(label1.Text, Script.LUA_VERSION, Script.VERSION);
        }

        public void queue(string code, string name = null) {
            _G.luaQueue.Enqueue(new KeyValuePair<string, string>(code, name));
        }

        private void formLoader_Load(object sender, EventArgs e) {
            if (!Directory.Exists(_G.path)) Directory.CreateDirectory(_G.path);
            _G.LuaState.Globals["MsgC"] = new CallbackFunction((ScriptExecutionContext context, CallbackArguments arguments) => {
                List<dynamic> args = new List<dynamic>();
                Closure isColor = (Closure)context.CurrentGlobalEnv["IsColor"];
                for (int i = 0; i < arguments.Count; i++) {
                    DynValue arg = arguments.RawGet(i, true);
                    if (arg == DynValue.Nil) continue;
                    if (arg.Type == DataType.String)
                    {
                        args.Add(arg.String);
                    } else if (isColor.Call(arg).CastToBool())
                    {
                        Table color = arg.Table;
                        int r = (int)color.Get("r").Number;
                        int g = (int)color.Get("g").Number;
                        int b = (int)color.Get("b").Number;
                        int a = (int)color.Get("a").Number;
                        args.Add(Color.FromArgb(a, r, g, b));
                    } else
                    {
                        throw ScriptRuntimeException.BadArgument(i, "MsgC", "expected color or string");
                    }
                    
                }
                Util.MsgC(this, args.ToArray());
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

            for (int i = 0; i < 10; i++) {
                _G.LuaState.Globals[$"clear_{_G.RandomString()}"] = new CallbackFunction((ScriptExecutionContext context, CallbackArguments arguments) => {
                    throw new ScriptRuntimeException("Attempted to clear console.");
                });
            }

            _G.LuaState.Globals[$"clear_{_G.GetSessionID()}"] = new CallbackFunction((ScriptExecutionContext context, CallbackArguments arguments) => {
                console.Clear();
                Util.MsgC(this, Script.GetBanner("Goose Lua"));
                return DynValue.Nil;
            });

            for (int i = 0; i < 10; i++) {
                _G.LuaState.Globals[$"clear_{_G.RandomString()}"] = new CallbackFunction((ScriptExecutionContext context, CallbackArguments arguments) => {
                    throw new ScriptRuntimeException("Attempted to clear console.");
                });
            }

            queue($"concommand.Add(\"clear\", function() clear_{_G.GetSessionID()}() end)", "formLoader");

            _G.LuaState.Globals["AddConsoleCommand"] = new CallbackFunction((ScriptExecutionContext context, CallbackArguments arguments) => {
                string name = arguments.AsStringUsingMeta(context, 0, "AddConsoleCommand");
                commands.Add(name);
                return DynValue.Nil;
            });

            commands.AddRange(new string[] {
                "help",
                "lua_run_cl"
            });

            string[] files = Directory.GetFiles(_G.path, "*.lua");
            foreach (string mod in files) {
                string modFile = Path.GetFileNameWithoutExtension(mod);
                modList.Items.Add(modFile);
                try {
                    string code = File.ReadAllText(mod);
                    queue(code, modFile);
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
            metroTextBox1.AutoCompleteCustomSource = commands;

            if (e.KeyCode == Keys.Enter) {
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
                string code = "print(\"] " + safe + " " + string.Join(" ", args) + "\") concommand.Run(\"" + safe + "\", " + argstable + ", \"" + string.Join(" ", args) + "\")";
                queue(code, "console");
                metroTextBox1.Clear();
            }
        }

        public RichTextBox console {
            get {
                return _console;
            }
        }

        private async void timer1_Tick(object sender, EventArgs e) {
            string users = "0";
            try {
                using(WebClient wc = new WebClient()) {
                    users = await wc.DownloadStringTaskAsync(_G.ApiURL + "users");
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

        private void metroLabel2_Click(object sender, EventArgs e) {
            API.Goose.playHonckSound();
        }

        private void metroButton1_Click(object sender, EventArgs e) {
            Process.Start(_G.ApiURL);
        }

        private void metroButton2_Click(object sender, EventArgs e) {
            Process.Start(_G.ApiURL + "/discord");
        }

        private void metroButton3_Click(object sender, EventArgs e) {
            queue(metroTextBox2.Text, "Lua Editor");
        }
    }
}
