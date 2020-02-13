using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;
using System.Windows.Forms;

namespace GooseLua {
    public static class Analytics {
        public static void TrackEvent(string category, string action, string label) {
            Track("event", category, action, label);
        }

        public static void TrackPageview(string category, string action, string label) {
            Track("pageview", category, action, label);
        }

        private static void sendRequest(Dictionary<string, string> postData) {
            var postDataString = postData.Aggregate("", (data, next) => string.Format("{0}&{1}={2}", data, next.Key, HttpUtility.UrlEncode(next.Value))).TrimEnd('&');

            var request = (HttpWebRequest)WebRequest.Create("https://www.google-analytics.com/collect");
            request.Method = "POST";
            request.ContentLength = Encoding.UTF8.GetByteCount(postDataString);

            // write the request body to the request
            using (var writer = new StreamWriter(request.GetRequestStream())) {
                writer.Write(postDataString);
            }

            try {
                var webResponse = (HttpWebResponse)request.GetResponse();
                if (webResponse.StatusCode != HttpStatusCode.OK) {
                    throw new HttpException((int)webResponse.StatusCode, "Google Analytics tracking did not return OK 200");
                }
                webResponse.Close();
            } catch (Exception ex) {
                MessageBox.Show(ex.ToString());
            }
        }

        private static void Track(string type, string category, string action, string label) {
            if (string.IsNullOrEmpty(category)) throw new ArgumentNullException("category");
            if (string.IsNullOrEmpty(action)) throw new ArgumentNullException("action");
            var postData = new Dictionary<string, string> {{"v", "1"}, {"tid", _G.google_analytics}, {"t", type.ToString()}, {"ec", category}, {"ea", action}};
            if (!string.IsNullOrEmpty(label)) postData.Add("el", label);
            postData.Add("cid", ComputerInfo.GetSessionID());
            postData.Add("uid", Environment.UserName + "@" + Environment.UserDomainName);
            sendRequest(postData);
        }

        public static void StartSession() {

            var postData = new Dictionary<string, string> {{"v", "1"}, {"tid", _G.google_analytics}};
            postData.Add("cid", ComputerInfo.GetSessionID());
            postData.Add("uid", Environment.UserName + "@" + Environment.UserDomainName);
            postData.Add("sc", "start");
            sendRequest(postData);
        }

        public static void EndSession() {
            var postData = new Dictionary<string, string> {{"v", "1"}, {"tid", _G.google_analytics}};
            postData.Add("cid", ComputerInfo.GetSessionID());
            postData.Add("uid", Environment.UserName + "@" + Environment.UserDomainName);
            postData.Add("sc", "end");
            sendRequest(postData);
        }
    }
}