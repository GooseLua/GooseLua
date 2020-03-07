using MoonSharp.Interpreter;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http.Headers;
using System.Text;

namespace GooseLua.Lua {
    class Http {
        private class RequestState {
            public HttpWebRequest request;
            public Closure success, failure;
        }

        public static bool Request(Table args) {
            var url = args.Get("url")?.String;
            var method = args.Get("method")?.String;
            var headers = args.Get("headers")?.Table;
            var parameters = args.Get("parameters")?.Table;
            var body = args.Get("body")?.String;
            var type = args.Get("type")?.String;
            var success = args.Get("success")?.Function;
            var failure = args.Get("failure")?.Function;

            if (url == null || method == null) {
                return false;
            }

            var request = CreateRequest(url, method, headers, type, body, parameters);
            var state = new RequestState() {
                success = success,
                failure = failure,
                request = request
            };
            request.BeginGetResponse(HandleResponse, state);

            return true;
        }

        private static void HandleResponse(IAsyncResult result) {
            var state = (RequestState)result.AsyncState;
            HttpWebResponse response;
            try {
                response = (HttpWebResponse)state.request.EndGetResponse(result);
            } catch (WebException ex) {
                if (ex.Response is HttpWebResponse) {
                    response = (HttpWebResponse)ex.Response;
                } else if (state.failure != null) {
                    _G.mainQueue.Enqueue(() => {
                        state.failure.Call(ex.Message);
                    });
                    return;
                } else {
                    return;
                }
            }

            if (state.success != null) {
                var encoding = Encoding.GetEncoding(response.CharacterSet ?? "iso-8859-1");
                using (var stream = response.GetResponseStream()) {
                    var streamReader = new StreamReader(stream, encoding);
                    var body = streamReader.ReadToEnd();
                    var headers = response.Headers.AllKeys.ToDictionary(k => response.Headers[k]);

                    _G.mainQueue.Enqueue(() => {
                        state.success.Call((int)response.StatusCode, body, headers);
                    });
                }
            }
        }

        private static HashSet<string> BodiedMethods = new HashSet<string>() { "post", "put", "patch" };
        
        private static HttpWebRequest CreateRequest(string url, string method, Table headers, string type, string body, Table parameters) {
            var requestHasBody = BodiedMethods.Contains(method.ToLower());

            // Create body/parameters
            if (body == null && parameters != null) {
                // encode as form
                var sb = new StringBuilder();
                foreach (var pair in parameters.Pairs) {
                    sb.Append(Uri.EscapeDataString(pair.Key.CastToString()));
                    sb.Append("=");
                    sb.Append(Uri.EscapeDataString(pair.Value.CastToString()));
                    sb.Append("&");
                }
                if (sb.Length > 0) {
                    // remove last "&"
                    sb.Length -= 1;
                }
                
                if (requestHasBody) {
                    body = sb.ToString();
                    if (type == null) {
                        type = "application/x-www-form-urlencoded";
                    }
                } else if (url.Contains("?")) {
                    if (url.Last() != '&') {
                        url += "&";
                    }
                    url += sb.ToString();
                } else {
                    url += "?" + sb.ToString();
                }
            }

            // Create request
            var request = WebRequest.CreateHttp(url);
            request.Method = method.ToUpper();
            request.Accept = "*/*";
            request.KeepAlive = false;
            request.AllowAutoRedirect = false;
            if (requestHasBody && body != null && body.Length > 0) {
                using (var stream = request.GetRequestStream()) {
                    var bodyBytes = Encoding.Default.GetBytes(body);
                    stream.Write(bodyBytes, 0, bodyBytes.Length);
                }
            }

            // Set content type
            if (type != null) {
                request.ContentType = type;
            }

            // Add headers
            foreach (var header in headers.Pairs) {
                var name = header.Key.CastToString();
                var value = header.Value.CastToString();
                switch(name.ToLower()) {
                    case "accept":
                        request.Accept = value;
                        break;
                    case "connection":
                        request.Connection = value;
                        break;
                    case "content-length":
                        request.ContentLength = long.Parse(value);
                        break;
                    case "content-type":
                        request.ContentType = value;
                        break;
                    case "date":
                        if (DateTime.TryParse(value, out var date)) {
                            request.Date = date;
                        }
                        break;
                    case "expect":
                        request.Expect = value;
                        break;
                    case "host":
                        request.Host = value;
                        break;
                    case "if-modified-since":
                        if (DateTime.TryParse(value, out var ifModifiedSince)) {
                            request.IfModifiedSince = ifModifiedSince;
                        }
                        break;
                    case "range":
                        if (RangeHeaderValue.TryParse(value, out var ranges)) {
                            foreach(var range in ranges.Ranges) {
                                if (range.From.HasValue && range.To.HasValue) {
                                    request.AddRange(ranges.Unit, range.From.Value, range.To.Value);
                                } else if (range.From.HasValue) {
                                    request.AddRange(ranges.Unit, range.From.Value);
                                } else if (range.To.HasValue) {
                                    request.AddRange(ranges.Unit, range.To.Value);
                                }
                            }
                        }
                        break;
                    case "referer":
                        request.Referer = value;
                        break;
                    case "transfer-encoding":
                        request.TransferEncoding = value;
                        break;
                    case "user-agent":
                        request.UserAgent = value;
                        break;
                    default:
                        request.Headers.Add(name, value);
                        break;
                }
            }

            return request;
        }
    }
}
