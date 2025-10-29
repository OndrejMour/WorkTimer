using System;
using System.Windows.Forms;
using WorkTimer.App.Models;
using WorkTimer.App.Services;

namespace WorkTimer.App;

public class FormSettings : Form
{
 private readonly NumericUpDown _hours = new() { Minimum =0, Maximum =24, DecimalPlaces =0, Width =60 };
 private readonly NumericUpDown _minutes = new() { Minimum =0, Maximum =59, DecimalPlaces =0, Width =60 };
 private readonly CheckBox _cbHalf = new() { AutoSize = false, Dock = DockStyle.Fill };
 private readonly CheckBox _cbEnd = new() { AutoSize = false, Dock = DockStyle.Fill };
 private readonly CheckBox _cbEscape = new() { AutoSize = false, Dock = DockStyle.Fill };
 private readonly CheckBox _cbMinimizeToTray = new() { AutoSize = false, Dock = DockStyle.Fill };
 private readonly CheckBox _cbMinimizeToTrayOnClose = new() { AutoSize = false, Dock = DockStyle.Fill };
 private readonly ComboBox _cbLanguage = new() { DropDownStyle = ComboBoxStyle.DropDownList, Width =160 };
 private readonly Button _ok = new() { DialogResult = DialogResult.OK };
 private readonly Button _cancel = new() { DialogResult = DialogResult.Cancel };
 private readonly ToolTip _tips = new();

 public AppSettings Settings { get; private set; }

 public FormSettings(AppSettings settings, Shift shift)
 {
 // Window look & size so content does not wrap
 FormBorderStyle = FormBorderStyle.FixedDialog;
 MaximizeBox = false; MinimizeBox = false;
 StartPosition = FormStartPosition.CenterParent;
 MinimumSize = new System.Drawing.Size(520,300);

 Settings = new AppSettings
 {
 TargetShift = settings.TargetShift,
 NotifyHalf = settings.NotifyHalf,
 NotifyEnd = settings.NotifyEnd,
 NotifyEscapeWindow = settings.NotifyEscapeWindow,
 Language = settings.Language,
 MinimizeToTray = settings.MinimizeToTray,
 MinimizeToTrayOnClose = settings.MinimizeToTrayOnClose
 };

 // Initial values
 _hours.Value = (int)Settings.TargetShift.TotalHours;
 _minutes.Value = Settings.TargetShift.Minutes;
 _cbHalf.Checked = Settings.NotifyHalf;
 _cbEnd.Checked = Settings.NotifyEnd;
 _cbEscape.Checked = Settings.NotifyEscapeWindow;
 _cbMinimizeToTray.Checked = Settings.MinimizeToTray;
 _cbMinimizeToTrayOnClose.Checked = Settings.MinimizeToTrayOnClose;
 _cbLanguage.Items.AddRange(new object[] { Localization.T(Settings.Language, "LanguageCs"), Localization.T(Settings.Language, "LanguageEn") });
 _cbLanguage.SelectedIndex = Settings.Language == AppLanguage.Cs ?0 :1;

 // Layout
 var pane = new TableLayoutPanel { Dock = DockStyle.Fill, Padding = new Padding(10), ColumnCount =2, AutoSize = true, AutoSizeMode = AutoSizeMode.GrowAndShrink };
 pane.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
 pane.ColumnStyles.Add(new ColumnStyle(SizeType.Percent,100));

 var timeRow = new FlowLayoutPanel { FlowDirection = FlowDirection.LeftToRight, AutoSize = true, WrapContents = false };
 timeRow.Controls.Add(new Label { AutoSize = true, Padding = new Padding(0,6,6,0), Name = "lblTarget" });
 timeRow.Controls.Add(_hours);
 timeRow.Controls.Add(new Label { AutoSize = true, Padding = new Padding(6,6,6,0), Name = "lblH" });
 timeRow.Controls.Add(_minutes);
 timeRow.Controls.Add(new Label { AutoSize = true, Padding = new Padding(6,6,6,0), Name = "lblM" });
 pane.Controls.Add(timeRow,0,0);
 pane.SetColumnSpan(timeRow,2);

 // notifications
 _cbHalf.Margin = new Padding(3,6,3,0);
 _cbEnd.Margin = new Padding(3,3,3,0);
 _cbEscape.Margin = new Padding(3,3,3,0);
 _cbMinimizeToTray.Margin = new Padding(3,3,3,0);
 _cbMinimizeToTrayOnClose.Margin = new Padding(3,3,3,0);
 pane.Controls.Add(_cbHalf,0,1);
 pane.SetColumnSpan(_cbHalf,2);
 pane.Controls.Add(_cbEnd,0,2);
 pane.SetColumnSpan(_cbEnd,2);
 pane.Controls.Add(_cbEscape,0,3);
 pane.SetColumnSpan(_cbEscape,2);
 pane.Controls.Add(_cbMinimizeToTray,0,4);
 pane.SetColumnSpan(_cbMinimizeToTray,2);
 pane.Controls.Add(_cbMinimizeToTrayOnClose,0,5);
 pane.SetColumnSpan(_cbMinimizeToTrayOnClose,2);

 // language
 pane.Controls.Add(new Label { AutoSize = true, Padding = new Padding(0,8,6,0), Name = "lblLang" },0,6);
 pane.Controls.Add(_cbLanguage,1,6);

 // buttons
 var buttons = new FlowLayoutPanel { Dock = DockStyle.Bottom, Height =40 };
 buttons.Controls.Add(_ok);
 buttons.Controls.Add(_cancel);
 Controls.Add(pane);
 Controls.Add(buttons);

 AcceptButton = _ok; CancelButton = _cancel;

 ApplyLocalization();

 // Live preview of selected language inside this dialog
 _cbLanguage.SelectedIndexChanged += (_, __) =>
 {
 Settings.Language = _cbLanguage.SelectedIndex ==1 ? AppLanguage.En : AppLanguage.Cs;
 ApplyLocalization();
 };

 _ok.Click += (_, __) => SaveBack();
 }

 private void ApplyLocalization()
 {
 var L = Settings.Language;
 Text = Localization.T(L, "SettingsTitle");
 _cbHalf.Text = Localization.T(L, "BubblesHalf");
 _cbEnd.Text = Localization.T(L, "BubblesEnd");
 _cbEscape.Text = Localization.T(L, "BubblesEscape");
 _cbMinimizeToTray.Text = Localization.T(L, "MinimizeToTray");
 _cbMinimizeToTrayOnClose.Text = Localization.T(L, "MinimizeToTrayOnClose");
 _ok.Text = Localization.T(L, "Ok");
 _cancel.Text = Localization.T(L, "Cancel");
 if (Controls[0] is TableLayoutPanel pane)
 {
 if (pane.Controls[0] is FlowLayoutPanel timeRow)
 {
 foreach (Control c in timeRow.Controls)
 {
 if (c.Name == "lblTarget") c.Text = Localization.T(L, "TargetShiftLength");
 if (c.Name == "lblH") c.Text = Localization.T(L, "HoursShort");
 if (c.Name == "lblM") c.Text = Localization.T(L, "MinutesShort");
 }
 }
 foreach (Control c in pane.Controls)
 {
 if (c.Name == "lblLang") c.Text = Localization.T(L, "Language");
 }
 }
 // Refresh language list text
 var cs = Localization.T(L, "LanguageCs");
 var en = Localization.T(L, "LanguageEn");
 _cbLanguage.Items.Clear();
 _cbLanguage.Items.AddRange(new object[] { cs, en });
 _cbLanguage.SelectedIndex = Settings.Language == AppLanguage.Cs ?0 :1;

 // Tooltips clarifying notification options
 _tips.SetToolTip(_cbHalf, Localization.T(L, "TipBubblesHalf"));
 _tips.SetToolTip(_cbEnd, Localization.T(L, "TipBubblesEnd"));
 _tips.SetToolTip(_cbEscape, Localization.T(L, "TipBubblesEscape"));
 _tips.SetToolTip(_cbMinimizeToTray, Localization.T(L, "TipMinimizeToTray"));
 _tips.SetToolTip(_cbMinimizeToTrayOnClose, Localization.T(L, "TipMinimizeToTrayOnClose"));
 }

 private void SaveBack()
 {
 var ts = System.TimeSpan.FromHours((double)_hours.Value) + System.TimeSpan.FromMinutes((double)_minutes.Value);
 Settings.TargetShift = ts;
 Settings.NotifyHalf = _cbHalf.Checked;
 Settings.NotifyEnd = _cbEnd.Checked;
 Settings.NotifyEscapeWindow = _cbEscape.Checked;
 Settings.MinimizeToTray = _cbMinimizeToTray.Checked;
 Settings.MinimizeToTrayOnClose = _cbMinimizeToTrayOnClose.Checked;
 Settings.Language = _cbLanguage.SelectedIndex ==1 ? AppLanguage.En : AppLanguage.Cs;
 }
}
