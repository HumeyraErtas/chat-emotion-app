import gradio as gr
from transformers import pipeline

# Model yükle
analyzer = pipeline("sentiment-analysis")

# Ana API fonksiyonu
def analyze(message):
    result = analyzer(message)[0]
    label = result["label"]

    if label == "POSITIVE":
        label = "Positive"
    elif label == "NEGATIVE":
        label = "Negative"
    else:
        label = "Neutral"
    
    return {"label": label, "score": float(result["score"])}

# ✅ API modunda çalıştır
demo = gr.Interface(
    fn=analyze,
    inputs=gr.Text(),
    outputs=gr.JSON(),
    allow_flagging="never"
).queue()

if __name__ == "__main__":
    demo.launch(server_name="0.0.0.0", server_port=7860)
