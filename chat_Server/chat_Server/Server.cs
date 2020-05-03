using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net;
using System.Net;
using System.Net.Sockets;
using System.Threading;



namespace chat_Server
{
    public partial class Server : Form
    {
        private string ip = "127.0.0.1";
        private int port = 8000;
        private Thread listenThread; //Accept()가 블럭
        private Thread receiveThread; //Receive() 작업
        private Socket clientSocket; //연결된 클라이언트 소켓


        public Server()
        {
            InitializeComponent();
        }

        delegate void SetLog(string msg);
        private void Log(string msg)
        {
            if (this.InvokeRequired)
            {
                SetLog sl = new SetLog(Log);
                this.Invoke(sl, new object[] { msg });
            }
            else
            {
                this.listBox1.Items.Add(string.Format(
                    "[{0}] {1}", DateTime.Now.ToString(), msg
                ));
            }
            /*listBox1.Items.Add(string.Format(
            "[{0}] {1}", DateTime.Now.ToString(), msg
            ));*/
        }


        private void button1_Click(object sender, EventArgs e)
        {
            //서버 시작하기

            if (connectBtn.Text == "시작")
            {
                connectBtn.Text = "멈춤";
                Log("서버 시작됨");

                //Listen스레드 처리
                listenThread = new Thread(new ThreadStart(Listen));
                listenThread.IsBackground = true;
                listenThread.Start();
            }

            else
            {
                connectBtn.Text = "시작";
                Log("서버 멈춤");
            }

        }

        private void Listen()
        {
            //종단점
            IPAddress ipaddress = IPAddress.Parse(ip);

            IPEndPoint endPoint = new IPEndPoint(ipaddress, port);

            //소켓생성
            Socket listenSocket = new Socket(

            AddressFamily.InterNetwork,
            SocketType.Stream,
            ProtocolType.Tcp
            );

            //바인드
            listenSocket.Bind(endPoint);
            //준비
            listenSocket.Listen(10);
            //수신대기
            // - 여기서 블럭이 걸려야 하지만 스레드로 따로 뺏기때문에 다른 작업이가능
            Log("클라이언트 요청 대기중..");

            clientSocket = listenSocket.Accept();
            Log("클라이언트 접속됨 - " + clientSocket.LocalEndPoint.ToString());

            //Receive 스레드 호출
            receiveThread = new Thread(new ThreadStart(Receive));
            receiveThread.IsBackground = true;
            receiveThread.Start(); //Receive() 호출
        }

        //수신처리..
        private void Receive()
        {
            while (true)
            {
                //연결된 클라이언트가 보낸 데이터 수신
                byte[] receiveBuffer = new byte[512];
                int length = clientSocket.Receive(
                receiveBuffer, receiveBuffer.Length, SocketFlags.None
                );

                //디코딩
                string msg = Encoding.UTF8.GetString(receiveBuffer);
                //엔터처리
                //richTextBox1.AppendText(msg);
                ShowMsg("상대] " + msg);
                Log("메시지 수신함");
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
                //richTextBox에서 개행이 정상적으로 적용되지 않는다면
                //아래와 같이 따로
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

        private void Form1_Load(object sender, EventArgs e)
        {
            textBox1.Focus();
            Log("서버가 로드됨");
        }

        private void textBox1_KeyDown(object sender, KeyEventArgs e)
        {
            //메시지 전송하기
            if (textBox1.Text.Trim() != "" && e.KeyCode == Keys.Enter)
            {
                byte[] sendBuffer =
                    Encoding.UTF8.GetBytes(textBox1.Text.Trim());
                clientSocket.Send(sendBuffer);
                Log("메시지 전송됨");
                ShowMsg("나] " + textBox1.Text);
                textBox1.Text = "";//초기화
            }

        }
    }
}
