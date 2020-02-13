using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace GooseLua {
    class Util {
        public static void include(string library) {
            _G.RunString(getResource("GooseLua.Includes." + library + ".lua"));
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
