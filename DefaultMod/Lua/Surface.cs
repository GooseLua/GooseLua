using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Media;
using MoonSharp.Interpreter;

namespace GooseLua.Lua
{
    [MoonSharpUserData]
    class Surface
    {
        private Script script;
        private Graphics graphics { get => _G.graphics; }
        private Color drawColor = Color.White;
        private Pen drawPen = new Pen(new SolidBrush(Color.White));
        private Dictionary<string, Font> fonts = new Dictionary<string, Font>();
        private Closure isColor = null;
        private Point textPos = Point.Empty;
        private Color textColor = Color.White;
        private Font textFont = null;
        private Dictionary<string, SoundPlayer> sounds = new Dictionary<string, SoundPlayer>();

        [MoonSharpHidden]
        public Surface(Script script)
        {
            this.script = script;
            InitFonts();
        }

        [MoonSharpHidden]
        public Font GetFont(string name) => fonts.ContainsKey(name) ? fonts[name] : fonts["Default"];

        private void InitFonts() {
            CreateFont("DermaDefault", "Tahoma", 13f);
            CreateFont("DermaDefaultBold", "Tahoma", 13f, FontStyle.Bold);
            CreateFont("DermaLarge", "Tahoma", 32f);
            CreateFont("DebugFixed", "Courier New", 14f);
            CreateFont("DebugFixedSmall", "Courier New", 14f);
            CreateFont("Default", "Verdana", 12f);
            CreateFont("Marlett", "Marlett", 14f);
            CreateFont("Trebuchet18", "Trebuchet MS", 18f);
            CreateFont("Trebuchet24", "Trebuchet MS", 24f);
            CreateFont("HudHintTextLarge", "Verdana", 14f);
            CreateFont("HudHintTextSmall", "Verdana", 11f);
            CreateFont("CenterPrintText", "Trebuchet MS", 18f);
            CreateFont("HudSelectionText", "Verdana", 8f);
            CreateFont("CloseCaption_Normal", "Tahoma", 26f);
            CreateFont("CloseCaption_Italic", "Tahoma", 26f, FontStyle.Italic);
            CreateFont("CloseCaption_Bold", "Tahoma", 26f, FontStyle.Bold);
            CreateFont("CloseCaption_BoldItalic", "Tahoma", 26f, FontStyle.Bold | FontStyle.Italic);
            CreateFont("TargetID", "Trebuchet MS", 24f, FontStyle.Bold);
            CreateFont("TargetIDSmall", "Trebuchet MS", 18f, FontStyle.Bold);
            CreateFont("BudgetLabel", "Courier New", 14f);
            CreateFont("HudNumbers", "Trebuchet MS", 32f);
            SetFont("Default");
        }

        private Color GetColor(DynValue rOrColor, int g, int b, int a) {
            if (isColor == null) {
                isColor = (Closure)script.Globals["IsColor"];
            }

            int r;

            if (isColor.Call(rOrColor).CastToBool()) {
                Table color = rOrColor.Table;
                r = (int)color.Get("r").Number;
                g = (int)color.Get("g").Number;
                b = (int)color.Get("b").Number;
                a = (int)color.Get("a").Number;
            } else {
                r = (int)(rOrColor.CastToNumber() ?? 255.0);
            }

            Util.Clamp(ref r, 0, 255);
            Util.Clamp(ref g, 0, 255);
            Util.Clamp(ref b, 0, 255);
            Util.Clamp(ref a, 0, 255);

            return Color.FromArgb(a, r, g, b);
        }

        public static Color GetColor(Table color, Color? defaultColor = null) {
            if (color == null) {
                return defaultColor.GetValueOrDefault();
            }
            int r = (int)color.Get("r").Number;
            int g = (int)color.Get("g").Number;
            int b = (int)color.Get("b").Number;
            int a = (int)color.Get("a").Number;
            Util.Clamp(ref r, 0, 255);
            Util.Clamp(ref g, 0, 255);
            Util.Clamp(ref b, 0, 255);
            Util.Clamp(ref a, 0, 255);

            return Color.FromArgb(a, r, g, b);
        }

        public static Table GetColorTable(Script script, Color color) {
            return script.DoString($"Color({color.R},{color.G},{color.B},{color.A})").Table;
        }

        private Table GetColorTable(Color color) => GetColorTable(script, color);

        private void CheckGraphics() {
            if (graphics == default(Graphics)) throw new ScriptRuntimeException("Graphics not initialized or invalid hook.");
        }

        public void CreateFont(string name, Table attributes) {
            FontStyle style = FontStyle.Regular;
            double size = attributes.Get("size")?.CastToNumber() ?? 13.0;
            double weight = attributes.Get("weight")?.CastToNumber() ?? 400;
            if (attributes.Get("underline").CastToBool()) {
                style |= FontStyle.Underline;
            }
            if (attributes.Get("italic").CastToBool()) {
                style |= FontStyle.Italic;
            }
            if (attributes.Get("bold").CastToBool() || weight >= 600 ) {
                style |= FontStyle.Bold;
            }
            if (attributes.Get("strikeout").CastToBool()) {
                style |= FontStyle.Strikeout;
            }

            CreateFont(name, attributes.Get("font").CastToString(), (float)size, style);
        }

        private void CreateFont(string fontKey, string fontName, float size, FontStyle style = FontStyle.Regular) {
            var font = new Font(fontName, size, style);
            if (fonts.ContainsKey(fontKey)) {
                fonts.Remove(fontKey);
            }
            fonts.Add(fontKey, font);
        }

        public void DrawCircle(int originX, int originY, int radius, DynValue rOrColor, int g = 255, int b = 255, int a = 255) {
            CheckGraphics();
            var pen = new Pen(GetColor(rOrColor, g, b, a));
            graphics.DrawEllipse(pen, new Rectangle(originX - radius, originY - radius, 2*radius, 2*radius));
        }

        public void DrawLine(int sx, int sy, int fx, int fy) {
            CheckGraphics();
            graphics.DrawLine(drawPen, sx, sy, fx, fy);
        }

        public void DrawOutlinedRect(int x, int y, int w, int h) {
            CheckGraphics();
            graphics.DrawRectangle(drawPen, x, y, w, h);
        }

        public void DrawPoly(Table vertices) {
            CheckGraphics();
            var points = vertices.Pairs.Select(v =>
                new PointF((float)v.Value.Table.Get("x").Number, (float)v.Value.Table.Get("y").Number)
            ).ToArray<PointF>();
            graphics.FillPolygon(new SolidBrush(drawColor), points);
        }

        public void DrawRect(int x, int y, int w, int h) {
            CheckGraphics();
            graphics.FillRectangle(new SolidBrush(drawColor), new Rectangle(x, y, w, h));
        }
        public void DrawText(string text) {
            CheckGraphics();
            graphics.DrawString(text, textFont, new SolidBrush(textColor), textPos);
        }

        public Table GetDrawColor() {
            return GetColorTable(drawColor);
        }
        public Table GetTextColor() {
            return GetColorTable(textColor);
        }

        public DynValue GetTextSize(string text) {
            if (graphics == default(Graphics) || textFont == null) {
                return DynValue.Nil;
            }
            SizeF size = graphics.MeasureString(text, textFont);
            return DynValue.NewTuple(DynValue.NewNumber(size.Width), DynValue.NewNumber(size.Height));
        }

        public void PlaySound(string soundfile) {
            if (!sounds.TryGetValue(soundfile, out SoundPlayer player)) {
                player = new SoundPlayer(soundfile);
                sounds.Add(soundfile, player);
            }
            player.Play();
        }
        public void SetDrawColor(DynValue rOrColor, int g = 255, int b = 255, int a = 255) {
            drawColor = GetColor(rOrColor, g, b, a);
        }
        
        public void SetFont(string fontName) {
            textFont = GetFont(fontName);
        }

        public void SetTextColor(DynValue rOrColor, int g = 255, int b = 255, int a = 255) {
            textColor = GetColor(rOrColor, g, b, a);
        }

        public void SetTextPos(int x, int y) {
            textPos = new Point(x, y);
        }
    }
}
