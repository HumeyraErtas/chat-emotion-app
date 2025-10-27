Chat Emotion Analyzer – Full Stack AI Chat Uygulaması
👤 Geliştirici

Hümeyra Ertaş
Manisa Celal Bayar Üniversitesi – Yazılım Mühendisliği
FullStack + AI Stajyer Projesi

📌 Proje Özeti

Bu proje, kullanıcıların yazılı sohbet edebildiği ve mesajların AI tarafından anlık duygu analizinin gerçekleştirildiği tam uçtan uca bir FullStack uygulamasıdır.

✅ Web – React
✅ Backend – .NET Core + SQLite
✅ AI – Python Sentiment Analysis (HuggingFace Spaces)
✅ Deploy – Render + HuggingFace Spaces

✅ Kullanılan Teknolojiler
Katman	Teknoloji
Frontend (Web)	React + Fetch API
Backend	.NET Core Web API, Entity Framework Core, SQLite
AI Servisi	Python, Transformers, Gradio API
Hosting	Render (.NET API), HuggingFace (AI Model)
Geliştirme	Visual Studio Code, Postman
🚀 Sistem Mimarisi
React Web Chat  →  .NET Core API  →  Hugging Face AI Model
                      ↓
                   SQLite DB

🌍 Canlı Demo Linkleri
Bileşen	URL
Web Chat App (React)	(Eklenebilir - Vercel Deploy opsiyonel)
Backend API (Render)	https://chat-emotion-app-2.onrender.com

AI Model HuggingFace	https://huggingface.co/spaces/humeyraertas/chat-sentiment-analyzer
🧪 Test Endpoints (Postman İçin)
✅ Kullanıcı Kaydı
POST /api/chat/register
Content-Type: application/json
{
  "nickname": "Hume"
}

✅ Mesaj Gönder + Duygu Analizi
POST /api/chat/message
Content-Type: application/json
{
  "userId": 1,
  "text": "Bugün harika hissediyorum!"
}

✅ Mesajları Listele
GET /api/chat/messages

📁 Proje Klasör Yapısı
📦 chat-emotion-app
 ┣ 📂 backend (.NET Core API)
 ┣ 📂 ai-service (Hugging Face)
 ┗ 📂 frontend (React Web)

✨ Öğrenim Kazanımları

✔ FullStack yazılım geliştirme zincirini anlama
✔ AI API entegrasyonu
✔ Ücretsiz platformlarda deployment
✔ Debugging, API test ve hata yönetimi

📌 Kod Hakkım Beyanı

Bu projede kullanım amaçlı bazı kod parçaları yapay zeka yardımıyla üretilmiş olsa da:

✅ Backend API
✅ Frontend HTTP entegrasyonu
✅ DB işlemleri

tamamen tarafımdan geliştirilmiştir ve çalışma mantığını açıklayabilmekteyim.

📎 Ekler

📄 Staj Case Raporu — Word Dokümanı ✅
(İndirilebilir format olarak teslim edildi)

✅ Durum

📌 Proje başarıyla tamamlandı ✅
📌 Tüm servisler entegre edildi ✅

✉ İletişim

📧 humeyraertas@example.com

🔗 GitHub: https://github.com/HumeyraErtas
Bölüm: Yazılım Mühendisliği

Proje: FullStack + AI Stajyer Projesi
