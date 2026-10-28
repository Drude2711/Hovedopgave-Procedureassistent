import { ISource } from "./ISource";

export interface IAskResponse {
  answer: string;
  sources: ISource[];
}
