using System;

namespace thecalcify.News
{
    partial class NewsSetting
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
                NewsSetting_Leave(null, null);
            }
            base.Dispose(disposing);
        }

        #region Component Designer generated code

        private void InitializeComponent()
        {
            this.newsDNDLabel = new System.Windows.Forms.Label();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.dndOn = new System.Windows.Forms.RadioButton();
            this.dndOff = new System.Windows.Forms.RadioButton();
            this.keywordLable = new System.Windows.Forms.Label();
            this.flowTopics = new System.Windows.Forms.FlowLayoutPanel();
            this.flowKeywords = new System.Windows.Forms.FlowLayoutPanel();
            this.topicLabel = new System.Windows.Forms.Label();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // newsDNDLabel
            // 
            this.newsDNDLabel.AutoSize = true;
            this.newsDNDLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.newsDNDLabel.Location = new System.Drawing.Point(59, 42);
            this.newsDNDLabel.Name = "newsDNDLabel";
            this.newsDNDLabel.Size = new System.Drawing.Size(84, 25);
            this.newsDNDLabel.TabIndex = 0;
            this.newsDNDLabel.Text = "DND :- ";
            this.newsDNDLabel.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.dndOn);
            this.groupBox1.Controls.Add(this.dndOff);
            this.groupBox1.Location = new System.Drawing.Point(189, 26);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(169, 41);
            this.groupBox1.TabIndex = 1;
            this.groupBox1.TabStop = false;
            // 
            // dndOn
            // 
            this.dndOn.AutoSize = true;
            this.dndOn.Location = new System.Drawing.Point(26, 14);
            this.dndOn.Name = "dndOn";
            this.dndOn.Size = new System.Drawing.Size(45, 20);
            this.dndOn.TabIndex = 2;
            this.dndOn.TabStop = true;
            this.dndOn.Text = "On";
            this.dndOn.UseVisualStyleBackColor = true;
            this.dndOn.CheckedChanged += new System.EventHandler(this.UpdateDNDStatus);
            // 
            // dndOff
            // 
            this.dndOff.AutoSize = true;
            this.dndOff.Location = new System.Drawing.Point(94, 14);
            this.dndOff.Name = "dndOff";
            this.dndOff.Size = new System.Drawing.Size(44, 20);
            this.dndOff.TabIndex = 3;
            this.dndOff.TabStop = true;
            this.dndOff.Text = "Off";
            this.dndOff.UseVisualStyleBackColor = true;
            this.dndOff.CheckedChanged += new System.EventHandler(this.UpdateDNDStatus);
            // 
            // keywordLable
            // 
            this.keywordLable.AutoSize = true;
            this.keywordLable.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold);
            this.keywordLable.Location = new System.Drawing.Point(916, 93);
            this.keywordLable.Name = "keywordLable";
            this.keywordLable.Size = new System.Drawing.Size(128, 25);
            this.keywordLable.TabIndex = 2;
            this.keywordLable.Text = "Keywords :-";
            // 
            // flowTopics
            // 
            this.flowTopics.AutoScroll = true;
            this.flowTopics.Location = new System.Drawing.Point(64, 142);
            this.flowTopics.Name = "flowTopics";
            this.flowTopics.Size = new System.Drawing.Size(838, 526);
            this.flowTopics.TabIndex = 4;
            // 
            // flowKeywords
            // 
            this.flowKeywords.AutoScroll = true;
            this.flowKeywords.Location = new System.Drawing.Point(921, 142);
            this.flowKeywords.Name = "flowKeywords";
            this.flowKeywords.Size = new System.Drawing.Size(838, 529);
            this.flowKeywords.TabIndex = 5;
            // 
            // topicLabel
            // 
            this.topicLabel.AutoSize = true;
            this.topicLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold);
            this.topicLabel.Location = new System.Drawing.Point(59, 93);
            this.topicLabel.Name = "topicLabel";
            this.topicLabel.Size = new System.Drawing.Size(98, 25);
            this.topicLabel.TabIndex = 3;
            this.topicLabel.Text = "Topics :-";
            // 
            // NewsSetting
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.WhiteSmoke;
            this.Controls.Add(this.topicLabel);
            this.Controls.Add(this.flowKeywords);
            this.Controls.Add(this.flowTopics);
            this.Controls.Add(this.keywordLable);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.newsDNDLabel);
            this.Name = "NewsSetting";
            this.Size = new System.Drawing.Size(1763, 671);
            this.Leave += new System.EventHandler(this.NewsSetting_Leave);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label newsDNDLabel;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.RadioButton dndOn;
        private System.Windows.Forms.RadioButton dndOff;
        private System.Windows.Forms.Label keywordLable;
        private System.Windows.Forms.FlowLayoutPanel flowTopics; // ✅ NEW
        private System.Windows.Forms.FlowLayoutPanel flowKeywords;
        private System.Windows.Forms.Label topicLabel;
    }
}