import * as React from "react";
import { WebPartContext } from "@microsoft/sp-webpart-base";
import { GlobalContext, IGlobalContext } from "../../Context/GlobalContext";
import { ApiProvider } from "../../Api/Providers/ApiProvider";
import { ChatFeature } from "../../Features/ChatFeature";

export interface IAiProcedureAssistentAppProps {
  context: WebPartContext;
  title: string;
  apiUrl: string;
  apiResource: string;
}

export const AiProcedureAssistentApp: React.FunctionComponent<IAiProcedureAssistentAppProps> = (props: IAiProcedureAssistentAppProps) => {
  const { context, title, apiUrl, apiResource } = props;

  const contextValue: IGlobalContext = React.useMemo(() => ({
    context: context,
    apiProvider: new ApiProvider(context, apiUrl, apiResource)
  }), [context, apiUrl, apiResource]);

  return (
    <GlobalContext.Provider value={contextValue}>
      <ChatFeature title={title} />
    </GlobalContext.Provider>
  );
};
