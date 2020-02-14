using GooseShared;
using MoonSharp.Interpreter;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;

namespace GooseLua {
    public class ModEntryPoint : IMod {
        public static formLoader form = new formLoader();
        Graphics graphics;

        [DllImport("user32.dll")]
        public static extern short GetAsyncKeyState(Keys vKey);

        void IMod.Init() {
            _G.path = Path.GetFullPath(Path.Combine(API.Helper.getModDirectory(this), "Mods"));

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
                        graphics.DrawString(text, new Font(font, 8f), Brushes.White, new PointF(x, y));
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

            _G.LuaState.Globals["draw"] = draw;

            Table surface = new Table(_G.LuaState);

            Color drawColor = Color.FromArgb(255, 255, 255);

            surface["SetDrawColor"] = new CallbackFunction((ScriptExecutionContext context, CallbackArguments arguments) => {
                try {
                    if (graphics == default(Graphics)) throw new ScriptRuntimeException("Graphics not initialized or invalid hook.");
                    if (arguments.Count < 3) throw new ScriptRuntimeException("surface.SetDrawColor requires 3 arguments.");
                    int r = arguments.AsInt(0, "surface.SetDrawColor");
                    int g = arguments.AsInt(1, "surface.SetDrawColor");
                    int b = arguments.AsInt(2, "surface.SetDrawColor");
                    int a = arguments.Count == 4 ? arguments.AsInt(3, "surface.SetDrawColor") : 255;

                    Util.Clamp(ref r, 1, 255);
                    Util.Clamp(ref g, 1, 255);
                    Util.Clamp(ref b, 1, 255);
                    Util.Clamp(ref a, 1, 255);
                    
                    try {
                        drawColor = Color.FromArgb(r, g, b, a);
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

            Util.include("math");
            Util.include("string");
            Util.include("table");
            Util.include("bit");
            Util.include("color");
            Util.include("concommand");
            Util.include("defaultcmds");

            KeyEnums.Load();

            InjectionPoints.PreTickEvent += preTick;
            InjectionPoints.PostTickEvent += postTick;
            InjectionPoints.PreRenderEvent += preRender;
            InjectionPoints.PostRenderEvent += postRender;
            InjectionPoints.PreUpdateRigEvent += preRig;
            InjectionPoints.PostUpdateRigEvent += postRig;

            InjectionPoints.PreRenderEvent += updateGoose;

            new Thread(() => {
                form = new formLoader();
                form.ShowDialog();
            }).Start();

            System.Windows.Forms.Timer timer = new System.Windows.Forms.Timer();
            timer.Tick += timerTick;
            timer.Interval = 1;
            timer.Enabled = true;
        }

        public void timerTick(object s, EventArgs e) {
            if (_G.luaQueue.Count > 0) {
                _G.LuaState.DoString(_G.luaQueue[0]);
                _G.luaQueue.RemoveAt(0);
            }
        }

        public void updateGoose(GooseEntity g, dynamic _) {
            _G.goose = g;
            Table goose = new Table(_G.LuaState);
            Table position = new Table(_G.LuaState);
            position["x"] = g.position.x;
            position["y"] = g.position.y;
            goose["position"] = position;
            goose["setTarget"] = new CallbackFunction((ScriptExecutionContext context, CallbackArguments arguments) => {
                int x = arguments.Count > 0 ? arguments.AsInt(0, "Goose.setTarget") : 0;
                int y = arguments.Count > 1 ? arguments.AsInt(1, "Goose.setTarget") : 0;
                g.targetPos = new SamEngine.Vector2(x, y);
                return DynValue.Nil;
            });
            goose["setPosition"] = new CallbackFunction((ScriptExecutionContext context, CallbackArguments arguments) => {
                int x = arguments.Count > 0 ? arguments.AsInt(0, "Goose.setPosition") : 0;
                int y = arguments.Count > 1 ? arguments.AsInt(1, "Goose.setPosition") : 0;
                g.position = new SamEngine.Vector2(x, y);
                return DynValue.Nil;
            });
            _G.LuaState.Globals["Goose"] = goose;

            _G.LuaState.Globals["_G"] = _G.LuaState.Globals;
        }

        public void callHooks(string hook) {
            foreach (Closure func in _G.hook.hooks[hook].Values) {
                func.Call();
            }
        }

        public void preTick(GooseEntity g) {
            callHooks("preTick");
        }

        public void postTick(GooseEntity g) {
            callHooks("postTick");
        }

        public void preRender(GooseEntity g, Graphics e) {
            e.PixelOffsetMode = PixelOffsetMode.HighSpeed;
            graphics = e;
            callHooks("preRender");
        }

        public void postRender(GooseEntity g, Graphics e) {
            e.PixelOffsetMode = PixelOffsetMode.HighSpeed;
            graphics = e;
            callHooks("postRender");
        }

        public void preRig(GooseEntity g) {
            callHooks("preRig");
        }

        public void postRig(GooseEntity g) {
            callHooks("postRig");
        }
    }
}