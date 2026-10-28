import { ISource } from "./ISource";

export interface IChatMessage {
  id: string;
  role: "user" | "assistant";
  text: string;
  sources?: ISource[];
  isError?: boolean;
}
