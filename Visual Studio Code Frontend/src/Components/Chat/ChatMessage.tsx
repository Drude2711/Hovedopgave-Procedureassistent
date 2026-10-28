import * as React from "react";
import { IChatMessage } from "../../Api/Models/IChatMessage";
import { SourceList } from "./SourceList";
import styles from "./Chat.module.scss";

export interface IChatMessageProps {
  message: IChatMessage;
}

export const ChatMessage: React.FunctionComponent<IChatMessageProps> = (props: IChatMessageProps) => {
  const { message } = props;
  const isUser = message.role === "user";

  const bubbleClass = [
    styles.bubble,
    isUser ? styles.user : styles.assistant,
    message.isError ? styles.error : ""
  ].join(" ");

  return (
    <div className={isUser ? styles.rowUser : styles.rowAssistant}>
      <div className={bubbleClass}>
        <div className={styles.text}>{message.text}</div>
        {!isUser && message.sources && <SourceList sources={message.sources} />}
      </div>
    </div>
  );
};
