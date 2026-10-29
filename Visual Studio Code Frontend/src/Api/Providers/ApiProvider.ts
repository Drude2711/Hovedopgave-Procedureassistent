import { AadHttpClient, HttpClient, HttpClientResponse } from "@microsoft/sp-http";
import { WebPartContext } from "@microsoft/sp-webpart-base";
import { IAskRequest } from "../Models/IAskRequest";
import { IAskResponse } from "../Models/IAskResponse";
import { IChatTurn } from "../Models/IChatTurn";

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

  /** Uden login: API-URL er sat, men Application ID URI er tom (walking skeleton trin 4). */
  public get isAnonymous(): boolean {
    return !!this.apiUrl && !this.apiResource;
  }

  public async ask(question: string, history: IChatTurn[] = []): Promise<IAskResponse> {
    if (this.isMock) {
      return this.mockAnswer(question);
    }

    const body: IAskRequest = { question, history };
    const options = {
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify(body)
    };

    let response: HttpClientResponse;
    if (this.isAnonymous) {
      // Trin 4: kald API'et direkte uden token
      response = await this.context.httpClient.post(this.apiUrl, HttpClient.configurations.v1, options);
    } else {
      // Trin 5: kald API'et med brugerens token fra Entra ID
      const client: AadHttpClient = await this.context.aadHttpClientFactory.getClient(this.apiResource);
      response = await client.post(this.apiUrl, AadHttpClient.configurations.v1, options);
    }

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
