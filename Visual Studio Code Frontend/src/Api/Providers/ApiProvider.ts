import { AadHttpClient, HttpClientResponse } from "@microsoft/sp-http";
import { WebPartContext } from "@microsoft/sp-webpart-base";
import { IAskRequest } from "../Models/IAskRequest";
import { IAskResponse } from "../Models/IAskResponse";

export class ApiProvider {

  constructor(
    private context: WebPartContext,
    private apiUrl: string,
    private apiResource: string
  ) { }

  /** Testtilstand: bruges indtil API-URL er sat i webpartens egenskaber. */
  public get isMock(): boolean {
    return !this.apiUrl;
  }

  public async ask(question: string): Promise<IAskResponse> {
    if (this.isMock) {
      return this.mockAnswer(question);
    }

    const body: IAskRequest = { question };
    const client: AadHttpClient = await this.context.aadHttpClientFactory.getClient(this.apiResource);
    const response: HttpClientResponse = await client.post(this.apiUrl, AadHttpClient.configurations.v1, {
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify(body)
    });

    if (!response.ok) {
      throw new Error(`API-fejl: ${response.status} ${response.statusText}`);
    }
    return await response.json() as IAskResponse;
  }

  private async mockAnswer(question: string): Promise<IAskResponse> {
    await new Promise<void>((resolve) => setTimeout(resolve, 800));
    return {
      answer: `Dette er et testsvar, fordi backend ikke er forbundet endnu. Du spurgte: "${question}"`,
      sources: [{ title: "Testdokument.docx" }]
    };
  }
}
