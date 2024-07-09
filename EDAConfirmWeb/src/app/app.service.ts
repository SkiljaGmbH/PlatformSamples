
import {map, mergeMap} from 'rxjs/operators';
import { Injectable } from '@angular/core';
import { Observable ,  BehaviorSubject } from 'rxjs';
import { IDtoEventDrivenNotification, IDtoWorkItemData, IActivitySettings,
    IDtoDocument, QueryParams, DtoNotificationStatus } from './app.models';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { OidcSecurityService, OidcConfigService, OpenIdConfiguration } from 'angular-auth-oidc-client';
import { JsonPipe } from '@angular/common';

@Injectable()
export class AppService {

    params: QueryParams;
    edaUrl: string;
    wiUrl: string;
    aiUrl: string;
    envUrl: string;
    docUrl: string;
    authService: string;
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
        const edaAppendix = '/EventDrivenNotifications';
        const wiAppendix = '/WorkItems';
        const aiAppendix = '/ActivityInstances';
        const envAppendix = '/Environment';
        const docAppendix = '/Document';
        
        this.params = query;

        const prcUrl = this.params.runtimeUrl + '/processservice' + apiPrefix + prcAppendix;
        const cfgUrl = this.params.runtimeUrl +  '/configurationservice' + apiPrefix + cfgAppendix;
        const docUrl = this.params.runtimeUrl +  '/documentservice' + apiPrefix + docThinAppendix;

        this.edaUrl = prcUrl + edaAppendix;
        this.wiUrl = prcUrl + wiAppendix;
        this.aiUrl = cfgUrl + aiAppendix;
        this.envUrl = cfgUrl + envAppendix;
        this.docUrl = docUrl + docAppendix;
    }

    initOAuth() : Promise<boolean>{
        return new Promise<boolean>(resolve => {
            const url_get = this.envUrl + '/Authentication';
            this.http.get<string>(url_get).toPromise().then(data=> {
                this.authService = data;
                this.oidcConfigService.load_using_stsServer(data);
                resolve(true);
            })
        })
        
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

    LockNotification(notificationID: number): Observable<IDtoEventDrivenNotification> {
        if (!this.params.fromNotification) {
            const notificationSource = new BehaviorSubject<IDtoEventDrivenNotification>({ActivityInstanceID: -1,
                CreationTime: null,
                ID: -1,
                Message: null,
                ModifiedAt: null,
                RelatedStream: null,
                Status: DtoNotificationStatus.Locked,
                TimeStamp: null,
                WorkItemID: this.params.workItemID });
                return notificationSource.asObservable();
        } else {
            const url_get = this.edaUrl + '/' + notificationID;
            return this.http.get(url_get, { headers : this.createRequestHeaders() }).pipe(mergeMap(response_get => {
               const res = response_get as IDtoEventDrivenNotification;
               const url_put = this.edaUrl + '/' + res.ID + '/status';
               return this.http.put(url_put,
                   {Status: DtoNotificationStatus.Locked},
                   { headers : this.createRequestHeaders(res.TimeStamp) }
                   ).pipe(map(response_post => {
                   return response_post as IDtoEventDrivenNotification;
               }));
           }));
        }
    }

    LockWorkItem(notification: IDtoEventDrivenNotification, userTracking: boolean): Observable<IDtoWorkItemData> {
        const url_get = this.wiUrl + '/' + notification.WorkItemID + '/load';
         return this.http.get(url_get, {headers: this.createRequestHeaders()}).pipe(mergeMap(response_get => {
            const res = response_get as IDtoWorkItemData;
            const url_put = this.wiUrl + '/' + res.WorkItemID + '/lock' + (userTracking ? '?userTracking=true' : '');
            return this.http.put(url_put,
                res,
                {headers :  this.createRequestHeaders(res.TimeStamp)}).pipe(map(response_post => {
                return response_post as IDtoWorkItemData;
            }));
        }));
    }

    LoadConfig(workItem: IDtoWorkItemData): Observable<IActivitySettings> {
        const url_get = this.aiUrl + '/' + workItem.ActivityInstanceID + '/Configuration';
        return this.http.get<any>(url_get, {headers:  this.createRequestHeaders()}).pipe(map((response) => {
            return  JSON.parse(atob(response.Configuration)) as IActivitySettings;
        }));
    }

    LoadDocument(workItem: IDtoWorkItemData): Observable<IDtoDocument> {
        const url = this.docUrl + '/' + workItem.DocumentID + '/false';
        return this.http.get(url, {headers:  this.createRequestHeaders(workItem.TimeStamp, workItem)}).pipe(map((response) => {
            return response as IDtoDocument;
        }));
    }

    SaveDocument(workItem: IDtoWorkItemData, rootDocument: IDtoDocument): Observable<string> {
        return this.http.put(this.docUrl,
            JSON.stringify(rootDocument),
            {headers:  this.createRequestHeaders(workItem.TimeStamp, workItem), observe:'response'}).pipe(map((response) => {
                if(response.ok){
                    return response.headers.get('Etag').replace('"', '').replace('"', '');
                } else {
                    return '';
                }
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

    AcknowledgeNotification(notification: IDtoEventDrivenNotification): Observable<boolean> {
        if (!this.params.fromNotification) {
            const notificationSource = new BehaviorSubject<boolean>(true);
            return notificationSource.asObservable();
        } else {
            const url = this.edaUrl + '/' + notification.ID;
            return this.http.delete(url, {headers:  this.createRequestHeaders(notification.TimeStamp), observe: 'response'}).pipe(map(response => {
                return response.status === 204;
            }));
        }
    }
}
