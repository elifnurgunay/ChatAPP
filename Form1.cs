using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ChatAPP
{
    public partial class Form1 : Form
    {
        // Bağlı olan tüm istemcileri tutacağımız liste
        private List<TcpClient> clients = new List<TcpClient>();
        private TcpListener listener;

        public Form1()
        {
            InitializeComponent();
        }

        // Sunucuyu Başlat butonuna çift tıkladığınızda bu metot çalışır.
        private void btnStart_Click(object sender, EventArgs e)
        {
            try
            {
                // Sunucuyu 127.0.0.1 (localhost) IP'sinde ve 8888 portunda başlat
                listener = new TcpListener(IPAddress.Parse("127.0.0.1"), 8888);
                listener.Start();

                Log("Sunucu başlatıldı. İstemciler bekleniyor...");
                btnStart.Enabled = false; // Sunucu başladıktan sonra butonu devre dışı bırak.

                // UI'ın donmaması için yeni bağlantıları ayrı bir Task (thread) içinde bekle
                Task.Run(() => ListenForClients());
            }
            catch (Exception ex)
            {
                Log($"Hata: Sunucu başlatılamadı: {ex.Message}");
            }
        }

        private async Task ListenForClients()
        {
            try
            {
                while (true)
                {
                    // Yeni bir istemci bağlantısını kabul et
                    TcpClient client = await listener.AcceptTcpClientAsync();

                    // clients listesi kilitli iken ekleme yapıyoruz
                    lock (clients)
                    {
                        clients.Add(client);
                    }

                    string clientIP = ((IPEndPoint)client.Client.RemoteEndPoint).Address.ToString();
                    Log($"Yeni bir istemci bağlandı: {clientIP}");

                    // Her istemci için mesajları dinleyecek ayrı bir Task başlat
                    Task.Run(() => HandleClient(client));
                }
            }
            catch (SocketException ex) when (ex.ErrorCode == 10004)
            {
                // listener.Stop() çağrıldığında oluşan yaygın bir hata. Göz ardı edilebilir.
                Log("Sunucu dinlemesi durduruldu.");
            }
            catch (Exception ex)
            {
                Log($"Genel Sunucu Hatası: {ex.Message}");
            }
        }

        private async Task HandleClient(TcpClient client)
        {
            NetworkStream stream = null;
            string clientID = ((IPEndPoint)client.Client.RemoteEndPoint).Address.ToString();

            try
            {
                stream = client.GetStream();
                byte[] buffer = new byte[1024];
                int bytesRead;

                while ((bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length)) > 0)
                {
                    string message = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                    Log($"Alınan mesaj ({clientID}): {message}");

                    // Gelen mesajı diğer tüm istemcilere yayınla
                    BroadcastMessage(message, client);
                }
            }
            catch (Exception)
            {
                // Hata veya bağlantı kopması durumunda
            }
            finally
            {
                Log($"İstemci bağlantısı koptu veya kapatıldı: {clientID}");

                // clients listesi kilitli iken kaldırıyoruz
                lock (clients)
                {
                    clients.Remove(client);
                }
                client.Close();
                // Opsiyonel: Diğer istemcilere ayrılma mesajı yayınlayabilirsiniz.
            }
        }

        private void BroadcastMessage(string message, TcpClient excludeClient)
        {
            byte[] buffer = Encoding.UTF8.GetBytes(message);

            // Kilitleme (Lock) işlemi, client listesi üzerinde işlem yapılırken (ekleme/çıkarma) hata oluşmasını engeller.
            lock (clients)
            {
                for (int i = clients.Count - 1; i >= 0; i--)
                {
                    TcpClient client = clients[i];
                    // Kendisi hariç ve bağlı olan istemcilere gönder
                    if (client != excludeClient && client.Connected)
                    {
                        try
                        {
                            NetworkStream stream = client.GetStream();
                            stream.Write(buffer, 0, buffer.Length);
                        }
                        catch (Exception)
                        {
                            // Mesaj gönderilemeyen istemciyi listeden çıkar
                            Log($"Bir istemciye mesaj gönderilemedi. Bağlantı kesiliyor.");
                            clients.RemoveAt(i);
                            client.Close();
                        }
                    }
                }
            }
        }

        // RichTextBox'a log yazmak için yardımcı metod (Thread-safe)
        private void Log(string message)
        {
            // Eğer farklı bir thread'den çağrılıyorsa (Task.Run metotları gibi)
            if (this.rtbLog.InvokeRequired)
            {
                // UI thread'ine geçerek metodu tekrar çağır
                this.rtbLog.Invoke(new Action<string>(Log), message);
                return;
            }
            // UI thread'indeysen metni ekle
            this.rtbLog.AppendText($"{DateTime.Now:HH:mm:ss} - {message}\n");
            // Otomatik olarak aşağı kaydır
            this.rtbLog.ScrollToCaret();
        }

        // Form kapanırken sunucuyu durdur
        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            listener?.Stop();
        }
    }
}
