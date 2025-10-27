Chat Emotion Analyzer

Bu proje, kullanıcıların mesajlaştığı ve mesajların yapay zekâ ile duygu analizinin (pozitif/nötr/negatif) canlı gösterildiği bir web + mobil uygulamasıdır.

Amaç

React → .NET Core → Python AI uçtan uca zinciri kurmak

Hugging Face üzerinde çalışan bir duygu analizi servisini kullanmak

Ücretsiz platformlara dağıtım (Render, Hugging Face, Vercel)

Teknolojiler ve Hosting
Katman	Teknoloji	Hosting
Frontend (Web)	React (Vite)	Vercel
Backend API	.NET 7 + SQLite	Render
AI Servisi	Python + Gradio + Transformers	Hugging Face Spaces
Mobil (opsiyonel)	React Native CLI	Lokal/APK
Mimari Akış

Kullanıcı frontend’den mesaj gönderir.

Backend mesajı veritabanına kaydeder ve AI servisine yollar.

AI sonucu (label + score) backend’e döner.

Backend son duyguyu frontend’e gönderir ve listede gösterilir.

Proje Klasör Yapısı
chat-emotion-app/
├─ backend/                # .NET Core API (Render)
│  ├─ Controllers/
│  ├─ Data/
│  ├─ Models/
│  └─ Program.cs
├─ frontend/               # React Web (Vercel)
│  ├─ src/
│  └─ index.html, vite.config.ts vb.
└─ ai-service/             # Hugging Face Space
   ├─ app.py
   └─ requirements.txt

Backend API

Temel adres (örnek):
https://chat-emotion-app-2.onrender.com

Endpointler

POST /api/chat/register
Kullanıcı kaydı (sadece rumuz).

Request

{
  "nickname": "Humeyra"
}


Response

{
  "success": true,
  "userId": 1,
  "nickname": "Humeyra"
}


POST /api/chat/message
Mesajı kaydeder, AI ile duyguyu tespit eder.

Request

{
  "userId": 1,
  "text": "Bugün harika hissediyorum!"
}


Response (örnek)

{
  "success": true,
  "messageId": 10,
  "userId": 1,
  "text": "Bugün harika hissediyorum!",
  "emotion": "Positive"
}


GET /api/chat/messages
En son mesajları döner.

Response (örnek)

[
  {
    "id": 10,
    "userId": 1,
    "text": "Bugün harika hissediyorum!",
    "emotion": "Positive",
    "createdAt": "2025-10-27T10:00:00Z"
  }
]

AI Servisi

Hugging Face Space URL (örnek):
https://huggingface.co/spaces/humeyraertas/chat-sentiment-analyzer

Model: distilbert-base-uncased-finetuned-sst-2-english

Beklenen çıktı:

{
  "label": "POSITIVE",
  "score": 0.98
}


Not: Hugging Face ücretsiz planda ilk istekler “uyandırma” nedeniyle gecikebilir.

Kurulum
Backend (lokal)
cd backend
dotnet restore
dotnet run


Varsayılan: http://localhost:5000

Frontend (lokal)
cd frontend
npm install
npm run dev


Varsayılan: http://localhost:5173

Mobil (opsiyonel)
cd mobile
npx react-native run-android

Deployment Linkleri
Servis	URL
Web (Vercel)	(buraya ekleyin)
Backend API (Render)	https://chat-emotion-app-2.onrender.com

AI Servisi (Hugging Face)	https://huggingface.co/spaces/humeyraertas/chat-sentiment-analyzer
Test Durumu
Test	Sonuç
Kullanıcı Kaydı	Başarılı
Mesaj Gönderimi	Başarılı
AI Duygu Analizi	Başarılı (uyandırma sonrası)
Veritabanı Kaydı	Başarılı
Web Arayüzü	Başarılı
Geliştirici

İsim: Hümeyra Ertaş

Üniversite: Manisa Celal Bayar Üniversitesi

Bölüm: Yazılım Mühendisliği

Proje: FullStack + AI Stajyer Projesi
