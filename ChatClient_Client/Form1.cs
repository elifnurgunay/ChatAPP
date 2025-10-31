using System;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ChatClient
{
    public partial class Form1 : Form
    {
        // IP ve Port'u burada SABİT YAPIYORUZ (Sunucu koduyla aynı olmalı)
        private const string SERVER_IP = "127.0.0.1";
        private const int SERVER_PORT = 8888;

        private TcpClient client;
        private NetworkStream stream;

        public Form1()
        {
            InitializeComponent();
            // Form yüklenirken gönderim butonunu devre dışı bırakalım (Sadece bağlantı sonrası aktif olacak)
            btnSend.Enabled = false;
            this.btnSend.Click += new System.EventHandler(this.btnSend_Click);
        }

        // Bağlan Butonu Olayı (btnConnect_Click)
        private void btnConnect_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(txtUsername.Text))
            {
                MessageBox.Show("Lütfen bir kullanıcı adı girin.", "Uyarı");
                return;
            }

            try
            {
                client = new TcpClient();
                // Sabit IP ve Port'a bağlanıyoruz
                client.Connect(SERVER_IP, SERVER_PORT);

                stream = client.GetStream();
                btnConnect.Enabled = false;
                btnSend.Enabled = true; // Gönder butonunu aktif et
                txtUsername.ReadOnly = true;

                // Mesaj dinlemeyi başlat
                Task.Run(() => ReceiveMessages());

                AppendToChat("[SİSTEM] Sunucuya başarıyla bağlandı. Mesajlaşmaya başlayabilirsiniz.");
                SendInitialMessage($"[SİSTEM] {txtUsername.Text} sohbete katıldı.");
            }
            catch (Exception ex)
            {
                // Bağlanılamazsa hata mesajı verilir
                MessageBox.Show($"[HATA] Sunucuya bağlanılamadı. Kod: {ex.Message}", "Bağlantı Hatası");
            }
        }

        // Gönder Butonu Olayı (btnSend_Click)
        private void btnSend_Click(object sender, EventArgs e)
        {
            if (client == null || !client.Connected || string.IsNullOrEmpty(txtMessage.Text))
            {
                txtMessage.Clear();
                return;
            }

            try
            {
                string rawMessage = txtMessage.Text;
                string messageToSend = $"[{txtUsername.Text}] {rawMessage}";

                byte[] buffer = Encoding.UTF8.GetBytes(messageToSend);
                stream.Write(buffer, 0, buffer.Length);

                // Kendi gönderdiğimiz mesajı hemen ekranda göster
                AppendToChat($"[SİZ] {rawMessage}");
                txtMessage.Clear();
            }
            catch (Exception ex)
            {
                AppendToChat($"[HATA] Mesaj gönderilemedi: {ex.Message}");
                client.Close();
            }
        }

        // Sunucuya bilgi mesajı gönderme (Katılım/Ayrılma)
        private void SendInitialMessage(string message)
        {
            try
            {
                if (client != null && client.Connected)
                {
                    byte[] buffer = Encoding.UTF8.GetBytes(message);
                    stream.Write(buffer, 0, buffer.Length);
                }
            }
            catch (Exception) { /* Hata görmezden gelindi */ }
        }

        // Sunucudan sürekli mesaj dinleme metodu
        private async Task ReceiveMessages()
        {
            byte[] buffer = new byte[1024];
            int bytesRead;

            try
            {
                while (client.Connected && (bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length)) > 0)
                {
                    string message = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                    AppendToChat(message); // Gelen mesajı ekrana bas
                }
            }
            catch (Exception)
            {
                AppendToChat("[SİSTEM] Sunucu ile bağlantı koptu.");
            }
            finally
            {
                if (client != null)
                {
                    client.Close();
                    // UI güncellemeleri için Invoke kullan (Thread-safe)
                    this.Invoke((MethodInvoker)delegate
                    {
                        btnConnect.Enabled = true;
                        btnSend.Enabled = false;
                        txtUsername.ReadOnly = false;
                    });
                }
            }
        }

        // RichTextBox'a mesaj eklemek için yardımcı metod (Thread-safe)
        private void AppendToChat(string message)
        {
            if (this.rtbChat.InvokeRequired)
            {
                this.rtbChat.Invoke(new Action<string>(AppendToChat), message);
                return;
            }
            this.rtbChat.AppendText(message + Environment.NewLine);
            this.rtbChat.ScrollToCaret();
        }

        // Form kapanırken bağlantıyı kapat
        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (client != null && client.Connected)
            {
                SendInitialMessage($"[SİSTEM] {txtUsername.Text} sohbetten ayrıldı.");
            }

            stream?.Close();
            client?.Close();
        }
    }
}