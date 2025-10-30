using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace MobiMeter;

public class FormOverlay : Form
{
	public struct RECT
	{
		public int Left;

		public int Top;

		public int Right;

		public int Bottom;
	}

	private const string TargetWindowTitle = "마비노기 모바일";

	private Timer timer;

	private double opacity = 0.8;

	private const int GWL_EXSTYLE = -20;

	private const int WS_EX_LAYERED = 524288;

	private const int WS_EX_TRANSPARENT = 32;

	private IContainer components;

	private PictureBox pictureBox1;

	public FormOverlay()
	{
		InitializeComponent();
		base.FormBorderStyle = FormBorderStyle.None;
		base.TopMost = true;
		base.ShowInTaskbar = false;
		BackColor = Color.Magenta;
		base.TransparencyKey = Color.Magenta;
		int exStyle = GetWindowLong(base.Handle, -20);
		SetWindowLong(base.Handle, -20, exStyle | 0x80000 | 0x20);
		pictureBox1.Location = new Point(0, 0);
		timer = new Timer();
		timer.Interval = 100;
		timer.Tick += Timer_Tick;
		timer.Start();
		base.Opacity = 0.0;
	}

	public static void DrawBar(PictureBox pictureBox1, double value, string text, Color color)
	{
		int width = pictureBox1.Width;
		int height = pictureBox1.Height;
		float percent = Math.Max(0f, Math.Min(1f, (float)(value / 100.0)));
		Bitmap bmp = new Bitmap(width, height, PixelFormat.Format32bppArgb);
		Graphics graphics = Graphics.FromImage(bmp);
		graphics.Clear(Color.Transparent);
		graphics.SmoothingMode = SmoothingMode.None;
		Brush bg = new SolidBrush(Color.FromArgb(255, 60, 60, 60));
		Brush hpBrush = new SolidBrush(color);
		Brush textBrush = Brushes.White;
		_ = SystemFonts.DefaultFont;
		Font font = new Font("Segoe UI", (float)height / 2f, FontStyle.Bold);
		graphics.FillRectangle(bg, 0, 0, width, height);
		int fillWidth = (int)((float)width * percent);
		graphics.FillRectangle(hpBrush, 0, 0, fillWidth, height);
		SizeF textSize = graphics.MeasureString(text, font);
		PointF pos = new PointF(((float)width - textSize.Width) / 2f, ((float)height - textSize.Height) / 2f);
		graphics.DrawString(text, font, textBrush, pos);
		bg.Dispose();
		hpBrush.Dispose();
		font.Dispose();
		graphics.Dispose();
		pictureBox1.Image?.Dispose();
		pictureBox1.Image = bmp;
	}

	public void UpdateUI()
	{
		int value = (int)Form1.dps;
		if (Form1.optionData.dpmMode)
		{
			value = (int)Form1.dpm;
			DrawBar(pictureBox1, Form1.sliderValue, $"{value}", Color.Green);
		}
		else
		{
			DrawBar(pictureBox1, Form1.sliderValue, $"{value}", Color.Red);
		}
	}

	private void Timer_Tick(object sender, EventArgs e)
	{
		nint hWnd = FindWindow(null, "마비노기 모바일");
		if (hWnd == IntPtr.Zero)
		{
			return;
		}
		if (GetWindowRect(hWnd, out var rect))
		{
			int num = rect.Right - rect.Left;
			int hei = rect.Bottom - rect.Top;
			int offsetX = (int)(0.0 - ((double)Math.Max(num, hei) * 0.1 + (double)hei * 0.1));
			int offsetY = hei / 6;
			int targetX = rect.Right - pictureBox1.Width + offsetX;
			int targetY = rect.Top + offsetY;
			double bar_wid = (double)num * 0.1;
			double bar_hei = bar_wid * 0.15;
			pictureBox1.Size = new Size((int)bar_wid, (int)bar_hei);
			int radius = 30;
			radius = (int)bar_hei;
			GraphicsPath path = new GraphicsPath();
			path.StartFigure();
			path.AddArc(new Rectangle(0, 0, radius, radius), 180f, 90f);
			path.AddArc(new Rectangle(base.Width - radius, 0, radius, radius), 270f, 90f);
			path.AddArc(new Rectangle(base.Width - radius, base.Height - radius, radius, radius), 0f, 90f);
			path.AddArc(new Rectangle(0, base.Height - radius, radius, radius), 90f, 90f);
			path.CloseFigure();
			base.Region = new Region(path);
			base.Location = new Point(targetX, targetY);
			base.Size = pictureBox1.Size;
			pictureBox1.Location = new Point(0, 0);
		}
		if (Form1.running)
		{
			UpdateUI();
		}
		if (Form1.optionData.overlayAlways)
		{
			base.Opacity = 0.5;
			return;
		}
		bool draw = false;
		if (Form1.optionData.useLastAttack)
		{
			DateTime now = DateTime.Now;
			DateTime lastAttackTime = DamageMeter.LastAttackTime();
			if (lastAttackTime < now && lastAttackTime.AddSeconds(10.0) > now)
			{
				draw = true;
			}
		}
		else if ((int)Form1.dps > 0)
		{
			draw = true;
		}
		if (draw)
		{
			base.Opacity = opacity;
		}
		else
		{
			base.Opacity = Math.Max(base.Opacity - 0.05, 0.0);
		}
	}

	[DllImport("user32.dll", SetLastError = true)]
	private static extern nint FindWindow(string lpClassName, string lpWindowName);

	[DllImport("user32.dll")]
	private static extern bool GetWindowRect(nint hWnd, out RECT lpRect);

	[DllImport("user32.dll")]
	private static extern int GetWindowLong(nint hWnd, int nIndex);

	[DllImport("user32.dll")]
	private static extern int SetWindowLong(nint hWnd, int nIndex, int dwNewLong);

	protected override void Dispose(bool disposing)
	{
		if (disposing && components != null)
		{
			components.Dispose();
		}
		base.Dispose(disposing);
	}

	private void InitializeComponent()
	{
		this.pictureBox1 = new System.Windows.Forms.PictureBox();
		((System.ComponentModel.ISupportInitialize)this.pictureBox1).BeginInit();
		base.SuspendLayout();
		this.pictureBox1.Location = new System.Drawing.Point(12, 26);
		this.pictureBox1.Name = "pictureBox1";
		this.pictureBox1.Size = new System.Drawing.Size(776, 412);
		this.pictureBox1.TabIndex = 0;
		this.pictureBox1.TabStop = false;
		base.AutoScaleDimensions = new System.Drawing.SizeF(7f, 15f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		base.ClientSize = new System.Drawing.Size(800, 450);
		base.Controls.Add(this.pictureBox1);
		base.Name = "FormOverlay";
		base.Opacity = 0.5;
		this.Text = "FormOverlay";
		((System.ComponentModel.ISupportInitialize)this.pictureBox1).EndInit();
		base.ResumeLayout(false);
	}
}
