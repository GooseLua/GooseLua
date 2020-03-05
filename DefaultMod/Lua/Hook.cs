using MoonSharp.Interpreter;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace GooseLua.Lua {
    [MoonSharpUserData]
    class Hook {
        [MoonSharpHidden]
        public formLoader form;
        private Dictionary<string, Dictionary<string, Closure>> hooks = new Dictionary<string, Dictionary<string, Closure>>();
        private List<Tuple<string, string, Closure>> deferredUpdates = new List<Tuple<string, string, Closure>>();
        private bool callingHooks = false;

        public Hook() {
            hooks["preRig"] = new Dictionary<string, Closure>();
            hooks["postRig"] = new Dictionary<string, Closure>();
            hooks["preTick"] = new Dictionary<string, Closure>();
            hooks["postTick"] = new Dictionary<string, Closure>();
            hooks["preRender"] = new Dictionary<string, Closure>();
            hooks["postRender"] = new Dictionary<string, Closure>();
        }

        public void Add(string hook, string name, Closure action) {
            if (callingHooks) {
                // defer update
                deferredUpdates.Add(Tuple.Create(hook, name, action));
                return;
            }
            if (hooks[hook].ContainsKey(name)) {
                hooks[hook][name] = action;
            } else {
                hooks[hook].Add(name, action);
            }
        }

        public void Remove(string hook, string name) {
            if (callingHooks) {
                // defer update
                deferredUpdates.Add(Tuple.Create(hook, name, default(Closure)));
                return;
            }
            hooks[hook].Remove(name);
        }

        [MoonSharpHidden]
        public void CallHooks(string hook) {
            callingHooks = true;
            foreach (Closure func in hooks[hook].Values) {
                try {
                    func.Call();
                } catch (InterpreterException ex) {
                    Util.MsgC(form, Color.FromArgb(255, 0, 0), string.Format("[ERROR] {0}: {1}\r\n{2}", ex.Source, ex.DecoratedMessage, ex.StackTrace), "\r\n");
                } catch (Exception ex) {
                    MessageBox.Show(ex.ToString(), ex.Message, MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            callingHooks = false;
            RunDeferredUpdates();
        }

        private void RunDeferredUpdates() {
            foreach(var update in deferredUpdates) {
                if (update.Item3 == default(Closure)) {
                    Remove(update.Item1, update.Item2);
                } else {
                    Add(update.Item1, update.Item2, update.Item3);
                }
            }
            deferredUpdates.Clear();
        }
    }
}
