import * as React from 'react';
import * as ReactDom from 'react-dom';
import { Version } from '@microsoft/sp-core-library';
import {
  type IPropertyPaneConfiguration,
  PropertyPaneTextField
} from '@microsoft/sp-property-pane';
import { BaseClientSideWebPart } from '@microsoft/sp-webpart-base';

import * as strings from 'AiProcedureAssistentWebPartStrings';
import { AiProcedureAssistentApp, IAiProcedureAssistentAppProps } from './AiProcedureAssistentApp';

export interface IAiProcedureAssistentWebPartProps {
  title: string;
  apiUrl: string;
  apiResource: string;
}

export default class AiProcedureAssistentWebPart extends BaseClientSideWebPart<IAiProcedureAssistentWebPartProps> {

  public render(): void {
    const element: React.ReactElement<IAiProcedureAssistentAppProps> = React.createElement(
      AiProcedureAssistentApp,
      {
        context: this.context,
        title: this.properties.title || 'Procedureassistent',
        apiUrl: this.properties.apiUrl || '',
        apiResource: this.properties.apiResource || ''
      }
    );

    ReactDom.render(element, this.domElement);
  }

  protected onDispose(): void {
    ReactDom.unmountComponentAtNode(this.domElement);
  }

  protected get dataVersion(): Version {
    return Version.parse('1.0');
  }

  protected getPropertyPaneConfiguration(): IPropertyPaneConfiguration {
    return {
      pages: [
        {
          header: {
            description: strings.PropertyPaneDescription
          },
          groups: [
            {
              groupName: strings.BasicGroupName,
              groupFields: [
                PropertyPaneTextField('title', {
                  label: 'Titel'
                }),
                PropertyPaneTextField('apiUrl', {
                  label: 'API-URL',
                  description: 'Fx https://<din-function>.azurewebsites.net/api/ask. Tom = testtilstand.'
                }),
                PropertyPaneTextField('apiResource', {
                  label: 'API Application ID URI',
                  description: 'Fx api://<client-id> fra app-registreringen'
                })
              ]
            }
          ]
        }
      ]
    };
  }
}
