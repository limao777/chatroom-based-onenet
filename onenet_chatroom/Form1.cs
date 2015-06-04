using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Net;
using System.IO;
using System.Threading;


namespace onenet_chatroom
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

   //     string ip = Convert.ToString((IPAddress)Dns.GetHostAddresses(Dns.GetHostName()).GetValue(0));
        string ip = "";
        string url_chat_log = "http://api.heclouds.com/devices/你的设备ID/datapoints?datastream_id=chatlog";
        string url_chat_update = "http://api.heclouds.com/devices/你的设备ID/datapoints";
        string url_chat_online_people_get = "http://api.heclouds.com/devices/你的设备ID/datapoints?datastream_id=online1";
        string url_chat_online_people_post = "http://api.heclouds.com/devices/你的设备ID/datapoints";


        public string WebRequest(string method, string url, string postData)
        {
            HttpWebRequest webRequest = null;
            StreamWriter requestWriter = null;
            string responseData = "";

            webRequest = System.Net.WebRequest.Create(url) as HttpWebRequest;
            webRequest.Method = method.ToString();
            webRequest.ServicePoint.Expect100Continue = false;
            webRequest.UserAgent = "Mozilla/5.0 (Windows NT 6.1; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/40.0.2214.93 Safari/537.36";

                webRequest.Timeout = 5000;


                webRequest.Headers.Add(tb_header_ext);
            

            if (method == "POST" || method == "PUT")
            {
                if (method == "PUT")
                {
                    webRequest.ContentType = "text/xml";
                    webRequest.Method = "PUT";
                }
                else
                    webRequest.ContentType = "application/x-www-form-urlencoded";
                //webRequest.ContentType = "multipart/form-data";

                //POST the data.
                requestWriter = new StreamWriter(webRequest.GetRequestStream());
                try
                {
                    requestWriter.Write(postData);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.ToString());
                }
                finally
                {
                    requestWriter.Close();
                    requestWriter = null;
                }
            }

            responseData = WebResponseGet(webRequest);

            webRequest = null;

            return responseData;

        }

        public string WebResponseGet(HttpWebRequest webRequest)
        {
            StreamReader responseReader = null;
            string responseData = "";

            try
            {
                responseReader = new StreamReader(webRequest.GetResponse().GetResponseStream());
                responseData = responseReader.ReadToEnd();


                webRequest.GetResponse().GetResponseStream().Close();
                responseReader.Close();
                responseReader = null;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
            finally
            {

            }

            return responseData;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            lb1.Text = "发送中……";

            string post_text = "{\"datastreams\" : [{\"id\" : \"" + "chatlog" + "\",\"datapoints\" : [{"
                    + "\"value\" : {"
                    + "\"user\" : " + Environment.UserName + ","
                    + "\"content\" : " + tb2.Text
                    + "}}]}]}";


            WebRequest("POST", url_chat_update, post_text);

            tb2.Text = "";
            lb1.Text = "就绪";

        }

        private void button2_Click(object sender, EventArgs e)
        {
            listBox1.Items.Clear();
            listBox1.Items.Add(Environment.UserName + "-" + Environment.UserDomainName + "  " + ip);

            tb1.SelectionColor = Color.Red;
            tb1.AppendText("cs\r\n");

            tb1.SelectionColor = Color.Blue;
            tb1.AppendText("cs\r\n");

           // lb1.Text = DateTime.Now.AddMinutes(-10)+"";
            lb1.Text = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss");

        }

        private void clear_room() {
            //清空聊天室人数
            string post_text = "{\"datastreams\" : [{\"id\" : \"" + "online1" + "\",\"datapoints\" : [{"
+ "\"value\" : {"
+ "\"people\" : " + "在线的人："
+ "}}]}]}";


            WebRequest("POST", url_chat_online_people_post, post_text);
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            lb1.Text = "就绪";

            clear_room();
            



            Thread th_request;
            
            th_request = new Thread((ThreadStart)delegate()
            {
                do_th_request();
            });
            th_request.IsBackground = true;
            th_request.Start();
        

          Thread th_request2;
            
            th_request2 = new Thread((ThreadStart)delegate()
            {
                do_th_request2();
            });
            th_request2.IsBackground = true;
            th_request2.Start();
        }

        //聊天的人
        private void do_th_request2() {
            while (true)
            {
                this.getonliepeople();

                Thread.Sleep(3333);
            }
        }

        //聊天记录的获取
        private void do_th_request()
        {
            string time = "";
            while (true)
            {
                if (time == "") {
                    string post_text = "{\"datastreams\" : [{\"id\" : \"" + "chatlog" + "\",\"datapoints\" : [{"
+ "\"value\" : {"
+ "\"user\" : " + Environment.UserName + ","
+ "\"content\" : " + Environment.UserName + "加入了聊天室"
+ "}}]}]}";
                    WebRequest("POST", url_chat_update, post_text);
                    Thread.Sleep(1234);
                }
             
                    string res = WebRequest("GET", url_chat_log, "");

                    //          JsonReader reader = new JsonTextReader(new StringReader(res));

                    int at = res.IndexOf("datastreams\":[{\"datapoints\":[{\"at\":\"");
                    int user = res.IndexOf("{\"user\":");
                    int value = res.IndexOf(",\"content\":\"");
                    int value_end = res.IndexOf("\"}}],\"id");

                    string time_temp = time;

                    time = res.Substring(at + 36, 22);

                    string adduser = res.Substring(user + 9, value - user - 10);

                    string content = res.Substring(value + 12, value_end - value - 12);

                    Thread.Sleep(222);

                    if (time_temp == time)
                    {
                        continue;
                    }

                    this.Invoke((MethodInvoker)delegate()
                    {
                        if (adduser == Environment.UserName)
                        {
                            tb1.SelectionColor = Color.Green;
                        }
                        else
                        {
                            tb1.SelectionColor = Color.Blue;
                        }
                        tb1.AppendText(adduser + " "); tb1.AppendText(time + " ");
                        tb1.AppendText("\r\n");
                        tb1.SelectionColor = Color.Black;
                        tb1.AppendText(content + "\r\n\r\n");
                        //   tb1.AppendText(res);
                    });

               // break;
            }
            
        }


        private void getonliepeople() {

            string res = WebRequest("GET", url_chat_online_people_get, "");

            int pos_start = res.IndexOf(",\"value\":{\"people\":\"");
            int pos_end = res.IndexOf("\"}}],\"");

            string people = res.Substring(pos_start + 20, pos_end - pos_start-20);

            if (people.IndexOf(Environment.UserName + "|" + Environment.UserDomainName + "|" + ip) < 0)
            {
                people = people + "*" + Environment.UserName + "|" + Environment.UserDomainName + "|" + ip;
                string post_text = "{\"datastreams\" : [{\"id\" : \"" + "online1" + "\",\"datapoints\" : [{"
+ "\"value\" : {"
+ "\"people\" : " + people
+ "}}]}]}";

                WebRequest("POST", url_chat_online_people_post, post_text);
            }
            

            this.Invoke((MethodInvoker)delegate()
            {
                string[] sArray = people.Split('*');
                listBox1.Items.Clear();
                foreach (string i in sArray) { listBox1.Items.Add(i); }
        //        MessageBox.Show(people + "\r\n"+DateTime.Now + "");
                //lb1.Text = people + DateTime.Now + "";
            });
            


        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            clear_room();
           // this.getonliepeople();
        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }
    }
}
