namespace DynaAppX
{
    partial class MyPluginControl
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

        #region Component Designer generated code

        private void InitializeComponent()
        {
            this.toolStripMenu = new System.Windows.Forms.ToolStrip();
            this.tsbClose = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.btnReloadData = new System.Windows.Forms.ToolStripButton();
            this.tabControl = new System.Windows.Forms.TabControl();
            this.tabAccessCheck = new System.Windows.Forms.TabPage();
            this.tabInvokeFlow = new System.Windows.Forms.TabPage();
            this.tableLayoutPanelAccess = new System.Windows.Forms.TableLayoutPanel();
            this.lblUser = new System.Windows.Forms.Label();
            this.cbUsers = new System.Windows.Forms.ComboBox();
            this.lblEntity = new System.Windows.Forms.Label();
            this.cbEntities = new System.Windows.Forms.ComboBox();
            this.lblRecordId = new System.Windows.Forms.Label();
            this.txtRecordId = new System.Windows.Forms.TextBox();
            this.btnCheckAccess = new System.Windows.Forms.Button();
            this.lblAccessStatus = new System.Windows.Forms.Label();
            this.lblRoles = new System.Windows.Forms.Label();
            this.lbRoles = new System.Windows.Forms.ListBox();
            this.lblTeams = new System.Windows.Forms.Label();
            this.lbTeams = new System.Windows.Forms.ListBox();
            this.lblAccessRights = new System.Windows.Forms.Label();
            this.clbAccessRights = new System.Windows.Forms.CheckedListBox();
            this.tableLayoutPanelFlow = new System.Windows.Forms.TableLayoutPanel();
            this.lblFlow = new System.Windows.Forms.Label();
            this.cbFlows = new System.Windows.Forms.ComboBox();
            this.lblRecordType = new System.Windows.Forms.Label();
            this.cbRecordType = new System.Windows.Forms.ComboBox();
            this.lblInvokeRecordId = new System.Windows.Forms.Label();
            this.txtInvokeRecordId = new System.Windows.Forms.TextBox();
            this.btnInvoke = new System.Windows.Forms.Button();
            this.lblInvokeStatus = new System.Windows.Forms.Label();
            this.lblHistory = new System.Windows.Forms.Label();
            this.dgvHistory = new System.Windows.Forms.DataGridView();
            this.btnClearHistory = new System.Windows.Forms.Button();
            this.colTime = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colFlowName = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colStatus = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colResponse = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.toolStripMenu.SuspendLayout();
            this.tabControl.SuspendLayout();
            this.tabAccessCheck.SuspendLayout();
            this.tableLayoutPanelAccess.SuspendLayout();
            this.tabInvokeFlow.SuspendLayout();
            this.tableLayoutPanelFlow.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvHistory)).BeginInit();
            this.SuspendLayout();
            //
            // toolStripMenu
            //
            this.toolStripMenu.ImageScalingSize = new System.Drawing.Size(24, 24);
            this.toolStripMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tsbClose,
            this.toolStripSeparator1,
            this.btnReloadData});
            this.toolStripMenu.Location = new System.Drawing.Point(0, 0);
            this.toolStripMenu.Name = "toolStripMenu";
            this.toolStripMenu.Size = new System.Drawing.Size(1194, 38);
            this.toolStripMenu.TabIndex = 4;
            this.toolStripMenu.Text = "toolStrip1";
            //
            // tsbClose
            //
            this.tsbClose.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.tsbClose.Name = "tsbClose";
            this.tsbClose.Size = new System.Drawing.Size(135, 33);
            this.tsbClose.Text = "Close this tool";
            this.tsbClose.Click += new System.EventHandler(this.tsbClose_Click);
            //
            // toolStripSeparator1
            //
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(6, 38);
            //
            // btnReloadData
            //
            this.btnReloadData.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.btnReloadData.Name = "btnReloadData";
            this.btnReloadData.Size = new System.Drawing.Size(100, 33);
            this.btnReloadData.Text = "Reload Data";
            this.btnReloadData.Click += new System.EventHandler(this.btnReloadData_Click);
            //
            // tabControl
            //
            this.tabControl.Controls.Add(this.tabAccessCheck);
            this.tabControl.Controls.Add(this.tabInvokeFlow);
            this.tabControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControl.Location = new System.Drawing.Point(0, 38);
            this.tabControl.Name = "tabControl";
            this.tabControl.SelectedIndex = 0;
            this.tabControl.Size = new System.Drawing.Size(1194, 721);
            this.tabControl.TabIndex = 5;
            //
            // tabAccessCheck
            //
            this.tabAccessCheck.Controls.Add(this.tableLayoutPanelAccess);
            this.tabAccessCheck.Location = new System.Drawing.Point(4, 27);
            this.tabAccessCheck.Name = "tabAccessCheck";
            this.tabAccessCheck.Padding = new System.Windows.Forms.Padding(10);
            this.tabAccessCheck.Size = new System.Drawing.Size(1186, 690);
            this.tabAccessCheck.TabIndex = 0;
            this.tabAccessCheck.Text = "AccessCheck";
            this.tabAccessCheck.UseVisualStyleBackColor = true;
            //
            // tableLayoutPanelAccess
            //
            this.tableLayoutPanelAccess.ColumnCount = 4;
            this.tableLayoutPanelAccess.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 100));
            this.tableLayoutPanelAccess.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25));
            this.tableLayoutPanelAccess.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25));
            this.tableLayoutPanelAccess.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50));
            this.tableLayoutPanelAccess.Controls.Add(this.lblUser, 0, 0);
            this.tableLayoutPanelAccess.Controls.Add(this.cbUsers, 1, 0);
            this.tableLayoutPanelAccess.Controls.Add(this.lblEntity, 2, 0);
            this.tableLayoutPanelAccess.Controls.Add(this.cbEntities, 3, 0);
            this.tableLayoutPanelAccess.Controls.Add(this.lblRecordId, 0, 1);
            this.tableLayoutPanelAccess.Controls.Add(this.txtRecordId, 1, 1);
            this.tableLayoutPanelAccess.Controls.Add(this.btnCheckAccess, 2, 1);
            this.tableLayoutPanelAccess.Controls.Add(this.lblAccessStatus, 3, 1);
            this.tableLayoutPanelAccess.Controls.Add(this.lblRoles, 0, 2);
            this.tableLayoutPanelAccess.Controls.Add(this.lbRoles, 1, 2);
            this.tableLayoutPanelAccess.Controls.Add(this.lblTeams, 2, 2);
            this.tableLayoutPanelAccess.Controls.Add(this.lbTeams, 3, 2);
            this.tableLayoutPanelAccess.Controls.Add(this.lblAccessRights, 0, 3);
            this.tableLayoutPanelAccess.Controls.Add(this.clbAccessRights, 1, 3);
            this.tableLayoutPanelAccess.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanelAccess.Location = new System.Drawing.Point(10, 10);
            this.tableLayoutPanelAccess.Name = "tableLayoutPanelAccess";
            this.tableLayoutPanelAccess.RowCount = 4;
            this.tableLayoutPanelAccess.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 40));
            this.tableLayoutPanelAccess.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 40));
            this.tableLayoutPanelAccess.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50));
            this.tableLayoutPanelAccess.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50));
            this.tableLayoutPanelAccess.Size = new System.Drawing.Size(1166, 670);
            this.tableLayoutPanelAccess.TabIndex = 0;
            //
            // lblUser
            //
            this.lblUser.AutoSize = true;
            this.lblUser.Location = new System.Drawing.Point(10, 10);
            this.lblUser.Name = "lblUser";
            this.lblUser.Size = new System.Drawing.Size(45, 18);
            this.lblUser.TabIndex = 0;
            this.lblUser.Text = "User:";
            //
            // cbUsers
            //
            this.cbUsers.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbUsers.FormattingEnabled = true;
            this.cbUsers.Location = new System.Drawing.Point(110, 10);
            this.cbUsers.Name = "cbUsers";
            this.cbUsers.Size = new System.Drawing.Size(280, 26);
            this.cbUsers.TabIndex = 1;
            this.cbUsers.SelectedIndexChanged += new System.EventHandler(this.cbUsers_SelectedIndexChanged);
            //
            // lblEntity
            //
            this.lblEntity.AutoSize = true;
            this.lblEntity.Location = new System.Drawing.Point(400, 10);
            this.lblEntity.Name = "lblEntity";
            this.lblEntity.Size = new System.Drawing.Size(55, 18);
            this.lblEntity.TabIndex = 2;
            this.lblEntity.Text = "Entity:";
            //
            // cbEntities
            //
            this.cbEntities.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbEntities.FormattingEnabled = true;
            this.cbEntities.Location = new System.Drawing.Point(460, 10);
            this.cbEntities.Name = "cbEntities";
            this.cbEntities.Size = new System.Drawing.Size(280, 26);
            this.cbEntities.TabIndex = 3;
            //
            // lblRecordId
            //
            this.lblRecordId.AutoSize = true;
            this.lblRecordId.Location = new System.Drawing.Point(10, 50);
            this.lblRecordId.Name = "lblRecordId";
            this.lblRecordId.Size = new System.Drawing.Size(75, 18);
            this.lblRecordId.TabIndex = 4;
            this.lblRecordId.Text = "Record ID:";
            //
            // txtRecordId
            //
            this.txtRecordId.Location = new System.Drawing.Point(110, 50);
            this.txtRecordId.Name = "txtRecordId";
            this.txtRecordId.Size = new System.Drawing.Size(280, 25);
            this.txtRecordId.TabIndex = 5;
            //
            // btnCheckAccess
            //
            this.btnCheckAccess.Location = new System.Drawing.Point(400, 50);
            this.btnCheckAccess.Name = "btnCheckAccess";
            this.btnCheckAccess.Size = new System.Drawing.Size(120, 30);
            this.btnCheckAccess.TabIndex = 6;
            this.btnCheckAccess.Text = "Check Access";
            this.btnCheckAccess.UseVisualStyleBackColor = true;
            this.btnCheckAccess.Click += new System.EventHandler(this.btnCheckAccess_Click);
            //
            // lblAccessStatus
            //
            this.lblAccessStatus.AutoSize = true;
            this.lblAccessStatus.Location = new System.Drawing.Point(530, 55);
            this.lblAccessStatus.Name = "lblAccessStatus";
            this.lblAccessStatus.Size = new System.Drawing.Size(0, 18);
            this.lblAccessStatus.TabIndex = 7;
            //
            // lblRoles
            //
            this.lblRoles.AutoSize = true;
            this.lblRoles.Location = new System.Drawing.Point(10, 100);
            this.lblRoles.Name = "lblRoles";
            this.lblRoles.Size = new System.Drawing.Size(75, 18);
            this.lblRoles.TabIndex = 8;
            this.lblRoles.Text = "User Roles:";
            //
            // lbRoles
            //
            this.lbRoles.FormattingEnabled = true;
            this.lbRoles.ItemHeight = 18;
            this.lbRoles.Location = new System.Drawing.Point(110, 90);
            this.lbRoles.Name = "lbRoles";
            this.lbRoles.Size = new System.Drawing.Size(280, 220);
            this.lbRoles.TabIndex = 9;
            //
            // lblTeams
            //
            this.lblTeams.AutoSize = true;
            this.lblTeams.Location = new System.Drawing.Point(400, 100);
            this.lblTeams.Name = "lblTeams";
            this.lblTeams.Size = new System.Drawing.Size(80, 18);
            this.lblTeams.TabIndex = 10;
            this.lblTeams.Text = "User Teams:";
            //
            // lbTeams
            //
            this.lbTeams.FormattingEnabled = true;
            this.lbTeams.ItemHeight = 18;
            this.lbTeams.Location = new System.Drawing.Point(490, 90);
            this.lbTeams.Name = "lbTeams";
            this.lbTeams.Size = new System.Drawing.Size(280, 220);
            this.lbTeams.TabIndex = 11;
            //
            // lblAccessRights
            //
            this.lblAccessRights.AutoSize = true;
            this.lblAccessRights.Location = new System.Drawing.Point(10, 320);
            this.lblAccessRights.Name = "lblAccessRights";
            this.lblAccessRights.Size = new System.Drawing.Size(95, 18);
            this.lblAccessRights.TabIndex = 12;
            this.lblAccessRights.Text = "Access Rights:";
            //
            // clbAccessRights
            //
            this.clbAccessRights.FormattingEnabled = true;
            this.clbAccessRights.Items.AddRange(new object[] {
            "ReadAccess",
            "WriteAccess",
            "CreateAccess",
            "DeleteAccess",
            "ShareAccess",
            "AssignAccess",
            "AppendAccess",
            "AppendToAccess"});
            this.clbAccessRights.Location = new System.Drawing.Point(110, 320);
            this.clbAccessRights.Name = "clbAccessRights";
            this.clbAccessRights.Size = new System.Drawing.Size(280, 220);
            this.clbAccessRights.TabIndex = 13;
            //
            // tabInvokeFlow
            //
            this.tabInvokeFlow.Controls.Add(this.tableLayoutPanelFlow);
            this.tabInvokeFlow.Location = new System.Drawing.Point(4, 27);
            this.tabInvokeFlow.Name = "tabInvokeFlow";
            this.tabInvokeFlow.Padding = new System.Windows.Forms.Padding(10);
            this.tabInvokeFlow.Size = new System.Drawing.Size(1186, 690);
            this.tabInvokeFlow.TabIndex = 1;
            this.tabInvokeFlow.Text = "InvokeFlow";
            this.tabInvokeFlow.UseVisualStyleBackColor = true;
            //
            // tableLayoutPanelFlow
            //
            this.tableLayoutPanelFlow.ColumnCount = 4;
            this.tableLayoutPanelFlow.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 100));
            this.tableLayoutPanelFlow.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50));
            this.tableLayoutPanelFlow.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 120));
            this.tableLayoutPanelFlow.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50));
            this.tableLayoutPanelFlow.Controls.Add(this.lblFlow, 0, 0);
            this.tableLayoutPanelFlow.Controls.Add(this.cbFlows, 1, 0);
            this.tableLayoutPanelFlow.Controls.Add(this.lblRecordType, 2, 0);
            this.tableLayoutPanelFlow.Controls.Add(this.cbRecordType, 3, 0);
            this.tableLayoutPanelFlow.Controls.Add(this.lblInvokeRecordId, 0, 1);
            this.tableLayoutPanelFlow.Controls.Add(this.txtInvokeRecordId, 1, 1);
            this.tableLayoutPanelFlow.Controls.Add(this.btnInvoke, 2, 1);
            this.tableLayoutPanelFlow.Controls.Add(this.lblInvokeStatus, 3, 1);
            this.tableLayoutPanelFlow.Controls.Add(this.lblHistory, 0, 2);
            this.tableLayoutPanelFlow.Controls.Add(this.dgvHistory, 1, 2);
            this.tableLayoutPanelFlow.Controls.Add(this.btnClearHistory, 2, 2);
            this.tableLayoutPanelFlow.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanelFlow.Location = new System.Drawing.Point(10, 10);
            this.tableLayoutPanelFlow.Name = "tableLayoutPanelFlow";
            this.tableLayoutPanelFlow.RowCount = 3;
            this.tableLayoutPanelFlow.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 40));
            this.tableLayoutPanelFlow.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 40));
            this.tableLayoutPanelFlow.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100));
            this.tableLayoutPanelFlow.Size = new System.Drawing.Size(1166, 670);
            this.tableLayoutPanelFlow.TabIndex = 0;
            //
            // lblFlow
            //
            this.lblFlow.AutoSize = true;
            this.lblFlow.Location = new System.Drawing.Point(10, 10);
            this.lblFlow.Name = "lblFlow";
            this.lblFlow.Size = new System.Drawing.Size(45, 18);
            this.lblFlow.TabIndex = 0;
            this.lblFlow.Text = "Flow:";
            //
            // cbFlows
            //
            this.cbFlows.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbFlows.FormattingEnabled = true;
            this.cbFlows.Location = new System.Drawing.Point(110, 10);
            this.cbFlows.Name = "cbFlows";
            this.cbFlows.Size = new System.Drawing.Size(400, 26);
            this.cbFlows.TabIndex = 1;
            //
            // lblRecordType
            //
            this.lblRecordType.AutoSize = true;
            this.lblRecordType.Location = new System.Drawing.Point(520, 10);
            this.lblRecordType.Name = "lblRecordType";
            this.lblRecordType.Size = new System.Drawing.Size(85, 18);
            this.lblRecordType.TabIndex = 2;
            this.lblRecordType.Text = "Record Type:";
            //
            // cbRecordType
            //
            this.cbRecordType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbRecordType.FormattingEnabled = true;
            this.cbRecordType.Location = new System.Drawing.Point(610, 10);
            this.cbRecordType.Name = "cbRecordType";
            this.cbRecordType.Size = new System.Drawing.Size(280, 26);
            this.cbRecordType.TabIndex = 3;
            //
            // lblInvokeRecordId
            //
            this.lblInvokeRecordId.AutoSize = true;
            this.lblInvokeRecordId.Location = new System.Drawing.Point(10, 50);
            this.lblInvokeRecordId.Name = "lblInvokeRecordId";
            this.lblInvokeRecordId.Size = new System.Drawing.Size(75, 18);
            this.lblInvokeRecordId.TabIndex = 4;
            this.lblInvokeRecordId.Text = "Record ID:";
            //
            // txtInvokeRecordId
            //
            this.txtInvokeRecordId.Location = new System.Drawing.Point(110, 50);
            this.txtInvokeRecordId.Name = "txtInvokeRecordId";
            this.txtInvokeRecordId.Size = new System.Drawing.Size(400, 25);
            this.txtInvokeRecordId.TabIndex = 5;
            //
            // btnInvoke
            //
            this.btnInvoke.Location = new System.Drawing.Point(520, 50);
            this.btnInvoke.Name = "btnInvoke";
            this.btnInvoke.Size = new System.Drawing.Size(100, 30);
            this.btnInvoke.TabIndex = 6;
            this.btnInvoke.Text = "Invoke";
            this.btnInvoke.UseVisualStyleBackColor = true;
            this.btnInvoke.Click += new System.EventHandler(this.btnInvoke_Click);
            //
            // lblInvokeStatus
            //
            this.lblInvokeStatus.AutoSize = true;
            this.lblInvokeStatus.Location = new System.Drawing.Point(630, 55);
            this.lblInvokeStatus.Name = "lblInvokeStatus";
            this.lblInvokeStatus.Size = new System.Drawing.Size(0, 18);
            this.lblInvokeStatus.TabIndex = 7;
            //
            // lblHistory
            //
            this.lblHistory.AutoSize = true;
            this.lblHistory.Location = new System.Drawing.Point(10, 100);
            this.lblHistory.Name = "lblHistory";
            this.lblHistory.Size = new System.Drawing.Size(75, 18);
            this.lblHistory.TabIndex = 8;
            this.lblHistory.Text = "History:";
            //
            // dgvHistory
            //
            this.dgvHistory.AllowUserToAddRows = false;
            this.dgvHistory.AllowUserToDeleteRows = false;
            this.dgvHistory.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.dgvHistory.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvHistory.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.colTime,
            this.colFlowName,
            this.colStatus,
            this.colResponse});
            this.dgvHistory.Location = new System.Drawing.Point(110, 90);
            this.dgvHistory.Name = "dgvHistory";
            this.dgvHistory.ReadOnly = true;
            this.dgvHistory.RowHeadersWidth = 50;
            this.dgvHistory.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgvHistory.Size = new System.Drawing.Size(780, 550);
            this.dgvHistory.TabIndex = 9;
            //
            // btnClearHistory
            //
            this.btnClearHistory.Location = new System.Drawing.Point(520, 90);
            this.btnClearHistory.Name = "btnClearHistory";
            this.btnClearHistory.Size = new System.Drawing.Size(100, 30);
            this.btnClearHistory.TabIndex = 10;
            this.btnClearHistory.Text = "Clear History";
            this.btnClearHistory.UseVisualStyleBackColor = true;
            this.btnClearHistory.Click += new System.EventHandler(this.btnClearHistory_Click);
            //
            // colTime
            //
            this.colTime.HeaderText = "Time";
            this.colTime.Name = "colTime";
            this.colTime.ReadOnly = true;
            //
            // colFlowName
            //
            this.colFlowName.HeaderText = "Flow Name";
            this.colFlowName.Name = "colFlowName";
            this.colFlowName.ReadOnly = true;
            //
            // colStatus
            //
            this.colStatus.HeaderText = "Status";
            this.colStatus.Name = "colStatus";
            this.colStatus.ReadOnly = true;
            //
            // colResponse
            //
            this.colResponse.HeaderText = "Response";
            this.colResponse.Name = "colResponse";
            this.colResponse.ReadOnly = true;
            //
            // MyPluginControl
            //
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 18F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.tabControl);
            this.Controls.Add(this.toolStripMenu);
            this.Name = "MyPluginControl";
            this.Size = new System.Drawing.Size(1194, 759);
            this.Load += new System.EventHandler(this.MyPluginControl_Load);
            this.toolStripMenu.ResumeLayout(false);
            this.toolStripMenu.PerformLayout();
            this.tabControl.ResumeLayout(false);
            this.tabAccessCheck.ResumeLayout(false);
            this.tableLayoutPanelAccess.ResumeLayout(false);
            this.tableLayoutPanelAccess.PerformLayout();
            this.tabInvokeFlow.ResumeLayout(false);
            this.tableLayoutPanelFlow.ResumeLayout(false);
            this.tableLayoutPanelFlow.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvHistory)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        #endregion

        private System.Windows.Forms.ToolStrip toolStripMenu;
        private System.Windows.Forms.ToolStripButton tsbClose;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripButton btnReloadData;
        private System.Windows.Forms.TabControl tabControl;
        private System.Windows.Forms.TabPage tabAccessCheck;
        private System.Windows.Forms.TabPage tabInvokeFlow;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanelAccess;
        private System.Windows.Forms.Label lblUser;
        private System.Windows.Forms.ComboBox cbUsers;
        private System.Windows.Forms.Label lblEntity;
        private System.Windows.Forms.ComboBox cbEntities;
        private System.Windows.Forms.Label lblRecordId;
        private System.Windows.Forms.TextBox txtRecordId;
        private System.Windows.Forms.Button btnCheckAccess;
        private System.Windows.Forms.Label lblAccessStatus;
        private System.Windows.Forms.Label lblRoles;
        private System.Windows.Forms.ListBox lbRoles;
        private System.Windows.Forms.Label lblTeams;
        private System.Windows.Forms.ListBox lbTeams;
        private System.Windows.Forms.Label lblAccessRights;
        private System.Windows.Forms.CheckedListBox clbAccessRights;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanelFlow;
        private System.Windows.Forms.Label lblFlow;
        private System.Windows.Forms.ComboBox cbFlows;
        private System.Windows.Forms.Label lblRecordType;
        private System.Windows.Forms.ComboBox cbRecordType;
        private System.Windows.Forms.Label lblInvokeRecordId;
        private System.Windows.Forms.TextBox txtInvokeRecordId;
        private System.Windows.Forms.Button btnInvoke;
        private System.Windows.Forms.Label lblInvokeStatus;
        private System.Windows.Forms.Label lblHistory;
        private System.Windows.Forms.DataGridView dgvHistory;
        private System.Windows.Forms.Button btnClearHistory;
        private System.Windows.Forms.DataGridViewTextBoxColumn colTime;
        private System.Windows.Forms.DataGridViewTextBoxColumn colFlowName;
        private System.Windows.Forms.DataGridViewTextBoxColumn colStatus;
        private System.Windows.Forms.DataGridViewTextBoxColumn colResponse;
    }
}