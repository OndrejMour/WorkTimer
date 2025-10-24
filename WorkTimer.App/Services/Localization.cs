using System;
using WorkTimer.App.Models;

namespace WorkTimer.App.Services;

public static class Localization
{
 public static string T(AppLanguage lang, string key)
 {
 return lang switch
 {
 AppLanguage.En => key switch
 {
 // App
 "AppName" => "Work Timer",
 "History" => "History",

 // Header
 "Start" => "Start",
 "Now" => "Now",
 "SetStart" => "Set start",
 "EndShift" => "End shift",
 "WorkedShift" => "Worked (shift):",
 "WorkedTasks" => "Worked (tasks):",
 "RemainToTarget" => "Remain (to target):",
 "ActiveTask" => "Active task:",
 "ShiftEnd" => "Shift end:",
 "NextEscape" => "Next leave window:",
 "From" => "from",
 "PausedState" => "(pause)",
 "Unnamed" => "(unnamed)",
 "ShiftState" => "Shift:",
 "ShiftRunning" => "running",
 "ShiftPaused" => "paused",
 "ShiftEnded" => "ended",
 "StartBreak" => "Start break",
 "EndBreak" => "End break",
 "OnBreak" => "on break",

 // Tasks
 "TasksCaption" => "Tasks",
 "StartTask" => "Start task",
 "Pause" => "Pause",
 "StopTask" => "Stop task",
 "Export" => "Export",
 "Settings" => "Settings",
 "Hide" => "Hide",
 "Rename" => "Rename",
 "DeleteTask" => "Delete task",
 "DeleteTaskTitle" => "Delete task",
 "DeleteTaskConfirm" => "Do you really want to delete task '{0}' including all its records? This action cannot be undone.",
 "Paused" => "Paused",
 "Active" => "Active",
 "Finished" => "Finished",
 "Time" => "Time:",
 "NoTasksYet" => "No tasks yet.",
 "RenameTaskPrompt" => "Rename task '{0}' to:",

 // Input
 "InputTitle" => "Input",
 "TaskNamePrompt" => "Task name:",
 "Ok" => "OK",
 "Cancel" => "Cancel",

 // Settings
 "SettingsTitle" => "Settings",
 "TargetShiftLength" => "Target shift length:",
 "HoursShort" => "h",
 "MinutesShort" => "m",
 "BubblesHalf" => "Balloon: half of shift",
 "BubblesEnd" => "Balloon: end of shift",
 "BubblesEscape" => "Balloon: leave window",
 // Tooltips
 "TipBubblesHalf" => "Shows a tray balloon when you reach half of your target shift (based on worked task time).",
 "TipBubblesEnd" => "Shows a tray balloon when you reach the target shift length (based on worked task time).",
 "TipBubblesEscape" => "Shows a tray balloon at the end of each15?minute leave window (e.g.,8:00–8:15,8:15–8:30) computed from your arrival.",
 "Language" => "Language:",
 "LanguageCs" => "Èeština",
 "LanguageEn" => "English",

 // Set start dialog
 "SetStartTitle" => "Set start",
 "ArrivalTime" => "Arrival time:",

 // History
 "HistoryTitle" => "Shift history",
 "Close" => "Close",
 "ColumnStart" => "Start",
 "ColumnEnd" => "End",
 "ColumnDuration" => "Duration",
 "ColumnBreaks" => "Breaks",
 "ColumnTasks" => "Tasks",

 // Balloons
 "HalfReached" => "Half of the shift reached.",
 "ShiftEndReached" => "Shift end.",
 _ => key
 },
 _ => key switch
 {
 // App
 "AppName" => "Pracovní èasovaè",
 "History" => "Historie",

 // Header
 "Start" => "Pøíchod",
 "Now" => "Nyní",
 "SetStart" => "Nastavit zaèátek",
 "EndShift" => "Ukonèit smìnu",
 "WorkedShift" => "Odpracováno (smìna):",
 "WorkedTasks" => "Odpracováno (úkoly):",
 "RemainToTarget" => "Zbıvá (do cíle):",
 "ActiveTask" => "Aktivní úkol:",
 "ShiftEnd" => "Konec smìny:",
 "NextEscape" => "Nejbliší èas odchodu:",
 "From" => "od",
 "PausedState" => "(pauza)",
 "Unnamed" => "(bez názvu)",
 "ShiftState" => "Smìna:",
 "ShiftRunning" => "bìí",
 "ShiftPaused" => "pauza",
 "ShiftEnded" => "ukonèená",
 "StartBreak" => "Pøestávka",
 "EndBreak" => "Konec pøestávky",
 "OnBreak" => "pøestávka",

 // Tasks
 "TasksCaption" => "Úkoly",
 "StartTask" => "Zaèít úkol",
 "Pause" => "Pauza",
 "StopTask" => "Ukonèit úkol",
 "Export" => "Exportovat",
 "Settings" => "Nastavení",
 "Hide" => "Skrıt",
 "Rename" => "Pøejmenovat",
 "DeleteTask" => "Smazat úkol",
 "DeleteTaskTitle" => "Smazat úkol",
 "DeleteTaskConfirm" => "Opravdu si pøejete smazat úkol '{0}' vèetnì všech jeho záznamù? Tato akce je nevratná.",
 "Paused" => "Pozastavenı",
 "Active" => "Aktivní",
 "Finished" => "Ukonèenı",
 "Time" => "Èas:",
 "NoTasksYet" => "Zatím ádné úkoly.",
 "RenameTaskPrompt" => "Pøejmenovat úkol '{0}' na:",

 // Input
 "InputTitle" => "Vstup",
 "TaskNamePrompt" => "Název úkolu:",
 "Ok" => "OK",
 "Cancel" => "Zrušit",

 // Settings
 "SettingsTitle" => "Nastavení",
 "TargetShiftLength" => "Cílová délka smìny:",
 "HoursShort" => "h",
 "MinutesShort" => "m",
 "BubblesHalf" => "Bublina: polovina smìny",
 "BubblesEnd" => "Bublina: konec smìny",
 "BubblesEscape" => "Bublina: únikové okno",
 // Tooltips
 "TipBubblesHalf" => "Zobrazí bublinu v tray, kdy dosáhnete poloviny cílové smìny (poèítáno podle odpracovanıch úkolù).",
 "TipBubblesEnd" => "Zobrazí bublinu v tray v okamiku dosaení cílové délky smìny (poèítáno podle odpracovanıch úkolù).",
 "TipBubblesEscape" => "Zobrazí bublinu v tray na konci kadého15min okna pro odchod (napø.8:00–8:15,8:15–8:30) vypoèteného od pøíchodu.",
 "Language" => "Jazyk:",
 "LanguageCs" => "Èeština",
 "LanguageEn" => "English",

 // Set start dialog
 "SetStartTitle" => "Nastavit pøíchod",
 "ArrivalTime" => "Èas pøíchodu:",

 // History
 "HistoryTitle" => "Historie smìn",
 "Close" => "Zavøít",
 "ColumnStart" => "Start",
 "ColumnEnd" => "Konec",
 "ColumnDuration" => "Délka",
 "ColumnBreaks" => "Pøestávky",
 "ColumnTasks" => "Úkoly",

 // Balloons
 "HalfReached" => "Polovina smìny dosaena.",
 "ShiftEndReached" => "Konec smìny.",
 _ => key
 }
 };
 }
}
