using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MoonSharp.Interpreter;

namespace GooseLua.Lua
{
    [MoonSharpUserData]
    class Draw
    {
        private Script script;
        private Graphics graphics { get => _G.graphics; }

        [MoonSharpHidden]
        public Draw(Script script)
        {
            this.script = script;
        }

        public void SimpleText(string text, string font, int x, int y) {
            if (graphics == default(Graphics)) throw new ScriptRuntimeException("Graphics not initialized or invalid _G.hook.");
            graphics.DrawString(text, new Font(font, 12f), Brushes.White, new PointF(x, y));
        }

        public void SimpleTextOutlined (string text, string font, int x, int y) => SimpleText(text, font, x, y);
        
        public float GetFontHeight(string font) {
            if (graphics == default(Graphics)) throw new ScriptRuntimeException("Graphics not initialized or invalid _G.hook.");
            return graphics.MeasureString("A", new Font(font, 12f)).Height;
        }

        public DynValue MeasureText(string text, string font = "Segoe UI Light")
        {
            if (graphics == default(Graphics)) throw new ScriptRuntimeException("Graphics not initialized or invalid _G.hook.");
            SizeF size = graphics.MeasureString(text, new Font(font, 12f));
            return DynValue.NewTable(new Table(_G.LuaState, DynValue.NewNumber(size.Width), DynValue.NewNumber(size.Height)));
        }
    }
}
