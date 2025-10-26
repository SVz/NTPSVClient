namespace STPSVClient;

using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.IO;
using System.Text.Json;
using System.Text;
using System.Windows.Forms;

public partial class Form1 : Form
{
    public Form1()
    {
        InitializeComponent();
    }

private static bool IsAdministrator()
{
    using var identity = WindowsIdentity.GetCurrent();
    var principal = new WindowsPrincipal(identity);
    return principal.IsInRole(WindowsBuiltInRole.Administrator);
}

private static void RestartElevated()
{
    var psi = new ProcessStartInfo
    {
        FileName = Application.ExecutablePath,
        UseShellExecute = true,
        Verb = "runas"
    };
    try
    {
        Process.Start(psi);
    }
    catch
    {
        // L'utilisateur a annulé l'élévation — ne rien faire
    }
    Application.Exit();
}

private async void button1_Click(object sender, EventArgs e)
    {
        try
        {
            DateTime networkTime = await NtpClient.GetNetworkTimeAsync(textBox1.Text);
            richTextBox1.AppendText($"Get time successfully on {textBox1.Text}!\nNew time: {networkTime}\n");
            
            // Set the system time if ckeckbox is checked
            if (checkBox1.Checked)
            {
                if (NtpClient.SetSystemDateTime(networkTime))
                {
                    richTextBox1.AppendText($"System time synchronized successfully!\nNew time: {networkTime}\n");
                }
                else
                {
                    MessageBox.Show(
                        "Failed to set system time. Make sure the application is running with administrator privileges.",
                        "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error getting network time: {ex.Message}", 
                "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void Form1_Load(object sender, EventArgs e)
    {
        if (!IsAdministrator())
        {
            var r = MessageBox.Show("L'application nécessite des droits administrateur pour régler l'heure système. Redémarrer en administrateur ?", "Élévation requise", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (r == DialogResult.Yes) RestartElevated();
            // sinon continuer sans modifier l'heure
        }

        // open config file and read default NTP server
        var configuredServer = LoadDefaultNtpServerFromConfig();
        if (!string.IsNullOrWhiteSpace(configuredServer))
        {
            textBox1.Text = configuredServer;
            richTextBox1.AppendText($"NTP server loaded from configuration : {configuredServer}\n");
        }
        else
        {
            textBox1.Text = NtpClient.DefaultNtpServer;
            richTextBox1.AppendText($"No NTP server in configuration\nusing default value : {NtpClient.DefaultNtpServer}\n");
        }
    }

    /// <summary>
    /// Cherche un fichier `appsettings.json` à côté de l'exécutable et lit `Ntp:Server` si présent.
    /// Format attendu :
    /// {
    ///   "Ntp": {
    ///     "Server": "time.windows.com"
    ///   }
    /// }
    /// Si le fichier manque ou la clé est absente, retourne null.
    /// </summary>
    private static string? LoadDefaultNtpServerFromConfig()
    {
        try
        {
            var exeDir = AppDomain.CurrentDomain.BaseDirectory;
            var configPath = Path.Combine(exeDir, "appsettings.json");

            if (!File.Exists(configPath))
                return null;

            var json = File.ReadAllText(configPath, Encoding.UTF8);
            using var doc = JsonDocument.Parse(json);
            if (doc.RootElement.TryGetProperty("Ntp", out var ntpElement) &&
                ntpElement.ValueKind == JsonValueKind.Object &&
                ntpElement.TryGetProperty("Server", out var serverElement) &&
                serverElement.ValueKind == JsonValueKind.String)
            {
                var server = serverElement.GetString();
                if (!string.IsNullOrWhiteSpace(server))
                    return server!.Trim();
            }

            return null;
        }
        catch (JsonException)
        {
            // fichier malformé
            MessageBox.Show("Le fichier de configuration 'appsettings.json' est invalide (JSON malformé).", "Configuration invalide", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return null;
        }
        catch (Exception ex)
        {
            // erreurs E/S ou autres
            MessageBox.Show($"Impossible de lire le fichier de configuration : {ex.Message}", "Erreur de configuration", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return null;
        }
    }
}

public class NtpClient
{
    public const string DefaultNtpServer = "time.windows.com";
    private const int NtpPort = 123;
    private const int NtpDataLength = 48;
    // Import Windows API function to set system time
    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern bool SetSystemTime(ref SYSTEMTIME st);

    [StructLayout(LayoutKind.Sequential)]
    private struct SYSTEMTIME
    {
        public short wYear;
        public short wMonth;
        public short wDayOfWeek;
        public short wDay;
        public short wHour;
        public short wMinute;
        public short wSecond;
        public short wMilliseconds;
    }
    public static async Task<DateTime> GetNetworkTimeAsync(string ntpServer)
    {
        var ntpData = new byte[NtpDataLength];

        // LI = 0 (no warning), VN = 3 (IPv4 only), Mode = 3 (Client Mode)
        ntpData[0] = 0x1B;

        var addresses = await Dns.GetHostEntryAsync(ntpServer);
        var ipEndPoint = new IPEndPoint(addresses.AddressList[0], NtpPort);

        using var socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        socket.ReceiveTimeout = 3000; // 3 seconds timeout

        await socket.ConnectAsync(ipEndPoint);
        await socket.SendAsync(ntpData, SocketFlags.None);
        await socket.ReceiveAsync(ntpData, SocketFlags.None);

        // Offset to get to the "Transmit Timestamp" field (time at which the reply 
        // departed the server for the client, in 64-bit timestamp format)
        const byte serverReplyTime = 40;

        // Get the seconds part
        ulong intPart = BitConverter.ToUInt32(ntpData, serverReplyTime);

        // Get the seconds fraction
        ulong fractPart = BitConverter.ToUInt32(ntpData, serverReplyTime + 4);

        // Convert from big-endian to little-endian
        intPart = SwapEndianness(intPart);
        fractPart = SwapEndianness(fractPart);

        var milliseconds = (intPart * 1000) + ((fractPart * 1000) / 0x100000000L);

        // NTP timestamp is seconds since 1/1/1900
        var networkDateTime = new DateTime(1900, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            .AddMilliseconds((long)milliseconds);

        return networkDateTime.ToLocalTime();
    }
    public static bool SetSystemDateTime(DateTime dateTime)
    {
        // Convert to UTC for system time setting
        DateTime utcTime = dateTime.ToUniversalTime();

        SYSTEMTIME st = new SYSTEMTIME
        {
            wYear = (short)utcTime.Year,
            wMonth = (short)utcTime.Month,
            wDayOfWeek = (short)utcTime.DayOfWeek,
            wDay = (short)utcTime.Day,
            wHour = (short)utcTime.Hour,
            wMinute = (short)utcTime.Minute,
            wSecond = (short)utcTime.Second,
            wMilliseconds = (short)utcTime.Millisecond
        };

        return SetSystemTime(ref st);
    }

    private static uint SwapEndianness(ulong x)
    {
        return (uint)(((x & 0x000000ff) << 24) +
                      ((x & 0x0000ff00) << 8) +
                      ((x & 0x00ff0000) >> 8) +
                      ((x & 0xff000000) >> 24));
    }
}