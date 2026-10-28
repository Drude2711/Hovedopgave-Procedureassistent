import * as React from "react";
import { MessageBar, MessageBarType } from "@fluentui/react";
import { GlobalContext } from "../Context/GlobalContext";
import { IChatMessage } from "../Api/Models/IChatMessage";
import { ChatWindow } from "../Components/Chat/ChatWindow";
import { ChatInput } from "../Components/Chat/ChatInput";
import styles from "../Components/Chat/Chat.module.scss";

export interface IChatFeatureProps {
  title: string;
}

const newId = (): string => `${Date.now()}-${Math.random().toString(36).slice(2)}`;

export const ChatFeature: React.FunctionComponent<IChatFeatureProps> = (props: IChatFeatureProps) => {
  const [messages, setMessages] = React.useState<IChatMessage[]>([]);
  const [loading, setLoading] = React.useState<boolean>(false);

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

  return (
    <section className={styles.chat}>
      <h2 className={styles.title}>{props.title}</h2>
      {globalContext?.apiProvider.isMock && (
        <MessageBar messageBarType={MessageBarType.warning}>
          Testtilstand: Der er ikke angivet en API-URL i webpartens egenskaber, så svarene er testsvar.
        </MessageBar>
      )}
      <ChatWindow messages={messages} loading={loading} />
      <ChatInput disabled={loading} onSend={(question) => { void askQuestion(question); }} />
    </section>
  );
};
