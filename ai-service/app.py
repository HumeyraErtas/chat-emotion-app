import gradio as gr
from transformers import pipeline

# ✅ Turkish sentiment analysis model
analyzer = pipeline("sentiment-analysis", model="savasy/bert-base-turkish-sentiment-cased")

def analyze_text(text):
    result = analyzer(text)[0]
    label = result["label"].upper()

    if "POS" in label:
        emotion = "Positive"
    elif "NEG" in label:
        emotion = "Negative"
    else:
        emotion = "Neutral"

    return {"label": emotion, "score": float(result["score"])}

# ✅ API Endpoint
def api_predict(request):
    text = request["data"][0]
    return analyze_text(text)

gr.Interface(
    fn=analyze_text,
    inputs="text",
    outputs="json",
    allow_flagging="never"
).launch()

import gradio as gr
from fastapi import FastAPI, Request

app = FastAPI()

@app.post("/predict")
async def predict(req: Request):
    data = await req.json()
    return analyze_text(data["data"][0])
