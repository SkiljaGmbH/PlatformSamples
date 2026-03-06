import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { BehaviorSubject } from 'rxjs';
import { Config } from '../../models/config.model';

@Injectable()
export class ConfigService {
  constructor(
    private http: HttpClient
  ) { }


  async init(): Promise<Config> {
    return this.http.get<Config>('./assets/config.json?@DATA[PRODUCT_VERSION]', { headers: new HttpHeaders({ skipAuth: 'true' }) }).toPromise()
      .then((data: Config) => {
        return this.InitAndSelfDiscover(data)
          .toPromise()
      });
  }

  config = new BehaviorSubject<Config>(null);

  InitAndSelfDiscover(configData: Config) {
    return this.http.get(configData.serverUrl + configData.versionSuffix + configData.configService + '/environment/endpoints?oidc=true', { headers: new HttpHeaders({ skipAuth: 'true' }) }).map((data: any) => {
      configData.authService = data.AuthenticationServiceEndpoint.Uri;
      configData.configService = data.ConfigurationServiceEndpoint.Uri + configData.versionSuffix + configData.configService;
      configData.processService = data.ProcessServiceEndpoint.Uri + configData.versionSuffix + configData.processService;
      configData.signalRUrl = data.ProcessServiceEndpoint.Uri + configData.signalRUrl;
      this.config.next(configData);
      return configData;
    });
  }
}
