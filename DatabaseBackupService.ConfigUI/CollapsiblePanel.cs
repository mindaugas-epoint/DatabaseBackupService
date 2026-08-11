using System.Drawing.Drawing2D;
using System.ComponentModel;

namespace DatabaseBackupService.ConfigUI
{
    public class CollapsiblePanel : Panel
    {
        private Panel _headerPanel = null!;
        private Panel _contentPanel = null!;
        private Label _titleLabel = null!;
        private Label _expandCollapseLabel = null!;
        private bool _isExpanded = true;
        private int _collapsedHeight = 40;
        private int _expandedHeight = 200;

        public CollapsiblePanel()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            // Header Panel
            _headerPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = _collapsedHeight,
                BackColor = Color.FromArgb(240, 240, 240),
                Cursor = Cursors.Hand
            };
            _headerPanel.Paint += HeaderPanel_Paint;
            _headerPanel.Click += HeaderPanel_Click;
            _headerPanel.MouseEnter += (s, e) => _headerPanel.BackColor = Color.FromArgb(230, 230, 230);
            _headerPanel.MouseLeave += (s, e) => _headerPanel.BackColor = Color.FromArgb(240, 240, 240);

            // Title Label
            _titleLabel = new Label
            {
                AutoSize = false,
                Dock = DockStyle.Fill,
                Text = "Panel Title",
                Font = new Font("Segoe UI", 10F, FontStyle.Regular),
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(15, 0, 0, 0),
                ForeColor = Color.FromArgb(50, 50, 50)
            };
            _titleLabel.Click += HeaderPanel_Click;

            // Expand/Collapse Indicator
            _expandCollapseLabel = new Label
            {
                AutoSize = false,
                Dock = DockStyle.Right,
                Width = 40,
                Text = "▼",
                Font = new Font("Segoe UI", 9F),
                TextAlign = ContentAlignment.MiddleCenter,
                ForeColor = Color.FromArgb(100, 100, 100)
            };
            _expandCollapseLabel.Click += HeaderPanel_Click;

            _headerPanel.Controls.Add(_titleLabel);
            _headerPanel.Controls.Add(_expandCollapseLabel);

            // Content Panel
            _contentPanel = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(15, 10, 15, 10),
                BackColor = Color.White
            };

            this.Controls.Add(_contentPanel);
            this.Controls.Add(_headerPanel);

            this.BorderStyle = BorderStyle.None;
            this.Padding = new Padding(0);
            this.BackColor = Color.White;
        }

        private void HeaderPanel_Paint(object? sender, PaintEventArgs e)
        {
            // Draw bottom border
            using (Pen pen = new Pen(Color.FromArgb(220, 220, 220), 1))
            {
                e.Graphics.DrawLine(pen, 0, _headerPanel.Height - 1, _headerPanel.Width, _headerPanel.Height - 1);
            }
        }

        private void HeaderPanel_Click(object? sender, EventArgs e)
        {
            ToggleExpanded();
        }

        public event EventHandler? ExpandedChanged;

        public void ToggleExpanded()
        {
            IsExpanded = !IsExpanded;
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        [DefaultValue(true)]
        public bool IsExpanded
        {
            get => _isExpanded;
            set
            {
                _isExpanded = value;
                _contentPanel.Visible = _isExpanded;
                _expandCollapseLabel.Text = _isExpanded ? "▼" : "▶";

                if (_isExpanded)
                {
                    this.Height = _expandedHeight;
                }
                else
                {
                    this.Height = _collapsedHeight;
                }

                ExpandedChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public string HeaderText
        {
            get => _titleLabel.Text;
            set => _titleLabel.Text = value;
        }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Panel ContentPanel => _contentPanel;

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        [DefaultValue(200)]
        public int ExpandedHeight
        {
            get => _expandedHeight;
            set
            {
                _expandedHeight = value;
                if (_isExpanded)
                {
                    this.Height = _expandedHeight;
                }
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public Color HeaderBackColor
        {
            get => _headerPanel.BackColor;
            set => _headerPanel.BackColor = value;
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public Color HeaderForeColor
        {
            get => _titleLabel.ForeColor;
            set => _titleLabel.ForeColor = value;
        }
    }
}
