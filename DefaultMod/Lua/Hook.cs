using MoonSharp.Interpreter;
using System.Collections.Generic;

namespace GooseLua.Lua {
    [MoonSharpUserData]
    class Hook {
        [MoonSharpHidden]
        public Dictionary<string, Dictionary<string, Closure>> hooks = new Dictionary<string, Dictionary<string, Closure>>();
        
        public void Add(string hook, string name, Closure action) {
            hooks[hook].Add(name, action);
        }

        public void Remove(string hook, string name) {
            hooks[hook].Remove(name);
        }
    }
}
