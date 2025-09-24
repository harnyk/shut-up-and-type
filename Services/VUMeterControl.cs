using System;
using System.Drawing;
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

            BackColor = Color.Transparent;
            Size = new Size(200, 30);

            // Animation timer for smooth level transitions
            _animationTimer = new System.Windows.Forms.Timer();
            _animationTimer.Interval = 100; // 10 FPS
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

            int centerX = Width / 2;
            int centerY = Height / 2;
            int maxLength = Width - 10;

            // Draw coordinate line (thin baseline)
            using (Pen coordPen = new Pen(Color.FromArgb(100, ForeColor), 1))
            {
                g.DrawLine(coordPen, 5, centerY, Width - 5, centerY);

                // Draw scale marks every 10%
                for (int i = 0; i <= 20; i++)
                {
                    int markX = 5 + (maxLength * i / 20);
                    g.DrawLine(coordPen, markX, centerY - 3, markX, centerY + 3);
                }
            }

            // Draw level indicator line centered
            if (_displayLevel > 0)
            {
                int totalLength = (int)(maxLength * _displayLevel);
                int halfLength = totalLength / 2;
                using (Pen pen = new Pen(ForeColor, 3))
                {
                    g.DrawLine(pen, centerX - halfLength, centerY, centerX + halfLength, centerY);
                }
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