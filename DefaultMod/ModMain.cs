using GooseLua.Lua;
using GooseShared;
using MoonSharp.Interpreter;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Threading;

namespace GooseLua {
    public class ModEntryPoint : IMod {
        Graphics graphics;

        void IMod.Init() {
            _G.path = Path.GetFullPath(Path.Combine(API.Helper.getModDirectory(this), "Mods"));

            _G.hook.hooks["preRig"] = new Dictionary<string, Closure>();
            _G.hook.hooks["postRig"] = new Dictionary<string, Closure>();
            _G.hook.hooks["preTick"] = new Dictionary<string, Closure>();
            _G.hook.hooks["postTick"] = new Dictionary<string, Closure>();
            _G.hook.hooks["preRender"] = new Dictionary<string, Closure>();
            _G.hook.hooks["postRender"] = new Dictionary<string, Closure>();

            Table draw = new Table(_G.LuaState);

            draw["SimpleText"] = new CallbackFunction((ScriptExecutionContext context, CallbackArguments arguments) => {
                try {
                    if (graphics == default(Graphics)) throw new ScriptRuntimeException("Graphics not initialized or invalid _G.hook.");
                    string text = arguments.Count >= 0 ? arguments.AsStringUsingMeta(context, 0, "draw.SimpleText") : "Text";
                    string font = arguments.Count > 0 ? arguments.AsStringUsingMeta(context, 1, "draw.SimpleText") : "Courier New";
                    int x = arguments.Count > 1 ? arguments.AsInt(2, "draw.SimpleText") : 0;
                    int y = arguments.Count > 2 ? arguments.AsInt(3, "draw.SimpleText") : 0;
                    try {
                        graphics.DrawString(text, new Font(font, 8f), Brushes.White, new PointF(x, y));
                    } catch (Exception ex) {
                        return DynValue.NewString(ex.ToString());
                    }
                    return DynValue.Nil;
                } catch (Exception ex) {
                    return DynValue.NewString(ex.ToString());
                }
            });

            _G.LuaState.Globals["draw"] = draw;

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

            _G.LuaState.Globals["_G"] = _G.LuaState.Globals;

            new Thread(new ThreadStart(() => new formLoader().ShowDialog())).Start();

            InjectionPoints.PreTickEvent += preTick;
            InjectionPoints.PostTickEvent += postTick;
            InjectionPoints.PreRenderEvent += preRender;
            InjectionPoints.PostRenderEvent += postRender;
            InjectionPoints.PreUpdateRigEvent += preRig;
            InjectionPoints.PostUpdateRigEvent += postRig;

            InjectionPoints.PreRenderEvent += updateGoose;
        }

        public void updateGoose(GooseEntity g, dynamic _) {
            _G.goose = g;
            Table goose = new Table(_G.LuaState);
            Table position = new Table(_G.LuaState);
            position["x"] = g.position.x;
            position["y"] = g.position.y;
            goose["position"] = position;
            goose["setTarget"] = new CallbackFunction((ScriptExecutionContext context, CallbackArguments arguments) => {
                int x = arguments.Count > 0 ? arguments.AsInt(0, "goose.setTarget") : 0;
                int y = arguments.Count > 1 ? arguments.AsInt(1, "goose.setTarget") : 0;
                g.targetPos = new SamEngine.Vector2(x, y);
                return DynValue.Nil;
            });
            goose["setPosition"] = new CallbackFunction((ScriptExecutionContext context, CallbackArguments arguments) => {
                int x = arguments.Count > 0 ? arguments.AsInt(0, "goose.setTarget") : 0;
                int y = arguments.Count > 1 ? arguments.AsInt(1, "goose.setTarget") : 0;
                g.position = new SamEngine.Vector2(x, y);
                return DynValue.Nil;
            });
            _G.LuaState.Globals["Goose"] = goose;
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