<img width="1919" height="1025" alt="Ekran görüntüsü 2025-10-14 190813" src="https://github.com/user-attachments/assets/4693d7fe-9a86-4f0f-9257-c628f9b01b40" />

Bu README dosyası, projenin amacını, özelliklerini, kurulumunu ve bileşenlerini açıklar.

C# TCP Çok Kullanıcılı Sohbet Uygulaması
Bu proje, C# ve Windows Forms (.NET Framework) kullanılarak geliştirilmiş, iki yönlü haberleşme (Sunucu-İstemci) sağlayabilen temel bir sohbet uygulamasıdır. Amacı, ağ programlama (Socket programlama) temellerini ve TcpListener / TcpClient sınıflarının kullanımını göstermektir.

🚀 Özellikler
Çoklu Bağlantı Desteği: Sunucu, aynı anda birden fazla istemci bağlantısını kabul edebilir.

Asenkron Haberleşme: async/await yapısı kullanılarak ağ işlemleri uygulamanın ana arayüzünü (UI) kilitlemeden gerçekleştirilir.

Gerçek Zamanlı İletişim: Mesajlar, bağlı olan tüm istemcilere anında yayınlanır (broadcast).

Güvenli UI Güncelleme: Invoke metodu ile farklı thread'lerdeki ağ verilerinin arayüze güvenli bir şekilde yazılması sağlanır.

Hata Yönetimi: Bağlantı hataları ve kopmaları durumunda kullanıcıya bildirim sağlar.

⚙️ Proje Yapısı
Çözüm (Solution) iki temel ve bağımsız projeden oluşur:

ChatAPP: Sunucu tarafı. Belirtilen portu dinler, istemcileri kabul eder ve mesajları dağıtır.

ChatClient: İstemci tarafı. Kullanıcı adı ve IP/Port bilgisiyle sunucuya bağlanır ve mesaj gönderip alır.

 Çalıştırma
ChatAPP ve ChatClient dosyalarını birlikte çalıştırın.

Sunucu penceresinde Başlat butonuna tıklayın. (Sunucu 5000 portunda başlatıldı... mesajını görmelisiniz.)

İstemci penceresinde Bağlan butonuna tıklayın. (Sunucuya başarıyla bağlandı. mesajını görmelisiniz.)

Bağlandıktan sonra mesajınızı yazmaya başlayabilirsiniz.

⚠️ Dikkat Edilmesi Gerekenler
Yerel Test: Uygulama yerel makinede test edildiği için IP adresi varsayılan olarak 127.0.0.1 (localhost) olarak ayarlanmıştır. Başka bir bilgisayardan bağlantı kurmak isterseniz, bu adresi sunucunun gerçek ağ IP adresiyle değiştirmeniz gerekir.

Port Çakışması: Eğer 5000 portu başka bir uygulama tarafından kullanılıyorsa, hem sunucu hem de istemcideki port numarasını değiştirmeniz gerekir.

Güvenlik: Bu temel bir TCP uygulamasıdır ve veri şifrelemesi içermez. Gerçek dünya uygulamaları için SSL/TLS gibi güvenlik katmanları eklenmelidir.
