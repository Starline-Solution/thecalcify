using ClosedXML.Excel;
using DocumentFormat.OpenXml.Office.PowerPoint.Y2021.M06.Main;
using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Windows.Forms;
using thecalcify;
using thecalcify.Modern_UI;

namespace thecalcify.Modern_UI
{
	public class ModernUIManager
	{
		private thecalcify _form;
		private Panel _topPanel;

		// --- COLORS (Pro Light Theme) ---
		Color c_HeaderBg = Color.White;
		Color c_TextMain = Color.FromArgb(30, 41, 59);
		Color c_Accent = Color.FromArgb(14, 165, 233);
		Color c_Border = Color.FromArgb(226, 232, 240);
		Color c_InputBg = Color.FromArgb(241, 245, 249);
		private Panel pnlFont;
		private Panel pnlSearch;
		private ToolStripMenuItem _modernNewWatchlistItem;
		public ModernUIManager(thecalcify form)
		{
			_form = form;
		}

		public void ApplyModernUI()
		{
			// 1. Hide Old Elements
			if (_form.Controls["headerPanel"] != null) _form.Controls["headerPanel"].Visible = false;
			if (_form.menuStrip1 != null) _form.menuStrip1.Visible = false;

			// 2. Create New Top Bar
			_topPanel = new Panel
			{
				Dock = DockStyle.Top,
				Height = 100,
				BackColor = c_HeaderBg,
				Padding = new Padding(15, 0, 15, 0)
			};

			// Draw subtle bottom border
			_topPanel.Paint += (s, e) =>
			{
				e.Graphics.DrawLine(new Pen(c_Border, 1), 0, _topPanel.Height - 1, _topPanel.Width, _topPanel.Height - 1);
			};

			_form.Controls.Add(_topPanel);
			_topPanel.BringToFront();

			Panel pnlUpper = new Panel
			{
				Dock = DockStyle.Top,
				Height = 50,
				BackColor = Color.Transparent
			};

			Panel pnlLower = new Panel
			{
				Dock = DockStyle.Fill, // Takes remaining space (bottom half)
				BackColor = Color.Transparent,
				Padding = new Padding(10, 10, 10, 0)
			};

			// Add Lower first, then Top (Standard Docking Order)
			_topPanel.Controls.Add(pnlLower);
			_topPanel.Controls.Add(pnlUpper);

			// 3. Add Logo
			PictureBox pbLogo = new PictureBox
			{
				// ✅ Load image from resources
				Image = global::thecalcify.Properties.Resources.starline_solution,
				SizeMode = PictureBoxSizeMode.Zoom, // Ensures logo fits without stretching
				Size = new Size(180, 50), // Adjust Width (180) if your logo is wider/narrower
				Location = new Point(15, 8), // Vertically centered roughly
				BackColor = Color.Transparent,
				Left = (pnlUpper.Width - 180) / 2,
				Top = (pnlUpper.Height - 50) / 2,

			};
			pnlUpper.Controls.Add(pbLogo);

			// 4. Create Menu Buttons (Left side)
			int currentX = -10;

			// Tools
			Button btnTools = CreateMenuButton("Tools", currentX);
			btnTools.Click += (s, e) => ShowToolsMenu(btnTools);
			pnlLower.Controls.Add(btnTools);

			// Market Watch
			currentX += 90;
			Button btnMarket = CreateMenuButton("Market Watch", currentX);
			btnMarket.Width = 140;
			btnMarket.Click += (s, e) => ShowMarketMenu(btnMarket);
			pnlLower.Controls.Add(btnMarket);

			// News
			currentX += 140;
			Button btnNews = CreateMenuButton("News", currentX);
			btnNews.Click += (s, e) => ShowNewsMenu(btnNews);
			pnlLower.Controls.Add(btnNews);

			// About
			currentX += 90;
			Button btnAbout = CreateMenuButton("About", currentX);
			btnAbout.Click += (s, e) => _form.AboutToolStripMenuItem_Click(s, e);
			pnlLower.Controls.Add(btnAbout);

			// ====================================================================
			// 5. RIGHT SIDE CONTROLS (Perfect Alignment Logic)
			// ====================================================================

			// ✅ Unified Height for all controls
			int controlHeight = 35;
			int panelCenterY = (pnlLower.Height - controlHeight) / 2; // (60 - 40) / 2 = 10
			int rightX = _form.ClientSize.Width - 20;
			int spacing = 10;

			// --- A. REFRESH BUTTON ---
			Button btnRefresh = new Button
			{
				Text = "↻",
				Font = new Font("Segoe UI Symbol", 17, FontStyle.Regular),
				FlatStyle = FlatStyle.Flat,
				Size = new Size(40, controlHeight), // 40x40 Square
				Cursor = Cursors.Hand,
				ForeColor = Color.Gray,
				TextAlign = ContentAlignment.MiddleCenter,
				UseCompatibleTextRendering = true,
				Visible = _form.refreshMarketWatchHost.Available // Sync initial state
			};
			btnRefresh.FlatAppearance.BorderSize = 0;
			btnRefresh.FlatAppearance.MouseOverBackColor = c_InputBg;

			btnRefresh.Location = new Point(rightX - (btnRefresh.Width + 10), panelCenterY);
			btnRefresh.Click += (s, e) => _form.RefreshMarketWatchHost_Click(s, e);

			pnlLower.Controls.Add(btnRefresh);
			rightX -= (btnRefresh.Width + spacing);

			// --- B. SAVE BUTTON ---
			// Added logic: Same position logic as Refresh, but to the left of it (or replacing it if handled by visibility)
			Button btnSave = new Button
			{
				Text = "Save MarketWatch",
				Font = new Font("Segoe UI", 10, FontStyle.Bold),
				FlatStyle = FlatStyle.Flat,
				Size = new Size(200, controlHeight),
				Cursor = Cursors.Hand,
				ForeColor = c_Accent, // Blue text to indicate action
				TextAlign = ContentAlignment.MiddleCenter,
				Visible = _form.saveMarketWatchHost.Available // Sync initial state
			};
			btnSave.FlatAppearance.BorderColor = c_Accent;
			btnSave.FlatAppearance.BorderSize = 1;
			btnSave.Location = new Point(rightX - (btnSave.Width - 30), panelCenterY);
			btnSave.Click += (s, e) => _form.SaveMarketWatchHost_Click(s, e);

			pnlLower.Controls.Add(btnSave);
			//rightX -= (btnSave.Width + spacing);

			// --- C. FONT COMBO BOX ---
			ComboBox fontCombo = _form.Controls.Find("fontSizeComboBox", true)[0] as ComboBox;
			pnlFont = null;
			if (fontCombo != null)
			{
				pnlFont = new Panel
				{
					Size = new Size(fontCombo.Width + 10, controlHeight), // Slightly wider than raw combo
					BackColor = c_InputBg,
					Location = new Point(rightX - (fontCombo.Width + 20), panelCenterY),
					Visible = fontCombo.Visible // Sync initial visibility
				};

				pnlFont.Paint += (s, e) =>
				{
					ControlPaint.DrawBorder(e.Graphics, pnlFont.ClientRectangle, c_Border, ButtonBorderStyle.Solid);
				};

				fontCombo.Parent = pnlFont;
				fontCombo.FlatStyle = FlatStyle.Flat;
				fontCombo.BackColor = c_InputBg;

				fontCombo.Location = new Point(5, (controlHeight - fontCombo.Height) / 2);
				fontCombo.Width = pnlFont.Width - 10;

				pnlLower.Controls.Add(pnlFont);
				rightX -= (pnlFont.Width + spacing);
			}

			// --- D. SEARCH BOX ---
			pnlSearch = new Panel
			{
				Size = new Size(300, controlHeight),
				BackColor = c_InputBg,
				Location = new Point(rightX - 310, panelCenterY),
				Cursor = Cursors.IBeam
			};
			pnlSearch.Paint += (s, e) =>
			{
				ControlPaint.DrawBorder(e.Graphics, pnlSearch.ClientRectangle, c_Border, ButtonBorderStyle.Solid);
			};

			// 1. The Label "Search Text:"
			Label lblSearch = new Label
			{
				Text = "Search Text:",
				ForeColor = Color.Gray,
				AutoSize = true,
				Font = new Font("Segoe UI", 9, FontStyle.Regular),
				BackColor = c_InputBg,
			};
			// Dynamic vertical centering for Label
			pnlSearch.Controls.Add(lblSearch);
			lblSearch.Location = new Point(8, (controlHeight - lblSearch.Height) / 2);

			// 2. The TextBox
			TextBox searchBox = _form.Controls.Find("txtsearch", true)[0] as TextBox;
			if (searchBox != null)
			{
				searchBox.Parent = pnlSearch;
				searchBox.BorderStyle = BorderStyle.None;
				searchBox.BackColor = c_InputBg;
				searchBox.Font = new Font("Segoe UI", 9, FontStyle.Regular);
				searchBox.Width = pnlSearch.Width - lblSearch.Width - 25;

				// Dynamic vertical centering for TextBox (+1px for visual baseline fix)
				searchBox.Location = new Point(lblSearch.Right + 5, ((controlHeight - searchBox.Height) / 2) + 1);

				pnlSearch.Click += (s, e) => searchBox.Focus();
				lblSearch.Click += (s, e) => searchBox.Focus();
			}

			pnlLower.Controls.Add(pnlSearch);

			Label titleLabel = _form.Controls.Find("titleLabel", true).FirstOrDefault() as Label;

			if (titleLabel != null)
			{
				if (titleLabel.Parent != null)
					titleLabel.Parent.Controls.Remove(titleLabel);

				titleLabel.BackColor = Color.Transparent;
				titleLabel.Font = new Font("Segoe UI", 14, FontStyle.Bold);
				titleLabel.ForeColor = c_TextMain;
				titleLabel.AutoSize = true;

				pnlLower.Controls.Add(titleLabel);
				titleLabel.BringToFront();

				void CenterTitle()
				{
					titleLabel.Left = (pnlLower.Width - titleLabel.Width) / 2;
					titleLabel.Top = (pnlLower.Height - titleLabel.Height) / 2;
				}

				_form.Shown += (s, e) => CenterTitle();

				pnlLower.Resize += (s, e) => CenterTitle();
				titleLabel.TextChanged += (s, e) => CenterTitle();
			}

			// ====================================================================
			// 6. SYNCHRONIZATION EVENTS (Hooking Modern UI to Old Logic)
			// ====================================================================

			// Sync Refresh Button Visibility (using AvailableChanged because MenuStrip is hidden)
			_form.refreshMarketWatchHost.AvailableChanged += (s, e) =>
			{
				btnRefresh.Visible = _form.refreshMarketWatchHost.Available;
			};


			// Sync Save Button Visibility
			_form.saveMarketWatchHost.AvailableChanged += (s, e) =>
			{
				btnSave.Visible = _form.saveMarketWatchHost.Available;
			};
		}

		// --- HELPER METHODS ---

		private Button CreateMenuButton(string text, int x)
		{
			Button btn = new Button
			{
				Text = text,
				Location = new Point(x, 0),
				Size = new Size(90, 40),
				FlatStyle = FlatStyle.Flat,
				BackColor = Color.Transparent,
				ForeColor = c_TextMain,
				Font = new Font("Segoe UI", 10, FontStyle.Bold),
				Cursor = Cursors.Hand,
				TextAlign = ContentAlignment.MiddleCenter
			};
			btn.FlatAppearance.BorderSize = 0;
			btn.FlatAppearance.MouseOverBackColor = c_InputBg;
			return btn;
		}

		// --- MENU RENDERING ---

		private void ShowToolsMenu(Control anchor)
		{
			ContextMenuStrip cm = new ContextMenuStrip();
			cm.Renderer = new ModernMenuRenderer();

			ToolStripMenuItem item1 = new ToolStripMenuItem("❌ Disconnect (Shift+ESC)");
			//item1.Padding = new Padding(0, 8, 0, 15); 
			item1.Click += (s, e) => _form.DisconnectESCToolStripMenuItem_Click(s, e);

			ToolStripMenuItem item2 = new ToolStripMenuItem("🔲 Full Screen (ESC)");
			//item2.Padding = new Padding(0, 8, 0, 15);
			item2.Click += (s, e) => _form.FullScreenF11ToolStripMenuItem_Click(s, e);

			cm.Items.Add(item1);
			cm.Items.Add(item2);

			cm.Show(anchor, new Point(0, anchor.Height + 2)); // +2px gap looks nicer
		}

		private void ShowMarketMenu(Control anchor)
		{
			ContextMenuStrip cm = new ContextMenuStrip();
			cm.Renderer = new ModernMenuRenderer();
			_modernNewWatchlistItem = new ToolStripMenuItem("➕ New Watchlist (Ctrl+N)");
			_modernNewWatchlistItem.Click += (s, e) => _form.NewCTRLNToolStripMenuItem1_Click(s, e);
			if (_form.newCTRLNToolStripMenuItem1.Enabled)
			{
				_modernNewWatchlistItem.Enabled = true;
			}
			else
			{
				_modernNewWatchlistItem.Enabled = false;
			}

			cm.Items.Add(_modernNewWatchlistItem);

			ToolStripMenuItem viewItem = new ToolStripMenuItem("📈 View Watchlist");
			cm.Items.Add(viewItem);
			cm.Items.Add(new ToolStripSeparator());
			cm.Items.Add("🗑 Delete", null, (s, e) => _form.DeleteToolStripMenuItem_Click(s, e));

			if (_form.viewToolStripMenuItem != null && _form.viewToolStripMenuItem.DropDownItems.Count > 0)
			{
				foreach (ToolStripItem originalItem in _form.viewToolStripMenuItem.DropDownItems)
				{
					// Create visual copy
					ToolStripMenuItem modernItem = new ToolStripMenuItem("👁️‍🗨️ " + originalItem.Text);

					// PROXY CLICK: Run the logic defined in Home.cs
					modernItem.Click += (s, e) => originalItem.PerformClick();

					viewItem.DropDownItems.Add(modernItem);
				}
			}

			cm.Show(anchor, new Point(0, anchor.Height));
		}

		private void ShowNewsMenu(Control anchor)
		{
			ContextMenuStrip cm = new ContextMenuStrip();
			cm.Renderer = new ModernMenuRenderer();

			ToolStripMenuItem newsListParent = new ToolStripMenuItem("📰 News Watchlist");
			newsListParent.DropDownItems.Add("📋 News List", null, (s, e) => _form.NewsListToolStripMenuItem_Click(s, e));
			newsListParent.DropDownItems.Add("📜 News History", null, (s, e) => _form.NewsHistoryToolStripMenuItem_Click(s, e));

			cm.Items.Add(newsListParent);
			if (_form.notificationSettings.Available)
			{
				cm.Items.Add("🔔 Notification Settings", null, (s, e) => _form.NewsSettingsToolStrip_Click(s, e));
			}

			cm.Show(anchor, new Point(0, anchor.Height));
		}

		public void SetFontSizeComboBoxVisibility(bool visible)
		{
			if (pnlFont != null)
			{
				pnlFont.Visible = visible;
			}
		}

		public void SetSearchBoxVisibility(bool visible)
		{
			if (pnlSearch != null)
			{
				pnlSearch.Visible = visible;
			}
		}
	}
}