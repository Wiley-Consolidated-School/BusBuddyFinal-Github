using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using System.ComponentModel;
using BusBuddy.UI.Theme;

namespace BusBuddy.UI.Controls
{
    /// <summary>
    /// KICK A$$ Material Design DataGridView with custom painting, animations, and visual effects
    /// </summary>
    public class MaterialDataGridView : DataGridView
    {
        private System.Windows.Forms.Timer _animationTimer;
        private float _animationProgress = 0f;
        private int _hoveredRowIndex = -1;
        private int _selectedRowIndex = -1;
        private bool _useAlternatingRowColors = true;
        private Color _primaryColor = AppTheme.PrimaryColor;
        private Color _accentColor = AppTheme.PrimaryColorLight;
        private Color _hoverColor = Color.FromArgb(30, 255, 255, 255);
        private Color _alternateRowColor = Color.FromArgb(20, 255, 255, 255);

        public MaterialDataGridView()
        {
            InitializeKickAssStyle();
            InitializeAnimationTimer();
            SetupEventHandlers();
        }

        [Category("Material Design")]
        [Description("Use alternating row colors for better readability")]
        public bool UseAlternatingRowColors
        {
            get => _useAlternatingRowColors;
            set
            {
                _useAlternatingRowColors = value;
                Invalidate();
            }
        }

        [Category("Material Design")]
        [Description("Primary color for selection and highlights")]
        public Color PrimaryColor
        {
            get => _primaryColor;
            set
            {
                _primaryColor = value;
                Invalidate();
            }
        }

        [Category("Material Design")]
        [Description("Accent color for special highlights")]
        public Color AccentColor
        {
            get => _accentColor;
            set
            {
                _accentColor = value;
                Invalidate();
            }
        }

        private void InitializeKickAssStyle()
        {
            // Basic setup for KICK A$$ appearance
            SetStyle(ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.UserPaint |
                     ControlStyles.DoubleBuffer |
                     ControlStyles.ResizeRedraw, true);

            // Grid appearance
            BorderStyle = BorderStyle.None;
            BackgroundColor = AppTheme.BackgroundGray;
            GridColor = AppTheme.SecondaryGray;

            // Selection and interaction
            SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            MultiSelect = false;
            ReadOnly = true;
            AllowUserToAddRows = false;
            AllowUserToDeleteRows = false;
            AllowUserToResizeRows = false;

            // Auto-sizing
            AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            ColumnHeadersHeight = 50;
            RowTemplate.Height = 60; // Taller rows for more content

            // Scrollbars
            ScrollBars = ScrollBars.Vertical;

            // Header styling
            EnableHeadersVisualStyles = false;
            ColumnHeadersDefaultCellStyle.BackColor = _primaryColor;
            ColumnHeadersDefaultCellStyle.ForeColor = AppTheme.TextOnPrimary;
            ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 11, FontStyle.Bold);
            ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
            ColumnHeadersDefaultCellStyle.SelectionBackColor = _primaryColor;
            ColumnHeadersDefaultCellStyle.SelectionForeColor = AppTheme.TextOnPrimary;
            ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.None;

            // Row styling
            DefaultCellStyle.BackColor = AppTheme.BackgroundGray;
            DefaultCellStyle.ForeColor = AppTheme.TextPrimary;
            DefaultCellStyle.Font = new Font("Segoe UI", 10, FontStyle.Regular);
            DefaultCellStyle.SelectionBackColor = Color.FromArgb(80, _primaryColor);
            DefaultCellStyle.SelectionForeColor = AppTheme.TextPrimary;
            DefaultCellStyle.Padding = new Padding(8, 4, 8, 4);

            // Alternating row colors
            AlternatingRowsDefaultCellStyle.BackColor = _alternateRowColor;
            AlternatingRowsDefaultCellStyle.ForeColor = AppTheme.TextPrimary;
            AlternatingRowsDefaultCellStyle.SelectionBackColor = Color.FromArgb(80, _primaryColor);
            AlternatingRowsDefaultCellStyle.SelectionForeColor = AppTheme.TextPrimary;

            // Remove focus rectangle
            RowsDefaultCellStyle.SelectionBackColor = Color.FromArgb(60, _primaryColor);
            CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
        }

        private void InitializeAnimationTimer()
        {
            _animationTimer = new System.Windows.Forms.Timer();
            _animationTimer.Interval = 16; // ~60 FPS
            _animationTimer.Tick += AnimationTimer_Tick;
        }

        private void SetupEventHandlers()
        {
            CellMouseEnter += MaterialDataGridView_CellMouseEnter;
            CellMouseLeave += MaterialDataGridView_CellMouseLeave;
            SelectionChanged += MaterialDataGridView_SelectionChanged;
            CellPainting += MaterialDataGridView_CellPainting;
            // Remove the problematic events for now - they don't exist in standard DataGridView
            // ColumnHeaderMouseEnter += MaterialDataGridView_ColumnHeaderMouseEnter;
            // ColumnHeaderMouseLeave += MaterialDataGridView_ColumnHeaderMouseLeave;
        }

        private void MaterialDataGridView_CellMouseEnter(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                _hoveredRowIndex = e.RowIndex;
                StartHoverAnimation();
            }
        }

        private void MaterialDataGridView_CellMouseLeave(object sender, DataGridViewCellEventArgs e)
        {
            _hoveredRowIndex = -1;
            StartHoverAnimation();
        }

        private void MaterialDataGridView_SelectionChanged(object sender, EventArgs e)
        {
            if (SelectedRows.Count > 0)
            {
                _selectedRowIndex = SelectedRows[0].Index;
            }
            else
            {
                _selectedRowIndex = -1;
            }
        }

        private void MaterialDataGridView_ColumnHeaderMouseEnter(object sender, DataGridViewCellMouseEventArgs e)
        {
            Cursor = Cursors.Hand;
        }

        private void MaterialDataGridView_ColumnHeaderMouseLeave(object sender, DataGridViewCellMouseEventArgs e)
        {
            Cursor = Cursors.Default;
        }

        private void StartHoverAnimation()
        {
            if (!_animationTimer.Enabled)
            {
                _animationProgress = 0f;
                _animationTimer.Start();
            }
        }

        private void AnimationTimer_Tick(object sender, EventArgs e)
        {
            _animationProgress += 0.1f;

            if (_animationProgress >= 1.0f)
            {
                _animationProgress = 1.0f;
                _animationTimer.Stop();
            }

            Invalidate(); // Trigger repaint for smooth animation
        }

        private void MaterialDataGridView_CellPainting(object sender, DataGridViewCellPaintingEventArgs e)
        {
            if (e.RowIndex < 0) return; // Skip header painting

            // Custom paint the cells with KICK A$$ effects
            e.Handled = true;

            var cellRect = e.CellBounds;
            var g = e.Graphics;
            if (g != null)
            {
                g.SmoothingMode = SmoothingMode.AntiAlias;

                // Base background
                var bgColor = GetCellBackgroundColor(e.RowIndex);
                using (var bgBrush = new SolidBrush(bgColor))
                {
                    g.FillRectangle(bgBrush, cellRect);
                }

            // Hover effect with smooth animation
            if (e.RowIndex == _hoveredRowIndex)
            {
                var hoverAlpha = (int)(30 * _animationProgress);
                var hoverColor = Color.FromArgb(hoverAlpha, _accentColor);
                using (var hoverBrush = new SolidBrush(hoverColor))
                {
                    g.FillRectangle(hoverBrush, cellRect);
                }

                // Add subtle glow effect
                DrawGlowEffect(g, cellRect, _accentColor, _animationProgress);
            }

            // Selection indicator (left border)
            if (e.RowIndex == _selectedRowIndex)
            {
                var selectionRect = new Rectangle(cellRect.X, cellRect.Y, 4, cellRect.Height);
                using (var selectionBrush = new SolidBrush(_primaryColor))
                {
                    g.FillRectangle(selectionBrush, selectionRect);
                }
            }

            // Cell content with proper formatting
            DrawCellContent(g, e);

                // Bottom border
                using (var borderPen = new Pen(AppTheme.SecondaryGray, 1))
                {
                    g.DrawLine(borderPen, cellRect.X, cellRect.Bottom - 1, cellRect.Right, cellRect.Bottom - 1);
                }
            }
        }

        private Color GetCellBackgroundColor(int rowIndex)
        {
            if (rowIndex == _selectedRowIndex)
            {
                return Color.FromArgb(40, _primaryColor);
            }
            else if (_useAlternatingRowColors && rowIndex % 2 == 1)
            {
                return _alternateRowColor;
            }
            else
            {
                return AppTheme.BackgroundGray;
            }
        }

        private void DrawGlowEffect(Graphics g, Rectangle rect, Color glowColor, float intensity)
        {
            var glowRect = rect;
            glowRect.Inflate(2, 1);

            var alpha = (int)(20 * intensity);
            using (var glowBrush = new SolidBrush(Color.FromArgb(alpha, glowColor)))
            {
                g.FillRectangle(glowBrush, glowRect);
            }
        }

        private void DrawCellContent(Graphics g, DataGridViewCellPaintingEventArgs e)
        {
            var cellRect = e.CellBounds;
            var contentRect = new Rectangle(
                cellRect.X + 12,
                cellRect.Y + 8,
                cellRect.Width - 24,
                cellRect.Height - 16
            );

            // Text color based on selection/theme
            var textColor = e.RowIndex == _selectedRowIndex ?
                AppTheme.TextPrimary : AppTheme.TextPrimary;

            // Draw cell value with proper alignment
            if (e.Value != null)
            {
                var text = e.Value.ToString();
                var font = e.CellStyle?.Font ?? DefaultCellStyle?.Font ?? DefaultFont;

                // Add icons for special columns
                if (IsStatusColumn(e.ColumnIndex))
                {
                    DrawStatusIndicator(g, contentRect, text);
                }
                else if (IsProgressColumn(e.ColumnIndex))
                {
                    DrawProgressBar(g, contentRect, text);
                }
                else
                {
                    // Regular text with enhanced formatting
                    using (var textBrush = new SolidBrush(textColor))
                    {
                        var stringFormat = new StringFormat
                        {
                            Alignment = GetColumnAlignment(e.ColumnIndex),
                            LineAlignment = StringAlignment.Center,
                            Trimming = StringTrimming.EllipsisCharacter
                        };

                        g.DrawString(text, font, textBrush, contentRect, stringFormat);
                    }
                }
            }
        }

        private bool IsStatusColumn(int columnIndex)
        {
            if (Columns.Count > columnIndex)
            {
                var columnName = Columns[columnIndex].Name.ToLower();
                return columnName.Contains("status") || columnName.Contains("state");
            }
            return false;
        }

        private bool IsProgressColumn(int columnIndex)
        {
            if (Columns.Count > columnIndex)
            {
                var columnName = Columns[columnIndex].Name.ToLower();
                return columnName.Contains("progress") || columnName.Contains("percentage");
            }
            return false;
        }

        private void DrawStatusIndicator(Graphics g, Rectangle rect, string status)
        {
            var indicatorRect = new Rectangle(rect.X, rect.Y + rect.Height / 2 - 6, 12, 12);
            var color = GetStatusColor(status);

            // Draw circle indicator
            g.SmoothingMode = SmoothingMode.AntiAlias;
            using (var brush = new SolidBrush(color))
            {
                g.FillEllipse(brush, indicatorRect);
            }

            // Draw status text
            var textRect = new Rectangle(rect.X + 20, rect.Y, rect.Width - 20, rect.Height);
            using (var textBrush = new SolidBrush(AppTheme.TextPrimary))
            {
                var stringFormat = new StringFormat
                {
                    LineAlignment = StringAlignment.Center,
                    Alignment = StringAlignment.Near
                };
                g.DrawString(status, DefaultCellStyle.Font, textBrush, textRect, stringFormat);
            }
        }

        private void DrawProgressBar(Graphics g, Rectangle rect, string progressText)
        {
            if (float.TryParse(progressText.Replace("%", ""), out float percentage))
            {
                var progressRect = new Rectangle(rect.X, rect.Y + rect.Height / 2 - 4, rect.Width - 40, 8);
                var fillWidth = (int)(progressRect.Width * (percentage / 100f));

                // Background
                using (var bgBrush = new SolidBrush(Color.FromArgb(30, Color.White)))
                {
                    g.FillRoundedRectangle(bgBrush, progressRect, 4);
                }

                // Progress fill
                if (fillWidth > 0)
                {
                    var fillRect = new Rectangle(progressRect.X, progressRect.Y, fillWidth, progressRect.Height);
                    using (var fillBrush = new SolidBrush(_primaryColor))
                    {
                        g.FillRoundedRectangle(fillBrush, fillRect, 4);
                    }
                }

                // Percentage text
                var textRect = new Rectangle(progressRect.Right + 8, rect.Y, 32, rect.Height);
                using (var textBrush = new SolidBrush(AppTheme.TextPrimary))
                {
                    var stringFormat = new StringFormat
                    {
                        LineAlignment = StringAlignment.Center,
                        Alignment = StringAlignment.Near
                    };
                    g.DrawString($"{percentage:F0}%", DefaultCellStyle.Font, textBrush, textRect, stringFormat);
                }
            }
        }

        private Color GetStatusColor(string status)
        {
            return status?.ToLower() switch
            {
                "active" => AppTheme.SuccessGreen,
                "inactive" => AppTheme.ErrorRed,
                "pending" => AppTheme.WarningAmber,
                "maintenance" => AppTheme.WarningAmber,
                "completed" => AppTheme.SuccessGreen,
                "failed" => AppTheme.ErrorRed,
                _ => AppTheme.TextSecondary
            };
        }

        private StringAlignment GetColumnAlignment(int columnIndex)
        {
            if (Columns.Count > columnIndex)
            {
                var columnName = Columns[columnIndex].Name.ToLower();
                if (columnName.Contains("number") || columnName.Contains("count") ||
                    columnName.Contains("amount") || columnName.Contains("price"))
                {
                    return StringAlignment.Far; // Right align numbers
                }
            }
            return StringAlignment.Near; // Left align text
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _animationTimer?.Dispose();
            }
            base.Dispose(disposing);
        }
    }

    // Extension methods for enhanced graphics
    public static class GraphicsExtensions
    {
        public static void FillRoundedRectangle(this Graphics g, Brush brush, Rectangle rect, int radius)
        {
            using (var path = new GraphicsPath())
            {
                path.AddArc(rect.X, rect.Y, radius * 2, radius * 2, 180, 90);
                path.AddArc(rect.Right - radius * 2, rect.Y, radius * 2, radius * 2, 270, 90);
                path.AddArc(rect.Right - radius * 2, rect.Bottom - radius * 2, radius * 2, radius * 2, 0, 90);
                path.AddArc(rect.X, rect.Bottom - radius * 2, radius * 2, radius * 2, 90, 90);
                path.CloseFigure();
                g.FillPath(brush, path);
            }
        }
    }
}
