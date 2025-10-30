using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace MobiMeter;

public class FormOption : Form
{
	private IContainer components;

	private TextBox textBox1_dpsCheckTime;

	private Label label1;

	private Label label2;

	private Button button1;

	private CheckBox checkBox_overlayAlways;

	private CheckBox checkBox_DPMStopWatch;

	private CheckBox checkBox_lastAttack;

	private TextBox textBox_minDamage;

	public FormOption()
	{
		InitializeComponent();
		OptionData optionData = Form1.optionData;
		textBox1_dpsCheckTime.Text = optionData.dpsCheckTime.ToString();
		textBox_minDamage.Text = optionData.minDamage.ToString();
		checkBox_overlayAlways.Checked = optionData.overlayAlways;
		checkBox_DPMStopWatch.Checked = optionData.dpmStopWatch;
		checkBox_lastAttack.Checked = optionData.useLastAttack;
	}

	private void textBox1_TextChanged(object sender, EventArgs e)
	{
		string text = textBox1_dpsCheckTime.Text;
		text = new string(Array.FindAll(text.ToCharArray(), (char c) => char.IsDigit(c)));
		if (textBox1_dpsCheckTime.Text != text)
		{
			textBox1_dpsCheckTime.Text = text;
		}
	}

	private void button1_Click(object sender, EventArgs e)
	{
		OptionData optionData = Form1.optionData;
		if (double.TryParse(textBox1_dpsCheckTime.Text, out var result))
		{
			optionData.dpsCheckTime = Math.Max(result, 3.0);
		}
		if (double.TryParse(textBox_minDamage.Text, out var result2))
		{
			optionData.minDamage = Math.Max(result2, 0.0);
		}
		optionData.overlayAlways = checkBox_overlayAlways.Checked;
		optionData.dpmStopWatch = checkBox_DPMStopWatch.Checked;
		optionData.useLastAttack = checkBox_lastAttack.Checked;
		Form1.optionData = optionData;
		OptionManager.Save(optionData);
		Close();
		Form1 form = (Form1)Application.OpenForms["Form1"];
		if (form == null)
		{
			form.UpdateText();
		}
	}

	private void checkBox1_dpmGauge_CheckedChanged(object sender, EventArgs e)
	{
	}

	private void textBox2_dpmCheckTime_TextChanged(object sender, EventArgs e)
	{
	}

	private void textBox_minDamage_TextChanged(object sender, EventArgs e)
	{
		string text = textBox_minDamage.Text;
		text = new string(Array.FindAll(text.ToCharArray(), (char c) => char.IsDigit(c)));
		if (textBox_minDamage.Text != text)
		{
			textBox_minDamage.Text = text;
		}
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
		this.textBox1_dpsCheckTime = new System.Windows.Forms.TextBox();
		this.label1 = new System.Windows.Forms.Label();
		this.label2 = new System.Windows.Forms.Label();
		this.button1 = new System.Windows.Forms.Button();
		this.checkBox_overlayAlways = new System.Windows.Forms.CheckBox();
		this.checkBox_DPMStopWatch = new System.Windows.Forms.CheckBox();
		this.checkBox_lastAttack = new System.Windows.Forms.CheckBox();
		this.textBox_minDamage = new System.Windows.Forms.TextBox();
		base.SuspendLayout();
		this.textBox1_dpsCheckTime.Location = new System.Drawing.Point(110, 6);
		this.textBox1_dpsCheckTime.Name = "textBox1_dpsCheckTime";
		this.textBox1_dpsCheckTime.Size = new System.Drawing.Size(83, 23);
		this.textBox1_dpsCheckTime.TabIndex = 0;
		this.textBox1_dpsCheckTime.Text = "3";
		this.textBox1_dpsCheckTime.TextChanged += new System.EventHandler(textBox1_TextChanged);
		this.label1.AutoSize = true;
		this.label1.Location = new System.Drawing.Point(5, 9);
		this.label1.Name = "label1";
		this.label1.Size = new System.Drawing.Size(99, 15);
		this.label1.TabIndex = 2;
		this.label1.Text = "DPS 측정시간 (s)";
		this.label2.AutoSize = true;
		this.label2.Location = new System.Drawing.Point(94, 115);
		this.label2.Name = "label2";
		this.label2.Size = new System.Drawing.Size(111, 15);
		this.label2.TabIndex = 3;
		this.label2.Text = "이하의 데미지 무시";
		this.button1.Location = new System.Drawing.Point(64, 141);
		this.button1.Name = "button1";
		this.button1.Size = new System.Drawing.Size(75, 23);
		this.button1.TabIndex = 5;
		this.button1.Text = "저장";
		this.button1.UseVisualStyleBackColor = true;
		this.button1.Click += new System.EventHandler(button1_Click);
		this.checkBox_overlayAlways.AutoSize = true;
		this.checkBox_overlayAlways.Location = new System.Drawing.Point(5, 62);
		this.checkBox_overlayAlways.Name = "checkBox_overlayAlways";
		this.checkBox_overlayAlways.Size = new System.Drawing.Size(98, 19);
		this.checkBox_overlayAlways.TabIndex = 6;
		this.checkBox_overlayAlways.Text = "상시오버레이";
		this.checkBox_overlayAlways.UseVisualStyleBackColor = true;
		this.checkBox_DPMStopWatch.AutoSize = true;
		this.checkBox_DPMStopWatch.Location = new System.Drawing.Point(5, 37);
		this.checkBox_DPMStopWatch.Name = "checkBox_DPMStopWatch";
		this.checkBox_DPMStopWatch.Size = new System.Drawing.Size(102, 19);
		this.checkBox_DPMStopWatch.TabIndex = 7;
		this.checkBox_DPMStopWatch.Text = "스탑워치 사용";
		this.checkBox_DPMStopWatch.UseVisualStyleBackColor = true;
		this.checkBox_lastAttack.AutoSize = true;
		this.checkBox_lastAttack.Location = new System.Drawing.Point(5, 87);
		this.checkBox_lastAttack.Name = "checkBox_lastAttack";
		this.checkBox_lastAttack.Size = new System.Drawing.Size(182, 19);
		this.checkBox_lastAttack.TabIndex = 8;
		this.checkBox_lastAttack.Text = "마지막 타격을 기점으로 계산";
		this.checkBox_lastAttack.UseVisualStyleBackColor = true;
		this.textBox_minDamage.Location = new System.Drawing.Point(5, 112);
		this.textBox_minDamage.Name = "textBox_minDamage";
		this.textBox_minDamage.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
		this.textBox_minDamage.Size = new System.Drawing.Size(83, 23);
		this.textBox_minDamage.TabIndex = 9;
		this.textBox_minDamage.Text = "0";
		this.textBox_minDamage.TextChanged += new System.EventHandler(textBox_minDamage_TextChanged);
		base.AutoScaleDimensions = new System.Drawing.SizeF(7f, 15f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		base.ClientSize = new System.Drawing.Size(211, 172);
		base.Controls.Add(this.textBox_minDamage);
		base.Controls.Add(this.checkBox_lastAttack);
		base.Controls.Add(this.checkBox_DPMStopWatch);
		base.Controls.Add(this.checkBox_overlayAlways);
		base.Controls.Add(this.button1);
		base.Controls.Add(this.label2);
		base.Controls.Add(this.label1);
		base.Controls.Add(this.textBox1_dpsCheckTime);
		base.Name = "FormOption";
		this.Text = "옵션";
		base.ResumeLayout(false);
		base.PerformLayout();
	}
}
