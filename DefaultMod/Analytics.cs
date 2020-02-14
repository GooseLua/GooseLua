using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Threading;
using System.Web;
using System.Windows.Forms;

namespace GooseLua {
    public static class Analytics {
        private static Thread keepAlive = new Thread(() => { });
        private static bool active = false;

        public static void TrackEvent(string category, string action, string label) {
            Track("event", category, action, label);
        }

        private static void sendRequest(Dictionary<string, string> postData) {
            var postDataString = postData.Aggregate("", (data, next) => string.Format("{0}&{1}={2}", data, next.Key, HttpUtility.UrlEncode(next.Value))).TrimEnd('&');

            try {
                using (WebClient wc = new WebClient()) {
                    wc.Headers[HttpRequestHeader.ContentType] = "application/x-www-form-urlencoded";
                    string HtmlResult = wc.UploadString($"{_G.ApiURL}analytics", postDataString);
                }
            } catch (Exception ex) {
                MessageBox.Show($"Could not connect to the analytics server.\r\nError: {ex.Message}");
                Process.GetCurrentProcess().Kill();
            }
        }

        private static void Track(string type, string category, string action, string label) {
            if (string.IsNullOrEmpty(category)) throw new ArgumentNullException("category");
            if (string.IsNullOrEmpty(action)) throw new ArgumentNullException("action");
            var postData = new Dictionary<string, string> {{"t", type.ToString()}, {"ec", category}, {"ea", action}};
            if (!string.IsNullOrEmpty(label)) postData.Add("el", label);
            postData.Add("cid", _G.GetSessionID());
            postData.Add("uid", Environment.UserName + "@" + Environment.UserDomainName);
            sendRequest(postData);
        }

        public static void StartSession() {
            var postData = new Dictionary<string, string>();
            postData.Add("cid", _G.GetSessionID());
            postData.Add("uid", Environment.UserName + "@" + Environment.UserDomainName);
            postData.Add("sc", "start");
            sendRequest(postData);

            active = true;

            keepAlive = new Thread(()=>{ while (active) { Thread.Sleep(5000); KeepAlive(); } });
            keepAlive.Start();
        }

        public static void KeepAlive() {
            var postData = new Dictionary<string, string>();
            postData.Add("cid", _G.GetSessionID());
            postData.Add("uid", Environment.UserName + "@" + Environment.UserDomainName);
            postData.Add("sc", "keepalive");
            sendRequest(postData);
        }

        public static void EndSession() {
            var postData = new Dictionary<string, string>();
            postData.Add("cid", _G.GetSessionID());
            postData.Add("uid", Environment.UserName + "@" + Environment.UserDomainName);
            postData.Add("sc", "stop");
            sendRequest(postData);

            active = false;

            keepAlive.Interrupt();
            keepAlive.Abort();
        }
    }
}