import { Injectable } from '@angular/core';
import { DtoNotificationStatus, IActivitySettings, IDtoDocument, IDtoEventDrivenNotification, IDtoStepItem, IDtoWorkItemData, ParametersError, QueryParams } from './app.model';
import { HttpClient, HttpHeaders, HttpParams } from '@angular/common/http';
import { BehaviorSubject, firstValueFrom, map, mergeMap, Observable } from 'rxjs';
import { OpenIdConfiguration,  ConfigurationService, } from 'angular-auth-oidc-client';

@Injectable({
  providedIn: 'root'
})
export class AppService {

  params?: QueryParams = undefined;
  edaUrl: string = '';
  wiUrl: string = '';
  aiUrl: string = '';
  envUrl: string = '';
  docUrl: string = '';
  authService: string = '';
  clientId : string  = 'Web_Samples'
  bearerHeader = 'Bearer ';

  constructor(private http: HttpClient, private oidcConfigService: ConfigurationService ) { }

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

  extractParams(step: IDtoStepItem) {
    const url = window.location.href;
    if (url.includes('?')) {
      const httpParams = new HttpParams({ fromString: url.split('?')[1] });
      return this.constructQueryParamObjects(httpParams);
     
    } else {
      throw new Error('The application must provide minimal query parameters (runtimeUrl and notification or work item)');
    }
  }

  validateParams (params: QueryParams): Array<string> {
    const ret = new Array<string>();
    if (!params.runtimeUrl) {
      ret.push('The Runtime url parameter is required');
    }
    if (!params.notificationId && !params.workItemID) {
      ret.push('The notification or work item parameter is required');
    }
    return ret;
  }

  private constructQueryParamObjects(params: HttpParams): QueryParams {
    const ret = new QueryParams();
    if (params.has('an')) {
      ret.activityName = params.get('an')!;
    }

    if (params.has('msg')) {
      ret.message = params.get('msg')!;
    }

    if (params.has('cv')) {
      ret.approveValue = params.get('cv')!;
    }

    if (params.has('dv')) {
      ret.denyValue = params.get('dv')!;
    }

    if (params.has('t')) {
      ret.token = params.get('t')!;
    }

    if (params.has('rt')) {
      ret.runtimeUrl = params.get('rt')!;
    }

    if (params.has('nt')) {
      ret.notificationId = Number.parseInt(params.get('nt')!);
    }

    if (params.has('wi')) {
      ret.workItemID = Number.parseInt(params.get('wi')!);
    }

    if (params.has('ut')){
      ret.userTracking = params.get('ut')!.toLowerCase() == 'true';
    }

    if (ret.notificationId) {
      ret.fromNotification = true;
    }

    return ret;
  }


  private createRequestHeaders(ts?: string, wi?: IDtoWorkItemData): HttpHeaders {

    let  h =  new HttpHeaders();
    h = h.set ('authorization',  this.bearerHeader + this.params!.token);
    h = h.set('content-type', 'application/json; charset=utf-8');
    if (wi) {
        h = h.set('workItemData', JSON.stringify(wi));
    }
    if (ts) {
        h = h.set('If-Match', '"' + ts + '"');
    }
    return h;
  }

  LockNotification(notificationID: number): Observable<IDtoEventDrivenNotification> {
    if (!this.params?.fromNotification) {
      const notificationSource = new BehaviorSubject<IDtoEventDrivenNotification>({
        ActivityInstanceID: -1,
        CreationTime: '',
        ID: -1,
        Status: DtoNotificationStatus.Locked,
        WorkItemID: this.params?.workItemID ?? -1 
      });
      return notificationSource.asObservable();
    } else {
      const url_get = this.edaUrl + '/' + notificationID;
      return this.http.get<IDtoEventDrivenNotification>(url_get, { headers : this.createRequestHeaders() })
        .pipe(mergeMap(response_get => {
          const res = response_get;
          const url_put = this.edaUrl + '/' + res.ID + '/status';
          return this.http.put<IDtoEventDrivenNotification>(url_put,
            {Status: DtoNotificationStatus.Locked}, 
            { headers : this.createRequestHeaders(res.TimeStamp) })
            .pipe(map(response_post => {
              return response_post;
            }));
        })
      );
    }
  }

  LockWorkItem(notification: IDtoEventDrivenNotification, userTracking: boolean): Observable<IDtoWorkItemData> {
    const url_get = this.wiUrl + '/' + notification.WorkItemID + '/load';
    return this.http.get<IDtoWorkItemData>(url_get, {headers: this.createRequestHeaders()})
      .pipe(mergeMap(response_get => {
        const res = response_get;
        const url_put = this.wiUrl + '/' + res.WorkItemID + '/lock' + (userTracking ? '?userTracking=true' : '');
        return this.http.put<IDtoWorkItemData>(url_put, res, {headers :  this.createRequestHeaders(res.TimeStamp)})
          .pipe(map(response_post => {
            return response_post;
          })
        );
      })
    );
  }

  LoadConfig(workItem: IDtoWorkItemData): Observable<IActivitySettings> {
    const url_get = this.aiUrl + '/' + workItem.ActivityInstanceID + '/Configuration';
    return this.http.get<any>(url_get, {headers:  this.createRequestHeaders()})
      .pipe(map((response) => {
        return JSON.parse(atob(response.Configuration)) as IActivitySettings;
      })
    );
  }

  LoadDocument(workItem: IDtoWorkItemData): Observable<IDtoDocument> {
    const url = this.docUrl + '/' + workItem.DocumentID + '/false';
    return this.http.get<IDtoDocument>(url, {headers:  this.createRequestHeaders(workItem.TimeStamp, workItem)})
      .pipe(map((response) => {
        return response;
      })
    );
  }

  SaveDocument(workItem: IDtoWorkItemData, rootDocument: IDtoDocument): Observable<string> {
    return this.http.put<IDtoDocument>(this.docUrl, 
      JSON.stringify(rootDocument),
      {headers:  this.createRequestHeaders(workItem.TimeStamp, workItem), observe:'response'})
        .pipe(map((response) => {
          if(response.ok){
            return response.headers.get('Etag')!.replace('"', '').replace('"', '');
          } else {
            return '';
          }
        }
      )
    );
  }

  ReleaseWorkItem(workItem: IDtoWorkItemData, userTracking: boolean): Observable<boolean> {
    const url = this.wiUrl + '/move' + (userTracking ? '?userTracking=true' : '');
    return this.http.post<IDtoWorkItemData>(url, workItem, {headers: this.createRequestHeaders(workItem.TimeStamp)})
      .pipe(map((response) => {
        return response != null;
      })
    );
  }

  AcknowledgeNotification(notification: IDtoEventDrivenNotification): Observable<boolean> {
    if (!this.params?.fromNotification) {
        const notificationSource = new BehaviorSubject<boolean>(true);
        return notificationSource.asObservable();
    } else {
      const url = this.edaUrl + '/' + notification.ID;
      return this.http.delete(url, {headers:  this.createRequestHeaders(notification.TimeStamp), observe: 'response'})
        .pipe(map(response => {
          return response.status === 204;
        })
      );
    }
  }

  
}
