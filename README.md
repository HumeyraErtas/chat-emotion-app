# PROJE PLANI
# 🎯 Chat Emotion Analyzer  
Full-Stack + AI Chat Uygulaması

Bu proje, kullanıcıların mesaj yazarak sohbet ettiği ve yazışmaların **AI tarafından duygu analizi yapılarak** anlık olarak görüntülendiği bir web + mobil uygulamasıdır.

---

## 🧠 Proje Amaçları  
- Full-stack yapı geliştirme (React → .NET Core → Python AI)
- Gerçek zamanlı duygu analizi entegrasyonu
- Ücretsiz cloud deployment süreçlerini öğrenme
- AI modeli API tüketimi

---

## 🚀 Teknoloji Stack’i
| Katman | Teknoloji | Hosting |
|--------|----------|---------|
| Frontend (Web) | React | Vercel |
| Frontend (Mobil) | React Native CLI | Lokal / APK Build |
| Backend API | .NET Core + SQLite | Render |
| AI Service | Python + Hugging Face Transformers | HuggingFace Spaces |

---

## 📌 Mimari Akış
Kullanıcı mesaj gönderir ➝  
Backend API veriyi kaydeder ➝  
AI servisine gönderir ➝  
AI duyguyu analiz eder ➝  
Frontend ekranda gösterir ✅

---

## 🗂 Proje Klasör Yapısı

chat-emotion-app/
│
├── backend/ → .NET Core API + Database (Render)
│ ├── Controllers/
│ ├── Data/
│ ├── Models/
│ └── Program.cs
│
├── frontend/ → React Web (Vercel)
│ ├── src/
│ └── App.jsx
│
└── ai-service/ → Hugging Face Space (Gradio + Transformers)
├── app.py
└── requirements.txt

yaml
Kodu kopyala

---

## 🔌 Backend API Endpointleri

| METHOD | URL | Açıklama |
|--------|-----|----------|
| POST | `/api/chat/register` | Kullanıcı kaydı |
| POST | `/api/chat/message` | AI duygu analizi + mesaj kaydı |
| GET | `/api/chat/messages` | Mesaj listesini getir |

📝 **Örnek POST Body:**
```json
{
  "userId": 1,
  "text": "Bugün çok mutluyum!"
}
🔥 AI Servisi (HuggingFace)
Model: distilbert-base-uncased-finetuned-sst-2-english

Endpoint: HuggingFace arayüzü üzerinden çağrılır

Çıktı formatı:

json
Kodu kopyala
{
  "label": "POSITIVE",
  "score": 0.98
}
✅ Kurulum & Çalıştırma
🔹 Backend
sh
Kodu kopyala
cd backend
dotnet restore
dotnet run
➡ API: http://localhost:5000

🔹 Frontend
sh
Kodu kopyala
cd frontend
npm install
npm run dev
➡ Web UI: http://localhost:5173

🔹 Mobil (Opsiyonel)
sh
Kodu kopyala
cd mobile
npx react-native run-android
🧪 Testler
Postman ile API test edildi

Duygu analizi + DB kayıt işlemleri doğrulandı ✅

##🌍 Deployment Linkleri
Servis	Link
🌐 Web Uygulaması	(Vercel linki gelecektir)
🧩 AI Servisi	https://huggingface.co/spaces/humeyraertas/chat-sentiment-analyzer
🛠 Backend API	https://chat-emotion-app-2.onrender.com

Not: HuggingFace ücretsiz olduğu için Space sleep moduna girebilir.
İlk çağrıda açılması birkaç saniye sürebilir ⏳

##👩‍💻 Geliştirici
Hümeyra Ertaş
Manisa Celal Bayar Üniversitesi – Yazılım Mühendisliği
📌 FullStack + AI Stajyer Projesi


