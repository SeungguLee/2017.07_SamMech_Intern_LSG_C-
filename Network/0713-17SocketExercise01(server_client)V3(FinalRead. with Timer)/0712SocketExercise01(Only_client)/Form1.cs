﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;


using System.Net;
using System.Net.Sockets;
using System.IO;

//V3. Timer를 이용하여 데이터 송신.
namespace _0712SocketExercise01_server_client_
{
    public partial class Form1 : Form
    {

        Socket client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        Byte[] _data = new Byte[2048];
        Byte[] _sdata = new Byte[2048];
        string receivestr;
        string sendstr;
        int length;
        float intervaltime;
        string[] namearr = new string[2048];
        string[] hexarr = new string[2048];
        string[] decarr = new string[2048];


        public Form1()
        {
            InitializeComponent();
            IPAddress[] localIP = Dns.GetHostAddresses(Dns.GetHostName()); //get my own IP
            foreach (IPAddress address in localIP)
            {
                if (address.AddressFamily == AddressFamily.InterNetwork)
                {
                    textBox1.Text = address.ToString();
                }
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            IPEndPoint ipep = new IPEndPoint(IPAddress.Parse(textBox1.Text), int.Parse(textBox2.Text));
            try
            {
                client.Connect(ipep);
                MessageBox.Show("Socket Connected");
                if (client.Connected)
                {
                    textBox3.AppendText("Connected to Server" + "\n");

                    backgroundWorker1.RunWorkerAsync();     // start receiving Data in background
                    backgroundWorker2.WorkerSupportsCancellation = false;  // Ability to cancel this thread
                }
            }
            catch (Exception x)
            {
                MessageBox.Show(x.Message.ToString());
            }
        }



        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e) //receive data
        {
            while (client.Connected)
            {
                try
                {
                    client.Receive(_sdata);
                    receivestr = Encoding.Default.GetString(_sdata);
                    this.textBox3.Invoke(new MethodInvoker(delegate() { textBox3.AppendText("\n\n\nOthers : " + receivestr + "\n\n\n"); }));


                    if (receivestr.Substring(18, 4) == "0000")        //종료코드가 0000이 아닐때는 예외.
                    {
                        length = Convert.ToInt32(receivestr.Substring(14, 4), 16) - 4;    //순수 데이터의 길이.
                        int j = 22;
                        int namenum = Int32.Parse(sendstr.Substring(34, 4));

                        for (int i = 0; i < 10 && i < length / 4; i++)    //값들을 각각의 배열에 넣어준다. textbox가 10개 이므로, 10번 이상은 돌리지 않는다.
                        {
                            namearr[i] = sendstr.Substring(30, 1) + namenum.ToString();
                            hexarr[i] = receivestr.Substring(j, 4);
                            decarr[i] = Convert.ToInt32(hexarr[i], 16).ToString();
                            namenum++;
                            j += 4;
                        }

                        for (int i = 0; i < length / 4 && i < 10; i++)  //값들을 각각의 텍스트박스에 넣어준다.
                        {
                            TextBox target = (Controls.Find("text_n" + i.ToString(), true)[0] as TextBox);
                            target.Text = namearr[i];

                            target = (Controls.Find("text_h" + i.ToString(), true)[0] as TextBox);
                            target.Text = hexarr[i];

                            target = (Controls.Find("text_d" + i.ToString(), true)[0] as TextBox);
                            target.Text = decarr[i];

                        }
                        receivestr = "";                        //받은 값 저장변수를 null로 초기화

                    }
                    else
                    {
                        MessageBox.Show("종료코드 에러 발생");
                    }



                }
                catch (Exception x)
                {
                    MessageBox.Show(x.Message.ToString());
                }
            }
        }

        private void backgroundWorker2_DoWork(object sender, DoWorkEventArgs e) //send data
        {
            if (client.Connected)
            {
                client.Send(_data);
                sendstr = Encoding.Default.GetString(_data);    //_data는 byte형식이기 때문에, string으로 형변환이 필요하다.
                this.textBox3.Invoke(new MethodInvoker(delegate() { textBox3.AppendText("\n\n\nMe : " + sendstr + "\n\n\n"); }));
            }
            else
            {
                MessageBox.Show("Send failed!");
            }

            backgroundWorker2.CancelAsync();
        }



        private void button3_Click(object sender, EventArgs e)              //send Start Button
        {
            timer1_Tick(sender, e);
            timer1.Interval = 100; //1초에 한번 씩 자동으로 실행
            intervaltime = timer1.Interval;
            label10.Text = (intervaltime / 1000).ToString() + "초 간격으로 전송";
            timer1.Start();

        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            label10.Text = "";
            for (int i = 0; i < 10; i++)                                    //textbox 내용 초기화
            {
                TextBox target = (Controls.Find("text_n" + i.ToString(), true)[0] as TextBox);
                target.Text = "";

                target = (Controls.Find("text_h" + i.ToString(), true)[0] as TextBox);
                target.Text = "";

                target = (Controls.Find("text_d" + i.ToString(), true)[0] as TextBox);
                target.Text = "";

            }

            label10.Text = (intervaltime / 1000).ToString() + "초 간격으로 전송";
            _data = Encoding.Default.GetBytes(textBox4.Text);
            backgroundWorker2.RunWorkerAsync();
        }


        private void Form1_FormClosing(object sender, FormClosingEventArgs e)   //종료 버튼을 눌렀을 때
        {
            try
            {
                if (System.Windows.Forms.MessageBox.Show("종료하시겠습니까?", "종료", MessageBoxButtons.YesNo) == DialogResult.Yes)
                {
                    backgroundWorker1.Dispose();
                    client.Close();
                    e.Cancel = false;
                }
            }
            catch (Exception x)
            {
                MessageBox.Show(x.Message.ToString());
            }
        }

        private void button4_Click(object sender, EventArgs e)  //Send Stop Button
        {
            timer1.Stop();
            textBox4.Text = "";
        }


    }
}