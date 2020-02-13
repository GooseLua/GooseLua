using System.Drawing;
using System.IO;
using System.Reflection;

namespace GooseLua {
    class Util {
        public static void Clamp(ref int num, int min, int max) {
            if (num > max) num = max;
            if (num < min) num = min;
        }

        public static void MsgC(formLoader form, params dynamic[] args) {
            foreach (dynamic arg in args) {
                if (arg is Color) {
                    form.console.SelectionStart = form.console.TextLength;
                    form.console.SelectionLength = 0;
                    form.console.SelectionColor = (Color)arg;
                } else {
                    form.console.AppendText(arg);
                    form.console.SelectionColor = form.console.ForeColor;
                }
            }
        }

        public static void include(string library) {
            _G.RunString(getResource("GooseLua.Includes." + library + ".lua"), library);
        }

        public static string getResource(string path) {
            Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(path);
            if (stream == default(Stream)) return "error(\"Could not find " + path + "\")";
            using (var reader = new StreamReader(stream)) {
                string content = reader.ReadToEnd();
                if (content == default(string) || content.Length < 0) return "error(\"Could not find " + path + "\")";
                return content;
            }
        }
    }
}
