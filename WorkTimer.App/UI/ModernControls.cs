using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace WorkTimer.App.UI;

/// <summary>
/// Modern styled progress bar with customizable appearance
/// </summary>
public class ModernProgressBar : Control
{
    private int _value;
    private int _maximum = 100;
    private int _cornerRadius = 8;
    private Color _barColor = Theme.ProgressForeground;

    public int Value
    {
 get => _value;
        set
        {
       _value = Math.Clamp(value, 0, _maximum);
            Invalidate();
        }
    }

    public int Maximum
    {
   get => _maximum;
        set
  {
            _maximum = Math.Max(1, value);
            _value = Math.Clamp(_value, 0, _maximum);
            Invalidate();
        }
    }

    public int CornerRadius
    {
  get => _cornerRadius;
        set
     {
            _cornerRadius = Math.Max(0, value);
  Invalidate();
        }
}

    public Color BarColor
    {
    get => _barColor;
        set
     {
        _barColor = value;
    Invalidate();
        }
    }

    public ModernProgressBar()
    {
        SetStyle(ControlStyles.UserPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.AllPaintingInWmPaint, true);
        UpdateStyles();
        Height = 24;
        BackColor = Theme.ProgressBackground;
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
   
        var g = e.Graphics;
        g.SmoothingMode = SmoothingMode.AntiAlias;

        // Background
 using (var bgBrush = new SolidBrush(Theme.ProgressBackground))
      using (var bgPath = GetRoundedRect(ClientRectangle, _cornerRadius))
        {
   g.FillPath(bgBrush, bgPath);
   }

        // Foreground
   if (_value > 0 && _maximum > 0)
 {
 float percentage = (float)_value / _maximum;
       int fillWidth = (int)(ClientRectangle.Width * percentage);
    
            if (fillWidth > 0)
    {
        var fillRect = new Rectangle(0, 0, fillWidth, ClientRectangle.Height);
        using (var fgBrush = new SolidBrush(_barColor))
                using (var fgPath = GetRoundedRect(fillRect, _cornerRadius))
   {
    g.FillPath(fgBrush, fgPath);
      }
    }
   }

    // Border
        using (var borderPen = new Pen(Theme.Border, 1))
        using (var borderPath = GetRoundedRect(new Rectangle(0, 0, ClientRectangle.Width - 1, ClientRectangle.Height - 1), _cornerRadius))
        {
            g.DrawPath(borderPen, borderPath);
   }
    }

    private GraphicsPath GetRoundedRect(Rectangle rect, int radius)
    {
        var path = new GraphicsPath();
        
   if (radius <= 0)
        {
path.AddRectangle(rect);
   return path;
        }

   int diameter = radius * 2;
   var arc = new Rectangle(rect.Location, new Size(diameter, diameter));

     // Top left
        path.AddArc(arc, 180, 90);
        
        // Top right
    arc.X = rect.Right - diameter;
        path.AddArc(arc, 270, 90);
        
        // Bottom right
        arc.Y = rect.Bottom - diameter;
        path.AddArc(arc, 0, 90);
        
      // Bottom left
        arc.X = rect.Left;
        path.AddArc(arc, 90, 90);
        
        path.CloseFigure();
        return path;
  }
}

/// <summary>
/// Modern styled button with hover effects
/// </summary>
public class ModernButton : Button
{
    private bool _isHovered;
    private bool _isPrimary;
    private int _cornerRadius = 6;

    public bool IsPrimary
    {
        get => _isPrimary;
     set
        {
            _isPrimary = value;
            UpdateColors();
            Invalidate();
        }
    }

    public int CornerRadius
    {
     get => _cornerRadius;
        set
 {
_cornerRadius = Math.Max(0, value);
            Invalidate();
}
    }

    public ModernButton()
    {
        SetStyle(ControlStyles.UserPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.AllPaintingInWmPaint, true);
    UpdateStyles();
     
      FlatStyle = FlatStyle.Flat;
        FlatAppearance.BorderSize = 0;
        Height = 36;
        Cursor = Cursors.Hand;
        
        UpdateColors();
    }

    private void UpdateColors()
    {
      if (_isPrimary)
        {
BackColor = Theme.Primary;
            ForeColor = Color.White;
        }
      else
      {
        BackColor = Theme.ButtonBackground;
   ForeColor = Theme.ButtonText;
        }
    }

 protected override void OnMouseEnter(EventArgs e)
    {
        base.OnMouseEnter(e);
    _isHovered = true;
        Invalidate();
    }

    protected override void OnMouseLeave(EventArgs e)
    {
        base.OnMouseLeave(e);
        _isHovered = false;
        Invalidate();
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        var g = e.Graphics;
        g.SmoothingMode = SmoothingMode.AntiAlias;
        g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

        // Background
        Color bgColor;
        if (!Enabled)
    {
bgColor = Theme.Disabled;
        }
        else if (_isHovered)
        {
         bgColor = _isPrimary ? Theme.PrimaryHover : Theme.ButtonHover;
        }
        else
        {
            bgColor = _isPrimary ? Theme.Primary : Theme.ButtonBackground;
        }

     using (var bgBrush = new SolidBrush(bgColor))
        using (var path = GetRoundedRect(ClientRectangle, _cornerRadius))
        {
          g.FillPath(bgBrush, path);
        }

        // Text with icon support
        if (!string.IsNullOrEmpty(Text))
   {
       var textColor = Enabled ? (_isPrimary ? Color.White : Theme.ButtonText) : Theme.TextSecondary;
            using (var textBrush = new SolidBrush(textColor))
      {
     var sf = new StringFormat
        {
         Alignment = StringAlignment.Center,
         LineAlignment = StringAlignment.Center
      };
    
     var textRect = ClientRectangle;
      if (Image != null)
    {
     // Draw image
        int imageX = (Width - Image.Width - TextRenderer.MeasureText(Text, Font).Width - 4) / 2;
int imageY = (Height - Image.Height) / 2;
          g.DrawImage(Image, imageX, imageY);
          
        // Adjust text position
  textRect = new Rectangle(imageX + Image.Width + 4, 0, Width - imageX - Image.Width - 4, Height);
     sf.Alignment = StringAlignment.Near;
      }
                
          g.DrawString(Text, Font, textBrush, textRect, sf);
  }
    }
        else if (Image != null)
        {
            // Just draw centered image
          int imageX = (Width - Image.Width) / 2;
            int imageY = (Height - Image.Height) / 2;
          g.DrawImage(Image, imageX, imageY);
        }
    }

  private GraphicsPath GetRoundedRect(Rectangle rect, int radius)
    {
        var path = new GraphicsPath();
        
   if (radius <= 0)
{
            path.AddRectangle(rect);
            return path;
   }

     int diameter = radius * 2;
        var arc = new Rectangle(rect.Location, new Size(diameter, diameter));

        path.AddArc(arc, 180, 90);
        arc.X = rect.Right - diameter;
        path.AddArc(arc, 270, 90);
        arc.Y = rect.Bottom - diameter;
        path.AddArc(arc, 0, 90);
        arc.X = rect.Left;
        path.AddArc(arc, 90, 90);
        
        path.CloseFigure();
        return path;
    }
}

/// <summary>
/// Modern card panel with shadow effect
/// </summary>
public class ModernCard : Panel
{
    private bool _isHovered;
    private bool _isActive;
    private int _cornerRadius = 8;
    private bool _showShadow = true;

    public bool IsActive
    {
        get => _isActive;
    set
        {
         _isActive = value;
       Invalidate();
        }
 }

    public int CornerRadius
    {
        get => _cornerRadius;
        set
        {
    _cornerRadius = Math.Max(0, value);
Invalidate();
        }
    }

    public bool ShowShadow
    {
        get => _showShadow;
  set
  {
            _showShadow = value;
   Invalidate();
      }
    }

    public ModernCard()
    {
        SetStyle(ControlStyles.UserPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.AllPaintingInWmPaint, true);
     UpdateStyles();
        
   BackColor = Theme.CardBackground;
        Padding = new Padding(12);
        Margin = new Padding(0, 0, 0, 8);
}

    protected override void OnMouseEnter(EventArgs e)
    {
        base.OnMouseEnter(e);
        _isHovered = true;
 Invalidate();
    }

    protected override void OnMouseLeave(EventArgs e)
    {
   base.OnMouseLeave(e);
        _isHovered = false;
      Invalidate();
    }

    protected override void OnPaint(PaintEventArgs e)
    {
      var g = e.Graphics;
  g.SmoothingMode = SmoothingMode.AntiAlias;

        // Background color based on state
   Color bgColor = _isActive ? Theme.CardActive : (_isHovered ? Theme.CardHover : Theme.CardBackground);

        // Shadow (simple implementation)
        if (_showShadow && Theme.Current == AppTheme.Light)
        {
   using (var shadowBrush = new SolidBrush(Color.FromArgb(20, 0, 0, 0)))
 using (var shadowPath = GetRoundedRect(new Rectangle(2, 2, Width - 4, Height - 4), _cornerRadius))
       {
           g.FillPath(shadowBrush, shadowPath);
    }
     }

        // Background
   using (var bgBrush = new SolidBrush(bgColor))
        using (var path = GetRoundedRect(new Rectangle(0, 0, Width - 1, Height - 1), _cornerRadius))
        {
          g.FillPath(bgBrush, path);
        }

        // Border
   using (var borderPen = new Pen(Theme.CardBorder, 1))
        using (var borderPath = GetRoundedRect(new Rectangle(0, 0, Width - 1, Height - 1), _cornerRadius))
        {
            g.DrawPath(borderPen, borderPath);
        }

  // Paint children
        base.OnPaint(e);
    }

  private GraphicsPath GetRoundedRect(Rectangle rect, int radius)
    {
        var path = new GraphicsPath();
        
     if (radius <= 0)
        {
          path.AddRectangle(rect);
     return path;
    }

        int diameter = radius * 2;
 var arc = new Rectangle(rect.Location, new Size(diameter, diameter));

        path.AddArc(arc, 180, 90);
        arc.X = rect.Right - diameter;
        path.AddArc(arc, 270, 90);
 arc.Y = rect.Bottom - diameter;
        path.AddArc(arc, 0, 90);
        arc.X = rect.Left;
    path.AddArc(arc, 90, 90);
        
        path.CloseFigure();
        return path;
    }
}
