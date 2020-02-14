using GooseLua.Lua;
using GooseShared;
using MoonSharp.Interpreter;
using System.Collections.Generic;
using System.Drawing;

namespace GooseLua {
    class _G {
        public static Script LuaState = new Script(CoreModules.Preset_SoftSandbox | CoreModules.Debug);
        public static GooseEntity goose;
        public static string path;
        public static Hook hook = new Hook();
        public static List<string> luaQueue = new List<string>();
        public static string ApiURL = "https://gooselua.my.to/";
        private static string SessionID = "";

        public static string GetSessionID() {
            if (string.IsNullOrEmpty(SessionID)) SessionID = GenerateSessionID();
            return SessionID;
        }

        private static string GenerateSessionID(int length = 16, string chars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890-_") {
            using (RNGCryptoServiceProvider crypto = new RNGCryptoServiceProvider()) {
                byte[] data = new byte[length];
                byte[] buffer = null;
                int maxRandom = byte.MaxValue - ((byte.MaxValue + 1) % chars.Length);
                crypto.GetBytes(data);
                char[] result = new char[length];
                for (int i = 0; i < length; i++) {
                    byte value = data[i];
                    while (value > maxRandom) {
                        if (buffer == null) {
                            buffer = new byte[1];
                        }
                        crypto.GetBytes(buffer);
                        value = buffer[0];
                    }
                    result[i] = chars[value % chars.Length];
                }
                return new string(result);
            }
        }

        public static void RunString(string code, string name = "RunString") {
            try {
                LuaState.DoString(code, LuaState.Globals, name);
            } catch (ScriptRuntimeException ex) {
                Util.MsgC(ModEntryPoint.form, Color.FromArgb(255, 0, 0), string.Format("[ERROR] {0}: {1}\r\n{2}", ex.Source, ex.DecoratedMessage, ex.StackTrace), "\r\n");
            }
        }
    }
}
