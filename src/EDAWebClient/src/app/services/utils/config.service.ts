import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Injectable, signal } from '@angular/core';
import { lastValueFrom, map } from 'rxjs';
import { Config } from '../../models/config.model';

@Injectable({ providedIn: 'root' })
export class ConfigService {

  private readonly _configSignal = signal<Config | null>(null)
  readonly config = this._configSignal.asReadonly();
  constructor(
    private http: HttpClient
  ) { }


  async init(): Promise<Config> {
    const headers = new HttpHeaders({ skipAuth: 'true' });

    const initialData = await lastValueFrom(
      this.http.get<Config>('./assets/config.json?@DATA[PRODUCT_VERSION]', { headers })
    );

    return await lastValueFrom(this.initAndSelfDiscover(initialData));
  }

  initAndSelfDiscover(configData: Config) {
    const headers = new HttpHeaders({ skipAuth: 'true' });
    const url = `${configData.serverUrl}${configData.versionSuffix}${configData.configService}/environment/endpoints?oidc=true`;

    return this.http.get<any>(url, { headers }).pipe(
      map(data => {
        const rawUrl = data.AuthenticationServiceEndpoint.Uri;
        const authUrl = new URL(rawUrl);
        let stsUrl: string;

        if (authUrl.pathname === '/' || authUrl.pathname === '') {
          stsUrl = authUrl.origin + '/';
        } else {
          stsUrl = rawUrl.replace(/\/$/, "");
        }

        configData.authService = stsUrl;
        configData.configService = data.ConfigurationServiceEndpoint.Uri + configData.versionSuffix + configData.configService;
        configData.processService = data.ProcessServiceEndpoint.Uri + configData.versionSuffix + configData.processService;
        configData.signalRUrl = data.ProcessServiceEndpoint.Uri + configData.signalRUrl;

        this._configSignal.set(configData);
        return configData;
      })
    );
  }
}
