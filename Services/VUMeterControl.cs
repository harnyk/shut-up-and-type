using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace ShutUpAndType.Services
{
    public class VUMeterControl : UserControl
    {
        private float _currentLevel = 0f;
        private float _peakLevel = 0f;
        private float _displayLevel = 0f;
        private readonly System.Windows.Forms.Timer _animationTimer;
        private readonly System.Windows.Forms.Timer _peakDecayTimer;
        private const float DECAY_RATE = 0.95f;
        private const float PEAK_DECAY_RATE = 0.98f;
        private const float SMOOTHING_FACTOR = 0.3f;

        public VUMeterControl()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.UserPaint |
                     ControlStyles.DoubleBuffer |
                     ControlStyles.ResizeRedraw, true);

            BackColor = Color.Black;
            Size = new Size(200, 30);

            // Animation timer for smooth level transitions
            _animationTimer = new System.Windows.Forms.Timer();
            _animationTimer.Interval = 16; // ~60 FPS
            _animationTimer.Tick += OnAnimationTick;
            _animationTimer.Start();

            // Peak decay timer
            _peakDecayTimer = new System.Windows.Forms.Timer();
            _peakDecayTimer.Interval = 50;
            _peakDecayTimer.Tick += OnPeakDecayTick;
            _peakDecayTimer.Start();
        }

        public void SetLevel(float level)
        {
            // Convert to 0-1 range and apply logarithmic scaling
            level = Math.Max(0, Math.Min(1, level));

            // Apply logarithmic scaling for more realistic VU behavior
            if (level > 0)
            {
                // Convert linear to dB, then back to 0-1 range
                float db = 20 * (float)Math.Log10(level);
                db = Math.Max(-60, Math.Min(0, db)); // Clamp between -60dB and 0dB
                level = (db + 60) / 60; // Convert to 0-1 range
            }

            _currentLevel = level;

            // Update peak level
            if (level > _peakLevel)
            {
                _peakLevel = level;
            }
        }

        private void OnAnimationTick(object? sender, EventArgs e)
        {
            // Smooth level transitions
            float targetLevel = _currentLevel;
            _displayLevel += (targetLevel - _displayLevel) * SMOOTHING_FACTOR;

            // Apply decay when no signal
            if (_currentLevel == 0)
            {
                _displayLevel *= DECAY_RATE;
            }

            if (Math.Abs(_displayLevel - targetLevel) > 0.001f || _displayLevel > 0.001f)
            {
                Invalidate();
            }
        }

        private void OnPeakDecayTick(object? sender, EventArgs e)
        {
            // Slowly decay peak level
            _peakLevel *= PEAK_DECAY_RATE;
            if (_peakLevel < 0.001f)
            {
                _peakLevel = 0f;
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            // Clear background with deep black
            g.Clear(Color.FromArgb(10, 10, 10));

            // Draw the tube background (dark glass effect)
            DrawTubeBackground(g);

            // Draw the phosphor glow
            DrawPhosphorGlow(g);

            // Draw the scale marks
            DrawScale(g);

            // Draw glass reflection effect
            DrawGlassReflection(g);
        }

        private void DrawTubeBackground(Graphics g)
        {
            Rectangle tubeRect = new Rectangle(5, 5, Width - 10, Height - 10);

            // Create gradient for tube glass effect
            using (LinearGradientBrush brush = new LinearGradientBrush(
                tubeRect,
                Color.FromArgb(40, 40, 40),
                Color.FromArgb(20, 20, 20),
                LinearGradientMode.Vertical))
            {
                g.FillRectangle(brush, tubeRect);
            }

            // Draw tube border
            using (Pen borderPen = new Pen(Color.FromArgb(60, 60, 60), 1))
            {
                g.DrawRectangle(borderPen, tubeRect);
            }
        }

        private void DrawPhosphorGlow(Graphics g)
        {
            if (_displayLevel <= 0) return;

            Rectangle glowRect = new Rectangle(8, 8, Width - 16, Height - 16);
            int levelWidth = (int)(glowRect.Width * _displayLevel);
            int peakWidth = (int)(glowRect.Width * _peakLevel);

            if (levelWidth > 0)
            {
                // Main phosphor glow
                Rectangle levelRect = new Rectangle(glowRect.X, glowRect.Y, levelWidth, glowRect.Height);
                DrawPhosphorSegment(g, levelRect, _displayLevel);
            }

            // Draw peak indicator
            if (peakWidth > 2 && _peakLevel > _displayLevel)
            {
                Rectangle peakRect = new Rectangle(
                    glowRect.X + peakWidth - 2,
                    glowRect.Y,
                    3,
                    glowRect.Height);

                using (SolidBrush peakBrush = new SolidBrush(GetPhosphorColor(1.0f)))
                {
                    g.FillRectangle(peakBrush, peakRect);
                }

                // Peak glow effect
                using (LinearGradientBrush glowBrush = new LinearGradientBrush(
                    new Rectangle(peakRect.X - 3, peakRect.Y - 2, peakRect.Width + 6, peakRect.Height + 4),
                    Color.FromArgb(100, GetPhosphorColor(1.0f)),
                    Color.Transparent,
                    LinearGradientMode.Horizontal))
                {
                    g.FillRectangle(glowBrush, new Rectangle(peakRect.X - 3, peakRect.Y - 2, peakRect.Width + 6, peakRect.Height + 4));
                }
            }
        }

        private void DrawPhosphorSegment(Graphics g, Rectangle rect, float intensity)
        {
            // Create gradient for phosphor glow
            Color baseColor = GetPhosphorColor(intensity);
            Color glowColor = Color.FromArgb(80, baseColor);

            using (LinearGradientBrush brush = new LinearGradientBrush(
                rect,
                Color.FromArgb(20, baseColor),
                baseColor,
                LinearGradientMode.Vertical))
            {
                g.FillRectangle(brush, rect);
            }

            // Add outer glow
            Rectangle glowRect = new Rectangle(rect.X - 1, rect.Y - 1, rect.Width + 2, rect.Height + 2);
            using (LinearGradientBrush glowBrush = new LinearGradientBrush(
                glowRect,
                glowColor,
                Color.Transparent,
                LinearGradientMode.Vertical))
            {
                g.FillRectangle(glowBrush, glowRect);
            }
        }

        private Color GetPhosphorColor(float intensity)
        {
            // Authentic CRT phosphor green with intensity-based color shift
            if (intensity < 0.7f)
            {
                // Green phosphor (P31)
                int green = (int)(255 * intensity * 1.2f);
                int red = (int)(80 * intensity);
                return Color.FromArgb(red, Math.Min(255, green), red / 2);
            }
            else
            {
                // Shift to yellow/white for high levels (phosphor saturation effect)
                float yellowShift = (intensity - 0.7f) / 0.3f;
                int red = (int)(80 + 175 * yellowShift);
                int green = 255;
                int blue = (int)(40 * yellowShift);
                return Color.FromArgb(red, green, blue);
            }
        }

        private void DrawScale(Graphics g)
        {
            // Draw scale marks like on vintage VU meters
            Rectangle scaleRect = new Rectangle(8, Height - 15, Width - 16, 10);

            using (Pen scalePen = new Pen(Color.FromArgb(100, 100, 100), 1))
            {
                // Draw major scale marks
                for (int i = 0; i <= 10; i++)
                {
                    int x = scaleRect.X + (scaleRect.Width * i / 10);
                    g.DrawLine(scalePen, x, scaleRect.Y, x, scaleRect.Y + (i % 5 == 0 ? 6 : 3));
                }
            }

            // Draw "VU" label
            using (Font font = new Font("Arial", 6, FontStyle.Bold))
            using (SolidBrush textBrush = new SolidBrush(Color.FromArgb(120, 120, 120)))
            {
                g.DrawString("VU", font, textBrush, Width - 20, Height - 12);
            }
        }

        private void DrawGlassReflection(Graphics g)
        {
            // Subtle glass reflection effect
            Rectangle reflectRect = new Rectangle(5, 5, Width - 10, (Height - 10) / 3);

            using (LinearGradientBrush reflectBrush = new LinearGradientBrush(
                reflectRect,
                Color.FromArgb(30, 255, 255, 255),
                Color.Transparent,
                LinearGradientMode.Vertical))
            {
                g.FillRectangle(reflectBrush, reflectRect);
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _animationTimer?.Stop();
                _animationTimer?.Dispose();
                _peakDecayTimer?.Stop();
                _peakDecayTimer?.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}