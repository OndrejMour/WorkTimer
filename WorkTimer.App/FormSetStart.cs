using System;
using System.Windows.Forms;
using WorkTimer.App.Services;
using WorkTimer.App.Models;

namespace WorkTimer.App;

public class FormSetStart : Form
{
 private readonly DateTimePicker _tp = new() { Format = DateTimePickerFormat.Custom, CustomFormat = "HH:mm", ShowUpDown = true, Width =80 };
 private readonly Button _ok = new() { DialogResult = DialogResult.OK };
 private readonly Button _cancel = new() { DialogResult = DialogResult.Cancel };

 public TimeSpan SelectedTime => _tp.Value.TimeOfDay;

 public FormSetStart(TimeSpan initialTime)
 {
 // Title localized when showing (default to Czech to avoid mojibake)
 _tp.Value = DateTime.Today.Add(initialTime);
 var body = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.TopDown, Padding = new Padding(8) };
 body.Controls.Add(new Label { AutoSize = true, Name = "lblArrival" });
 body.Controls.Add(_tp);
 var bottom = new FlowLayoutPanel { Dock = DockStyle.Bottom, Height =40 };
 bottom.Controls.Add(_ok);
 bottom.Controls.Add(_cancel);
 Controls.Add(body);
 Controls.Add(bottom);
 AcceptButton = _ok; CancelButton = _cancel;
 }

 public void ApplyLocalization(AppLanguage L)
 {
 Text = Localization.T(L, "SetStartTitle");
 if (Controls[0] is FlowLayoutPanel body)
 {
 foreach (Control c in body.Controls)
 {
 if (c.Name == "lblArrival") c.Text = Localization.T(L, "ArrivalTime");
 }
 }
 _ok.Text = Localization.T(L, "Ok");
 _cancel.Text = Localization.T(L, "Cancel");
 }
}
