import { Injectable } from '@angular/core';
import {HttpClient} from '@angular/common/http';
import {BehaviorSubject} from 'rxjs';
import {Config} from '../../models/config.model';

@Injectable()
export class ConfigService {
  constructor(
    private http: HttpClient
  ) {}

  config = new BehaviorSubject<Config>(null);

  InitAndSelfDiscover(configData: Config) {
    return this.http.get(configData.serverUrl + configData.versionSuffix + configData.configService + '/environment/endpoints?oidc=true').map((data: any) => {
      configData.authService = data.AuthenticationServiceEndpoint.Uri;
      configData.configService = data.ConfigurationServiceEndpoint.Uri + configData.versionSuffix + configData.configService;
      configData.processService = data.ProcessServiceEndpoint.Uri + configData.versionSuffix + configData.processService;
      configData.signalRUrl = data.ProcessServiceEndpoint.Uri + configData.signalRUrl;
      this.config.next(configData);
      return configData;
    });
  }
}
