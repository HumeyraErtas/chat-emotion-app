import gradio as gr
from transformers import AutoTokenizer, AutoModelForSequenceClassification, pipeline

model_name = "savasy/bert-base-turkish-sentiment-cased"

tokenizer = AutoTokenizer.from_pretrained(model_name)
model = AutoModelForSequenceClassification.from_pretrained(model_name)

analyzer = pipeline("sentiment-analysis", model=model, tokenizer=tokenizer)

def analyze_text(text):
    result = analyzer(text)[0]
    label = result["label"].upper()
    
    # HF sonucu "positive" / "negative" / "neutral" şeklinde
    return {
        "label": label,
        "score": round(result["score"], 3)
    }

iface = gr.Interface(
    fn=analyze_text,
    inputs="text",
    outputs="json"
)

if __name__ == "__main__":
    iface.launch(server_name="0.0.0.0", server_port=7860)
