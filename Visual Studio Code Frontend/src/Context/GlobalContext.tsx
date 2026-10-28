import { createContext } from "react";
import { WebPartContext } from "@microsoft/sp-webpart-base";
import { ApiProvider } from "../Api/Providers/ApiProvider";

export interface IGlobalContext {
  context: WebPartContext;
  apiProvider: ApiProvider;
}

export const GlobalContext = createContext<IGlobalContext | null>(null);
