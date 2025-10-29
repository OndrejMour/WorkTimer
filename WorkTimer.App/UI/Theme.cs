using System.Drawing;
using System.Windows.Forms;

namespace WorkTimer.App.UI;

public enum AppTheme
{
    Light,
    Dark
}

public static class Theme
{
    // Current active theme
    private static AppTheme _current = AppTheme.Light;
    
    public static AppTheme Current
    {
        get => _current;
        set
    {
      _current = value;
         LoadColors();
 }
    }

 // Theme colors
    public static Color Background { get; private set; }
    public static Color Surface { get; private set; }
    public static Color SurfaceVariant { get; private set; }
    public static Color Border { get; private set; }
    public static Color Text { get; private set; }
    public static Color TextSecondary { get; private set; }
    public static Color Primary { get; private set; }
    public static Color PrimaryHover { get; private set; }
    public static Color Success { get; private set; }
    public static Color Warning { get; private set; }
    public static Color Error { get; private set; }
    public static Color Info { get; private set; }
    public static Color Disabled { get; private set; }
public static Color ProgressBackground { get; private set; }
    public static Color ProgressForeground { get; private set; }
    public static Color CardBackground { get; private set; }
    public static Color CardBorder { get; private set; }
    public static Color CardActive { get; private set; }
 public static Color CardHover { get; private set; }
    public static Color ButtonBackground { get; private set; }
    public static Color ButtonHover { get; private set; }
    public static Color ButtonText { get; private set; }
    public static Color InputBackground { get; private set; }
    public static Color InputBorder { get; private set; }
    public static Color InputFocusBorder { get; private set; }
  
    private static void LoadColors()
    {
        if (_current == AppTheme.Dark)
        {
 // Dark theme colors - modern dark palette
 Background = Color.FromArgb(18, 18, 18);
            Surface = Color.FromArgb(28, 28, 28);
            SurfaceVariant = Color.FromArgb(38, 38, 38);
            Border = Color.FromArgb(60, 60, 60);
      Text = Color.FromArgb(240, 240, 240);
      TextSecondary = Color.FromArgb(160, 160, 160);
            Primary = Color.FromArgb(100, 181, 246); // Light blue
  PrimaryHover = Color.FromArgb(79, 195, 247);
     Success = Color.FromArgb(102, 187, 106);
            Warning = Color.FromArgb(255, 167, 38);
            Error = Color.FromArgb(239, 83, 80);
            Info = Color.FromArgb(66, 165, 245);
   Disabled = Color.FromArgb(80, 80, 80);
            ProgressBackground = Color.FromArgb(45, 45, 45);
            ProgressForeground = Color.FromArgb(100, 181, 246);
            CardBackground = Color.FromArgb(33, 33, 33);
        CardBorder = Color.FromArgb(60, 60, 60);
 CardActive = Color.FromArgb(45, 45, 45);
            CardHover = Color.FromArgb(40, 40, 40);
        ButtonBackground = Color.FromArgb(66, 66, 66);
     ButtonHover = Color.FromArgb(80, 80, 80);
     ButtonText = Color.FromArgb(240, 240, 240);
    InputBackground = Color.FromArgb(40, 40, 40);
   InputBorder = Color.FromArgb(70, 70, 70);
      InputFocusBorder = Color.FromArgb(100, 181, 246);
      }
        else
   {
  // Light theme colors - modern clean palette
Background = Color.FromArgb(250, 250, 250);
 Surface = Color.White;
          SurfaceVariant = Color.FromArgb(245, 245, 245);
 Border = Color.FromArgb(224, 224, 224);
 Text = Color.FromArgb(33, 33, 33);
     TextSecondary = Color.FromArgb(117, 117, 117);
   Primary = Color.FromArgb(25, 118, 210); // Blue
         PrimaryHover = Color.FromArgb(21, 101, 192);
     Success = Color.FromArgb(46, 125, 50);
        Warning = Color.FromArgb(237, 108, 2);
   Error = Color.FromArgb(211, 47, 47);
      Info = Color.FromArgb(2, 136, 209);
       Disabled = Color.FromArgb(189, 189, 189);
     ProgressBackground = Color.FromArgb(224, 224, 224);
            ProgressForeground = Color.FromArgb(25, 118, 210);
            CardBackground = Color.White;
 CardBorder = Color.FromArgb(224, 224, 224);
    CardActive = Color.FromArgb(245, 250, 255);
  CardHover = Color.FromArgb(250, 250, 250);
            ButtonBackground = Color.FromArgb(240, 240, 240);
            ButtonHover = Color.FromArgb(230, 230, 230);
     ButtonText = Color.FromArgb(33, 33, 33);
    InputBackground = Color.White;
    InputBorder = Color.FromArgb(189, 189, 189);
  InputFocusBorder = Color.FromArgb(25, 118, 210);
        }
}

    static Theme()
    {
        LoadColors();
    }

    // Apply theme to control and its children recursively
    public static void ApplyToControl(Control control)
    {
        if (control == null) return;

        control.BackColor = Background;
     control.ForeColor = Text;

        // Apply specific styles based on control type
        switch (control)
        {
      case Form form:
    form.BackColor = Background;
  form.ForeColor = Text;
            break;
     
            case Button button:
             ApplyToButton(button);
                break;
     
            case TextBox textBox:
             ApplyToTextBox(textBox);
              break;
    
            case ComboBox comboBox:
         ApplyToComboBox(comboBox);
        break;
             
       case NumericUpDown numeric:
      ApplyToNumericUpDown(numeric);
   break;
   
            case Panel panel:
     panel.BackColor = Surface;
      break;
       
    case GroupBox groupBox:
 groupBox.BackColor = Background;
      groupBox.ForeColor = Text;
break;
     
    case Label label:
      // Labels inherit parent background
      if (label.Parent != null)
      {
      label.BackColor = Color.Transparent;
     }
         label.ForeColor = Text;
                break;
     
case CheckBox checkBox:
  checkBox.BackColor = Color.Transparent;
        checkBox.ForeColor = Text;
      break;
     
  case ProgressBar progressBar:
     ApplyToProgressBar(progressBar);
   break;
 }

        // Recursively apply to children
        foreach (Control child in control.Controls)
  {
            ApplyToControl(child);
        }
    }

    private static void ApplyToButton(Button button)
    {
        button.BackColor = ButtonBackground;
        button.ForeColor = ButtonText;
        button.FlatStyle = FlatStyle.Flat;
        button.FlatAppearance.BorderColor = ButtonBackground; // Same as background to hide border
        button.FlatAppearance.BorderSize = 0; // No border
        button.FlatAppearance.MouseOverBackColor = ButtonHover;
        button.FlatAppearance.MouseDownBackColor = PrimaryHover;
  }

    private static void ApplyToTextBox(TextBox textBox)
    {
        textBox.BackColor = InputBackground;
textBox.ForeColor = Text;
    textBox.BorderStyle = BorderStyle.FixedSingle;
    }

    private static void ApplyToComboBox(ComboBox comboBox)
    {
        comboBox.BackColor = InputBackground;
        comboBox.ForeColor = Text;
        comboBox.FlatStyle = FlatStyle.Flat;
    }

    private static void ApplyToNumericUpDown(NumericUpDown numeric)
    {
        numeric.BackColor = InputBackground;
        numeric.ForeColor = Text;
        numeric.BorderStyle = BorderStyle.FixedSingle;
    }

    private static void ApplyToProgressBar(ProgressBar progressBar)
    {
        progressBar.BackColor = ProgressBackground;
progressBar.ForeColor = ProgressForeground;
    }

    // Helper to create modern rounded button
    public static void MakeModernButton(Button button, bool isPrimary = false)
    {
    button.FlatStyle = FlatStyle.Flat;
   button.FlatAppearance.BorderSize = 0;
        button.Height = 36;
        button.Cursor = Cursors.Hand;
        
        if (isPrimary)
      {
    button.BackColor = Primary;
       button.ForeColor = Color.White;
      button.FlatAppearance.MouseOverBackColor = PrimaryHover;
  button.FlatAppearance.MouseDownBackColor = PrimaryHover;
      }
  else
   {
  button.BackColor = ButtonBackground;
    button.ForeColor = ButtonText;
            button.FlatAppearance.MouseOverBackColor = ButtonHover;
            button.FlatAppearance.MouseDownBackColor = Border;
        }
    }

    // Helper to create modern input
    public static void MakeModernInput(Control input)
    {
        input.BackColor = InputBackground;
        input.ForeColor = Text;
   
        if (input is TextBox textBox)
        {
            textBox.BorderStyle = BorderStyle.FixedSingle;
  }
        else if (input is NumericUpDown numeric)
        {
      numeric.BorderStyle = BorderStyle.FixedSingle;
    }
    }
}
