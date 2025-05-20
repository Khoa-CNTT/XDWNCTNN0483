/* =========  CHATBOX SCRIPT  ========= */
document.addEventListener("DOMContentLoaded", () => {
  /* ---------- DOM refs ---------- */
  const chatbox = document.getElementById("gemini-chatbox");
  const messagesContainer = document.getElementById("chatbox-messages");
  const input = document.getElementById("chatbox-input");
  const sendBtn = document.getElementById("chatbox-send-btn");
  const toggleBtn = document.getElementById("chatbox-toggle-img");
  const closeBtn = document.getElementById("chatbox-close-btn");
  const voiceBtn = document.getElementById("chatbox-voice-btn");

  /* ---------- Config ---------- */
  const apiEndpoint = "/api/chat"; // backend route

  /* ---------- Format helper ---------- */
  function cleanPerfumeParagraphJS(text) {
    if (!text) return text;

    // 1) remove * / ** marks
    let t = text.replace(/\*/g, "");

    // 2) add newline before "n." (after space, :, or start) if next is a letter
    const reIndex = /(^|[\s:])(\d+)\.(?=\s*[A-Za-zÀ-ỹ])/g;
    t = t.replace(reIndex, (_, p1, p2) => `${p1}\n${p2}.`);

    // 3) ensure space after dot
    t = t.replace(/\.(\S)/g, ". $1");

    // 4) collapse extra spaces
    t = t.replace(/[ \t]+/g, " ").trim();

    // 5) convert \n to <br> for HTML display
    return t.replace(/\n/g, "<br>");
  }

  /* ---------- UI helpers ---------- */
  function displayMessage(txt, sender) {
    const div = document.createElement("div");
    div.classList.add("message", `${sender}-message`);

    if (sender === "bot") {
      div.innerHTML = txt; // allow <br> in bot messages
    } else {
      div.textContent = txt; // protect user input
    }

    messagesContainer.appendChild(div);
    messagesContainer.scrollTop = messagesContainer.scrollHeight;
    return div;
  }

  function showThinkingIndicator() {
    const thinkingDiv = displayMessage("", "bot");
    thinkingDiv.classList.add("thinking"); // CSS shows animated dots
    return thinkingDiv;
  }

  /* ---------- Send to backend ---------- */
  async function sendMessage() {
    const messageText = input.value.trim();
    if (!messageText) return;

    displayMessage(messageText, "user");
    input.value = "";
    input.disabled = sendBtn.disabled = true;

    const thinking = showThinkingIndicator();

    try {
      const res = await fetch(apiEndpoint, {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({ message: messageText }),
      });

      messagesContainer.removeChild(thinking);

      if (res.ok) {
        const data = await res.json();
        const formatted = cleanPerfumeParagraphJS(data.reply);
        displayMessage(formatted, "bot");
      } else {
        const err = await res.json().catch(() => ({}));
        displayMessage(`Lỗi: ${err.error || res.statusText}`, "bot");
      }
    } catch (err) {
      if (messagesContainer.contains(thinking))
        messagesContainer.removeChild(thinking);
      console.error(err);
      displayMessage("Lỗi kết nối đến máy chủ.", "bot");
    } finally {
      input.disabled = sendBtn.disabled = false;
      input.focus();
    }
  }

  /* ---------- Events ---------- */
  sendBtn.addEventListener("click", sendMessage);
  input.addEventListener("keypress", (e) => {
    if (e.key === "Enter") sendMessage();
  });

  toggleBtn.addEventListener("click", () => {
    chatbox.classList.toggle("visible");
    if (chatbox.classList.contains("visible")) input.focus();
  });

  closeBtn.addEventListener("click", () => {
    chatbox.classList.remove("visible");
  });

  /* ---------- Voice input (optional) ---------- */
  let recognition;
  if ("webkitSpeechRecognition" in window) {
    recognition = new webkitSpeechRecognition();
    recognition.lang = "vi-VN";
    recognition.continuous = false;
    recognition.interimResults = false;

    recognition.onresult = (e) => {
      const transcript = e.results[0][0].transcript;
      input.value = transcript;
      sendMessage();
    };
    recognition.onerror = () => {
      console.warn("Không thể nhận diện giọng nói");
    };
    recognition.onend = () => voiceBtn.classList.remove("recording");

    voiceBtn.addEventListener("click", () => {
      voiceBtn.classList.add("recording");
      recognition.start();
    });
  } else {
    voiceBtn.style.display = "none";
    console.info("Trình duyệt không hỗ trợ Web Speech API");
  }
});
