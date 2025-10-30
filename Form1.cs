using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Media;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using MobiMeter.Properties;

namespace MobiMeter;

public class Form1 : Form
{
	private PacketCapture packetCapture;

	private Timer updateTimer;

	private Timer updateTimer1000;

	private Timer updateTimerDPS;

	public static OptionData optionData = null;

	private Size button1OriginalSize;

	private Point button1OriginalLocation;

	private Size button4OriginalSize;

	private Point button4OriginalLocation;

	private Image button4Start;

	private Image button4Stop;

	public static bool running = true;

	private static DateTime timerStart = DateTime.MaxValue;

	private static Image multitargetImage = null;

	private static Image singletargetImage = null;

	private static Image barImageDamage = null;

	private static Image barImageHeal = null;

	private int loadCount;

	private static int Update100Timer = 0;

	public static double sliderValue = 0.0;

	public static double dps = 0.0;

	public static double dpm = 0.0;

	public static double targetDPS = 0.0;

	public static double targetDPM = 0.0;

	public static double maxDPS = 0.0;

	public static double maxDPM = 0.0;

	public static double totalDamage = 0.0;

	private static int Update1000Timer = 0;

	private FormOverlay overlay;

	private IContainer components;

	private Button button1;

	private PictureBox pictureBox1;

	private Button button2;

	private PictureBox pictureBox2;

	private TextBox textBox1;

	private PictureBox pictureBox3;

	private Button button3;

	private Button button4;

	private Button button5_multitarget;

	public void Beep()
	{
		new SoundPlayer(Resources.Beep).Play();
	}

	public Form1()
	{
		InitializeComponent();
		barImageDamage = pictureBox1.Image;
		barImageHeal = Resources.HP_Bar_8;
		pictureBox1.Image = null;
		optionData = OptionManager.Load();
		multitargetImage = button5_multitarget.Image;
		singletargetImage = button5_multitarget.BackgroundImage;
		button5_multitarget.BackgroundImage = null;
		UpdateButton5();
		RoundedForm(this);
		packetCapture = new PacketCapture();
		packetCapture.Start();
		updateTimer = new Timer();
		updateTimer.Interval = 100;
		updateTimer.Tick += delegate
		{
			Update100();
		};
		updateTimer.Start();
		updateTimer1000 = new Timer();
		updateTimer1000.Interval = 1000;
		updateTimer1000.Tick += delegate
		{
			Update1000();
		};
		updateTimer1000.Start();
		textBox1.ReadOnly = true;
		textBox1.TabStop = false;
		button4Start = button4.Image;
		button4Stop = button4.BackgroundImage;
		button4.BackgroundImage = null;
		MakeButtonCircle(button4);
		UpdateButton4();
		button1OriginalSize = button1.Size;
		button1OriginalLocation = button1.Location;
		button4OriginalSize = button4.Size;
		button4OriginalLocation = button4.Location;
		button1.MouseEnter += Button_MouseEnter;
		button1.MouseLeave += Button_MouseLeave;
		button4.MouseEnter += Button_MouseEnter;
		button4.MouseLeave += Button_MouseLeave;
		OpenFormDetail();
	}

	public static void RoundedForm(Form form)
	{
		int radius = 32;
		GraphicsPath path = new GraphicsPath();
		path.StartFigure();
		path.AddArc(new Rectangle(0, 0, radius, radius), 180f, 90f);
		path.AddArc(new Rectangle(form.Width - radius, 0, radius, radius), 270f, 90f);
		path.AddArc(new Rectangle(form.Width - radius, form.Height - radius, radius, radius), 0f, 90f);
		path.AddArc(new Rectangle(0, form.Height - radius, radius, radius), 90f, 90f);
		path.CloseFigure();
		form.Region = new Region(path);
	}

	private void Button_MouseEnter(object sender, EventArgs e)
	{
		if (sender is Button btn)
		{
			int inflate = 6;
			if (btn == button1)
			{
				btn.Size = new Size(button1OriginalSize.Width + inflate, button1OriginalSize.Height + inflate);
				btn.Location = new Point(button1OriginalLocation.X - inflate / 2, button1OriginalLocation.Y - inflate / 2);
			}
			else if (btn == button4)
			{
				btn.Size = new Size(button4OriginalSize.Width + inflate, button4OriginalSize.Height + inflate);
				btn.Location = new Point(button4OriginalLocation.X - inflate / 2, button4OriginalLocation.Y - inflate / 2);
				MakeButtonCircle(btn);
			}
		}
	}

	private void Button_MouseLeave(object sender, EventArgs e)
	{
		if (sender is Button btn)
		{
			if (btn == button1)
			{
				btn.Size = button1OriginalSize;
				btn.Location = button1OriginalLocation;
			}
			else if (btn == button4)
			{
				btn.Size = button4OriginalSize;
				btn.Location = button4OriginalLocation;
				MakeButtonCircle(btn);
			}
		}
	}

	private void MakeButtonCircle(Button btn)
	{
		int diameter = Math.Min(btn.Width, btn.Height);
		int border = (int)((double)diameter * 0.05);
		diameter -= border;
		GraphicsPath path = new GraphicsPath();
		path.AddEllipse(border / 2, border / 2, diameter, diameter);
		btn.Region = new Region(path);
		btn.FlatStyle = FlatStyle.Flat;
		btn.FlatAppearance.BorderSize = 0;
		btn.ForeColor = Color.White;
	}

	public void UpdateButton5()
	{
		if (optionData.multiTarget)
		{
			button5_multitarget.Image = multitargetImage;
		}
		else
		{
			button5_multitarget.Image = singletargetImage;
		}
	}

	public void UpdateButton4()
	{
		if (running)
		{
			button4.Image = button4Stop;
			return;
		}
		button4.Image = button4Start;
		DateTime now = DateTime.Now;
		if (timerStart < now)
		{
			double total = (now - timerStart).TotalSeconds;
			double num = 3.0 - total;
			button4.Image = Resources.StartButton1;
			if (num > 1.0)
			{
				button4.Image = Resources.StartButton2;
			}
			if (num > 2.0)
			{
				button4.Image = Resources.StartButton3;
			}
			if (num < 0.0)
			{
				running = true;
				timerStart = DateTime.MaxValue;
				Beep();
			}
		}
	}

	private void button4_Click(object sender, EventArgs e)
	{
		StartStopButton();
	}

	private void StartStopButton()
	{
		if (!running)
		{
			if (optionData.dpmStopWatch)
			{
				if (timerStart > DateTime.Now)
				{
					timerStart = DateTime.Now;
				}
				else
				{
					timerStart = DateTime.MaxValue;
				}
				UpdateButton4();
				return;
			}
			timerStart = DateTime.MaxValue;
		}
		running = !running;
		UpdateButton4();
		UpdateUI();
		if (!running)
		{
			DamageMeter.Reset();
		}
	}

	[DllImport("user32.dll")]
	public static extern bool ReleaseCapture();

	[DllImport("user32.dll")]
	public static extern nint SendMessage(nint hWnd, int Msg, int wParam, int lParam);

	protected override void OnMouseDown(MouseEventArgs e)
	{
		base.OnMouseDown(e);
		if (e.Button == MouseButtons.Left)
		{
			ReleaseCapture();
			SendMessage(base.Handle, 161, 2, 0);
		}
	}

	public static void DrawBarImage(PictureBox pictureBox1, double value, string text)
	{
		Image barImage = barImageDamage;
		if (optionData.dpmMode)
		{
			barImage = barImageHeal;
		}
		int width = pictureBox1.Width;
		int height = pictureBox1.Height;
		float percent = Math.Max(0f, Math.Min(1f, (float)(value / 100.0)));
		Bitmap bmp = new Bitmap(width, height, PixelFormat.Format32bppArgb);
		Graphics g = Graphics.FromImage(bmp);
		g.Clear(Color.Transparent);
		g.SmoothingMode = SmoothingMode.AntiAlias;
		if (barImage != null)
		{
			int fillWidth = (int)((float)width * percent);
			Rectangle srcRect = new Rectangle(0, 0, (int)((float)barImage.Width * percent), barImage.Height);
			Rectangle destRect = new Rectangle(0, 0, fillWidth, height);
			g.DrawImage(barImage, destRect, srcRect, GraphicsUnit.Pixel);
		}
		Font font = new Font("Segoe UI", (float)height / 4f, FontStyle.Bold);
		Brush textBrush = Brushes.White;
		SizeF textSize = g.MeasureString(text, font);
		PointF pos = new PointF(((float)width - textSize.Width) / 2f, ((float)height - textSize.Height) / 2f - (float)(height / 10));
		g.DrawString(text, font, textBrush, pos);
		font.Dispose();
		g.Dispose();
		pictureBox1.Image?.Dispose();
		pictureBox1.Image = bmp;
	}

	public static double Lerp(double a, double b, double t)
	{
		return a + (b - a) * t;
	}

	public static void DrawBar(PictureBox pictureBox1, double value, string text)
	{
		int width = pictureBox1.Width;
		int height = pictureBox1.Height;
		float percent = Math.Max(0f, Math.Min(1f, (float)(value / 100.0)));
		Bitmap bmp = new Bitmap(width, height);
		Graphics g = Graphics.FromImage(bmp);
		g.SmoothingMode = SmoothingMode.AntiAlias;
		int radius = 10;
		GraphicsPath borderPath = RoundedRect(new Rectangle(0, 0, width - 1, height - 1), radius);
		using (Brush bg = new SolidBrush(Color.FromArgb(60, 60, 60)))
		{
			g.FillPath(bg, borderPath);
		}
		int fillWidth = (int)((float)(width - 1) * percent);
		Rectangle fillRect = new Rectangle(0, 0, fillWidth, height - 1);
		GraphicsPath fillPath = ((fillWidth >= radius * 2) ? RoundedRect(fillRect, radius) : RoundedRectLeftOnly(fillRect, radius));
		using (Brush hpBrush = new SolidBrush(Color.Red))
		{
			g.FillPath(hpBrush, fillPath);
		}
		using (Pen borderPen = new Pen(Color.Black, 2f))
		{
			g.DrawPath(borderPen, borderPath);
		}
		Font font = new Font(SystemFonts.DefaultFont.FontFamily, height / 4, FontStyle.Bold);
		Brush textBrush = Brushes.White;
		SizeF textSize = g.MeasureString(text, font);
		PointF pos = new PointF(((float)width - textSize.Width) / 2f, ((float)height - textSize.Height) / 2f);
		g.DrawString(text, font, textBrush, pos);
		pictureBox1.Image?.Dispose();
		pictureBox1.Image = bmp;
	}

	private static GraphicsPath RoundedRect(Rectangle bounds, int radius)
	{
		GraphicsPath path = new GraphicsPath();
		int d = radius * 2;
		if (radius == 0)
		{
			path.AddRectangle(bounds);
			return path;
		}
		path.AddArc(bounds.Left, bounds.Top, d, d, 180f, 90f);
		path.AddArc(bounds.Right - d, bounds.Top, d, d, 270f, 90f);
		path.AddArc(bounds.Right - d, bounds.Bottom - d, d, d, 0f, 90f);
		path.AddArc(bounds.Left, bounds.Bottom - d, d, d, 90f, 90f);
		path.CloseFigure();
		return path;
	}

	private static GraphicsPath RoundedRectLeftOnly(Rectangle bounds, int radius)
	{
		GraphicsPath path = new GraphicsPath();
		int effectiveRadius = Math.Min(radius, bounds.Width / 2);
		int d = effectiveRadius * 2;
		if (effectiveRadius <= 0)
		{
			path.AddRectangle(bounds);
			return path;
		}
		int left = bounds.Left;
		int right = bounds.Right;
		int top = bounds.Top;
		int bottom = bounds.Bottom;
		path.AddArc(left, top, d, d, 180f, 90f);
		path.AddLine(left + effectiveRadius, top, right, top);
		path.AddLine(right, top + effectiveRadius, right, bottom - effectiveRadius);
		path.AddLine(right, bottom, left + effectiveRadius, bottom);
		path.AddArc(left, bottom - d, d, d, 90f, 90f);
		path.CloseFigure();
		return path;
	}

	public double GetSliderValue()
	{
		if (optionData.dpmMode)
		{
			if (maxDPM > 0.0)
			{
				double sliderValueNew = dpm / maxDPM;
				sliderValueNew = 1.0 - sliderValueNew;
				sliderValueNew = Math.Pow(sliderValueNew, 2.0);
				sliderValueNew = 1.0 - sliderValueNew;
				sliderValueNew = 100.0 * sliderValueNew;
				if (targetDPS < dps)
				{
					return Lerp(sliderValue, sliderValueNew, 0.5);
				}
				return Lerp(sliderValue, sliderValueNew, 1.0);
			}
		}
		else if (maxDPS > 0.0)
		{
			double sliderValueNew2 = targetDPS / maxDPS;
			sliderValueNew2 = 1.0 - sliderValueNew2;
			sliderValueNew2 = Math.Pow(sliderValueNew2, 2.0);
			sliderValueNew2 = 1.0 - sliderValueNew2;
			sliderValueNew2 = 100.0 * sliderValueNew2;
			if (targetDPS < dps)
			{
				return Lerp(sliderValue, sliderValueNew2, 0.5);
			}
			return Lerp(sliderValue, sliderValueNew2, 1.0);
		}
		return 0.0;
	}

	public void UpdateBarUI()
	{
		if (optionData.dpmMode)
		{
			DrawBarImage(pictureBox1, sliderValue, $"HPS {(int)dpm}");
		}
		else
		{
			DrawBarImage(pictureBox1, sliderValue, $"DPS {(int)dps}");
		}
	}

	public void Update100()
	{
		targetDPS = DamageMeter.dps;
		targetDPM = DamageMeter.hps;
		UpdateButton4();
		if (optionData.dpsCheckTime > 10.0)
		{
			Update100Timer++;
			int overTime = 10;
			if (optionData.dpsCheckTime > 60.0)
			{
				overTime = 100;
			}
			Update100Timer++;
			if (Update100Timer < overTime)
			{
				return;
			}
			Update100Timer = 0;
		}
		if (targetDPS < dps)
		{
			dps = Lerp(dps, targetDPS, 0.8);
		}
		else
		{
			dps = Lerp(dps, targetDPS, 0.9);
		}
		if (targetDPM < dps)
		{
			dpm = Lerp(dpm, targetDPM, 0.5);
		}
		else
		{
			dpm = Lerp(dpm, targetDPM, 0.5);
		}
		maxDPS = Math.Max(maxDPS, targetDPS);
		maxDPM = Math.Max(maxDPM, targetDPM);
		sliderValue = GetSliderValue();
		if (!running)
		{
			return;
		}
		UpdateBarUI();
		if (optionData.dpmStopWatch)
		{
			double checkTime = optionData.dpsCheckTime;
			_ = optionData.dpmMode;
			if (DamageMeter.BattleTime() > checkTime)
			{
				Beep();
				StartStopButton();
			}
		}
	}

	public void Update1000()
	{
		DamageMeter.Update();
		if (!optionData.dpmMode)
		{
			Update1000Timer++;
			if (Update1000Timer < 3)
			{
				return;
			}
			Update1000Timer = 0;
		}
		textBox1.Multiline = true;
		totalDamage = DamageMeter.totalDamage;
		if (overlay == null)
		{
			overlay = new FormOverlay();
			overlay.Show();
		}
		if (running)
		{
			UpdateText();
		}
	}

	public void UpdateText()
	{
		string text = "";
		text = ((!optionData.dpmMode) ? (text + $"HPS: {(int)targetDPM}") : (text + $"DPS: {(int)targetDPS}"));
		text += $"\r\nMax DPS: {(int)maxDPS}\r\nMax HPS: {(int)maxDPM}";
		text += $"\r\nTotal: {(int)totalDamage}";
		text += $"\r\n전투시간: {(int)DamageMeter.BattleTime()}초";
		textBox1.Text = text;
		UpdateDetail();
	}

	public void UpdateUI()
	{
		UpdateText();
		UpdateBarUI();
	}

	private void Form1_FormClosing(object sender, FormClosingEventArgs e)
	{
		packetCapture?.Stop();
		updateTimer?.Stop();
	}

	private void button1_Click(object sender, EventArgs e)
	{
		maxDPS = 0.0;
		maxDPM = 0.0;
		dpm = 0.0;
		dps = 0.0;
		targetDPS = 0.0;
		targetDPM = 0.0;
		sliderValue = 0.0;
		totalDamage = 0.0;
		DamageMeter.Reset();
		Update1000();
		UpdateUI();
		overlay.UpdateUI();
	}

	private void checkBox1_CheckedChanged(object sender, EventArgs e)
	{
	}

	private void button2_Click(object sender, EventArgs e)
	{
		Application.Exit();
	}

	public void UpdateDetail()
	{
		Form form = Application.OpenForms["FormDetails"];
		if (form != null)
		{
			((FormDetails)form).UpdateUI();
		}
	}

	public void OpenFormDetail()
	{
		Form form = Application.OpenForms["FormDetails"];
		if (form == null)
		{
			form = new FormDetails();
			form.Show();
		}
		else
		{
			form.Activate();
		}
	}

	private void checkBox1_CheckedChanged_1(object sender, EventArgs e)
	{
	}

	private void button3_Click(object sender, EventArgs e)
	{
		Form form = Application.OpenForms["FormOption"];
		if (form == null)
		{
			form = new FormOption();
			form.Show();
		}
		else
		{
			form.Activate();
		}
	}

	public void OpenLink(string link)
	{
		Process.Start(new ProcessStartInfo
		{
			FileName = link,
			UseShellExecute = true
		});
	}

	private void pictureBox3_Click(object sender, EventArgs e)
	{
		OpenLink("https://x.com/ahzkwid");
		OpenLink("https://github.com/zjvlwid/MobiMeter/releases");
	}

	private void pictureBox1_Click(object sender, EventArgs e)
	{
		optionData.dpmMode = !optionData.dpmMode;
		OptionManager.Save(optionData);
		UpdateUI();
	}

	private void button5_Click(object sender, EventArgs e)
	{
		optionData.multiTarget = !optionData.multiTarget;
		OptionManager.Save(optionData);
		UpdateButton5();
		UpdateDetail();
	}

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
		System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MobiMeter.Form1));
		this.button1 = new System.Windows.Forms.Button();
		this.pictureBox1 = new System.Windows.Forms.PictureBox();
		this.button2 = new System.Windows.Forms.Button();
		this.pictureBox2 = new System.Windows.Forms.PictureBox();
		this.textBox1 = new System.Windows.Forms.TextBox();
		this.pictureBox3 = new System.Windows.Forms.PictureBox();
		this.button3 = new System.Windows.Forms.Button();
		this.button4 = new System.Windows.Forms.Button();
		this.button5_multitarget = new System.Windows.Forms.Button();
		((System.ComponentModel.ISupportInitialize)this.pictureBox1).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.pictureBox2).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.pictureBox3).BeginInit();
		base.SuspendLayout();
		this.button1.FlatAppearance.BorderSize = 0;
		this.button1.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
		this.button1.Image = (System.Drawing.Image)resources.GetObject("button1.Image");
		this.button1.Location = new System.Drawing.Point(9, 623);
		this.button1.Margin = new System.Windows.Forms.Padding(0);
		this.button1.Name = "button1";
		this.button1.Size = new System.Drawing.Size(107, 45);
		this.button1.TabIndex = 1;
		this.button1.UseVisualStyleBackColor = true;
		this.button1.Click += new System.EventHandler(button1_Click);
		this.pictureBox1.BackColor = System.Drawing.Color.FromArgb(163, 193, 129);
		this.pictureBox1.BackgroundImage = (System.Drawing.Image)resources.GetObject("pictureBox1.BackgroundImage");
		this.pictureBox1.Image = (System.Drawing.Image)resources.GetObject("pictureBox1.Image");
		this.pictureBox1.Location = new System.Drawing.Point(74, 43);
		this.pictureBox1.Name = "pictureBox1";
		this.pictureBox1.Size = new System.Drawing.Size(220, 65);
		this.pictureBox1.TabIndex = 4;
		this.pictureBox1.TabStop = false;
		this.pictureBox1.Click += new System.EventHandler(pictureBox1_Click);
		this.button2.FlatAppearance.BorderSize = 0;
		this.button2.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
		this.button2.Image = (System.Drawing.Image)resources.GetObject("button2.Image");
		this.button2.Location = new System.Drawing.Point(309, 9);
		this.button2.Margin = new System.Windows.Forms.Padding(0);
		this.button2.Name = "button2";
		this.button2.Size = new System.Drawing.Size(64, 64);
		this.button2.TabIndex = 6;
		this.button2.UseVisualStyleBackColor = true;
		this.button2.Click += new System.EventHandler(button2_Click);
		this.pictureBox2.Enabled = false;
		this.pictureBox2.Image = (System.Drawing.Image)resources.GetObject("pictureBox2.Image");
		this.pictureBox2.Location = new System.Drawing.Point(0, 0);
		this.pictureBox2.Name = "pictureBox2";
		this.pictureBox2.Size = new System.Drawing.Size(248, 43);
		this.pictureBox2.TabIndex = 7;
		this.pictureBox2.TabStop = false;
		this.textBox1.BackColor = System.Drawing.Color.FromArgb(163, 193, 129);
		this.textBox1.BorderStyle = System.Windows.Forms.BorderStyle.None;
		this.textBox1.Font = new System.Drawing.Font("맑은 고딕", 9f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 129);
		this.textBox1.ForeColor = System.Drawing.Color.White;
		this.textBox1.Location = new System.Drawing.Point(246, 149);
		this.textBox1.Multiline = true;
		this.textBox1.Name = "textBox1";
		this.textBox1.Size = new System.Drawing.Size(127, 84);
		this.textBox1.TabIndex = 8;
		this.textBox1.Text = "Loading";
		this.pictureBox3.Image = (System.Drawing.Image)resources.GetObject("pictureBox3.Image");
		this.pictureBox3.Location = new System.Drawing.Point(273, 634);
		this.pictureBox3.Name = "pictureBox3";
		this.pictureBox3.Size = new System.Drawing.Size(100, 31);
		this.pictureBox3.TabIndex = 9;
		this.pictureBox3.TabStop = false;
		this.pictureBox3.Click += new System.EventHandler(pictureBox3_Click);
		this.button3.FlatAppearance.BorderSize = 0;
		this.button3.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
		this.button3.Image = (System.Drawing.Image)resources.GetObject("button3.Image");
		this.button3.Location = new System.Drawing.Point(309, 82);
		this.button3.Margin = new System.Windows.Forms.Padding(0);
		this.button3.Name = "button3";
		this.button3.Size = new System.Drawing.Size(64, 64);
		this.button3.TabIndex = 10;
		this.button3.UseVisualStyleBackColor = true;
		this.button3.Click += new System.EventHandler(button3_Click);
		this.button4.BackgroundImage = (System.Drawing.Image)resources.GetObject("button4.BackgroundImage");
		this.button4.FlatAppearance.BorderSize = 0;
		this.button4.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
		this.button4.Image = (System.Drawing.Image)resources.GetObject("button4.Image");
		this.button4.Location = new System.Drawing.Point(129, 540);
		this.button4.Margin = new System.Windows.Forms.Padding(0);
		this.button4.Name = "button4";
		this.button4.Size = new System.Drawing.Size(128, 128);
		this.button4.TabIndex = 11;
		this.button4.UseVisualStyleBackColor = true;
		this.button4.Click += new System.EventHandler(button4_Click);
		this.button5_multitarget.BackgroundImage = (System.Drawing.Image)resources.GetObject("button5_multitarget.BackgroundImage");
		this.button5_multitarget.FlatAppearance.BorderSize = 0;
		this.button5_multitarget.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
		this.button5_multitarget.Image = (System.Drawing.Image)resources.GetObject("button5_multitarget.Image");
		this.button5_multitarget.Location = new System.Drawing.Point(9, 583);
		this.button5_multitarget.Margin = new System.Windows.Forms.Padding(0);
		this.button5_multitarget.Name = "button5_multitarget";
		this.button5_multitarget.Size = new System.Drawing.Size(107, 33);
		this.button5_multitarget.TabIndex = 12;
		this.button5_multitarget.UseVisualStyleBackColor = true;
		this.button5_multitarget.Click += new System.EventHandler(button5_Click);
		base.AutoScaleDimensions = new System.Drawing.SizeF(7f, 15f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		this.BackColor = System.Drawing.Color.FromArgb(163, 193, 129);
		this.BackgroundImage = (System.Drawing.Image)resources.GetObject("$this.BackgroundImage");
		this.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
		base.ClientSize = new System.Drawing.Size(386, 677);
		base.Controls.Add(this.button5_multitarget);
		base.Controls.Add(this.button4);
		base.Controls.Add(this.button3);
		base.Controls.Add(this.pictureBox3);
		base.Controls.Add(this.textBox1);
		base.Controls.Add(this.pictureBox2);
		base.Controls.Add(this.button2);
		base.Controls.Add(this.pictureBox1);
		base.Controls.Add(this.button1);
		this.DoubleBuffered = true;
		base.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
		base.Icon = (System.Drawing.Icon)resources.GetObject("$this.Icon");
		base.Name = "Form1";
		this.Text = "모카쨩 허수아비 미터기";
		((System.ComponentModel.ISupportInitialize)this.pictureBox1).EndInit();
		((System.ComponentModel.ISupportInitialize)this.pictureBox2).EndInit();
		((System.ComponentModel.ISupportInitialize)this.pictureBox3).EndInit();
		base.ResumeLayout(false);
		base.PerformLayout();
	}
}
