namespace DynaAppX
{
    partial class MyPluginControl
    {
        /// <summary>
        /// Variable nécessaire au concepteur.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Nettoyage des ressources utilisées.
        /// </summary>
        /// <param name="disposing">true si les ressources managées doivent être supprimées ; sinon, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Code généré par le Concepteur de composants

        /// <summary>
        /// Méthode requise pour la prise en charge du concepteur - ne modifiez pas
        /// le contenu de cette méthode avec l'éditeur de code.
        /// </summary>
        private void InitializeComponent()
        {
            this.toolStripMenu = new System.Windows.Forms.ToolStrip();
            this.tsbClose = new System.Windows.Forms.ToolStripButton();
            this.tsbWelcome = new System.Windows.Forms.ToolStripButton();
            this.tsbAccessCheck = new System.Windows.Forms.ToolStripButton();
            this.tsbInvokeFlow = new System.Windows.Forms.ToolStripButton();
            this.tsbGodPage = new System.Windows.Forms.ToolStripButton();
            this.tsbMetadataBrowser = new System.Windows.Forms.ToolStripButton();
            this.toolStripMenu.SuspendLayout();
            this.SuspendLayout();
            //
            // toolStripMenu
            //
            this.toolStripMenu.ImageScalingSize = new System.Drawing.Size(24, 24);
            this.toolStripMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
                this.tsbWelcome,
                this.tsbAccessCheck,
                this.tsbInvokeFlow,
                this.tsbGodPage,
                this.tsbMetadataBrowser,
                this.tsbClose});
            this.toolStripMenu.Location = new System.Drawing.Point(0, 0);
            this.toolStripMenu.Name = "toolStripMenu";
            this.toolStripMenu.Padding = new System.Windows.Forms.Padding(0, 0, 2, 0);
            this.toolStripMenu.Size = new System.Drawing.Size(839, 31);
            this.toolStripMenu.TabIndex = 4;
            this.toolStripMenu.Text = "toolStrip1";
            //
            // tsbClose
            //
            this.tsbClose.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.tsbClose.Name = "tsbClose";
            this.tsbClose.Size = new System.Drawing.Size(120, 28);
            this.tsbClose.Text = "Close this tool";
            this.tsbClose.Click += new System.EventHandler(this.tsbClose_Click);
            //
            // tsbWelcome
            //
            this.tsbWelcome.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.tsbWelcome.Name = "tsbWelcome";
            this.tsbWelcome.Size = new System.Drawing.Size(80, 28);
            this.tsbWelcome.Text = "Welcome";
            this.tsbWelcome.Click += new System.EventHandler(this.tsbWelcome_Click);
            //
            // tsbAccessCheck
            //
            this.tsbAccessCheck.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.tsbAccessCheck.Name = "tsbAccessCheck";
            this.tsbAccessCheck.Size = new System.Drawing.Size(90, 28);
            this.tsbAccessCheck.Text = "AccessCheck";
            this.tsbAccessCheck.Click += new System.EventHandler(this.tsbAccessCheck_Click);
            //
            // tsbInvokeFlow
            //
            this.tsbInvokeFlow.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.tsbInvokeFlow.Name = "tsbInvokeFlow";
            this.tsbInvokeFlow.Size = new System.Drawing.Size(85, 28);
            this.tsbInvokeFlow.Text = "InvokeFlow";
            this.tsbInvokeFlow.Click += new System.EventHandler(this.tsbInvokeFlow_Click);
            //
            // tsbGodPage
            //
            this.tsbGodPage.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.tsbGodPage.Name = "tsbGodPage";
            this.tsbGodPage.Size = new System.Drawing.Size(75, 28);
            this.tsbGodPage.Text = "GodPage";
            this.tsbGodPage.Click += new System.EventHandler(this.tsbGodPage_Click);
            //
            // tsbMetadataBrowser
            //
            this.tsbMetadataBrowser.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.tsbMetadataBrowser.Name = "tsbMetadataBrowser";
            this.tsbMetadataBrowser.Size = new System.Drawing.Size(115, 28);
            this.tsbMetadataBrowser.Text = "MetadataBrowser";
            this.tsbMetadataBrowser.Click += new System.EventHandler(this.tsbMetadataBrowser_Click);
            //
            // MyPluginControl
            //
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.toolStripMenu);
            this.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.Name = "DynaAppX";
            this.Size = new System.Drawing.Size(839, 462);
            this.Load += new System.EventHandler(this.MyPluginControl_Load);
            this.toolStripMenu.ResumeLayout(false);
            this.toolStripMenu.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        #endregion
        private System.Windows.Forms.ToolStrip toolStripMenu;
        private System.Windows.Forms.ToolStripButton tsbClose;
        private System.Windows.Forms.ToolStripButton tsbWelcome;
        private System.Windows.Forms.ToolStripButton tsbAccessCheck;
        private System.Windows.Forms.ToolStripButton tsbInvokeFlow;
        private System.Windows.Forms.ToolStripButton tsbGodPage;
        private System.Windows.Forms.ToolStripButton tsbMetadataBrowser;
    }
}
