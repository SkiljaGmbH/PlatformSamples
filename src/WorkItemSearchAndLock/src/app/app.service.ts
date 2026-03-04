import { Injectable } from '@angular/core';
import { DtoSTGDataType, IDtoWorkItemData, QueryParams, SearchOperator } from './app.models';
import { Observable, firstValueFrom, map,  } from 'rxjs';
import { HttpClient, HttpHeaders, HttpParams } from '@angular/common/http';
import { OidcSecurityService, } from 'angular-auth-oidc-client';


@Injectable({providedIn: 'root'})
export class AppService {

    params: QueryParams = new QueryParams;
    wiUrl!: string;
    aiUrl!: string;
    envUrl!: string;
    docUrl!: string;
    authService!: string;
    clientId : string  = 'Web_Samples'
    
    bearerHeader = 'Bearer ';

    constructor(
        private http: HttpClient,  
        private oidcSecurityService: OidcSecurityService ) {}

    async prepareConfigFromUrl(): Promise<string>{
        const url = window.location.href;
        if (url.includes('?')) {
            const httpParams = new HttpParams({ fromString: url.split('?')[1] });
            this.params = this.constructQueryParamObjects(httpParams);
        }

        if (!this.params.runtimeUrl) {
            throw new Error("Runtime URL missing in query params ('rt')");
        }

        const errors = this.validateParams();
        if (errors.length > 0) {
            console.error('Validation failed:', errors);
            throw new Error(errors.join(', ')); 
        }

        this.Init(this.params);

        const url_get = this.envUrl + '/Authentication';
        const stsUrl = await firstValueFrom(this.http.get(url_get, { responseType: 'text' }));
        this.authService = stsUrl;
    
        return stsUrl;
    }
    

    private constructQueryParamObjects(params: HttpParams): QueryParams {
        const ret = new QueryParams();
        if (params.has('ai')) ret.activityInstanceID = parseInt(params.get('ai')!);
        if (params.has('rc')) ret.requestCount = parseInt(params.get('rc')!);
        if (params.has('rt')) ret.runtimeUrl = params.get('rt')!;
        if (params.has('t'))  ret.token = params.get('t')!;
        if (params.has('ut')) ret.userTracking = params.get('ut')!.toLowerCase() === 'true';
        if (params.has('ord')) ret.orderBy = params.get('ord')!;
        if (params.has('din')) {
            ret.enableDocIndex = true;
            ret.docIndexName = params.get('din')!;
            ret.docIndexValue = params.get('div')!;
        }
        return ret;
    }



    Init(query: QueryParams) {
        const apiPrefix = '/api/v2.1';
        this.params = query;

        const prcUrl = this.params.runtimeUrl + '/processservice' + apiPrefix + '/ProcessService';
        const cfgUrl = this.params.runtimeUrl + '/configurationservice' + apiPrefix + '/ConfigService';
        const docUrl = this.params.runtimeUrl + '/documentservice' + apiPrefix + '/DocumentService/Thin';

        this.wiUrl = prcUrl + '/WorkItems';
        this.aiUrl = cfgUrl + '/ActivityInstances';
        this.envUrl = cfgUrl + '/Environment';
        this.docUrl = docUrl + '/Document';
    }

    createRequestHeaders(ts?: string, wi?: IDtoWorkItemData): HttpHeaders {

        let  h =  new HttpHeaders();
        const token = this.params.token || this.oidcSecurityService.getAccessToken();
        h = h.set ('authorization',  this.bearerHeader + token);
        h = h.set('content-type', 'application/json; charset=utf-8');
        if (wi) {
            h = h.set('workItemData', JSON.stringify(wi));
            // h.append('If-Match', '"' + wi.TimeStamp + '"');
        }
        if (ts) {
            h = h.set('If-Match', '"' + ts + '"');
        }

        return h;
    }

    private paramsToSearchQuery(queryParams: QueryParams) : any{
        return {
                ActivityInstancesIds: [queryParams.activityInstanceID],
                WorkItemStatuses: [0],
                DocumentIndexFilter: {
                    RootDocumentsOnly: true,
                    FilterData: (queryParams.enableDocIndex ? [{
                        Field: queryParams.docIndexName! ,
                        Operator: SearchOperator.Equal,
                        Type: DtoSTGDataType.STGString,
                        Value: queryParams.docIndexValue
                      }] : [])
                },
            Order: [{OrderBy: queryParams.orderBy ?? "WorkItemID", Descending: false}],
            PageSize: 1,
        }
    }

    SearchAndLockWorkItem(queryParams: QueryParams): Observable<IDtoWorkItemData | undefined> {
        const url_post = this.wiUrl + '/SearchAndLock'
            + (queryParams.userTracking ? "?userTracking=true" : "");

        return this.http.post(url_post, this.paramsToSearchQuery(queryParams),
            {headers :  this.createRequestHeaders()}).pipe(map(response_post => {
            return response_post as IDtoWorkItemData;
        }));
    }

    get token$() : Observable<string> {
        return this.oidcSecurityService.getAccessToken()
    }

    ReleaseWorkItem(workItem: IDtoWorkItemData, userTracking: boolean): Observable<boolean> {
        const url = this.wiUrl + '/move' + (userTracking ? '?userTracking=true' : '');
        return this.http.post(url, workItem, {headers: this.createRequestHeaders(workItem.TimeStamp)}).pipe(map((response) => {
            return response != null;
        }));
    }

    validateParams(): string[] {
        const errors: string[] = [];
        if (!this.params.runtimeUrl) errors.push('Runtime url (rt) is required.');
        if (!this.params.activityInstanceID) errors.push('Activity instance ID (ai) is required.');
        if (!this.params.requestCount || this.params.requestCount <= 0) errors.push('Request count (rc) must be > 0.');
        return errors;
    }
}
