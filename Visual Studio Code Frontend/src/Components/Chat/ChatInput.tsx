import * as React from "react";
import { PrimaryButton, TextField } from "@fluentui/react";
import styles from "./Chat.module.scss";

export interface IChatInputProps {
  disabled: boolean;
  onSend: (question: string) => void;
}

export const ChatInput: React.FunctionComponent<IChatInputProps> = (props: IChatInputProps) => {
  const { disabled, onSend } = props;
  const [value, setValue] = React.useState<string>("");

  const send = (): void => {
    const question = value.trim();
    if (!question || disabled) {
      return;
    }
    onSend(question);
    setValue("");
  };

  // Enter sender, Shift+Enter giver ny linje
  const onKeyDown = (event: React.KeyboardEvent<HTMLInputElement | HTMLTextAreaElement>): void => {
    if (event.key === "Enter" && !event.shiftKey) {
      event.preventDefault();
      send();
    }
  };

  return (
    <div className={styles.input}>
      <TextField
        className={styles.inputField}
        placeholder="Skriv dit spørgsmål…"
        multiline
        autoAdjustHeight
        rows={1}
        value={value}
        disabled={disabled}
        onChange={(_e, newValue) => setValue(newValue ?? "")}
        onKeyDown={onKeyDown}
      />
      <PrimaryButton text="Send" onClick={send} disabled={disabled || !value.trim()} />
    </div>
  );
};
