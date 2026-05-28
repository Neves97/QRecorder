using SilentTrayRecorder;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

class Program
{
    [STAThread]
    static void Main()
    {
        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);
        // passo zero, edite o arquivo .csproj e mude o OutputType para WinExe, isso é importante para não abrir a janela de console junto com o ícone

        using var recorder = new AudioManager();
        using var scheduler = new AudioScheduler(recorder);

        //criar menu
        ContextMenuStrip MainManu = new ContextMenuStrip();

        ToolStripMenuItem deviceMenu = new ToolStripMenuItem("Devices");

        List<AudioDevice> devices = recorder.ListarMicrofones();

        List<ToolStripMenuItem> listDevices = new List<ToolStripMenuItem>();
        foreach (var device in devices)
        {

            var deviceItem = new ToolStripMenuItem(device.Name, null, (sender, eventArgs) =>
            {
                AudioManager.deviceID = device.Id;

                foreach (ToolStripMenuItem item in deviceMenu.DropDownItems)
                {
                    item.Checked = false;
                }
                if (sender is ToolStripMenuItem clickedItem)
                {
                    clickedItem.Checked = true;
                }


            });
            deviceItem.Tag = device.Id;
            listDevices.Add(deviceItem);

        }

        foreach (var item in listDevices)
        {
            deviceMenu.DropDownItems.Add(item);
        }

        // Sintaxe: new ToolStripMenuItem("Texto que aparece", Ícone, Método_Que_Será_Executado)
        ToolStripMenuItem exitMenuItem = new ToolStripMenuItem("Exit", null, (sender, eventArgs) =>
            {
                Application.Exit();
            });

        ToolStripMenuItem recordMenuItem = new ToolStripMenuItem("Start", null, (sender, eventArgs) =>
        {
            var menuItem = sender as ToolStripMenuItem;
            if (menuItem == null) return;

            // Deixa a classe decidir o que fazer com base no estado interno dela
            if (!recorder.IsRecording)
            {
                recorder.Start();
                menuItem.Text = "Stop";
            }
            else
            {
                recorder.Stop();
                menuItem.Text = "Start";
            }
        });

        string startText = Environment.GetEnvironmentVariable("Q_DRIVER_START") ?? "07:00";
        string stopText = Environment.GetEnvironmentVariable("Q_DRIVER_STOP") ?? "09:00";
        ToolStripMenuItem schedulerMenuItem = new ToolStripMenuItem($"{startText}-{stopText}", null, (sender, eventArgs) =>
        {
            AudioScheduler.Enabled = !AudioScheduler.Enabled;
            if (sender is ToolStripMenuItem menuItem)
            {
                menuItem.Checked = AudioScheduler.Enabled;
            }
        });
        schedulerMenuItem.Checked = AudioScheduler.Enabled;

        MainManu.Opening += (sender, eventArgs) =>
        {
            recordMenuItem.Text = recorder.IsRecording ? "Stop" : "Start";
            schedulerMenuItem.Checked = AudioScheduler.Enabled;
        };

        //associar
        MainManu.Items.Add(recordMenuItem);
        MainManu.Items.Add(schedulerMenuItem);
        MainManu.Items.Add(new ToolStripSeparator());
        MainManu.Items.Add(exitMenuItem);
        MainManu.Items.Add(deviceMenu);
        MainManu.Items.Add(new ToolStripSeparator());
        MainManu.Items.Add(deviceMenu);
        MainManu.Items.Add(new ToolStripMenuItem { Text = "Q_DRIVER_START/Q_DRIVER_STOP", Enabled = false });




        //  criar icone e assosciar os menus
        NotifyIcon trayIcon = new NotifyIcon();

        trayIcon.Icon = SystemIcons.WinLogo;
        trayIcon.Text = "AudioDriver";
        trayIcon.Visible = true;
        trayIcon.ContextMenuStrip = MainManu;


        // 2. Dispara o balão
        trayIcon.ShowBalloonTip(200, "System", "running", ToolTipIcon.Info);


        // 3. OBRIGATÓRIO: Isola a thread e impede o programa de fechar
        Application.Run();

        // Garante que o ícone na bandeja seja limpo ao fechar
        trayIcon.Dispose();
    }
}