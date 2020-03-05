using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using MoonSharp.Interpreter;

namespace GooseLua.Lua
{
    [MoonSharpUserData]
    class Draw
    {
        private Script script;
        private Surface surface;
        private Graphics graphics { get => _G.graphics; }

        [MoonSharpHidden]
        public Draw(Script script, Surface surface)
        {
            this.script = script;
            this.surface = surface;
        }

        private void CheckGraphics() {
            if (graphics == default(Graphics)) throw new ScriptRuntimeException("Graphics not initialized or invalid hook.");
        }

        private StringFormat GetStringFormat(int xAlign, int yAlign) {
            var format = new StringFormat();
            switch (xAlign) {
                case (int)TextAlign.TEXT_ALIGN_RIGHT:
                    format.Alignment = StringAlignment.Far;
                    break;
                case (int)TextAlign.TEXT_ALIGN_CENTER:
                    format.Alignment = StringAlignment.Center;
                    break;
                case (int)TextAlign.TEXT_ALIGN_LEFT:
                default:
                    format.Alignment = StringAlignment.Near;
                    break;
            }
            switch (yAlign) {
                case (int)TextAlign.TEXT_ALIGN_BOTTOM:
                    format.LineAlignment = StringAlignment.Far;
                    break;
                case (int)TextAlign.TEXT_ALIGN_CENTER:
                    format.LineAlignment = StringAlignment.Center;
                    break;

                case (int)TextAlign.TEXT_ALIGN_TOP:
                default:
                    format.LineAlignment = StringAlignment.Near;
                    break;
            }
            return format;
        }
        public void DrawText(string text, string font = "DermaDefault", int x = 0, int y = 0, Table color = null, int xAlign = (int)TextAlign.TEXT_ALIGN_LEFT) => SimpleText(text, font, x, y, color, xAlign);

        public float GetFontHeight(string font) {
            CheckGraphics();
            return surface.GetFont(font).Height;
        }

        public void RoundedBox(int cornerRadius, int x, int y, int width, int height, Table color) => RoundedBoxEx(cornerRadius, x, y, width, height, color, true, true, true, true);

        public void RoundedBoxEx(int cornerRadius, int x, int y, int width, int height, Table color, bool roundTopLeft = false, bool roundTopRight = false, bool roundBottomLeft = false, bool roundBottomRight = false) {
            CheckGraphics();
            int diameter = cornerRadius * 2;
            Rectangle arc = new Rectangle(x, y, diameter, diameter);
            GraphicsPath path = new GraphicsPath();

            if (roundTopLeft) {
                path.AddArc(arc, 180, 90);
            } else {
                path.AddLine(x, y, x, y);
            }

            arc.X = x + width - diameter;
            if (roundTopRight) {
                path.AddArc(arc, 270, 90);
            } else {
                path.AddLine(x + cornerRadius, y, x + width, y);
            }

            arc.Y = y + height - diameter;
            if (roundBottomRight) {
                path.AddArc(arc, 0, 90);
            } else {
                path.AddLine(x + width, y + height - cornerRadius, x + width, y + height);
            }

            arc.X = x;
            if (roundBottomLeft) {
                path.AddArc(arc, 90, 90);
            } else {
                path.AddLine(x + width - cornerRadius, y + height, x, y + height);
            }

            path.CloseFigure();

            graphics.FillPath(new SolidBrush(Surface.GetColor(color, Color.White)), path);
        }

        public DynValue SimpleText(string text, string font = "DermaDefault", int x = 0, int y = 0, Table color = null, int xAlign = (int)TextAlign.TEXT_ALIGN_LEFT, int yAlign = (int)TextAlign.TEXT_ALIGN_TOP) {
            var pos = new Point(x, y);
            var format = GetStringFormat(xAlign, yAlign);
            var textColor = Surface.GetColor(color, Color.White);

            var textFont = surface.GetFont(font);
            graphics.DrawString(text, textFont, new SolidBrush(textColor), pos, format);

            var size = graphics.MeasureString(text, textFont, pos, format);
            return DynValue.NewTuple(DynValue.NewNumber(size.Width), DynValue.NewNumber(size.Height));
        }

        public DynValue SimpleTextOutlined(string text, string font = "DermaDefault", int x = 0, int y = 0, Table color = null, int xAlign = (int)TextAlign.TEXT_ALIGN_LEFT, int yAlign = (int)TextAlign.TEXT_ALIGN_TOP, double outlineWidth = 1, Table outlineColor = null) {
            CheckGraphics();
            var path = new GraphicsPath();
            var textFont = surface.GetFont(font);
            var pos = new Point(x, y);
            var format = GetStringFormat(xAlign, yAlign);
            var fillColor = Surface.GetColor(color, Color.White);
            var textOutlineColor = Surface.GetColor(outlineColor, Color.White);
            path.AddString(text, textFont.FontFamily, (int)textFont.Style, textFont.Size, pos, format);

            if (fillColor.A > 0) {
                graphics.FillPath(new SolidBrush(fillColor), path);
            }
            graphics.DrawPath(new Pen(textOutlineColor, (float)outlineWidth), path);

            var size = graphics.MeasureString(text, textFont, pos, format);
            return DynValue.NewTuple(DynValue.NewNumber(size.Width), DynValue.NewNumber(size.Height));
        }

        public DynValue Text(Table tab) {
            return SimpleText(
                tab.Get("text").String,
                tab.Get("font")?.String ?? "DermaDefault",
                (int)(tab.Get("pos")?.Table.Get(1).Number ?? 0),
                (int)(tab.Get("pos")?.Table.Get(2).Number ?? 0),
                tab.Get("color")?.Table,
                (int)(tab.Get("xalign")?.Number ?? (int)TextAlign.TEXT_ALIGN_LEFT),
                (int)(tab.Get("yalign")?.Number ?? (int)TextAlign.TEXT_ALIGN_TOP)
                );
        }

        public DynValue TextShadow(Table tab, double distance, int alpha = 200) {
            Util.Clamp(ref alpha, 0, 255);
            return SimpleText(
                tab.Get("text").String,
                tab.Get("font")?.String ?? "DermaDefault",
                (int)(tab.Get("pos")?.Table.Get(1).Number ?? 0),
                (int)(tab.Get("pos")?.Table.Get(2).Number ?? 0),
                script.DoString($"Color(0,0,0,{alpha})").Table,
                (int)(tab.Get("xalign")?.Number ?? (int)TextAlign.TEXT_ALIGN_LEFT),
                (int)(tab.Get("yalign")?.Number ?? (int)TextAlign.TEXT_ALIGN_TOP)
                );
        }

        public DynValue WordBox(int borderSize, int x, int y, string text, string font, Table boxColor, Table textColor) {
            var textFont = surface.GetFont(font);
            var size = graphics.MeasureString(text, textFont);

            RoundedBox(borderSize, x, y, (int)size.Width + borderSize * 2, (int)size.Height + borderSize * 2, boxColor);
            graphics.DrawString(text, textFont, new SolidBrush(Surface.GetColor(textColor, Color.White)), x + borderSize, y + borderSize);

            return DynValue.NewTuple(DynValue.NewNumber(size.Width + borderSize * 2), DynValue.NewNumber(size.Height + borderSize * 2));
        }

        #region non-facepunch API
        public DynValue MeasureText(string text, string font = "DermaDefault") {
            CheckGraphics();
            SizeF size = graphics.MeasureString(text, surface.GetFont(font));
            return DynValue.NewTable(new Table(_G.LuaState, DynValue.NewNumber(size.Width), DynValue.NewNumber(size.Height)));
        }
        #endregion
    }
}
