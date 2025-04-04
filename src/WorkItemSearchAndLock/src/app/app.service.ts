import { Injectable } from '@angular/core';
import { DtoSTGDataType, IDtoDocumentIndexFilter, IDtoWorkItemData, QueryParams, SearchOperator } from './app.models';
import { BehaviorSubject, Observable, map, mergeMap } from 'rxjs';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { OidcSecurityService, OidcConfigService, OpenIdConfiguration } from 'angular-auth-oidc-client';
import { JsonPipe } from '@angular/common';

@Injectable()
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
        private oidcConfigService: OidcConfigService, 
        private oidcSecurityService: OidcSecurityService ) {
            this.oidcConfigService.onConfigurationLoaded.subscribe(data => {
                let pathName = window.location.pathname;
                if (pathName.length > 0 && pathName.endsWith('/')) {
                pathName = pathName.substring(0, pathName.length - 1);
                }

                const config: OpenIdConfiguration = {
                    stsServer : this.authService,
                    redirect_url : window.location.origin + pathName,
                    post_logout_redirect_uri : window.location.origin + pathName,
                    client_id : this.clientId,
                    scope : 'openid profile offline_access',
                    response_type: 'code',
                    silent_renew: true,
                    use_refresh_token : true,
                    log_console_debug_active : true,
                    log_console_warning_active : true,
                    disable_iat_offset_validation : true,
                    ignore_nonce_after_refresh : true,
                    trigger_authorization_result_event: true
                };
      
                this.oidcSecurityService.setupModule(
                    config,
                    data.authWellknownEndpoints
                );
            });
    }

    Init(query: QueryParams) {
        const apiPrefix = '/api/v2.1';
        const prcAppendix = '/ProcessService';
        const cfgAppendix = '/ConfigService';
        const docThinAppendix = '/DocumentService/Thin';
        const wiAppendix = '/WorkItems';
        const aiAppendix = '/ActivityInstances';
        const envAppendix = '/Environment';
        const docAppendix = '/Document';
        
        this.params = query;

        const prcUrl = this.params.runtimeUrl + '/processservice' + apiPrefix + prcAppendix;
        const cfgUrl = this.params.runtimeUrl +  '/configurationservice' + apiPrefix + cfgAppendix;
        const docUrl = this.params.runtimeUrl +  '/documentservice' + apiPrefix + docThinAppendix;

        this.wiUrl = prcUrl + wiAppendix;
        this.aiUrl = cfgUrl + aiAppendix;
        this.envUrl = cfgUrl + envAppendix;
        this.docUrl = docUrl + docAppendix;
    }

    initOAuth() : Promise<boolean>{
            const url_get = this.envUrl + '/Authentication';
        return this.http.get<string>(url_get).toPromise().then(data=> {
                this.authService = data!;
                this.oidcConfigService.load_using_stsServer(data!);
            return true;
        });
    }

    createRequestHeaders(ts?: string, wi?: IDtoWorkItemData): HttpHeaders {

        let  h =  new HttpHeaders();
        h = h.set ('authorization',  this.bearerHeader + this.params.token);
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

    Login() : Promise<string> {
        return new Promise<string>(resolve=> {
            resolve(this.oidcSecurityService.getToken());
        })
    }

    ReleaseWorkItem(workItem: IDtoWorkItemData, userTracking: boolean): Observable<boolean> {
        const url = this.wiUrl + '/move' + (userTracking ? '?userTracking=true' : '');
        return this.http.post(url, workItem, {headers: this.createRequestHeaders(workItem.TimeStamp)}).pipe(map((response) => {
            return response != null;
        }));
    }
}
