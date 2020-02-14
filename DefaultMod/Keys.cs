using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace GooseLua {
    class KeyEnums {
        // KEY_FIRST
        // KEY_NONE
        // KEY_0
        // KEY_1
        // KEY_2
        // KEY_3
        // KEY_4
        // KEY_5
        // KEY_6
        // KEY_7
        // KEY_8
        // KEY_9
        // KEY_A
        // KEY_B
        // KEY_C
        // KEY_D
        // KEY_E
        // KEY_F
        // KEY_G
        // KEY_H
        // KEY_I
        // KEY_J
        // KEY_K
        // KEY_L
        // KEY_M
        // KEY_N
        // KEY_O
        // KEY_P
        // KEY_Q
        // KEY_R
        // KEY_S
        // KEY_T
        // KEY_U
        // KEY_V
        // KEY_W
        // KEY_X
        // KEY_Y
        // KEY_Z
        // KEY_PAD_0
        // KEY_PAD_1
        // KEY_PAD_2
        // KEY_PAD_3
        // KEY_PAD_4
        // KEY_PAD_5
        // KEY_PAD_6
        // KEY_PAD_7
        // KEY_PAD_8
        // KEY_PAD_9
        // KEY_LBRACKET
        // KEY_RBRACKET
        // KEY_SEMICOLON
        // KEY_APOSTROPHE
        // KEY_BACKQUOTE
        // KEY_COMMA
        // KEY_PERIOD
        // KEY_SLASH
        // KEY_BACKSLASH
        // KEY_MINUS
        // KEY_EQUAL
        // KEY_ENTER
        // KEY_SPACE
        // KEY_BACKSPACE
        // KEY_TAB
        // KEY_CAPSLOCK
        // KEY_NUMLOCK
        // KEY_ESCAPE
        // KEY_SCROLLLOCK
        // KEY_INSERT
        // KEY_DELETE
        // KEY_HOME
        // KEY_END
        // KEY_PAGEUP
        // KEY_PAGEDOWN
        // KEY_BREAK
        // KEY_LSHIFT
        // KEY_RSHIFT
        // KEY_LALT
        // KEY_RALT
        // KEY_LCONTROL
        // KEY_RCONTROL
        // KEY_LWIN
        // KEY_RWIN
        // KEY_APP
        // KEY_UP
        // KEY_LEFT
        // KEY_DOWN
        // KEY_RIGHT
        // KEY_F1
        // KEY_F2
        // KEY_F3
        // KEY_F4
        // KEY_F5
        // KEY_F6
        // KEY_F7
        // KEY_F8
        // KEY_F9
        // KEY_F10
        // KEY_F11
        // KEY_F12
        // KEY_CAPSLOCKTOGGLE
        // KEY_NUMLOCKTOGGLE
        // KEY_LAST
        // KEY_SCROLLLOCKTOGGLE
        // KEY_COUNT
        public static void Load() {
            // Digits
            _G.LuaState.Globals["KEY_0"] = Keys.D0;
            _G.LuaState.Globals["KEY_1"] = Keys.D1;
            _G.LuaState.Globals["KEY_2"] = Keys.D2;
            _G.LuaState.Globals["KEY_3"] = Keys.D3;
            _G.LuaState.Globals["KEY_4"] = Keys.D4;
            _G.LuaState.Globals["KEY_5"] = Keys.D5;
            _G.LuaState.Globals["KEY_6"] = Keys.D6;
            _G.LuaState.Globals["KEY_7"] = Keys.D7;
            _G.LuaState.Globals["KEY_8"] = Keys.D8;
            _G.LuaState.Globals["KEY_9"] = Keys.D9;

            // Numpad
            _G.LuaState.Globals["KEY_PAD_0"] = Keys.NumPad0;
            _G.LuaState.Globals["KEY_PAD_1"] = Keys.NumPad1;
            _G.LuaState.Globals["KEY_PAD_2"] = Keys.NumPad2;
            _G.LuaState.Globals["KEY_PAD_3"] = Keys.NumPad3;
            _G.LuaState.Globals["KEY_PAD_4"] = Keys.NumPad4;
            _G.LuaState.Globals["KEY_PAD_5"] = Keys.NumPad5;
            _G.LuaState.Globals["KEY_PAD_6"] = Keys.NumPad6;
            _G.LuaState.Globals["KEY_PAD_7"] = Keys.NumPad7;
            _G.LuaState.Globals["KEY_PAD_8"] = Keys.NumPad8;
            _G.LuaState.Globals["KEY_PAD_9"] = Keys.NumPad9;

            // Function Keys
            _G.LuaState.Globals["KEY_F1"] = Keys.F1;
            _G.LuaState.Globals["KEY_F2"] = Keys.F2;
            _G.LuaState.Globals["KEY_F3"] = Keys.F3;
            _G.LuaState.Globals["KEY_F4"] = Keys.F4;
            _G.LuaState.Globals["KEY_F5"] = Keys.F5;
            _G.LuaState.Globals["KEY_F6"] = Keys.F6;
            _G.LuaState.Globals["KEY_F7"] = Keys.F7;
            _G.LuaState.Globals["KEY_F8"] = Keys.F8;
            _G.LuaState.Globals["KEY_F9"] = Keys.F9;
            _G.LuaState.Globals["KEY_F10"] = Keys.F10;
            _G.LuaState.Globals["KEY_F11"] = Keys.F11;
            _G.LuaState.Globals["KEY_F12"] = Keys.F12;
            _G.LuaState.Globals["KEY_F13"] = Keys.F13;
            _G.LuaState.Globals["KEY_F14"] = Keys.F14;
            _G.LuaState.Globals["KEY_F15"] = Keys.F15;
            _G.LuaState.Globals["KEY_F16"] = Keys.F16;
            _G.LuaState.Globals["KEY_F17"] = Keys.F17;
            _G.LuaState.Globals["KEY_F18"] = Keys.F18;
            _G.LuaState.Globals["KEY_F19"] = Keys.F19;
            _G.LuaState.Globals["KEY_F20"] = Keys.F20;
            _G.LuaState.Globals["KEY_F21"] = Keys.F21;
            _G.LuaState.Globals["KEY_F22"] = Keys.F22;
            _G.LuaState.Globals["KEY_F23"] = Keys.F23;
            _G.LuaState.Globals["KEY_F24"] = Keys.F24;

            // Alphabetic Keys
            _G.LuaState.Globals["KEY_A"] = Keys.A;
            _G.LuaState.Globals["KEY_B"] = Keys.B;
            _G.LuaState.Globals["KEY_C"] = Keys.C;
            _G.LuaState.Globals["KEY_D"] = Keys.D;
            _G.LuaState.Globals["KEY_E"] = Keys.E;
            _G.LuaState.Globals["KEY_F"] = Keys.F;
            _G.LuaState.Globals["KEY_G"] = Keys.G;
            _G.LuaState.Globals["KEY_H"] = Keys.H;
            _G.LuaState.Globals["KEY_I"] = Keys.I;
            _G.LuaState.Globals["KEY_J"] = Keys.J;
            _G.LuaState.Globals["KEY_K"] = Keys.K;
            _G.LuaState.Globals["KEY_L"] = Keys.L;
            _G.LuaState.Globals["KEY_M"] = Keys.M;
            _G.LuaState.Globals["KEY_N"] = Keys.N;
            _G.LuaState.Globals["KEY_O"] = Keys.O;
            _G.LuaState.Globals["KEY_P"] = Keys.P;
            _G.LuaState.Globals["KEY_Q"] = Keys.Q;
            _G.LuaState.Globals["KEY_R"] = Keys.R;
            _G.LuaState.Globals["KEY_S"] = Keys.S;
            _G.LuaState.Globals["KEY_T"] = Keys.T;
            _G.LuaState.Globals["KEY_U"] = Keys.U;
            _G.LuaState.Globals["KEY_V"] = Keys.V;
            _G.LuaState.Globals["KEY_W"] = Keys.W;
            _G.LuaState.Globals["KEY_X"] = Keys.X;
            _G.LuaState.Globals["KEY_Y"] = Keys.Y;
            _G.LuaState.Globals["KEY_Z"] = Keys.Z;

            // Arrow Keys
            _G.LuaState.Globals["KEY_UP"] = Keys.Up;
            _G.LuaState.Globals["KEY_DOWN"] = Keys.Down;
            _G.LuaState.Globals["KEY_LEFT"] = Keys.Left;
            _G.LuaState.Globals["KEY_RIGHT"] = Keys.Right;

            // Special Keys
            _G.LuaState.Globals["KEY_PAGEUP"] = Keys.PageUp;
            _G.LuaState.Globals["KEY_PAGEDOWN"] = Keys.PageDown;

            _G.LuaState.Globals["KEY_LCONTROL"] = Keys.LControlKey;
            _G.LuaState.Globals["KEY_RCONTROL"] = Keys.RControlKey;

            _G.LuaState.Globals["KEY_LSHIFT"] = Keys.LShiftKey;
            _G.LuaState.Globals["KEY_RSHIFT"] = Keys.RShiftKey;

            _G.LuaState.Globals["KEY_LALT"] = Keys.Alt;
            _G.LuaState.Globals["KEY_RALT"] = Keys.Alt;

            _G.LuaState.Globals["KEY_LWIN"] = Keys.LWin;
            _G.LuaState.Globals["KEY_RWIN"] = Keys.RWin;

            _G.LuaState.Globals["KEY_LSHIFT"] = Keys.LShiftKey;
            _G.LuaState.Globals["KEY_RSHIFT"] = Keys.RShiftKey;

            _G.LuaState.Globals["KEY_INSERT"] = Keys.Insert;
            _G.LuaState.Globals["KEY_DELETE"] = Keys.Delete;

            _G.LuaState.Globals["KEY_BACKSPACE"] = Keys.Back;
            _G.LuaState.Globals["KEY_COMMA"] = Keys.Oemcomma;
            _G.LuaState.Globals["KEY_ESCAPE"] = Keys.Escape;
            _G.LuaState.Globals["KEY_ENTER"] = Keys.Enter;
            _G.LuaState.Globals["KEY_SPACE"] = Keys.Space;
            _G.LuaState.Globals["KEY_APP"] = Keys.Apps;
            _G.LuaState.Globals["KEY_TAB"] = Keys.Tab;
            _G.LuaState.Globals["KEY_HOME"] = Keys.Home;
            _G.LuaState.Globals["KEY_END"] = Keys.End;
            _G.LuaState.Globals["KEY_NUMLOCK"] = Keys.NumLock;
            _G.LuaState.Globals["KEY_CAPSLOCK"] = Keys.CapsLock;
            _G.LuaState.Globals["KEY_SCROLLOCK"] = Keys.Scroll;
        }
    }
}
