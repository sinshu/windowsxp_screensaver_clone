using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace XpScreensaverClone
{
    public class Screensaver : Form
    {
        [DllImport("user32.dll")]
        static extern IntPtr SetParent(IntPtr hWndChild, IntPtr hWndNewParent);

        [DllImport("user32.dll")]
        static extern int SetWindowLong(IntPtr hWnd, int nIndex, IntPtr dwNewLong);

        [DllImport("user32.dll", SetLastError = true)]
        static extern int GetWindowLong(IntPtr hWnd, int nIndex);

        [DllImport("user32.dll")]
        static extern bool GetClientRect(IntPtr hWnd, out Rectangle lpRect);

        private Timer timer;

        private bool isPreviewMode;
        private Point originalMouseLocation;

        private Random random;
        private Point imageLocation;

        public Screensaver(IntPtr previewWindowHandle)
            : base()
        {
            BackColor = Color.Black;
            DoubleBuffered = true;
            FormBorderStyle = FormBorderStyle.None;
            if (previewWindowHandle == IntPtr.Zero)
            {
                Bounds = Screen.PrimaryScreen.Bounds;
                TopMost = true;
                Cursor.Hide();
                KeyPress += new KeyPressEventHandler(OnKeyPress);
                MouseClick += new MouseEventHandler(OnMouseClick);
                MouseMove += new MouseEventHandler(OnMouseMove);
            }
            else
            {
                SetParent(Handle, previewWindowHandle);
                SetWindowLong(Handle, -16,
                    new IntPtr(GetWindowLong(Handle, -16) | 0x40000000));
                Location = new Point(0, 0);
                Rectangle parentRect;
                GetClientRect(previewWindowHandle, out parentRect);
                Size = parentRect.Size;
                isPreviewMode = true;
            }
            Paint += new PaintEventHandler(OnPaint);

            originalMouseLocation = new Point(-1, -1);

            random = new Random();
            NextImageLocation();

            timer = new Timer();
            timer.Interval = 10000;
            timer.Tick += new EventHandler(OnTick);
            timer.Start();
        }

        ~Screensaver()
        {
            Dispose(false);
        }

        protected override void Dispose(bool disposing)
        {
            timer.Dispose();
            base.Dispose(disposing);
        }

        private void OnKeyPress(object sender, KeyPressEventArgs e)
        {
            Close();
        }

        private void OnMouseClick(object sender, MouseEventArgs e)
        {
            Close();
        }

        private void OnMouseMove(object sender, MouseEventArgs e)
        {
            if (originalMouseLocation == new Point(-1, -1))
            {
                originalMouseLocation = e.Location;
            }
            if (Math.Abs(e.X - originalMouseLocation.X) >= 10 ||
                Math.Abs(e.Y - originalMouseLocation.Y) >= 10)
            {
                Close();
            }
        }

        private void OnPaint(object sender, PaintEventArgs e)
        {
            if (!isPreviewMode)
            {
                e.Graphics.DrawImage(Resources.OitLogo,
                    new Rectangle(imageLocation, Resources.OitLogo.Size));
            }
            else
            {
                e.Graphics.DrawImage(Resources.OitLogo,
                    new Rectangle(imageLocation, new Size(32, 32)));
            }
        }

        private void OnTick(object sender, EventArgs e)
        {
            NextImageLocation();
            Refresh();
        }

        private void NextImageLocation()
        {
            if (!isPreviewMode)
            {
                imageLocation = new Point(random.Next(Size.Width - Resources.OitLogo.Size.Width),
                    random.Next(Size.Height - Resources.OitLogo.Size.Height));
            }
            else
            {
                imageLocation = new Point(random.Next(Size.Width - 32),
                    random.Next(Size.Height - 32));
            }
        }

        public static void Main(string[] args)
        {
            IntPtr previewWindowHandle;
            if (args.Length >= 1)
            {
                switch (args[0].Trim().Substring(0, 2).ToLower())
                {
                    case "/p":
                        previewWindowHandle = new IntPtr(long.Parse(args[1].Trim()));
                        break;
                    case "/c":
                        MessageBox.Show("ラ・ヨダソウ・スティアーナ", "それが世界の選択か");
                        return;
                    default:
                        previewWindowHandle = IntPtr.Zero;
                        break;
                }
            }
            else
            {
                previewWindowHandle = IntPtr.Zero;
            }
            using (Screensaver screensaver = new Screensaver(previewWindowHandle))
            {
                Application.Run(screensaver);
            }
        }
    }
}
