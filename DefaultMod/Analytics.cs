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
        private static bool active = false;

        private static void sendRequest(Dictionary<string, string> postData) {
            var postDataString = postData.Aggregate("", (data, next) => string.Format("{0}&{1}={2}", data, next.Key, HttpUtility.UrlEncode(next.Value))).TrimEnd('&');

            try {
                using (WebClient wc = new WebClient()) {
                    wc.Headers[HttpRequestHeader.ContentType] = "application/x-www-form-urlencoded";
                    string HtmlResult = wc.UploadString($"{_G.ApiURL}analytics", postDataString);
                }
            } catch (Exception ex) {
                active = false;
            }
        }

        public static void StartSession() {
            var postData = new Dictionary<string, string>();
            postData.Add("cid", _G.GetSessionID());
            postData.Add("uid", Environment.UserName + "@" + Environment.UserDomainName);
            postData.Add("sc", "start");
            sendRequest(postData);

            active = true;

            Thread thread = new Thread(()=>{ while (active) { Thread.Sleep(5000); KeepAlive(); } });
            thread.Start();
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
        }
    }
}