using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MoonSharp.Interpreter;

namespace GooseLua.Lua
{
    static class Enums
    {
        public static void Register(Script script)
        {
            RegisterEnum(typeof(Mouse), script);
            RegisterEnum(typeof(TextAlign), script);
        }

        public static void RegisterEnum(Type type, Script script, string prefix = "")
        {
            string[] names = Enum.GetNames(type);
            Array values = Enum.GetValues(type);
            for(int i=0; i < names.Length; i++)
            {
                script.Globals.Set(prefix + names[i], DynValue.FromObject(script, values.GetValue(i)));
            }
        }
    }
    enum Mouse
    {
        MOUSE_FIRST = 107,
        MOUSE_LEFT = 107,
        MOUSE_RIGHT = 108,
        MOUSE_MIDDLE = 109,
        MOUSE_4 = 110,
        MOUSE_5 = 111,
        MOUSE_WHEEL_UP = 112,
        MOUSE_WHEEL_DOWN = 113,
        MOUSE_LAST = 113,
        MOUSE_COUNT = 7
    }

    enum TextAlign
    {
        TEXT_ALIGN_LEFT = 0,
        TEXT_ALIGN_CENTER = 1,
        TEXT_ALIGN_RIGHT = 2,
        TEXT_ALIGN_TOP = 3,
        TEXT_ALIGN_BOTTOM = 4
    }
}
