import gradio as gr
from transformers import pipeline

# Hugging Face modelini yükle
analyzer = pipeline("sentiment-analysis")

# API fonksiyonu
def analyze_text(text):
    result = analyzer(text)[0]

    # Hugging Face Spaces 'data' formatı gereği liste içinde göndermeliyiz
    return {
        "data": [
            {
                "label": result["label"],
                "score": float(result["score"])
            }
        ]
    }

# API formatında çalışan Gradio interface
iface = gr.Interface(
    fn=analyze_text,
    inputs=gr.Textbox(label="Write Message"),
    outputs=gr.JSON(label="Sentiment Result"),
    title="Emotion Analyzer API",
    description="Returns sentiment for chat messages"
)

if __name__ == "__main__":
    iface.launch(server_name="0.0.0.0", server_port=7860)
