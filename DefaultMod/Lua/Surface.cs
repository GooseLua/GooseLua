using System.Drawing;
using MoonSharp.Interpreter;

namespace GooseLua.Lua
{
    [MoonSharpUserData]
    class Surface
    {
        private Script script;
        private Graphics graphics { get => _G.graphics; }
        private Color drawColor = Color.FromArgb(255, 255, 255);

        [MoonSharpHidden]
        public Surface(Script script)
        {
            this.script = script;
        }

        public void SetDrawColor(int r, int g, int b, int a = 255) {
            if (graphics == default(Graphics)) throw new ScriptRuntimeException("Graphics not initialized or invalid hook.");
            Util.Clamp(ref r, 0, 255);
            Util.Clamp(ref g, 0, 255);
            Util.Clamp(ref b, 0, 255);
            Util.Clamp(ref a, 0, 255);
            drawColor = Color.FromArgb(a, r, g, b);
        }

        public void DrawLine(int sx, int sy, int fx, int fy) {
            if (graphics == default(Graphics)) throw new ScriptRuntimeException("Graphics not initialized or invalid hook.");
            graphics.DrawLine(new Pen(new SolidBrush(drawColor), 1f), sx, sy, fx, fy);
        }
        
        public void DrawRect(int x, int y, int h, int w) {
            if (graphics == default(Graphics)) throw new ScriptRuntimeException("Graphics not initialized or invalid hook.");
            graphics.FillRectangle(new SolidBrush(drawColor), new Rectangle(x, y, h, w));
        }
    }
}
