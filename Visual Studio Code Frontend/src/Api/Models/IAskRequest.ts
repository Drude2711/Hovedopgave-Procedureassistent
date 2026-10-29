import { IChatTurn } from "./IChatTurn";

export interface IAskRequest {
  question: string;
  history: IChatTurn[];
}
