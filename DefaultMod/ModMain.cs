using GooseShared;
using MoonSharp.Interpreter;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Net;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;

namespace GooseLua {
    public class ModEntryPoint : IMod {
        public static formLoader form;

        void IMod.Init() {
            _G.path = Path.GetFullPath(Path.Combine(API.Helper.getModDirectory(this), "..", "..", "Lua Mods"));

            _G.hook.hooks["preRig"] = new Dictionary<string, Closure>();
            _G.hook.hooks["postRig"] = new Dictionary<string, Closure>();
            _G.hook.hooks["preTick"] = new Dictionary<string, Closure>();
            _G.hook.hooks["postTick"] = new Dictionary<string, Closure>();
            _G.hook.hooks["preRender"] = new Dictionary<string, Closure>();
            _G.hook.hooks["postRender"] = new Dictionary<string, Closure>();

            _G.LuaState.Globals["ScrW"] = new CallbackFunction((ScriptExecutionContext context, CallbackArguments arguments) => {
                return DynValue.NewNumber(Screen.PrimaryScreen.Bounds.Width);
            });

            _G.LuaState.Globals["ScrH"] = new CallbackFunction((ScriptExecutionContext context, CallbackArguments arguments) => {
                return DynValue.NewNumber(Screen.PrimaryScreen.Bounds.Height);
            });

            UserData.RegisterAssembly();
            Lua.Enums.Register(_G.LuaState);

            _G.LuaState.Globals["draw"] = new Lua.Draw(_G.LuaState);
            _G.LuaState.Globals["surface"] = new Lua.Surface(_G.LuaState);
            _G.LuaState.Globals["hook"] = _G.hook;
            _G.LuaState.Globals["input"] = new Lua.Input(_G.LuaState);
            _G.LuaState.Globals["Msg"] = _G.LuaState.Globals["print"];

            _G.LuaState.Globals["AddConsoleCommand"] = new CallbackFunction((ScriptExecutionContext context, CallbackArguments arguments) => {
                string name = arguments.AsStringUsingMeta(context, 0, "AddConsoleCommand");
                Util.addCommand(form, name);
                return DynValue.Nil;
            });

            _G.LuaState.Globals["Derma_StringRequest"] = new CallbackFunction((ScriptExecutionContext context, CallbackArguments arguments) => {
                string title = arguments.AsStringUsingMeta(context, 0, "Derma_StringRequest");
                string subtitle = arguments.AsStringUsingMeta(context, 1, "Derma_StringRequest");
                string value = arguments.AsStringUsingMeta(context, 2, "Derma_StringRequest");
                Closure confirm = arguments[3].Function;
                Closure cancel = arguments.Count > 4 ? arguments[4].Function : default(Closure);
                string confirmText = arguments.Count > 5 ? arguments.AsStringUsingMeta(context, 5, "Derma_StringRequest") : "OK";
                string cancelText = arguments.Count > 6 ? arguments.AsStringUsingMeta(context, 6, "Derma_StringRequest") : "Cancel";
                DialogResult res = _G.InputBox(ref value, title, subtitle, confirmText, cancelText);
                if (res == DialogResult.Cancel) {
                    if (cancel != default(Closure)) {
                        cancel.Call();
                    }
                } else if (res == DialogResult.OK) {
                    confirm.Call(DynValue.NewString(value));
                }
                return DynValue.Nil;
            });

            _G.LuaState.Globals["Derma_Message"] = new CallbackFunction((ScriptExecutionContext context, CallbackArguments arguments) => {
                string text = arguments.AsStringUsingMeta(context, 0, "Derma_Message");
                string title = arguments.AsStringUsingMeta(context, 1, "Derma_Message");
                string confirm = arguments.AsStringUsingMeta(context, 2, "Derma_Message");
                _G.MessageBox(text, title, confirm);
                return DynValue.Nil;
            });

            _G.LuaState.Globals["HTTP"] = new CallbackFunction((ScriptExecutionContext context, CallbackArguments arguments) => {
                Table args = arguments[0].Table;
                if (args["url"] == null || args["method"] == null) throw new ScriptRuntimeException("Invalid request table.");

                Action _ = async() => {
                    string result = "";

                    if ((string)args["method"] == "POST") {
                        using (WebClient wc = new WebClient()) {
                            wc.Headers[HttpRequestHeader.ContentType] = "application/x-www-form-urlencoded";
                            result = await wc.DownloadStringTaskAsync($"{_G.ApiURL}analytics");
                        }
                    } else if ((string)args["method"] == "GET") {
                        using (WebClient wc = new WebClient()) {
                            result = await wc.DownloadStringTaskAsync((string) args["url"]);
                        }
                    } else {
                        throw new ScriptRuntimeException($"Unsupported HTTP protocol \"{args["method"]}\".");
                    }

                    if(result.Trim() != "") {
                        ((Closure)args["success"]).Call(result);
                    }
                };

                _();

                return DynValue.Nil;
            });

            form = new formLoader();
            Util.include("http");
            Util.include("math");
            Util.include("string");
            Util.include("table");
            Util.include("bit");
            Util.include("color");
            Util.include("concommand");
            Util.include("defaultcmds");

            KeyEnums.Load();

            GooseProxy.Register();
            _G.LuaState.Globals["goose"] = new GooseProxy(_G.LuaState);
            _G.LuaState.Globals["GetModDirectory"] = new CallbackFunction((ScriptExecutionContext context, CallbackArguments arguments) => {
                return DynValue.NewString(_G.path);
            });

            _G.LuaState.Globals["RegisterTask"] = CallbackFunction.FromMethodInfo(_G.LuaState, typeof(Task).GetMethod("Register"));

            InjectionPoints.PreTickEvent += preTick;
            InjectionPoints.PostTickEvent += postTick;
            InjectionPoints.PreRenderEvent += preRender;
            InjectionPoints.PostRenderEvent += postRender;
            InjectionPoints.PreUpdateRigEvent += preRig;
            InjectionPoints.PostUpdateRigEvent += postRig;

            Thread thread = new Thread(() => {
                form.ShowDialog();
            });
            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();

            System.Windows.Forms.Timer timer = new System.Windows.Forms.Timer();
            timer.Tick += luaQueue;
            timer.Interval = 1;
            timer.Start();
        }

        public static void luaQueue(object sender, EventArgs args) {
            if (_G.luaQueue.Count > 0) {
                KeyValuePair<string, string> lua = _G.luaQueue.Dequeue();
                _G.RunString(lua.Key, lua.Value);
            }
        }

        public void callHooks(string hook) {
            foreach (Closure func in _G.hook.hooks[hook].Values) {
                try {
                    func.Call();
                } catch(InterpreterException ex) {
                    Util.MsgC(form, Color.FromArgb(255, 0, 0), string.Format("[ERROR] {0}: {1}\r\n{2}", ex.Source, ex.DecoratedMessage, ex.StackTrace), "\r\n");
                } catch(Exception ex) {
                    MessageBox.Show(ex.ToString(), ex.Message, MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        public void preTick(GooseEntity g) {
            _G.goose = g;
            callHooks("preTick");
        }

        public void postTick(GooseEntity g) {
            _G.goose = g;
            callHooks("postTick");
        }

        public void preRender(GooseEntity g, Graphics e) {
            _G.goose = g;
            _G.graphics = e;
            e.PixelOffsetMode = PixelOffsetMode.HighSpeed;
            callHooks("preRender");
        }

        public void postRender(GooseEntity g, Graphics e) {
            _G.goose = g;
            _G.graphics = e;
            e.PixelOffsetMode = PixelOffsetMode.HighSpeed;
            callHooks("postRender");
        }

        public void preRig(GooseEntity g) {
            _G.goose = g;
            callHooks("preRig");
        }

        public void postRig(GooseEntity g) {
            _G.goose = g;
            callHooks("postRig");
        }
    }
}
