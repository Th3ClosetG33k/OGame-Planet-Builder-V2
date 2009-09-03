using System;
using System.IO;
using System.Net;
using System.Threading;
using System.Windows.Forms;

namespace OGPB
{
    public class http
    {
        public bool development = false;
        public string version = "2.0.18", status = "beta";
        public GUI gui;
        public Encryption encrypt = new Encryption();
        private HttpWebRequest httpRequest;
        private CookieContainer httpCookies = new CookieContainer();
        private IWebProxy httpProxy = new WebProxy("128.208.04.198", 3124);
        public string server = "http://uni42.ogame.org/game/";
        public string referer = "http://ogame.org";
        public string session = "";
        public string token = "";
        public int currentPlanet = 0;
        public bool allPlanetsIDed = false;

        private HttpWebRequest PrepareRequest(string URL)
        {
            if (URL.IndexOf("http") != 0)
            {
                URL = server + URL;
            }
            httpRequest = (HttpWebRequest)WebRequest.Create(URL);
            httpRequest.CookieContainer = httpCookies;
            //httpRequest.Proxy = httpProxy;
            httpRequest.UserAgent = "Mozilla/5.0 (Macintosh; U; Intel Mac OS X 10.5; en-US; rv:1.9.1.2) Gecko/20090729 Firefox/3.5.2";
            httpRequest.Accept = "text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8";
            httpRequest.Headers.Add("Accept-Language", "en-us,en;q=0.5");
            httpRequest.Headers.Add("Accept-Charset", "ISO-8859-1,utf-8;q=0.7,*;q=0.7");
            httpRequest.Referer = referer;
            return httpRequest;
        }

        private string ReadResponseContent(WebResponse response)
        {
            if (response.ResponseUri.ToString().Contains("&ajax=1") == false)
            {
                referer = response.ResponseUri.ToString();
            }
            Stream dataStream = response.GetResponseStream();
            StreamReader reader = new StreamReader(dataStream);
            string content = reader.ReadToEnd();
            reader.Close();
            response.Close();
            if (content.Contains("<script>document.location.href='http://ogame.org';</script>") == true)
            {
                gui.ChangeStatus("We have been logged out!");
                Application.ExitThread();
            }
            return content;
        }

        public string Login(string Username, string Password)
        {
            PrepareRequest("reg/login2.php?&login=" + Username + "&pass=" + Password + "&v=2&is_utf8=0");
            WebResponse response = httpRequest.GetResponse();
            string location = ((HttpWebResponse)response).ResponseUri.ToString();
            if (location.IndexOf("session=") != -1)
            {
                session = location.Substring(location.IndexOf("session=") + "session=".Length);
                session = session.Remove(session.Length - 6);
                return ReadResponseContent(response);
            }
            else
            {
                referer = "http://ogame.org";
                return "";
            }
        }

        public string DownloadFile(string URL)
        {
            PrepareRequest(URL);
            WebResponse response = httpRequest.GetResponse();
            return ReadResponseContent(response);
        }

        public string PostFile(string URL, string post)
        {
            PrepareRequest(URL);
            httpRequest.Method = "POST";
            httpRequest.ContentType = "application/x-www-form-urlencoded";
            httpRequest.ContentLength = post.Length;
            using (StreamWriter writer = new StreamWriter(httpRequest.GetRequestStream(), System.Text.Encoding.ASCII))
            {
                writer.Write(post);
            }
            WebResponse response = httpRequest.GetResponse();
            return ReadResponseContent(response);
        }

        public string CommunicateWithServer(string page, string toSend)
        {
            PrepareRequest("http://ogpbv2.vndv.com/secure/" + page + ".php");
            toSend = "data=" + toSend;
            httpRequest.Method = "POST";
            httpRequest.ContentType = "application/x-www-form-urlencoded";
            httpRequest.ContentLength = toSend.Length;
            using (StreamWriter writer = new StreamWriter(httpRequest.GetRequestStream(), System.Text.Encoding.ASCII))
            {
                writer.Write(toSend);
            }
            WebResponse response = httpRequest.GetResponse();
            return ReadResponseContent(response);
        }

        public bool NeedUpdate()
        {
            string response = CommunicateWithServer("update", "version=" + version + ";status=" + status);
            gui.AddToLog("Currently running " + version + " " + status);
            gui.AddToLog("Update check: " + response);
            if (response == "OK") {
                return false;
            }
            else if (response.Contains("UPDATE-") == true)
            {
                DialogResult runUpdate = MessageBox.Show("There is an update available! Would you like to download it now?", "Update Available", MessageBoxButtons.YesNo);
                if (runUpdate == DialogResult.Yes)
                {
                    System.Diagnostics.Process.Start(response.Substring("UPDATE-".Length));
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                MessageBox.Show("The OGPB servers are offline or have moved. All paid and remote management features will be disabled! Please check to see if there is a new version!");
                return false;
            }
        }
    }
}
