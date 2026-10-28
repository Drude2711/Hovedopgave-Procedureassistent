import * as React from "react";
import { Spinner, SpinnerSize } from "@fluentui/react";
import { IChatMessage } from "../../Api/Models/IChatMessage";
import { ChatMessage } from "./ChatMessage";
import styles from "./Chat.module.scss";

export interface IChatWindowProps {
  messages: IChatMessage[];
  loading: boolean;
}

export const ChatWindow: React.FunctionComponent<IChatWindowProps> = (props: IChatWindowProps) => {
  const { messages, loading } = props;
  const bottomRef = React.useRef<HTMLDivElement>(null);

  // Rul ned til nyeste besked
  React.useEffect(() => {
    bottomRef.current?.scrollIntoView({ behavior: "smooth", block: "nearest" });
  }, [messages, loading]);

  return (
    <div className={styles.window}>
      {messages.length === 0 && (
        <div className={styles.empty}>
          Stil et spørgsmål om virksomhedens procedurer, fx &quot;Hvor lang tids varsel skal jeg give for en feriedag?&quot;
        </div>
      )}
      {messages.map((message) => (
        <ChatMessage key={message.id} message={message} />
      ))}
      {loading && (
        <div className={styles.rowAssistant}>
          <Spinner size={SpinnerSize.small} label="Søger i procedurerne…" labelPosition="right" />
        </div>
      )}
      <div ref={bottomRef} />
    </div>
  );
};
