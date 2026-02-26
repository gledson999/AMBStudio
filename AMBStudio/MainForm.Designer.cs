namespace AmtEditor
{
    partial class MainForm
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.btnOpen = new System.Windows.Forms.ToolStripMenuItem();
            this.btnSave = new System.Windows.Forms.ToolStripMenuItem();
            this.btnAbout = new System.Windows.Forms.ToolStripMenuItem();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.tvFiles = new System.Windows.Forms.TreeView();
            this.pbPreview = new System.Windows.Forms.PictureBox();
            this.pnlTextControls = new System.Windows.Forms.Panel();
            this.btnClearText = new System.Windows.Forms.Button();
            this.lblBackColor = new System.Windows.Forms.Label();
            this.pnlBackColor = new System.Windows.Forms.Panel();
            this.lblHexColor = new System.Windows.Forms.Label();
            this.grpAlignment = new System.Windows.Forms.GroupBox();
            this.rbAlignRight = new System.Windows.Forms.RadioButton();
            this.rbAlignCenter = new System.Windows.Forms.RadioButton();
            this.rbAlignLeft = new System.Windows.Forms.RadioButton();
            this.lblText = new System.Windows.Forms.Label();
            this.txtInput = new System.Windows.Forms.TextBox();
            this.cmbFonts = new System.Windows.Forms.ComboBox();
            this.lblFont = new System.Windows.Forms.Label();
            this.numPosX = new System.Windows.Forms.NumericUpDown();
            this.lblPosX = new System.Windows.Forms.Label();
            this.numPosY = new System.Windows.Forms.NumericUpDown();
            this.lblPosY = new System.Windows.Forms.Label();
            this.numLineHeight = new System.Windows.Forms.NumericUpDown();
            this.lblLineHeight = new System.Windows.Forms.Label();
            this.numFinalSpacing = new System.Windows.Forms.NumericUpDown();
            this.lblFinalSpacing = new System.Windows.Forms.Label();
            this.ctxParent = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.mnuExpandAll = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuCollapseAll = new System.Windows.Forms.ToolStripMenuItem();
            this.ctxChild = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.mnuExportExt = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuImportExt = new System.Windows.Forms.ToolStripMenuItem();
            this.ctxNeto = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.mnuExportBin = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuImportBin = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuExportPng = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuImportPng = new System.Windows.Forms.ToolStripMenuItem();
            this.toggleDarkBG = new System.Windows.Forms.CheckBox();
            this.menuStrip1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pbPreview)).BeginInit();
            this.pnlTextControls.SuspendLayout();
            this.grpAlignment.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numPosX)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numPosY)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numLineHeight)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numFinalSpacing)).BeginInit();
            this.ctxParent.SuspendLayout();
            this.ctxChild.SuspendLayout();
            this.ctxNeto.SuspendLayout();
            this.SuspendLayout();
            // 
            // menuStrip1
            // 
            this.menuStrip1.BackColor = System.Drawing.SystemColors.ControlText;
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.btnOpen,
            this.btnSave,
            this.btnAbout});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(974, 24);
            this.menuStrip1.TabIndex = 2;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // btnOpen
            // 
            this.btnOpen.ForeColor = System.Drawing.SystemColors.Control;
            this.btnOpen.Name = "btnOpen";
            this.btnOpen.Size = new System.Drawing.Size(48, 20);
            this.btnOpen.Text = "Open";
            this.btnOpen.Click += new System.EventHandler(this.btnOpen_Click);
            // 
            // btnSave
            // 
            this.btnSave.ForeColor = System.Drawing.SystemColors.Control;
            this.btnSave.Name = "btnSave";
            this.btnSave.Size = new System.Drawing.Size(43, 20);
            this.btnSave.Text = "Save";
            this.btnSave.Click += new System.EventHandler(this.btnSave_Click);
            // 
            // btnAbout
            // 
            this.btnAbout.ForeColor = System.Drawing.SystemColors.Control;
            this.btnAbout.Name = "btnAbout";
            this.btnAbout.Size = new System.Drawing.Size(52, 20);
            this.btnAbout.Text = "About";
            this.btnAbout.Click += new System.EventHandler(this.btnAbout_Click);
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.IsSplitterFixed = true;
            this.splitContainer1.Location = new System.Drawing.Point(0, 24);
            this.splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.tvFiles);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.BackColor = System.Drawing.Color.WhiteSmoke;
            this.splitContainer1.Panel2.Controls.Add(this.pbPreview);
            this.splitContainer1.Panel2.Controls.Add(this.pnlTextControls);
            this.splitContainer1.Size = new System.Drawing.Size(974, 626);
            this.splitContainer1.SplitterDistance = 270;
            this.splitContainer1.TabIndex = 1;
            // 
            // tvFiles
            // 
            this.tvFiles.BackColor = System.Drawing.SystemColors.Window;
            this.tvFiles.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.tvFiles.Location = new System.Drawing.Point(7, 8);
            this.tvFiles.Name = "tvFiles";
            this.tvFiles.Size = new System.Drawing.Size(263, 610);
            this.tvFiles.TabIndex = 0;
            this.tvFiles.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.tvFiles_AfterSelect);
            this.tvFiles.NodeMouseClick += new System.Windows.Forms.TreeNodeMouseClickEventHandler(this.tvFiles_NodeMouseClick);
            // 
            // pbPreview
            // 
            this.pbPreview.BackColor = System.Drawing.Color.Transparent;
            this.pbPreview.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pbPreview.Location = new System.Drawing.Point(4, 8);
            this.pbPreview.Name = "pbPreview";
            this.pbPreview.Size = new System.Drawing.Size(688, 471);
            this.pbPreview.SizeMode = System.Windows.Forms.PictureBoxSizeMode.CenterImage;
            this.pbPreview.TabIndex = 0;
            this.pbPreview.TabStop = false;
            // 
            // pnlTextControls
            // 
            this.pnlTextControls.BackColor = System.Drawing.Color.WhiteSmoke;
            this.pnlTextControls.Controls.Add(this.btnClearText);
            this.pnlTextControls.Controls.Add(this.lblBackColor);
            this.pnlTextControls.Controls.Add(this.pnlBackColor);
            this.pnlTextControls.Controls.Add(this.lblHexColor);
            this.pnlTextControls.Controls.Add(this.grpAlignment);
            this.pnlTextControls.Controls.Add(this.lblText);
            this.pnlTextControls.Controls.Add(this.txtInput);
            this.pnlTextControls.Controls.Add(this.cmbFonts);
            this.pnlTextControls.Controls.Add(this.lblFont);
            this.pnlTextControls.Controls.Add(this.numPosX);
            this.pnlTextControls.Controls.Add(this.lblPosX);
            this.pnlTextControls.Controls.Add(this.numPosY);
            this.pnlTextControls.Controls.Add(this.lblPosY);
            this.pnlTextControls.Controls.Add(this.numLineHeight);
            this.pnlTextControls.Controls.Add(this.lblLineHeight);
            this.pnlTextControls.Controls.Add(this.numFinalSpacing);
            this.pnlTextControls.Controls.Add(this.lblFinalSpacing);
            this.pnlTextControls.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.pnlTextControls.Location = new System.Drawing.Point(0, 486);
            this.pnlTextControls.Name = "pnlTextControls";
            this.pnlTextControls.Size = new System.Drawing.Size(700, 140);
            this.pnlTextControls.TabIndex = 1;
            // 
            // btnClearText
            // 
            this.btnClearText.Location = new System.Drawing.Point(589, 105);
            this.btnClearText.Name = "btnClearText";
            this.btnClearText.Size = new System.Drawing.Size(100, 25);
            this.btnClearText.TabIndex = 15;
            this.btnClearText.Text = "Clean Text";
            this.btnClearText.UseVisualStyleBackColor = true;
            this.btnClearText.Click += new System.EventHandler(this.btnClearText_Click);
            // 
            // lblBackColor
            // 
            this.lblBackColor.AutoSize = true;
            this.lblBackColor.Location = new System.Drawing.Point(320, 108);
            this.lblBackColor.Name = "lblBackColor";
            this.lblBackColor.Size = new System.Drawing.Size(95, 13);
            this.lblBackColor.TabIndex = 16;
            this.lblBackColor.Text = "Background Color:";
            // 
            // pnlBackColor
            // 
            this.pnlBackColor.BackColor = System.Drawing.Color.Black;
            this.pnlBackColor.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pnlBackColor.Location = new System.Drawing.Point(419, 105);
            this.pnlBackColor.Name = "pnlBackColor";
            this.pnlBackColor.Size = new System.Drawing.Size(50, 20);
            this.pnlBackColor.TabIndex = 14;
            // 
            // lblHexColor
            // 
            this.lblHexColor.AutoSize = true;
            this.lblHexColor.Location = new System.Drawing.Point(475, 108);
            this.lblHexColor.Name = "lblHexColor";
            this.lblHexColor.Size = new System.Drawing.Size(62, 13);
            this.lblHexColor.TabIndex = 16;
            this.lblHexColor.Text = "#000000FF";
            // 
            // grpAlignment
            // 
            this.grpAlignment.Controls.Add(this.rbAlignRight);
            this.grpAlignment.Controls.Add(this.rbAlignCenter);
            this.grpAlignment.Controls.Add(this.rbAlignLeft);
            this.grpAlignment.Location = new System.Drawing.Point(590, 8);
            this.grpAlignment.Name = "grpAlignment";
            this.grpAlignment.Size = new System.Drawing.Size(100, 90);
            this.grpAlignment.TabIndex = 13;
            this.grpAlignment.TabStop = false;
            this.grpAlignment.Text = "Text Align";
            // 
            // rbAlignRight
            // 
            this.rbAlignRight.AutoSize = true;
            this.rbAlignRight.Location = new System.Drawing.Point(6, 65);
            this.rbAlignRight.Name = "rbAlignRight";
            this.rbAlignRight.Size = new System.Drawing.Size(50, 17);
            this.rbAlignRight.TabIndex = 2;
            this.rbAlignRight.Text = "Right";
            this.rbAlignRight.UseVisualStyleBackColor = true;
            this.rbAlignRight.CheckedChanged += new System.EventHandler(this.OnTextSettingsChanged);
            // 
            // rbAlignCenter
            // 
            this.rbAlignCenter.AutoSize = true;
            this.rbAlignCenter.Location = new System.Drawing.Point(6, 42);
            this.rbAlignCenter.Name = "rbAlignCenter";
            this.rbAlignCenter.Size = new System.Drawing.Size(56, 17);
            this.rbAlignCenter.TabIndex = 1;
            this.rbAlignCenter.Text = "Center";
            this.rbAlignCenter.UseVisualStyleBackColor = true;
            this.rbAlignCenter.CheckedChanged += new System.EventHandler(this.OnTextSettingsChanged);
            // 
            // rbAlignLeft
            // 
            this.rbAlignLeft.AutoSize = true;
            this.rbAlignLeft.Checked = true;
            this.rbAlignLeft.Location = new System.Drawing.Point(6, 19);
            this.rbAlignLeft.Name = "rbAlignLeft";
            this.rbAlignLeft.Size = new System.Drawing.Size(43, 17);
            this.rbAlignLeft.TabIndex = 0;
            this.rbAlignLeft.TabStop = true;
            this.rbAlignLeft.Text = "Left";
            this.rbAlignLeft.UseVisualStyleBackColor = true;
            this.rbAlignLeft.CheckedChanged += new System.EventHandler(this.OnTextSettingsChanged);
            // 
            // lblText
            // 
            this.lblText.AutoSize = true;
            this.lblText.Location = new System.Drawing.Point(6, 8);
            this.lblText.Name = "lblText";
            this.lblText.Size = new System.Drawing.Size(31, 13);
            this.lblText.TabIndex = 17;
            this.lblText.Text = "Text:";
            // 
            // txtInput
            // 
            this.txtInput.Location = new System.Drawing.Point(4, 25);
            this.txtInput.Multiline = true;
            this.txtInput.Name = "txtInput";
            this.txtInput.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtInput.Size = new System.Drawing.Size(300, 107);
            this.txtInput.TabIndex = 0;
            this.txtInput.TextChanged += new System.EventHandler(this.OnTextSettingsChanged);
            // 
            // cmbFonts
            // 
            this.cmbFonts.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbFonts.FormattingEnabled = true;
            this.cmbFonts.Location = new System.Drawing.Point(379, 67);
            this.cmbFonts.Name = "cmbFonts";
            this.cmbFonts.Size = new System.Drawing.Size(200, 21);
            this.cmbFonts.TabIndex = 5;
            this.cmbFonts.SelectedIndexChanged += new System.EventHandler(this.OnTextSettingsChanged);
            // 
            // lblFont
            // 
            this.lblFont.AutoSize = true;
            this.lblFont.Location = new System.Drawing.Point(320, 70);
            this.lblFont.Name = "lblFont";
            this.lblFont.Size = new System.Drawing.Size(55, 13);
            this.lblFont.TabIndex = 18;
            this.lblFont.Text = "Text Font:";
            // 
            // numPosX
            // 
            this.numPosX.Location = new System.Drawing.Point(379, 8);
            this.numPosX.Maximum = new decimal(new int[] {
            5000,
            0,
            0,
            0});
            this.numPosX.Minimum = new decimal(new int[] {
            1000,
            0,
            0,
            -2147483648});
            this.numPosX.Name = "numPosX";
            this.numPosX.Size = new System.Drawing.Size(50, 20);
            this.numPosX.TabIndex = 1;
            this.numPosX.ValueChanged += new System.EventHandler(this.OnTextSettingsChanged);
            // 
            // lblPosX
            // 
            this.lblPosX.AutoSize = true;
            this.lblPosX.Location = new System.Drawing.Point(320, 10);
            this.lblPosX.Name = "lblPosX";
            this.lblPosX.Size = new System.Drawing.Size(57, 13);
            this.lblPosX.TabIndex = 19;
            this.lblPosX.Text = "Position X:";
            // 
            // numPosY
            // 
            this.numPosY.Location = new System.Drawing.Point(379, 33);
            this.numPosY.Maximum = new decimal(new int[] {
            5000,
            0,
            0,
            0});
            this.numPosY.Minimum = new decimal(new int[] {
            1000,
            0,
            0,
            -2147483648});
            this.numPosY.Name = "numPosY";
            this.numPosY.Size = new System.Drawing.Size(50, 20);
            this.numPosY.TabIndex = 2;
            this.numPosY.ValueChanged += new System.EventHandler(this.OnTextSettingsChanged);
            // 
            // lblPosY
            // 
            this.lblPosY.AutoSize = true;
            this.lblPosY.Location = new System.Drawing.Point(320, 35);
            this.lblPosY.Name = "lblPosY";
            this.lblPosY.Size = new System.Drawing.Size(57, 13);
            this.lblPosY.TabIndex = 20;
            this.lblPosY.Text = "Position Y:";
            // 
            // numLineHeight
            // 
            this.numLineHeight.Location = new System.Drawing.Point(529, 8);
            this.numLineHeight.Maximum = new decimal(new int[] {
            500,
            0,
            0,
            0});
            this.numLineHeight.Minimum = new decimal(new int[] {
            100,
            0,
            0,
            -2147483648});
            this.numLineHeight.Name = "numLineHeight";
            this.numLineHeight.Size = new System.Drawing.Size(50, 20);
            this.numLineHeight.TabIndex = 3;
            this.numLineHeight.ValueChanged += new System.EventHandler(this.OnTextSettingsChanged);
            // 
            // lblLineHeight
            // 
            this.lblLineHeight.AutoSize = true;
            this.lblLineHeight.Location = new System.Drawing.Point(463, 10);
            this.lblLineHeight.Name = "lblLineHeight";
            this.lblLineHeight.Size = new System.Drawing.Size(64, 13);
            this.lblLineHeight.TabIndex = 21;
            this.lblLineHeight.Text = "Line Height:";
            // 
            // numFinalSpacing
            // 
            this.numFinalSpacing.Location = new System.Drawing.Point(529, 33);
            this.numFinalSpacing.Maximum = new decimal(new int[] {
            5000,
            0,
            0,
            0});
            this.numFinalSpacing.Name = "numFinalSpacing";
            this.numFinalSpacing.Size = new System.Drawing.Size(50, 20);
            this.numFinalSpacing.TabIndex = 4;
            this.numFinalSpacing.ValueChanged += new System.EventHandler(this.OnTextSettingsChanged);
            // 
            // lblFinalSpacing
            // 
            this.lblFinalSpacing.AutoSize = true;
            this.lblFinalSpacing.Location = new System.Drawing.Point(453, 35);
            this.lblFinalSpacing.Name = "lblFinalSpacing";
            this.lblFinalSpacing.Size = new System.Drawing.Size(74, 13);
            this.lblFinalSpacing.TabIndex = 22;
            this.lblFinalSpacing.Text = "Final Spacing:";
            // 
            // ctxParent
            // 
            this.ctxParent.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.mnuExpandAll,
            this.mnuCollapseAll});
            this.ctxParent.Name = "ctxParent";
            this.ctxParent.Size = new System.Drawing.Size(151, 48);
            // 
            // mnuExpandAll
            // 
            this.mnuExpandAll.Name = "mnuExpandAll";
            this.mnuExpandAll.Size = new System.Drawing.Size(150, 22);
            this.mnuExpandAll.Text = "Expand All";
            this.mnuExpandAll.Click += new System.EventHandler(this.mnuExpandAll_Click);
            // 
            // mnuCollapseAll
            // 
            this.mnuCollapseAll.Name = "mnuCollapseAll";
            this.mnuCollapseAll.Size = new System.Drawing.Size(150, 22);
            this.mnuCollapseAll.Text = "Collapse All";
            this.mnuCollapseAll.Click += new System.EventHandler(this.mnuCollapseAll_Click);
            // 
            // ctxChild
            // 
            this.ctxChild.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.mnuExportExt,
            this.mnuImportExt});
            this.ctxChild.Name = "ctxChild";
            this.ctxChild.Size = new System.Drawing.Size(121, 48);
            this.ctxChild.Opening += new System.ComponentModel.CancelEventHandler(this.ctxChild_Opening);
            // 
            // mnuExportExt
            // 
            this.mnuExportExt.Name = "mnuExportExt";
            this.mnuExportExt.Size = new System.Drawing.Size(120, 22);
            this.mnuExportExt.Text = "Export";
            this.mnuExportExt.Click += new System.EventHandler(this.mnuExportExt_Click);
            // 
            // mnuImportExt
            // 
            this.mnuImportExt.Name = "mnuImportExt";
            this.mnuImportExt.Size = new System.Drawing.Size(120, 22);
            this.mnuImportExt.Text = "Import";
            this.mnuImportExt.Click += new System.EventHandler(this.mnuImportExt_Click);
            // 
            // ctxNeto
            // 
            this.ctxNeto.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.mnuExportBin,
            this.mnuImportBin,
            this.mnuExportPng,
            this.mnuImportPng});
            this.ctxNeto.Name = "ctxNeto";
            this.ctxNeto.Size = new System.Drawing.Size(148, 92);
            // 
            // mnuExportBin
            // 
            this.mnuExportBin.Name = "mnuExportBin";
            this.mnuExportBin.Size = new System.Drawing.Size(147, 22);
            this.mnuExportBin.Text = "Export BIN";
            this.mnuExportBin.Click += new System.EventHandler(this.mnuExportBin_Click);
            // 
            // mnuImportBin
            // 
            this.mnuImportBin.Name = "mnuImportBin";
            this.mnuImportBin.Size = new System.Drawing.Size(147, 22);
            this.mnuImportBin.Text = "Import BIN";
            this.mnuImportBin.Click += new System.EventHandler(this.mnuImportBin_Click);
            // 
            // mnuExportPng
            // 
            this.mnuExportPng.Name = "mnuExportPng";
            this.mnuExportPng.Size = new System.Drawing.Size(147, 22);
            this.mnuExportPng.Text = "Export PNG";
            this.mnuExportPng.Click += new System.EventHandler(this.mnuExportPng_Click);
            // 
            // mnuImportPng
            // 
            this.mnuImportPng.Name = "mnuImportPng";
            this.mnuImportPng.Size = new System.Drawing.Size(147, 22);
            this.mnuImportPng.Text = "Import PNG";
            this.mnuImportPng.Click += new System.EventHandler(this.mnuImportPng_Click);
            // 
            // toggleDarkBG
            // 
            this.toggleDarkBG.AutoSize = true;
            this.toggleDarkBG.BackColor = System.Drawing.SystemColors.ControlText;
            this.toggleDarkBG.ForeColor = System.Drawing.SystemColors.Control;
            this.toggleDarkBG.Location = new System.Drawing.Point(861, 4);
            this.toggleDarkBG.Name = "toggleDarkBG";
            this.toggleDarkBG.Size = new System.Drawing.Size(110, 17);
            this.toggleDarkBG.TabIndex = 3;
            this.toggleDarkBG.Text = "Dark Background";
            this.toggleDarkBG.UseVisualStyleBackColor = false;
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(974, 650);
            this.Controls.Add(this.toggleDarkBG);
            this.Controls.Add(this.splitContainer1);
            this.Controls.Add(this.menuStrip1);
            this.MainMenuStrip = this.menuStrip1;
            this.Name = "MainForm";
            this.Text = "AMB Studio 1.0";
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.pbPreview)).EndInit();
            this.pnlTextControls.ResumeLayout(false);
            this.pnlTextControls.PerformLayout();
            this.grpAlignment.ResumeLayout(false);
            this.grpAlignment.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numPosX)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numPosY)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numLineHeight)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numFinalSpacing)).EndInit();
            this.ctxParent.ResumeLayout(false);
            this.ctxChild.ResumeLayout(false);
            this.ctxNeto.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem btnSave;
        private System.Windows.Forms.ToolStripMenuItem btnOpen;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.TreeView tvFiles;
        private System.Windows.Forms.PictureBox pbPreview;
        private System.Windows.Forms.ContextMenuStrip ctxParent;
        private System.Windows.Forms.ToolStripMenuItem mnuExpandAll;
        private System.Windows.Forms.ToolStripMenuItem mnuCollapseAll;
        private System.Windows.Forms.ContextMenuStrip ctxChild;
        private System.Windows.Forms.ToolStripMenuItem mnuExportExt;
        private System.Windows.Forms.ToolStripMenuItem mnuImportExt;
        private System.Windows.Forms.ContextMenuStrip ctxNeto;
        private System.Windows.Forms.ToolStripMenuItem mnuExportBin;
        private System.Windows.Forms.ToolStripMenuItem mnuImportBin;
        private System.Windows.Forms.ToolStripMenuItem mnuExportPng;
        private System.Windows.Forms.ToolStripMenuItem mnuImportPng;
        private System.Windows.Forms.Panel pnlTextControls;
        private System.Windows.Forms.TextBox txtInput;
        private System.Windows.Forms.Label lblText;
        private System.Windows.Forms.NumericUpDown numPosX;
        private System.Windows.Forms.Label lblPosX;
        private System.Windows.Forms.NumericUpDown numPosY;
        private System.Windows.Forms.Label lblPosY;
        private System.Windows.Forms.NumericUpDown numLineHeight;
        private System.Windows.Forms.Label lblLineHeight;
        private System.Windows.Forms.NumericUpDown numFinalSpacing;
        private System.Windows.Forms.Label lblFinalSpacing;
        private System.Windows.Forms.ComboBox cmbFonts;
        private System.Windows.Forms.Label lblFont;
        private System.Windows.Forms.GroupBox grpAlignment;
        private System.Windows.Forms.RadioButton rbAlignRight;
        private System.Windows.Forms.RadioButton rbAlignCenter;
        private System.Windows.Forms.RadioButton rbAlignLeft;
        private System.Windows.Forms.Panel pnlBackColor;
        private System.Windows.Forms.Label lblBackColor;
        private System.Windows.Forms.Label lblHexColor;
        private System.Windows.Forms.Button btnClearText;
        private System.Windows.Forms.CheckBox toggleDarkBG;
        private System.Windows.Forms.ToolStripMenuItem btnAbout;
    }
}