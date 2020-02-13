using GooseLua.Lua;
using GooseShared;
using MoonSharp.Interpreter;
using System.Drawing;

namespace GooseLua {
    class _G {
        public static Script LuaState = new Script(CoreModules.Preset_SoftSandbox | CoreModules.Debug);
        public static GooseEntity goose;
        public static string path;
        public static Hook hook = new Hook();

        public static void RunString(string code) {
            code = code.Replace(" || ", " or ");
            code = code.Replace(" && ", " and ");
            code = code.Replace("!", "not");
            try {
                LuaState.DoString(code);
            } catch (ScriptRuntimeException ex) {
                ModEntryPoint.form.MsgC(Color.FromArgb(0, 0, 0), "Doh! An error occured! {0}", ex.DecoratedMessage);
            }
        }
    }
}
