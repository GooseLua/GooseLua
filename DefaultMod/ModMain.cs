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
        Graphics graphics;

        [DllImport("user32.dll")]
        public static extern short GetAsyncKeyState(Keys vKey);

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

            Table draw = new Table(_G.LuaState);

            draw["SimpleText"] = new CallbackFunction((ScriptExecutionContext context, CallbackArguments arguments) => {
                try {
                    if (graphics == default(Graphics)) throw new ScriptRuntimeException("Graphics not initialized or invalid _G.hook.");
                    string text = arguments.Count > 0 ? arguments.AsStringUsingMeta(context, 0, "draw.SimpleText") : "Text";
                    string font = arguments.Count > 1 ? arguments.AsStringUsingMeta(context, 1, "draw.SimpleText") : "Courier New";
                    int x = arguments.Count > 2 ? arguments.AsInt(2, "draw.SimpleText") : 0;
                    int y = arguments.Count > 3 ? arguments.AsInt(3, "draw.SimpleText") : 0;
                    try {
                        graphics.DrawString(text, new Font(font, 12f), Brushes.White, new PointF(x, y));
                    } catch (Exception ex) {
                        return DynValue.NewString(ex.Message);
                    }
                    return DynValue.Nil;
                } catch (ScriptRuntimeException ex) {
                    Util.MsgC(form, Color.FromArgb(255, 0, 0), ex.Message);
                } catch (Exception ex) {
                    return DynValue.NewString(ex.ToString());
                }
                return DynValue.Nil;
            });

            draw["GetFontHeight"] = new CallbackFunction((ScriptExecutionContext context, CallbackArguments arguments) => {
                try {
                    if (graphics == default(Graphics)) throw new ScriptRuntimeException("Graphics not initialized or invalid _G.hook.");
                    string font = arguments.AsStringUsingMeta(context, 0, "draw.SimpleText");
                    try {
                        return DynValue.NewNumber(graphics.MeasureString("A", new Font(font, 12f)).Height);
                    } catch (Exception ex) {
                        return DynValue.NewString(ex.Message);
                    }
                } catch (ScriptRuntimeException ex) {
                    Util.MsgC(form, Color.FromArgb(255, 0, 0), ex.Message);
                } catch (Exception ex) {
                    return DynValue.NewString(ex.ToString());
                }
                return DynValue.Nil;
            });

            draw["SimpleTextOutlined"] = (CallbackFunction)draw["SimpleText"];

            _G.LuaState.Globals["draw"] = draw;

            Table surface = new Table(_G.LuaState);

            Color drawColor = Color.FromArgb(255, 255, 255);

            surface["SetDrawColor"] = new CallbackFunction((ScriptExecutionContext context, CallbackArguments arguments) => {
                try {
                    if (graphics == default(Graphics)) throw new ScriptRuntimeException("Graphics not initialized or invalid hook.");
                    if (arguments.Count < 3) throw new ScriptRuntimeException("surface.SetDrawColor requires 3 arguments.");
                    if (arguments.Count > 3) throw new ScriptRuntimeException("surface.SetDrawColor requires 3 arguments.");
                    int r = arguments.AsInt(0, "surface.SetDrawColor");
                    int g = arguments.AsInt(1, "surface.SetDrawColor");
                    int b = arguments.AsInt(2, "surface.SetDrawColor");

                    Util.Clamp(ref r, 1, 255);
                    Util.Clamp(ref g, 1, 255);
                    Util.Clamp(ref b, 1, 255);
                    
                    try {
                        drawColor = Color.FromArgb(r, g, b);
                    } catch (ScriptRuntimeException ex) {
                        Util.MsgC(form, Color.FromArgb(255, 0, 0), ex.Message);
                    } catch (Exception ex) {
                        return DynValue.NewString(ex.ToString());
                    }
                    return DynValue.Nil;
                } catch (Exception ex) {
                    return DynValue.NewString(ex.ToString());
                }
            });

            surface["DrawLine"] = new CallbackFunction((ScriptExecutionContext context, CallbackArguments arguments) => {
                try {
                    if (graphics == default(Graphics)) throw new ScriptRuntimeException("Graphics not initialized or invalid hook.");
                    if (arguments.Count != 4) throw new ScriptRuntimeException("surface.DrawLine requires 4 arguments.");
                    int sx = arguments.AsInt(0, "surface.DrawLine");
                    int sy = arguments.AsInt(1, "surface.DrawLine");
                    int fx = arguments.AsInt(2, "surface.DrawLine");
                    int fy = arguments.AsInt(3, "surface.DrawLine");
                    try {
                        graphics.DrawLine(new Pen(new SolidBrush(drawColor), 1f), sx, sy, fx, fy);
                    } catch (Exception ex) {
                        return DynValue.NewString(ex.ToString());
                    }
                    return DynValue.Nil;
                } catch (ScriptRuntimeException ex) {
                    Util.MsgC(form, Color.FromArgb(255, 0, 0), ex.Message);
                } catch (Exception ex) {
                    return DynValue.NewString(ex.ToString());
                }
                return DynValue.Nil;
            });

            surface["DrawRect"] = new CallbackFunction((ScriptExecutionContext context, CallbackArguments arguments) => {
                try {
                    if (graphics == default(Graphics)) throw new ScriptRuntimeException("Graphics not initialized or invalid hook.");
                    if (arguments.Count != 4) throw new ScriptRuntimeException("surface.DrawRect requires 4 arguments.");
                    int x = arguments.AsInt(0, "surface.DrawRect");
                    int y = arguments.AsInt(1, "surface.DrawRect");
                    int h = arguments.AsInt(2, "surface.DrawRect");
                    int w = arguments.AsInt(3, "surface.DrawRect");
                    try {
                        graphics.FillRectangle(new SolidBrush(drawColor), new Rectangle(x, y, h, w));
                    } catch (Exception ex) {
                        return DynValue.NewString(ex.ToString());
                    }
                    return DynValue.Nil;
                } catch (ScriptRuntimeException ex) {
                    Util.MsgC(form, Color.FromArgb(255, 0, 0), ex.Message);
                } catch (Exception ex) {
                    return DynValue.NewString(ex.ToString());
                }
                return DynValue.Nil;
            });

            _G.LuaState.Globals["surface"] = surface;

            Table hook = new Table(_G.LuaState);

            hook["Add"] = new CallbackFunction((ScriptExecutionContext context, CallbackArguments arguments) => {
                try {
                    _G.hook.Add(arguments.AsStringUsingMeta(context, 0, "hook.Add"), arguments.AsStringUsingMeta(context, 1, "hook.Add"), arguments[2].Function);
                    return DynValue.Nil;
                } catch {
                    return DynValue.False;
                }
            });

            hook["Remove"] = new CallbackFunction((ScriptExecutionContext context, CallbackArguments arguments) => {
                try {
                    _G.hook.Remove(arguments.AsStringUsingMeta(context, 0, "hook.Add"), arguments.AsStringUsingMeta(context, 1, "hook.Add"));
                    return DynValue.Nil;
                } catch {
                    return DynValue.False;
                }
            });

            _G.LuaState.Globals["hook"] = hook;

            Table input = new Table(_G.LuaState);

            input["IsKeyDown"] = new CallbackFunction((ScriptExecutionContext context, CallbackArguments arguments) => {
                int key = arguments.AsInt(0, "input.IsKeyDown");
                return DynValue.NewBoolean(GetAsyncKeyState((Keys)key) != 0);
            });

            input["GetKeyName"] = new CallbackFunction((ScriptExecutionContext context, CallbackArguments arguments) => {
                int key = arguments.AsInt(0, "input.GetKeyName");
                return DynValue.NewString(((Keys)key).ToString());
            });

            _G.LuaState.Globals["input"] = input;

            _G.LuaState.Globals["Msg"] = _G.LuaState.Globals["print"];

            _G.LuaState.Globals["AddConsoleCommand"] = new CallbackFunction((ScriptExecutionContext context, CallbackArguments arguments) => {
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

            InjectionPoints.PreTickEvent += preTick;
            InjectionPoints.PostTickEvent += postTick;
            InjectionPoints.PreRenderEvent += preRender;
            InjectionPoints.PostRenderEvent += postRender;
            InjectionPoints.PreUpdateRigEvent += preRig;
            InjectionPoints.PostUpdateRigEvent += postRig;

            Thread thread = new Thread(() => {
                form = new formLoader();
                form.ShowDialog();
            });
            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
        }

        public void callHooks(string hook) {
            foreach (Closure func in _G.hook.hooks[hook].Values) {
                try {
                    func.Call();
                } catch(ScriptRuntimeException ex) {
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
            e.PixelOffsetMode = PixelOffsetMode.HighSpeed;
            graphics = e;
            callHooks("preRender");
        }

        public void postRender(GooseEntity g, Graphics e) {
            _G.goose = g;
            e.PixelOffsetMode = PixelOffsetMode.HighSpeed;
            graphics = e;
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