using GooseLua.Lua;
using GooseShared;
using MoonSharp.Interpreter;

namespace GooseLua {
    class _G {
        public static Script LuaState = new Script(CoreModules.Preset_SoftSandbox);
        public static GooseEntity goose;
        public static string path;
        public static Hook hook = new Hook();
    }
}
