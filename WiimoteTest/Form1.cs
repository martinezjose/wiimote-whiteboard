using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.Text;
using System.Windows.Forms;
using System.Diagnostics;
using System.Threading;
using System.Runtime.InteropServices;//for firing keyboard and mouse events (optional)
using System.IO;//for saving the reading the calibration data


using WiimoteLib;

namespace WiimoteWhiteboard
{
	public partial class Form1 : Form
	{
        //instance of the wii remote
		Wiimote wm = new Wiimote();

        const int smoothingBufferSize = 50;
        
        PointF[] smoothingBuffer = new PointF[smoothingBufferSize];
        int smoothingBufferIndex = 0;
        int smoothingAmount = 4;
        bool enableSmoothing = true;
        
        bool cursorControl = false;  

        int screenWidth = 1024;//defaults, gets replaced by actual screen size
        int screenHeight = 768;

        int calibrationState = 0;
        float calibrationMargin = .1f;

        CalibrationForm cf = null;

        Warper warper = new Warper();
        float[] srcX = new float[4];
        float[] srcY = new float[4];
        float[] dstX = new float[4];
        float[] dstY = new float[4];


        //declare consts for mouse messages


        const int INPUT_MOUSE = 0;
        const int INPUT_KEYBOARD = 1;
        const int INPUT_HARDWARE = 2;
        const uint KEYEVENTF_EXTENDEDKEY = 0x0001;
        const uint KEYEVENTF_KEYUP = 0x0002;
        const uint KEYEVENTF_UNICODE = 0x0004;
        const uint KEYEVENTF_SCANCODE = 0x0008;

        public const int MOUSEEVENTF_MOVE = 0x01;
        public const int MOUSEEVENTF_LEFTDOWN = 0x02;
        public const int MOUSEEVENTF_LEFTUP = 0x04;
        public const int MOUSEEVENTF_RIGHTDOWN = 0x08;
        public const int MOUSEEVENTF_RIGHTUP = 0x10;
        public const int MOUSEEVENTF_MIDDLEDOWN = 0x20;
        public const int MOUSEEVENTF_MIDDLEUP = 0x40;
        public const int MOUSEEVENTF_ABSOLUTE = 0x8000;

        //declare consts for key scan codes
        public const byte VK_TAB = 0x09;
        public const byte VK_MENU = 0x12; // VK_MENU is Microsoft talk for the ALT key
        public const byte VK_SPACE = 0x20;
        public const byte VK_RETURN = 0x0D;
        public const byte VK_LEFT  =0x25;
        public const byte VK_UP 	=0x26;
        public const byte VK_RIGHT 	=0x27;
        public const byte VK_DOWN 	=0x28;

        //for firing mouse and keyboard events








        struct INPUT
        {
            public int type;
            public InputUnion u;
        }

        [StructLayout(LayoutKind.Explicit)]
        struct InputUnion
        {
            [FieldOffset(0)]
            public MOUSEINPUT mi;
            [FieldOffset(0)]
            public KEYBDINPUT ki;
            [FieldOffset(0)]
            public HARDWAREINPUT hi;
        }

        [StructLayout(LayoutKind.Sequential)]
        struct MOUSEINPUT
        {
            public int dx;
            public int dy;
            public uint mouseData;
            public uint dwFlags;
            public uint time;
            public IntPtr dwExtraInfo;
        }

        [StructLayout(LayoutKind.Sequential)]
        struct KEYBDINPUT
        {
            /*Virtual Key code.  Must be from 1-254.  If the dwFlags member specifies KEYEVENTF_UNICODE, wVk must be 0.*/
            public ushort wVk;
            /*A hardware scan code for the key. If dwFlags specifies KEYEVENTF_UNICODE, wScan specifies a Unicode character which is to be sent to the foreground application.*/
            public ushort wScan;
            /*Specifies various aspects of a keystroke.  See the KEYEVENTF_ constants for more information.*/
            public uint dwFlags;
            /*The time stamp for the event, in milliseconds. If this parameter is zero, the system will provide its own time stamp.*/
            public uint time;
            /*An additional value associated with the keystroke. Use the GetMessageExtraInfo function to obtain this information.*/
            public IntPtr dwExtraInfo;
        }

        [StructLayout(LayoutKind.Sequential)]
        struct HARDWAREINPUT
        {
            public uint uMsg;
            public ushort wParamL;
            public ushort wParamH;
        }

        [DllImport("user32.dll")]
        static extern IntPtr GetMessageExtraInfo();

        [DllImport("user32.dll", SetLastError = true)]
        static extern uint SendInput(uint nInputs, INPUT[] pInputs, int cbSize);










        //imports mouse_event function from user32.dll

        [DllImport("user32.dll")]
        private static extern void mouse_event(
        long dwFlags, // motion and click options
        long dx, // horizontal position or change
        long dy, // vertical position or change
        long dwData, // wheel movement
        long dwExtraInfo // application-defined information
        );

        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool SetCursorPos(int X, int Y);

        //imports keybd_event function from user32.dll
        [DllImport("user32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        public static extern void keybd_event(byte bVk, byte bScan, long dwFlags, long dwExtraInfo);
        WiimoteState lastWiiState = new WiimoteState();//helps with event firing

        //end keyboard and mouse input emulation variables----------------------------------------

        Mutex mut = new Mutex();

		public Form1()
		{
            screenWidth = Screen.GetBounds(this).Width;
            screenHeight = Screen.GetBounds(this).Height;
            InitializeComponent();

            for (int i = 0; i < smoothingBufferSize; i++)
                smoothingBuffer[i] = new PointF();

            setSmoothing(smoothingAmount);
		}

        private void Form1_Load(object sender, EventArgs e)
		{
            //add event listeners to changes in the wiiremote
            //fired for every input report - usually 100 times per second if acclerometer is enabled
			wm.WiimoteChanged += new WiimoteChangedEventHandler(wm_OnWiimoteChanged); 
            //fired when the extension is attached on unplugged
			wm.WiimoteExtensionChanged += new WiimoteExtensionChangedEventHandler(wm_OnWiimoteExtensionChanged);
            
            try
            {
                //connect to wii remote
                wm.Connect();

                //set what features you want to enable for the remote, look at Wiimote.InputReport for options
                wm.SetReportType(Wiimote.InputReport.IRAccel, true);
                
                
                //set wiiremote LEDs with this enumerated ID
                wm.SetLEDs(true, false, false, false);
            }
            catch (Exception x)
            {
                MessageBox.Show("Exception: " + x.Message);
                this.Close();
            }
            loadCalibrationData();
		}

		void wm_OnWiimoteExtensionChanged(object sender, WiimoteExtensionChangedEventArgs args)
		{

            //if extension attached, enable it
			if(args.Inserted)
				wm.SetReportType(Wiimote.InputReport.IRExtensionAccel, true);
			else
				wm.SetReportType(Wiimote.InputReport.IRAccel, true);
		}

        float UpdateTrackingUtilization()
        {
            //area of ideal calibration coordinates (to match the screen)
            float idealArea = (1 - 2*calibrationMargin) * 1024 * (1 - 2*calibrationMargin) * 768;
            
            //area of quadrliatera
            float actualArea = 0.5f * Math.Abs((srcX[1] - srcX[2]) * (srcY[0] - srcY[3]) - (srcX[0] - srcX[3]) * (srcY[1] - srcY[2]));
            float util = (actualArea / idealArea)*100;
            BeginInvoke((MethodInvoker)delegate() { lblTrackingUtil.Text = util.ToString("f0"); });
            BeginInvoke((MethodInvoker)delegate() { pbTrackingUtil.Value = (int)util; });

            return util;

        }

        PointF getSmoothedCursor(int amount)
        {
            int start = smoothingBufferIndex - amount;
            if (start < 0)
                start = 0;
            PointF smoothed = new PointF(0,0);
            int count = smoothingBufferIndex - start;
            for (int i = start; i < smoothingBufferIndex; i++)
            {
                smoothed.X += smoothingBuffer[i%smoothingBufferSize].X;
                smoothed.Y += smoothingBuffer[i % smoothingBufferSize].Y;
            }
            smoothed.X /= count;
            smoothed.Y /= count;
            return smoothed;
        }

        void wm_OnWiimoteChanged(object sender, WiimoteChangedEventArgs args)
		{
            mut.WaitOne();

            //extract the wiimote state
            WiimoteState ws = args.WiimoteState;

            if (ws.IRState.Found1)
            {
                int x = ws.IRState.RawX1;
                int y = ws.IRState.RawY1;
                float warpedX = x;
                float warpedY = y;
                warper.warp(x, y, ref warpedX, ref warpedY);

                smoothingBuffer[smoothingBufferIndex % smoothingBufferSize].X = warpedX;
                smoothingBuffer[smoothingBufferIndex % smoothingBufferSize].Y = warpedY;
                smoothingBufferIndex++;

                if (!lastWiiState.IRState.Found1)//mouse down
                {
                    lastWiiState.IRState.Found1 = ws.IRState.Found1;
                    smoothingBufferIndex = 0;//resets the count

                    if (cursorControl)
                    {
                        INPUT[] buffer = new INPUT[2];

      
                        buffer[0].type = INPUT_MOUSE;
                        buffer[0].u = new InputUnion();
                        buffer[0].u.mi = new MOUSEINPUT();

                        buffer[0].u.mi.dx = (int)(warpedX *65535.0f/screenWidth);
                        buffer[0].u.mi.dy = (int)(warpedY * 65535.0f / screenHeight);
                        buffer[0].u.mi.mouseData = 0;
                        buffer[0].u.mi.dwFlags = MOUSEEVENTF_ABSOLUTE | MOUSEEVENTF_MOVE;
                        buffer[0].u.mi.time = 0;
                        buffer[0].u.mi.dwExtraInfo = (IntPtr)0;

                        buffer[1].type = INPUT_MOUSE;
                        buffer[1].u = new InputUnion();
                        buffer[1].u.mi = new MOUSEINPUT();

                        buffer[1].u.mi.dx = 0;
                        buffer[1].u.mi.dy = 0;
                        buffer[1].u.mi.mouseData = 0;
                        buffer[1].u.mi.dwFlags = MOUSEEVENTF_LEFTDOWN;
                        buffer[1].u.mi.time = 1;
                        buffer[1].u.mi.dwExtraInfo = (IntPtr)0;

                        SendInput(2, buffer, Marshal.SizeOf(buffer[0]));

                    }//cusor control

                    switch (calibrationState)
                    {
                        case 1:
                            srcX[calibrationState - 1] = x;
                            srcY[calibrationState - 1] = y;
                            calibrationState = 2;
                            doCalibration();
                            break;
                        case 2:
                            srcX[calibrationState - 1] = x;
                            srcY[calibrationState - 1] = y;
                            calibrationState = 3;
                            doCalibration();
                            break;
                        case 3:
                            srcX[calibrationState - 1] = x;
                            srcY[calibrationState - 1] = y;
                            calibrationState = 4;
                            doCalibration();
                            break;
                        case 4:
                            srcX[calibrationState - 1] = x;
                            srcY[calibrationState - 1] = y;
                            calibrationState = 5;
                            doCalibration();
                            break;
                        default:
                            break;
                    }//calibtation state
                }//mouse down                
                else
                {
                    if (cursorControl)//dragging
                    {
                        INPUT[] buffer = new INPUT[1];
                        buffer[0].type = INPUT_MOUSE;
                        if (enableSmoothing)
                        {
                            PointF s = getSmoothedCursor(smoothingAmount);
                            buffer[0].u.mi.dx = (int)(s.X * 65535.0f / screenWidth);
                            buffer[0].u.mi.dy = (int)(s.Y * 65535.0f / screenHeight);
                        }
                        else
                        {
                            buffer[0].u.mi.dx = (int)(warpedX * 65535.0f / screenWidth);
                            buffer[0].u.mi.dy = (int)(warpedY * 65535.0f / screenHeight);
                        }
                        buffer[0].u.mi.mouseData = 0;
                        buffer[0].u.mi.dwFlags = MOUSEEVENTF_ABSOLUTE | MOUSEEVENTF_MOVE;
                        buffer[0].u.mi.time = 0;
                        buffer[0].u.mi.dwExtraInfo = (IntPtr)0;
                        SendInput(1, buffer, Marshal.SizeOf(buffer[0]));

                    }
                }
            }//ir visible
            else
            {
                if (lastWiiState.IRState.Found1)//mouse up
                {
                    lastWiiState.IRState.Found1 = ws.IRState.Found1;
                    if (cursorControl)
                    {
                        INPUT[] buffer = new INPUT[2];
                        buffer[0].type = INPUT_MOUSE;
                        buffer[0].u.mi.dx = 0;
                        buffer[0].u.mi.dy = 0;
                        buffer[0].u.mi.mouseData = 0;
                        buffer[0].u.mi.dwFlags = MOUSEEVENTF_LEFTUP;
                        buffer[0].u.mi.time = 0;
                        buffer[0].u.mi.dwExtraInfo = (IntPtr)0;

                        buffer[1].type = INPUT_MOUSE;
                        buffer[1].u.mi.dx = 0;
                        buffer[1].u.mi.dy = 0;
                        buffer[1].u.mi.mouseData = 0;
                        buffer[1].u.mi.dwFlags = MOUSEEVENTF_MOVE;
                        buffer[1].u.mi.time = 0;
                        buffer[1].u.mi.dwExtraInfo = (IntPtr)0;

                        SendInput(2, buffer, Marshal.SizeOf(buffer[0]));

                    }
                }//ir lost
            }
            if (!lastWiiState.ButtonState.A && ws.ButtonState.A)
            {
                BeginInvoke((MethodInvoker)delegate() { btnCalibrate.PerformClick(); });
            }
            lastWiiState.ButtonState.A = ws.ButtonState.A;

            if (!lastWiiState.ButtonState.B && ws.ButtonState.B)
                keybd_event(VK_SPACE, 0x45, 0, 0);
            if (lastWiiState.ButtonState.B && !ws.ButtonState.B)
                keybd_event(VK_SPACE, 0x45, KEYEVENTF_KEYUP, 0);
            lastWiiState.ButtonState.B = ws.ButtonState.B;

            if (!lastWiiState.ButtonState.Up && ws.ButtonState.Up)
                keybd_event(VK_UP, 0x45, 0, 0);
            if (lastWiiState.ButtonState.Up && !ws.ButtonState.Up)
                keybd_event(VK_UP, 0x45, KEYEVENTF_KEYUP, 0);
            lastWiiState.ButtonState.Up = ws.ButtonState.Up;

            if (!lastWiiState.ButtonState.Down && ws.ButtonState.Down)
                keybd_event(VK_DOWN, 0x45, 0, 0);
            if (lastWiiState.ButtonState.Down && !ws.ButtonState.Down)
                keybd_event(VK_DOWN, 0x45, KEYEVENTF_KEYUP, 0);
            lastWiiState.ButtonState.Down = ws.ButtonState.Down;

            if (!lastWiiState.ButtonState.Left && ws.ButtonState.Left)
                keybd_event(VK_LEFT, 0x45, 0, 0);
            if (lastWiiState.ButtonState.Left && !ws.ButtonState.Left)
                keybd_event(VK_LEFT, 0x45, KEYEVENTF_KEYUP, 0);
            lastWiiState.ButtonState.Left = ws.ButtonState.Left;

            if (!lastWiiState.ButtonState.Right && ws.ButtonState.Right)
                keybd_event(VK_RIGHT, 0x45, 0, 0);
            if (lastWiiState.ButtonState.Right && !ws.ButtonState.Right)
                keybd_event(VK_RIGHT, 0x45, KEYEVENTF_KEYUP, 0);
            lastWiiState.ButtonState.Right = ws.ButtonState.Right;


            lastWiiState.IRState.Found1 = ws.IRState.Found1;
            lastWiiState.IRState.RawX1 = ws.IRState.RawX1;
            lastWiiState.IRState.RawY1 = ws.IRState.RawY1;
            lastWiiState.IRState.Found2 = ws.IRState.Found2;
            lastWiiState.IRState.RawX2 = ws.IRState.RawX2;
            lastWiiState.IRState.RawY2 = ws.IRState.RawY2;
            lastWiiState.IRState.Found3 = ws.IRState.Found3;
            lastWiiState.IRState.RawX3 = ws.IRState.RawX3;
            lastWiiState.IRState.RawY3 = ws.IRState.RawY3;
            lastWiiState.IRState.Found4 = ws.IRState.Found4;
            lastWiiState.IRState.RawX4 = ws.IRState.RawX4;
            lastWiiState.IRState.RawY4 = ws.IRState.RawY4;


            //draw battery value on GUI

            try
            {

                BeginInvoke((MethodInvoker)delegate () { pbBattery.Value = (ws.Battery > 0xc8 ? 0xc8 : (int)ws.Battery); });
                float f = (((100.0f * 48.0f * (float)(ws.Battery / 48.0f))) / 192.0f);
                BeginInvoke((MethodInvoker)delegate () { lblBattery.Text = f.ToString("f0") + "%"; });

                //check the GUI check boxes if the IR dots are visible
                String irstatus = "Visible IR dots: ";
                if (ws.IRState.Found1)
                    irstatus += "1 ";
                if (ws.IRState.Found2)
                    irstatus += "2 ";
                if (ws.IRState.Found3)
                    irstatus += "3 ";
                if (ws.IRState.Found4)
                    irstatus += "4 ";

                BeginInvoke((MethodInvoker)delegate () { lblIRvisible.Text = irstatus; });
            }catch
            {


            }
            mut.ReleaseMutex();        
        }


        public void loadCalibrationData()
        {
            // create reader & open file
            try
            {
                TextReader tr = new StreamReader("calibration.dat");
                for (int i = 0; i < 4; i++)
                {
                    srcX[i] = float.Parse(tr.ReadLine());
                    srcY[i] = float.Parse(tr.ReadLine());
                }
                smoothingAmount = int.Parse(tr.ReadLine());

                // close the stream
                tr.Close();
            }
            catch (Exception x)
            {
                //no prexsting calibration
                return;
            }

            warper.setDestination(  screenWidth * calibrationMargin,
                                    screenHeight * calibrationMargin,
                                    screenWidth * (1.0f-calibrationMargin),
                                    screenHeight * calibrationMargin,
                                    screenWidth * calibrationMargin,
                                    screenHeight * (1.0f - calibrationMargin),
                                    screenWidth * (1.0f - calibrationMargin),
                                    screenHeight * (1.0f - calibrationMargin));
            warper.setSource(srcX[0], srcY[0], srcX[1], srcY[1], srcX[2], srcY[2], srcX[3], srcY[3]);

            warper.computeWarp();

            setSmoothing(smoothingAmount);

            cursorControl = true;
            BeginInvoke((MethodInvoker)delegate() { cbCursorControl.Checked = cursorControl; });

            UpdateTrackingUtilization();
        }

        public void saveCalibrationData()
        {
            TextWriter tw = new StreamWriter("calibration.dat");

            // write a line of text to the file
            for (int i = 0; i < 4; i++)
            {
                tw.WriteLine(srcX[i]);
                tw.WriteLine(srcY[i]);
            }
            tw.WriteLine(smoothingAmount);
            // close the stream
            tw.Close();
        }

        public void doCalibration(){
            if (cf == null)
                return;
            int x = 0;
            int y = 0;
            int size = 25;
            Pen p = new Pen(Color.Red);
            switch (calibrationState)
            {
                case 1:
                    x = (int)(screenWidth * calibrationMargin);
                    y = (int)(screenHeight * calibrationMargin);
                    cf.showCalibration(x, y, size, p);
                    dstX[calibrationState - 1] = x;
                    dstY[calibrationState - 1] = y;
                    break;
                case 2:
                    x = screenWidth - (int)(screenWidth * calibrationMargin);
                    y = (int)(screenHeight * calibrationMargin);
                    cf.showCalibration(x, y, size, p);
                    dstX[calibrationState - 1] = x;
                    dstY[calibrationState - 1] = y;
                    break;
                case 3:
                    x = (int)(screenWidth * calibrationMargin);
                    y = screenHeight -(int)(screenHeight * calibrationMargin);
                    cf.showCalibration(x, y, size, p);
                    dstX[calibrationState - 1] = x;
                    dstY[calibrationState - 1] = y;
                    break;
                case 4:
                    x = screenWidth - (int)(screenWidth * calibrationMargin);
                    y = screenHeight -(int)(screenHeight * calibrationMargin);
                    cf.showCalibration(x, y, size, p);
                    dstX[calibrationState - 1] = x;
                    dstY[calibrationState - 1] = y;
                    break;
                case 5:
                    //compute warp
                    warper.setDestination(dstX[0], dstY[0], dstX[1], dstY[1], dstX[2], dstY[2], dstX[3], dstY[3]);
                    warper.setSource(srcX[0], srcY[0], srcX[1], srcY[1], srcX[2], srcY[2], srcX[3], srcY[3]);
                    warper.computeWarp();
                    BeginInvoke((MethodInvoker)delegate () {
                        cf.Close();
                    });

                    calibrationState = 0;
                    cursorControl = true;
                    BeginInvoke((MethodInvoker)delegate() { cbCursorControl.Checked = cursorControl; });
//                    saveCalibrationData();
                    UpdateTrackingUtilization();
                    break;
                default:
                    break;
            }

        }

		private void Form1_FormClosed(object sender, FormClosedEventArgs e)
		{
            //disconnect the wiimote
			wm.Disconnect();
            saveCalibrationData();
		}

        private void btnCalibrate_Click(object sender, EventArgs e)
        {
            if (cf == null)
            {
                cf = new CalibrationForm();
                cf.Show();
            }
            if (cf.IsDisposed)
            {
                cf = new CalibrationForm();
                cf.Show();
            }
            cursorControl = false;
            calibrationState = 1;
            doCalibration();
        }

        private void cbCursorControl_CheckedChanged(object sender, EventArgs e)
        {
            cursorControl = cbCursorControl.Checked;
        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void setSmoothing(int smoothing)
        {
            smoothingAmount = smoothing;
            trackBar1.Value = smoothing;
            enableSmoothing = (smoothingAmount != 0);
            lblSmoothing.Text = "Smoothing: " + smoothingAmount;
        }

        private void trackBar1_Scroll(object sender, EventArgs e)
        {

            smoothingAmount = trackBar1.Value;
            enableSmoothing = (smoothingAmount != 0);
            lblSmoothing.Text = "Smoothing: " + smoothingAmount;
        }

        private void lblIRvisible_Click(object sender, EventArgs e)
        {

        }
	}
}