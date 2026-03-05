import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { firstValueFrom } from 'rxjs';
import { ISelfDiscovery, QueryParams } from './app.models';


@Injectable({ providedIn: 'root' })
export class ConfigService {

    params: QueryParams = new QueryParams;
    wiUrl!: string;
    aiUrl!: string;
    envUrl!: string;
    docUrl!: string;
    authService!: string;
    clientId: string = 'Web_Samples'

    bearerHeader = 'Bearer ';
    apiPrefix = 'api/v2.1';

    constructor(private http: HttpClient) { }



    async prepareConfigFromUrl(): Promise<string> {
        const config = await firstValueFrom(this.http.get<{ runtimeUrl: string; client_id: string }>('./assets/config.json'));
        const url = window.location.href;
        if (url.includes('?')) {
            const httpParams = new HttpParams({ fromString: url.split('?')[1] });
            this.params = this.constructQueryParamObjects(httpParams);
        }

        if (!this.params.runtimeUrl) {
            this.params.runtimeUrl = config.runtimeUrl;
        }

        if (config.client_id) {
            this.clientId = config.client_id;
        }

        if (!this.params.runtimeUrl) {
            throw new Error("Runtime URL missing in query params ('rt')");
        }

        const errors = this.validateParams();
        if (errors.length > 0) {
            console.error('Validation failed:', errors);
            throw new Error(errors.join(', '));
        }

        const selfDiscovery = await firstValueFrom(this.http.get<ISelfDiscovery>(this.params.runtimeUrl + '/environment/endpoints?oidc=true'));

        this.Init(selfDiscovery);

        const rawUrl = selfDiscovery.AuthenticationServiceEndpoint.Uri;
        const authUrl = new URL(rawUrl);
        let stsUrl: string;

        if (authUrl.pathname === '/' || authUrl.pathname === '') {
            stsUrl = authUrl.origin + '/';
        } else {
            stsUrl = rawUrl.replace(/\/$/, "");
        }

        this.authService = stsUrl;

        return stsUrl;
    }


    private constructQueryParamObjects(params: HttpParams): QueryParams {
        const ret = new QueryParams();
        if (params.has('ai')) ret.activityInstanceID = parseInt(params.get('ai')!);
        if (params.has('rc')) ret.requestCount = parseInt(params.get('rc')!);
        if (params.has('rt')) ret.runtimeUrl = params.get('rt')!;
        if (params.has('t')) ret.token = params.get('t')!;
        if (params.has('ut')) ret.userTracking = params.get('ut')!.toLowerCase() === 'true';
        if (params.has('ord')) ret.orderBy = params.get('ord')!;
        if (params.has('din')) {
            ret.enableDocIndex = true;
            ret.docIndexName = params.get('din')!;
            ret.docIndexValue = params.get('div')!;
        }
        return ret;
    }



    Init(servicesData: ISelfDiscovery) {
        const prcUrl = servicesData.ProcessServiceEndpoint.Uri + this.apiPrefix + '/ProcessService';
        const cfgUrl = servicesData.ConfigurationServiceEndpoint.Uri + this.apiPrefix + '/ConfigService';
        const docUrl = servicesData.DocumentServiceEndpoint.Uri + + this.apiPrefix + '/DocumentService/Thin';

        this.wiUrl = prcUrl + '/WorkItems';
        this.aiUrl = cfgUrl + '/ActivityInstances';
        this.envUrl = cfgUrl + '/Environment';
        this.docUrl = docUrl + '/Document';
    }

    validateParams(): string[] {
        const errors: string[] = [];
        if (!this.params.runtimeUrl) errors.push('Runtime url (rt) is required.');
        return errors;
    }
}
