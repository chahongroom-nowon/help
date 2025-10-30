using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace MobiMeter;

public class FormDetails : Form
{
	private class BarData
	{
		public List<Label> labels = new List<Label>();

		public List<PictureBox> bars = new List<PictureBox>();
	}

	private List<Label> titles = new List<Label>();

	private BarData barDataFrom = new BarData();

	private BarData barDataTo = new BarData();

	private IContainer components;

	private Label label_title;

	private PictureBox pictureBox_damageBar;

	private Button button1_close;

	private Label label_damageBar;

	public FormDetails()
	{
		InitializeComponent();
		Form1.RoundedForm(this);
		base.Icon = System.Drawing.Icon.ExtractAssociatedIcon(Application.ExecutablePath);
		int count = 6;
		int spacing = 20;
		int groupSpacing = 200;
		Label originalTt = label_title;
		Label originalLb = label_damageBar;
		PictureBox originalPb = pictureBox_damageBar;
		label_damageBar.Text = "1";
		barDataFrom.labels.Add(originalLb);
		barDataFrom.bars.Add(originalPb);
		for (int x = 0; x < 2; x++)
		{
			BarData barData = ((x == 0) ? barDataFrom : barDataTo);
			int xOffset = x * groupSpacing;
			Label original = originalTt;
			Label lb = new Label
			{
				AutoSize = original.AutoSize,
				Font = original.Font,
				ForeColor = original.ForeColor,
				BackColor = original.BackColor,
				TextAlign = original.TextAlign,
				Size = original.Size,
				Text = "입은 피해",
				Location = new Point(original.Left + xOffset, original.Top)
			};
			titles.Add(lb);
			base.Controls.Add(lb);
			for (int y = 0; y < count; y++)
			{
				if (x != 0 || y != 0)
				{
					int yOffset = y * spacing;
					PictureBox pb = new PictureBox
					{
						Size = originalPb.Size,
						Image = originalPb.Image,
						BackColor = originalPb.BackColor,
						SizeMode = originalPb.SizeMode,
						Location = new Point(originalPb.Left + xOffset, originalPb.Top + yOffset),
						Enabled = originalPb.Enabled
					};
					base.Controls.Add(pb);
					barData.bars.Add(pb);
					Label lb2 = new Label
					{
						AutoSize = originalLb.AutoSize,
						Font = originalLb.Font,
						ForeColor = originalLb.ForeColor,
						BackColor = originalLb.BackColor,
						TextAlign = originalLb.TextAlign,
						Size = originalLb.Size,
						Text = (y + 1).ToString(),
						Location = new Point(originalLb.Left + xOffset, originalLb.Top + yOffset)
					};
					base.Controls.Add(lb2);
					barData.labels.Add(lb2);
				}
			}
		}
		originalTt.Visible = false;
		DrawBars();
	}

	public void DrawBars()
	{
		for (int x = 0; x < 2; x++)
		{
			string text = "";
			text = ((x != 0) ? "입은피해" : "가한피해");
			if (Form1.optionData.dpmMode)
			{
				text = ((x != 0) ? " 회복량" : "  총합");
			}
			titles[x].Text = text;
			double[] data = DamageMeter.MainTarget.barsFrom;
			BarData barData = barDataFrom;
			if (x == 1)
			{
				data = DamageMeter.MainTarget.barsTo;
				barData = barDataTo;
			}
			double max = data[0];
			for (int y = 0; y < barData.bars.Count; y++)
			{
				Color color = Color.Gray;
				if (y == 0)
				{
					if (Form1.optionData.multiTarget)
					{
						if (x == 0)
						{
							color = Color.Red;
							if (Form1.optionData.dpmMode)
							{
								color = Color.Green;
							}
						}
					}
					else if (x == 1)
					{
						color = Color.Red;
						if (Form1.optionData.dpmMode)
						{
							color = Color.Green;
						}
					}
				}
				PictureBox pictureBox = barData.bars[y];
				double value = 0.0;
				if (max > 0.0)
				{
					value = data[y] * 100.0 / max;
				}
				FormOverlay.DrawBar(pictureBox, value, data[y].ToString(), color);
			}
		}
	}

	private void button1_close_Click(object sender, EventArgs e)
	{
		Close();
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

	public void UpdateUI()
	{
		DrawBars();
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
		System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MobiMeter.FormDetails));
		this.label_title = new System.Windows.Forms.Label();
		this.pictureBox_damageBar = new System.Windows.Forms.PictureBox();
		this.button1_close = new System.Windows.Forms.Button();
		this.label_damageBar = new System.Windows.Forms.Label();
		((System.ComponentModel.ISupportInitialize)this.pictureBox_damageBar).BeginInit();
		base.SuspendLayout();
		this.label_title.AutoSize = true;
		this.label_title.Font = new System.Drawing.Font("맑은 고딕", 9f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 129);
		this.label_title.ForeColor = System.Drawing.Color.White;
		this.label_title.Location = new System.Drawing.Point(81, 11);
		this.label_title.Name = "label_title";
		this.label_title.Size = new System.Drawing.Size(59, 15);
		this.label_title.TabIndex = 0;
		this.label_title.Text = "가한 피해";
		this.pictureBox_damageBar.BackColor = System.Drawing.Color.Black;
		this.pictureBox_damageBar.Enabled = false;
		this.pictureBox_damageBar.Location = new System.Drawing.Point(32, 36);
		this.pictureBox_damageBar.Name = "pictureBox_damageBar";
		this.pictureBox_damageBar.Size = new System.Drawing.Size(156, 15);
		this.pictureBox_damageBar.TabIndex = 1;
		this.pictureBox_damageBar.TabStop = false;
		this.button1_close.FlatAppearance.BorderSize = 0;
		this.button1_close.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
		this.button1_close.Image = (System.Drawing.Image)resources.GetObject("button1_close.Image");
		this.button1_close.Location = new System.Drawing.Point(408, 11);
		this.button1_close.Name = "button1_close";
		this.button1_close.Size = new System.Drawing.Size(32, 32);
		this.button1_close.TabIndex = 2;
		this.button1_close.UseVisualStyleBackColor = true;
		this.button1_close.Click += new System.EventHandler(button1_close_Click);
		this.label_damageBar.AutoSize = true;
		this.label_damageBar.Font = new System.Drawing.Font("맑은 고딕", 9f, System.Drawing.FontStyle.Bold);
		this.label_damageBar.ForeColor = System.Drawing.Color.White;
		this.label_damageBar.Location = new System.Drawing.Point(12, 36);
		this.label_damageBar.Name = "label_damageBar";
		this.label_damageBar.Size = new System.Drawing.Size(14, 15);
		this.label_damageBar.TabIndex = 4;
		this.label_damageBar.Text = "1";
		base.AutoScaleDimensions = new System.Drawing.SizeF(7f, 15f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		this.BackColor = System.Drawing.Color.FromArgb(163, 193, 129);
		base.ClientSize = new System.Drawing.Size(451, 167);
		base.Controls.Add(this.label_damageBar);
		base.Controls.Add(this.button1_close);
		base.Controls.Add(this.pictureBox_damageBar);
		base.Controls.Add(this.label_title);
		base.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
		base.Name = "FormDetails";
		this.Text = "FormDetails";
		((System.ComponentModel.ISupportInitialize)this.pictureBox_damageBar).EndInit();
		base.ResumeLayout(false);
		base.PerformLayout();
	}
}
