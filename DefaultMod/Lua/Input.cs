using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using MoonSharp.Interpreter;

namespace GooseLua.Lua
{
    [MoonSharpUserData]
    class Input
    {
        private Script script;

        [DllImport("user32.dll")]
        private static extern short GetAsyncKeyState(Keys vKey);

        [MoonSharpHidden]
        public Input(Script script)
        {
            this.script = script;
        }

        public bool IsKeyDown(int key) => GetAsyncKeyState((Keys)key) != 0;

        public string GetKeyName(int key) => ((Keys)key).ToString();

        public DynValue GetCursorPos() => DynValue.NewTuple(DynValue.NewNumber(Cursor.Position.X), DynValue.NewNumber(Cursor.Position.Y));

        public void SetCursorPos(int x, int y) => Cursor.Position = new Point(x, y);
        
        public bool IsMouseDown(int button) {
            MouseButtons nativeButton;
            switch ((Mouse)button)
            {
                case Mouse.MOUSE_LEFT:
                    nativeButton = MouseButtons.Left;
                    break;
                case Mouse.MOUSE_RIGHT:
                    nativeButton = MouseButtons.Right;
                    break;
                case Mouse.MOUSE_MIDDLE:
                    nativeButton = MouseButtons.Middle;
                    break;
                case Mouse.MOUSE_4:
                    nativeButton = MouseButtons.XButton1;
                    break;
                case Mouse.MOUSE_5:
                    nativeButton = MouseButtons.XButton2;
                    break;
                default:
                    throw new ScriptRuntimeException("Unsupported mouse button " + button);
            }
            return (Control.MouseButtons & nativeButton) == nativeButton;
        }
    }
}
