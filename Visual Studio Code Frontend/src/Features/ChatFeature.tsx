import * as React from "react";
import { GlobalContext } from "../Context/GlobalContext";
import { IChatMessage } from "../Api/Models/IChatMessage";

export interface IChatFeatureProps {
  title: string;
}

const newId = (): string => `${Date.now()}-${Math.random().toString(36).slice(2)}`;

export const ChatFeature: React.FunctionComponent<IChatFeatureProps> = (props: IChatFeatureProps) => {
  const [messages, setMessages] = React.useState<IChatMessage[]>([]);
  const [loading, setLoading] = React.useState<boolean>(false);
  const [input, setInput] = React.useState<string>("");

  const globalContext = React.useContext(GlobalContext);

  async function askQuestion(question: string): Promise<void> {
    if (!globalContext) {
      return;
    }

    setMessages((prev) => [...prev, { id: newId(), role: "user", text: question }]);
    setLoading(true);

    try {
      const response = await globalContext.apiProvider.ask(question);
      setMessages((prev) => [...prev, {
        id: newId(),
        role: "assistant",
        text: response.answer,
        sources: response.sources
      }]);
    } catch (error) {
      console.error("Kunne ikke hente svar:", error);
      setMessages((prev) => [...prev, {
        id: newId(),
        role: "assistant",
        text: "Der opstod en fejl, og jeg kunne ikke hente et svar. Prøv igen senere.",
        isError: true
      }]);
    } finally {
      setLoading(false);
    }
  }

  function onSend(): void {
    const question = input.trim();
    if (!question || loading) {
      return;
    }
    setInput("");
    void askQuestion(question);
  }

  return (
    <section>
      <h2>{props.title}</h2>

      {/* Beskederne */}
      <div>
        {messages.map((message) => (
          <p key={message.id}>
            <strong>{message.role === "user" ? "Dig" : "Assistent"}:</strong> {message.text}
          </p>
        ))}
        {loading && <p>Søger i procedurerne…</p>}
      </div>

      {/* Input */}
      <input
        value={input}
        onChange={(e) => setInput(e.target.value)}
        placeholder="Skriv dit spørgsmål…"
        disabled={loading}
      />
      <button onClick={onSend} disabled={loading}>Send</button>
    </section>
  );
};