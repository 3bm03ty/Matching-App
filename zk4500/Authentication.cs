using AxZKFPEngXControl;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Script.Serialization;
using System.Windows.Forms;
using System.Threading.Tasks;
namespace zk4500
{
    public partial class Authentication : Form
    {
        private AxZKFPEngX ZkFprint = new AxZKFPEngX();
        private bool Check;
        String userTemplate;
        public Authentication()
        {
            InitializeComponent();

            
        }

        private void Authentication_Load(object sender, EventArgs e)
        {
            Controls.Add(ZkFprint);
            InitialAxZkfp();
            Play();
            FormBorderStyle = FormBorderStyle.None;
            WindowState = FormWindowState.Maximized;
            //webPostMethod("000", "https://moproject.herokuapp.com/matching/");
            String temp = webGetMethod("https://moproject.herokuapp.com/templates");

            if (ZkFprint.IsRegister)
            {
                ZkFprint.CancelEnroll();
            }
            ZkFprint.OnCapture += zkFprint_OnCapture;
            ZkFprint.BeginCapture();
            //ShowHintInfo("Please give fingerprint sample.");
            //JObject json = JObject.Parse(temp);
            /*string str = json11.template*/
            JavaScriptSerializer j = new JavaScriptSerializer();
            //System.Reflection.PropertyInfo pi = temp.GetType().GetProperty("template");
            //String template = (String)(pi.GetValue(temp, null));
            //String x = GetPropValue(json, "template");
            char[] spearator = { '"', '"' };
            Int32 count = 7;
            String[] x = temp.Split(spearator, count, StringSplitOptions.RemoveEmptyEntries);
            userTemplate = x[5];
            //ShowHintInfo(userTemplate);
        }
        public static object GetPropValue(object src, string propName)
        {
            return src.GetType().GetProperty(propName).GetValue(src, null);
        }
        public string webPostMethod(string postData, string URL)
        {
            string responseFromServer = "";
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(URL);
            request.Method = "POST";
            request.Credentials = CredentialCache.DefaultCredentials;
            ((HttpWebRequest)request).UserAgent =
                              "Mozilla/5.0 (compatible; MSIE 9.0; Windows NT 7.1; Trident/5.0)";
            request.Accept = "/";
            request.UseDefaultCredentials = true;
            request.Proxy.Credentials = System.Net.CredentialCache.DefaultCredentials;
            byte[] byteArray = Encoding.UTF8.GetBytes(postData);
            request.ContentType = "application/x-www-form-urlencoded";
            request.ContentLength = byteArray.Length;
            Stream dataStream = request.GetRequestStream();
            dataStream.Write(byteArray, 0, byteArray.Length);
            dataStream.Close();

            WebResponse response = request.GetResponse();
            dataStream = response.GetResponseStream();
            StreamReader reader = new StreamReader(dataStream);
            responseFromServer = reader.ReadToEnd();
            reader.Close();
            dataStream.Close();
            response.Close();
            ShowHintInfo(responseFromServer);
            return responseFromServer;
        }
        public string webGetMethod(string URL)
        {
            string jsonString = "";
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(URL);
            request.Method = "GET";
            request.Credentials = CredentialCache.DefaultCredentials;
            ((HttpWebRequest)request).UserAgent = "Mozilla/5.0 (compatible; MSIE 9.0; Windows NT 7.1; Trident/5.0)";
            request.Accept = "/";
            request.UseDefaultCredentials = true;
            request.Proxy.Credentials = System.Net.CredentialCache.DefaultCredentials;
            request.ContentType = "application/x-www-form-urlencoded";

            WebResponse response = request.GetResponse();
            StreamReader sr = new StreamReader(response.GetResponseStream());
            jsonString = sr.ReadToEnd();
            sr.Close();
            //ShowHintInfo(jsonString);
            return jsonString;
        }



        private void InitialAxZkfp()
        {
            try
            {

                ZkFprint.OnImageReceived += zkFprint_OnImageReceived;
                //zkFprint.OnFingerTouching 
                //zkFprint.OnFingerLeaving

                if (ZkFprint.InitEngine() == 0)
                {
                    ZkFprint.FPEngineVersion = "9";
                    ZkFprint.EnrollCount = 3;
                    //deviceSerial.Text += " " + ZkFprint.SensorSN + " Count: " + ZkFprint.SensorCount.ToString() + " Index: " + ZkFprint.SensorIndex.ToString();
                    //ShowHintInfo("Device successfully connected");
                }

            }
            catch (Exception ex)
            {
                //ShowHintInfo("Device init err, error: " + ex.Message);
            }
        }

        private void zkFprint_OnImageReceived(object sender, IZKFPEngXEvents_OnImageReceivedEvent e)
        {
            Graphics g = fpicture.CreateGraphics();
            Bitmap bmp = new Bitmap(fpicture.Width, fpicture.Height);
            g = Graphics.FromImage(bmp);
            int dc = g.GetHdc().ToInt32();
            ZkFprint.PrintImageAt(dc, 0, 0, bmp.Width, bmp.Height);
            g.Dispose();
            fpicture.Image = bmp;
        }
       
        public void Play()
        {
            System.Threading.Timer t = new System.Threading.Timer(timerC, null, 30000, 30000);
        }

        private void timerC(object state)
        {
            //this.Close();
            Environment.Exit(0);
        }

        private void zkFprint_OnCapture(object sender, IZKFPEngXEvents_OnCaptureEvent e)
        {
            string template = ZkFprint.EncodeTemplate1(e.aTemplate);

                ShowHintInfo("Verified");
                var httpWebRequest = (HttpWebRequest)WebRequest.Create("https://moproject.herokuapp.com/matching/");
                httpWebRequest.ContentType = "application/json";
                httpWebRequest.Method = "POST";
            if (ZkFprint.VerFingerFromStr(ref template, userTemplate, false, ref Check))
            {
               

                using (var streamWriter = new StreamWriter(httpWebRequest.GetRequestStream()))
                {
                    string json = "{\"matching\":\"Matched\"}";

                    streamWriter.Write(json);
                }

                
                this.Close();
            }
            else
            {
                ShowHintInfo("Not Verified");

                using (var streamWriter = new StreamWriter(httpWebRequest.GetRequestStream()))
                {
                    string json = "{\"matching\":\"Not Matched\"}";

                    streamWriter.Write(json);
                }
            }
                var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();
                using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                {
                    var result = streamReader.ReadToEnd();
                }
        }



        private void ShowHintInfo(String s)
        {
            prompt.Text = s;
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {

        }
    }
}
