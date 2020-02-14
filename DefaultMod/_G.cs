using GooseLua.Lua;
using GooseShared;
using MoonSharp.Interpreter;
using System.Collections.Generic;
using System.Drawing;

namespace GooseLua {
    class _G {
        public static Script LuaState = new Script(CoreModules.Preset_SoftSandbox | CoreModules.Debug);
        public static GooseEntity goose;
        public static string path;
        public static Hook hook = new Hook();
        public static List<string> luaQueue = new List<string>();

        public static void RunString(string code, string name = "RunString") {
            try {
                LuaState.DoString(code, LuaState.Globals, name);
            } catch (ScriptRuntimeException ex) {
                Util.MsgC(ModEntryPoint.form, Color.FromArgb(255, 0, 0), string.Format("Doh! An error occured! {0}", ex.DecoratedMessage), "\r\n");
            }
        }
    }
}
