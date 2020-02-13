using GooseShared;
using MoonSharp.Interpreter;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GooseLua.Lua {
    class Hook {
        public Dictionary<string, Dictionary<string, Closure>> hooks = new Dictionary<string, Dictionary<string, Closure>>();
        
        public void Add(string hook, string name, Closure action) {
            hooks[hook].Add(name, action);
        }

        public void Remove(string hook, string name) {
            hooks[hook].Remove(name);
        }
    }
}
