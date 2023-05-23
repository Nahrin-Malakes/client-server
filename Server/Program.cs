using System; //Exception, Environment, EventHandler
using System.Windows.Forms; //Label, MessageBox
using System.Drawing; //Color, Font Size, Point
using System.IO; //File
using System.Text; //Encoding
using System.Diagnostics; //Process
using System.Threading; //Thread
using System.Net.Sockets; //UdpClient
using System.Net; //IPEndPoint, udp
using System.Drawing.Imaging; //Image

//class Queue
class Queue<T>
{
    private T[] x;
    private int n, p;

    //Stack
    public Queue()
    {
        n = 0; //makom avur ereh haba
        p = 0; //rosh hator
        x = new T[100];
    }

    //Head
    public T Head()
    {
        return x[p];
    }

    //Remove
    public T Remove()
    {
        p++;
        return x[p - 1];
    }

    //Insert
    public void Insert(T k)
    {
        x[n++] = k;
    }

    //IsEmpty
    public bool IsEmpty()
    {
        if (n == p)
            return true;
        else
            return false;
    }

    //ToString
    public override string ToString()// After: q.Insert(1); q.Insert(2); q.Insert(3); we have: ---> <3 2 1> --->
    {
        string s = "< ";
        for (int i = n - 1; i >= p; i--)
        {
            s += x[i] + " ";
        }
        return s + ">";
    }
}

//класс MyForm
class MyForm : Form
{
    //кнопки и лейбл
    private Button btn1, btn2;
    private Label lb;
    private int N;
    private Queue<double> q;

    //сокет
    private Thread rec;
    private UdpClient udp;
    private bool stopReceive;

    //1. конструктор
    public MyForm()
    {
        N = 0;
        q = new Queue<double>();

        //сокет
        rec = null;
        udp = new UdpClient(15000); //15000 - порт сервера
        stopReceive = false;

        //форма
        ClientSize = new System.Drawing.Size(360, 400); //size of my form
        Text = "שרת";
        ControlBox = true; //false - remove all the Control Buttons (e.g. Minimize, Maximize, Exit) and also the icon
        StartPosition = FormStartPosition.Manual;
        BackColor = Color.Azure;
        ShowInTaskbar = false;
        Left = 1305; //координаты формы
        Top = 30; //координаты формы
        ControlBox = false;
        MinimizeBox = false;
        MaximizeBox = false;

        //кнопка 1
        btn1 = new Button();
        btn1.Enabled = true;
        btn1.Text = "Start";
        btn1.Font = new Font("Arial", 15.5F, FontStyle.Bold | FontStyle.Italic, GraphicsUnit.Point);
        btn1.Size = new Size(70, 70);
        btn1.FlatAppearance.BorderSize = 0;
        btn1.FlatStyle = FlatStyle.Flat;
        btn1.BackColor = Color.Blue;
        btn1.FlatAppearance.BorderColor = Color.Blue; //border color
        btn1.ForeColor = Color.White;
        btn1.Location = new Point(150, 120); //coordinates of my button
        Controls.Add(btn1);
        btn1.Click += new EventHandler(btn1_Click);

        //кнопка 2
        btn2 = new Button();
        btn2.Enabled = true;
        btn2.Text = "Stop";
        btn2.Font = new Font("Arial", 15.5F, FontStyle.Bold | FontStyle.Italic, GraphicsUnit.Point);
        btn2.Size = new Size(70, 70);
        btn2.FlatAppearance.BorderSize = 0;
        btn2.FlatStyle = FlatStyle.Flat;
        btn2.BackColor = Color.Black;
        btn2.FlatAppearance.BorderColor = Color.Black; //border color
        btn2.ForeColor = Color.White;
        btn2.Location = new Point(150, 250); //coordinates of my button
        Controls.Add(btn2);
        btn2.Click += new EventHandler(btn2_Click);

        //лейбл
        lb = new Label();
        lb.AutoSize = true;
        lb.Font = new System.Drawing.Font("Guttman-Aharoni", 30.75F, FontStyle.Bold | FontStyle.Italic, GraphicsUnit.Point, (byte)0);
        lb.Location = new System.Drawing.Point(130, 30); //coordinates of the label
        lb.Size = new System.Drawing.Size(352, 24);
        lb.Text = "שרת";
        lb.ForeColor = Color.Black;
        Controls.Add(lb);
    }

    //2. обработчик первой кнопки Start
    private void btn1_Click(object sender, EventArgs e)
    {
        stopReceive = false;
        rec = new Thread(new ThreadStart(ReceiveMessage));
        rec.Start();
        lb.Text = "השרת מאזין";
        lb.ForeColor = Color.Red;
        btn1.BackColor = Color.Green;
        btn1.FlatAppearance.BorderColor = Color.Green; //border color
        btn2.BackColor = Color.Goldenrod;
        btn2.FlatAppearance.BorderColor = Color.Goldenrod; //border color
        btn1.Visible = false;
        btn2.Location = new Point(150, 180); //coordinates of my button
        lb.Location = new System.Drawing.Point(11, 70);
    }

    //3. обработчик второй кнопки Stop
    private void btn2_Click(object sender, EventArgs e)
    {
        stopReceive = true;
        if (udp != null) udp.Close();
        if (rec != null) rec.Join();
        lb.Text = "Server";
        lb.Location = new System.Drawing.Point(110, 30);
        Close();
    }

    //4. функция ReceiveMessage для получения данныx от клиента
    void ReceiveMessage()
    {
        try
        {
            while (true)
            {
                IPEndPoint ipendpoint = null;
                byte[] messageByte = udp.Receive(ref ipendpoint);//ipendpoint=192.168.1.186:64000, где 64000 - порта клиента               
                string messageString = Encoding.UTF8.GetString(messageByte);

                if (messageString.Equals("sort"))
                {
                    //create arr[]
                    double[] arr = new double[N];
                    int j = 0;                    
                    while (!q.IsEmpty())
                    {
                        double x = q.Remove();
                        arr[j++] = x;
                    }

                    //sort arr[]
                    BubbleSort(arr);

                    //create list to send
                    string mehirim = "Ordered prices:\n\n";
                    for (int i = 0; i < arr.Length; i++)
                    {
                        mehirim += arr[i] + "\n";
                    }
                    udpSend(ipendpoint.Address.ToString(), 15000, mehirim);//где ipendpoint.Address.ToString() = 192.168.1.186 - ip клиента
                }
                else
                {
                    int x = messageString.IndexOf("Price:");
                    string myPriceTemp = messageString.Substring(x + 6);
                    int y = myPriceTemp.IndexOf("+");
                    string myPrice = myPriceTemp.Substring(0, y);
                    MessageBox.Show((N + 1) + ") " + myPrice + "");
                    q.Insert(double.Parse(myPrice));
                    N++;
                }

                if (stopReceive == true) break;
            }
        }
        catch (Exception e)
        {
            MessageBox.Show(e.Message);
        }
    }

    private int udpSend(string ipAddress, int port, string myMessage)
    {
        byte[] myMessageBytes = Encoding.UTF8.GetBytes(myMessage);
        UdpClient udp = new UdpClient();
        IPAddress ipad = IPAddress.Parse(ipAddress);
        IPEndPoint ipen = new IPEndPoint(ipad, port);
        int send = udp.Send(myMessageBytes, myMessageBytes.Length, ipen);
        udp.Close();
        return send;
    }

    public static void BubbleSort(double[] a)
    {
        for (int j = 0; j < a.Length - 1; j++)
        {
            for (int i = 0; i < a.Length - 1; i++)
            {
                if (a[i] > a[i + 1])
                {
                    double t = a[i];
                    a[i] = a[i + 1];
                    a[i + 1] = t;
                }
            }
        }
    }

    //5. Главный модуль 
    [STAThread]
    static void Main()
    {
        Application.Run(new MyForm());
    }
}
