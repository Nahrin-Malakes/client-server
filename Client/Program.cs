using System; //EventHandler
using System.Windows.Forms; //Form
using System.Drawing; //Point
using System.Text; //Encoding
using System.IO; //TextReader
using System.Drawing.Drawing2D; //GraphicsPath
using System.Net; //IPAddress, IPEndpoint
using System.Net.Sockets; //UdpClient
using System.Threading; //Sleep

//class CircularButton
class CircularButton : Button
{
    protected override void OnPaint(PaintEventArgs pevent)
    {
        GraphicsPath gp = new GraphicsPath();
        gp.AddEllipse(0, 0, ClientSize.Width, ClientSize.Height);
        this.Region = new Region(gp);
        base.OnPaint(pevent);
    }
}

class MyForm : Form
{
    private CircularButton bt2;
    private Label lb, lb1;
    private TextBox tbox;

    private bool stopReceive;
    private Thread rec;
    private UdpClient udp;

    private MainMenu menu;

    private int CLIENT_PORT;
    private int SERVER_PORT;
    private string SERVER_IP;

    public MyForm()
    {
        //ports and server ip
        CLIENT_PORT = 15000;
        SERVER_PORT = 15000;
        SERVER_IP = "192.168.1.47";

        //UdpClient
        rec = null;
        udp = new UdpClient(CLIENT_PORT); //15000 - client port (to receive a message from a server)
        stopReceive = false;

        //form
        StartPosition = FormStartPosition.Manual;
        Top = 30;
        Left = 1275;
        ClientSize = new Size(420, 300); //form size
        BackColor = Color.FromArgb(39, 55, 77);//form color
        FormBorderStyle = FormBorderStyle.Fixed3D;
        Text = "משתמש";
        ControlBox = false;
        MinimizeBox = false;
        MaximizeBox = false;

        //button2
        bt2 = new CircularButton();
        bt2.Size = new Size(120, 70);
        bt2.Location = new Point(160, 190);
        bt2.Font = new Font("Ariel", 15F, FontStyle.Bold, GraphicsUnit.Point, (byte)0);
        bt2.Text = "שלח";
        bt2.BackColor = Color.Black;
        bt2.ForeColor = Color.White;
        bt2.FlatStyle = FlatStyle.Flat;
        bt2.FlatAppearance.BorderColor = Color.Black;
        bt2.FlatAppearance.BorderSize = 0;
        bt2.Visible = false;
        Controls.Add(bt2);
        bt2.Click += new EventHandler(bt2_Click);

        //label
        lb = new Label();
        lb.AutoSize = true;
        lb.Font = new Font("Ariel", 35F, FontStyle.Bold | FontStyle.Italic, GraphicsUnit.Point, (byte)0);
        lb.Location = new Point(80, 45); //location label
        lb.Text = "בחר קובץ";
        lb.ForeColor = Color.Olive;
        Controls.Add(lb);

        //label1
        lb1 = new Label();
        lb1.AutoSize = true;
        lb1.Font = new Font("Ariel", 10F, FontStyle.Bold, GraphicsUnit.Point, (byte)0);
        lb1.Location = new Point(5, 135); //location label
        lb1.Text = "שם של הקובץ";
        lb1.ForeColor = Color.Black;
        Controls.Add(lb1);
        
        //textbox 
        tbox = new TextBox();
        tbox.Location = new Point(100, 130); //textbox location
        tbox.TabIndex = 0;
        tbox.ClientSize = new Size(250, 1);
        tbox.BackColor = SystemColors.GradientActiveCaption;
        tbox.ForeColor = Color.Black; //font color
        Font myfont = new Font("Arial", 14.0f); //font size
        tbox.Font = myfont;
        tbox.RightToLeft = RightToLeft.No;
        tbox.BorderStyle = BorderStyle.FixedSingle;
        tbox.Text = "";
        Controls.Add(tbox);

        //menu
        this.menu = new MainMenu();
        MenuItem m1 = new MenuItem("תפריט");
        this.menu.MenuItems.Add(m1);
        MenuItem i1 = new MenuItem("תתחיל");
        m1.MenuItems.Add(i1);
        MenuItem i2 = new MenuItem("תעצור");
        m1.MenuItems.Add(i2);
        i1.Click += Start;
        i2.Click += Stop;
        Menu = menu;
    }

    //5. button 2 event (שלח)
    private void bt2_Click(object sender, EventArgs e)
    {
        if (tbox.Text.Trim().Equals("sort"))
        {
            udpSend(SERVER_IP, SERVER_PORT, "sort");
            tbox.Text = "";
        }
        else
        {
            TextReader t = new StreamReader("C:\\Nahrin Malakes\\client-server\\data\\" + tbox.Text.Trim() + ".txt");
            string mymessage = t.ReadToEnd();
            t.Close();
            udpSend(SERVER_IP, SERVER_PORT, mymessage);
            tbox.Text = "";
        }            
    }

    //6. udpSend
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

    //7. ReceiveMessage
    void ReceiveMessage()
    {
        try
        {
            while (true)
            {
                IPEndPoint ipendpoin = null;
                byte[] messageFromServerByte = udp.Receive(ref ipendpoin);
                string messageFromServerStr = Encoding.UTF8.GetString(messageFromServerByte);
                MessageBox.Show(messageFromServerStr);
                if (stopReceive == true) break;
            }
        }
        catch (Exception e)
        {
            MessageBox.Show(e.Message);
        }
    }

    //8. Start 
    private void Start(object sender, EventArgs e)
    {
        stopReceive = false;
        rec = new Thread(new ThreadStart(ReceiveMessage));
        rec.Start();
        lb.Text = "אני כבר מתחיל";
        lb.Location = new Point(65, 45); //location label
        bt2.Visible = true;
    }

    //9. Stop
    private void Stop(object sender, EventArgs e)
    {
        stopReceive = true;
        if (udp != null)
            udp.Close();
        if (rec != null)
            rec.Join();
        Close();
    }

    //10. main
    [STAThread]
    static void Main()
    {
        Application.Run(new MyForm());
    }
}
