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
            this.btnAccessCheck = new System.Windows.Forms.ToolStripButton();
            this.btnInvokeFlow = new System.Windows.Forms.ToolStripButton();
            this.btnGodPage = new System.Windows.Forms.ToolStripButton();
            this.btnMetadata = new System.Windows.Forms.ToolStripButton();
            this.elementHostMain = new System.Windows.Forms.Integration.ElementHost();
            this.toolStripMenu.SuspendLayout();
            this.SuspendLayout();
            //
            // toolStripMenu
            //
            this.toolStripMenu.ImageScalingSize = new System.Drawing.Size(24, 24);
            this.toolStripMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
                this.btnAccessCheck,
                this.btnInvokeFlow,
                this.btnGodPage,
                this.btnMetadata,
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
            this.tsbClose.Size = new System.Drawing.Size(85, 28);
            this.tsbClose.Text = "Close this tool";
            this.tsbClose.Click += new System.EventHandler(this.tsbClose_Click);
            //
            // btnAccessCheck
            //
            this.btnAccessCheck.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.btnAccessCheck.Name = "btnAccessCheck";
            this.btnAccessCheck.Size = new System.Drawing.Size(103, 28);
            this.btnAccessCheck.Text = "🔑 AccessCheck";
            this.btnAccessCheck.Click += new System.EventHandler(this.btnAccessCheck_Click);
            //
            // btnInvokeFlow
            //
            this.btnInvokeFlow.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.btnInvokeFlow.Name = "btnInvokeFlow";
            this.btnInvokeFlow.Size = new System.Drawing.Size(97, 28);
            this.btnInvokeFlow.Text = "⚡ InvokeFlow";
            this.btnInvokeFlow.Click += new System.EventHandler(this.btnInvokeFlow_Click);
            //
            // btnGodPage
            //
            this.btnGodPage.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.btnGodPage.Name = "btnGodPage";
            this.btnGodPage.Size = new System.Drawing.Size(81, 28);
            this.btnGodPage.Text = "📝 GodPage";
            this.btnGodPage.Click += new System.EventHandler(this.btnGodPage_Click);
            //
            // btnMetadata
            //
            this.btnMetadata.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.btnMetadata.Name = "btnMetadata";
            this.btnMetadata.Size = new System.Drawing.Size(89, 28);
            this.btnMetadata.Text = "🌐 Metadata";
            this.btnMetadata.Click += new System.EventHandler(this.btnMetadata_Click);
            //
            // elementHostMain
            //
            this.elementHostMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.elementHostMain.Location = new System.Drawing.Point(0, 31);
            this.elementHostMain.Name = "elementHostMain";
            this.elementHostMain.Size = new System.Drawing.Size(839, 431);
            this.elementHostMain.TabIndex = 5;
            this.elementHostMain.Visible = true;
            //
            // MyPluginControl
            //
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.elementHostMain);
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
        private System.Windows.Forms.ToolStripButton btnAccessCheck;
        private System.Windows.Forms.ToolStripButton btnInvokeFlow;
        private System.Windows.Forms.ToolStripButton btnGodPage;
        private System.Windows.Forms.ToolStripButton btnMetadata;
        private System.Windows.Forms.Integration.ElementHost elementHostMain;
    }
}
