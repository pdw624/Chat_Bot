using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.IO;
using System.Xml.Linq;


namespace chat_Client
{
    public partial class Client : Form
    {
        private Socket socket; //소켓
        private Thread receiveThread; //대화수신용
        bool simsim = false;
        static string weather = "";
        static int rNum;
        public Client()
        {
            InitializeComponent();
        }

        private void Log(string msg)
        {
            listBox1.Items.Add(string.Format(
            "[{0}] {1}", DateTime.Now.ToString(), msg
            ));
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            textBox1.Focus();
            Log("클라이언트 로드됨!!");
        }

        private void button1_Click(object sender, EventArgs e)
        {
            //서버 연결하기
            IPAddress ipaddress = IPAddress.Parse(textBox2.Text);
            IPEndPoint endPoint = new IPEndPoint(
                ipaddress, int.Parse(textBox3.Text)
            );

            //연결 소켓생성
            socket = new Socket(
                AddressFamily.InterNetwork,
                SocketType.Stream,
                ProtocolType.Tcp
            );
            //연결하기
            Log("서버에 연결 시도중..");
            socket.Connect(endPoint);
            Log("서버에 접속됨");

            //Receive 스레드 처리(서버 <-> 클라이언트)
            receiveThread = new Thread(new ThreadStart(Receive));
            receiveThread.IsBackground = true;
            receiveThread.Start();
        }


        private void Receive()

        {

            while (true)
            {
                //연결된 클라이언트가 보낸 데이터 수신
                byte[] recevieBuffer = new byte[512];

                int length = socket.Receive(
                recevieBuffer,
                recevieBuffer.Length,
                SocketFlags.None
                );

                string msg = Encoding.UTF8.GetString(
                    recevieBuffer,
                    0,
                    length
                );
                ShowMsg("상대 ]" + msg);
            }
        }

        private void GetResponse(IAsyncResult ar)
        {
            HttpWebRequest wr = (HttpWebRequest)ar.AsyncState;
            HttpWebResponse wp = (HttpWebResponse)wr.EndGetResponse(ar);
            Stream stream = wp.GetResponseStream();
            StreamReader reader = new StreamReader(stream);
            string strRead = reader.ReadToEnd();
            XElement xmlMain = XElement.Parse(strRead);
            XElement xmlHead = xmlMain.Descendants("header").First();
            string strTitle = xmlHead.Element("title").Value;
            string strDate = xmlHead.Element("tm").Value;
            string strDesc = xmlHead.Element("wf").Value;
            strDesc = strDesc.Replace("<br/><br/>", "\n");
            string strTemp = strTitle + "\n" + strDate + "\n" + strDesc + "\n";
            weather = strTemp;
            this.Invoke(new Action(() =>
            {
                ShowSimMsg("심심이] " + weather);
            }
            ));
        }

        private void textBox1_KeyDown(object sender, KeyEventArgs e)
        {
            //메시지 전송하기
            if (textBox1.Text.Trim() != "" && e.KeyCode == Keys.Enter)
            {
                byte[] sendBuffer =
                Encoding.UTF8.GetBytes(textBox1.Text.Trim());
                socket.Send(sendBuffer);
                Log("메시지 전송됨");
                ShowMsg("나] " + textBox1.Text);

                simsimMsg sm = new simsimMsg();
                Random rn = new Random();
                
                
                if(textBox1.Text.Contains("심심아") || textBox1.Text.Contains("심심이"))
                {
                    richTextBox2.AppendText("\r\n");
                    richTextBox2.AppendText("안녕하세요 심심이에요! 아래 키워드를 입력해보세요");
                    richTextBox2.AppendText("\r\n");

                    richTextBox2.AppendText("1.날씨");
                    richTextBox2.AppendText("\r\n");
                    richTextBox2.AppendText("2.배고");
                    richTextBox2.AppendText("\r\n");
                    richTextBox2.AppendText("3.취미");
                    richTextBox2.AppendText("\r\n");
                    richTextBox2.AppendText("4.안녕");
                    richTextBox2.AppendText("\r\n");
                    richTextBox2.AppendText("5.축구");
                    richTextBox2.AppendText("\r\n");

                    simsim = true;
                }

                if(simsim == true)
                {
                    if (textBox1.Text.Contains("날씨"))
                    {
                        /*
                        richTextBox2.AppendText("심심이] " + sm.weatherMsg[rNum]);
                        richTextBox2.AppendText("\r\n");*/
                        string strUrl = "http://www.weather.go.kr/weather/forecast/mid-term-xml.jsp";
                        UriBuilder ub = new UriBuilder(strUrl);
                        ub.Query = "srnLd=109";
                        HttpWebRequest request;
                        request = HttpWebRequest.Create(ub.Uri) as HttpWebRequest;
                        request.BeginGetResponse(new AsyncCallback(GetResponse), request);

                        
                    }
                    if (textBox1.Text.Contains("안녕"))
                    {
                        rNum = rn.Next(0, sm.helloMsg.Length);
                        ShowSimMsg("심심이 ]" + sm.helloMsg[rNum]);
                    }
                    if (textBox1.Text.Contains("배고"))
                    {
                        rNum = rn.Next(0, sm.hungryMsg.Length);
                        ShowSimMsg("심심이 ]" + sm.hungryMsg[rNum]);
                    }
                    if (textBox1.Text.Contains("취미"))
                    {
                        rNum = rn.Next(0, sm.hobbyMsg.Length);
                        ShowSimMsg("심심이 ]" + sm.hobbyMsg[rNum]);

                    }
                    if (textBox1.Text.Contains("축구"))
                    {
                        rNum = rn.Next(0, sm.footballMsg.Length);
                        ShowSimMsg("심심이 ]" + sm.footballMsg[rNum]);

                    }
                    if (textBox1.Text.Contains("종료"))
                    {
                        richTextBox2.AppendText("심심이] " + "다음에 또 봐요!!");
                        richTextBox2.AppendText("\r\n");
                        simsim = false;
                    }
                }
                

                textBox1.Text = "";
            }
        }

        //송수신 메시지를 대화창에 출력
        delegate void SetShowMsg(string msg);
        private void ShowMsg(string msg)
        {
            if (this.InvokeRequired)
            {
                SetShowMsg ssm = new SetShowMsg(ShowMsg);
                this.Invoke(ssm, new object[] { msg });
            }
            else
            {
                //richTextBox에서 개행이 정상적으로 적용되지 않는다면 아래와 같이 따로 처리함
                richTextBox1.AppendText(msg);
                richTextBox1.AppendText("\r\n");

              
                //입력된 텍스트에 맞게 스크롤을 내려준다
                this.Activate();
                textBox1.Focus();
                //캐럿(커서)를 텍스트박스의 끝으로 내려준다
                richTextBox1.SelectionStart = richTextBox1.Text.Length;
                richTextBox1.ScrollToCaret(); //스크롤을 캐럿위치에 맞춰준다
            }

        }


        //심심이 메시지
        delegate void SetShowSimMsg(string msg);
        private void ShowSimMsg(string msg)
        {
            if (this.InvokeRequired)
            {
                SetShowSimMsg sssm = new SetShowSimMsg(ShowMsg);
                this.Invoke(sssm, new object[] { msg });
            }
            else
            {
                //richTextBox에서 개행이 정상적으로 적용되지 않는다면 아래와 같이 따로 처리함
                richTextBox2.AppendText(msg);
                richTextBox2.AppendText("\r\n");


                //입력된 텍스트에 맞게 스크롤을 내려준다
                this.Activate();
                textBox1.Focus();
                //캐럿(커서)를 텍스트박스의 끝으로 내려준다
                richTextBox2.SelectionStart = richTextBox2.Text.Length;
                richTextBox2.ScrollToCaret(); //스크롤을 캐럿위치에 맞춰준다
            }

        }









    }
}

class simsimMsg
{

    public string[] weatherMsg =
    {
        "오늘 날씨는 맑네요",
        "비가 내려 우중충 하네요",
        "구름이 끼어 있어요",
        "눈이 내려요",
        "천둥이 치고 우박이 떨어져요"
    };
    public string[] helloMsg =
    {
        "안녕하세요!",
        "반가워요!",
        "별로 안녕하지 못하네요",
        "오랜만이에요",
        "ㅎㅇㅎㅇ"
    };
    public string[] hungryMsg =
    {
        "저는 치킨 먹어서 배부른데 ㅋㅋ",
        "저희 집 앞에 있는 해장국집 정말 맛있더라구요",
        "이 시간까지 밥 안먹고 뭐했어요?",
        "아 네 ㅋㅋ",
        "저도 배고파요 ㅠ"
    };
    public string[] hobbyMsg =
    {
        "제 취미는 축구예요",
        "전 야구를 아주 잘해요 뭘 잘하세요?",
        "잠자는게 취미예요",
        "책 읽는 걸 아주 좋아해요",
        "없는데요?"
    };
    public string[] footballMsg =
    {
        "호날두가 사람이라고 생각하세요?",
        "맨시티는 근본구단이에요",
        "리버풀은 우승할 수 없어요",
        "ㄹㅈㄸ",
        "맨시티 10번 아구에로, 토트넘 10번 케인, 아스날 10번 외질, 리버풀 10번 마네, 맨유 10번 래쉬포드 ㅋㅋ"
    };
    

}