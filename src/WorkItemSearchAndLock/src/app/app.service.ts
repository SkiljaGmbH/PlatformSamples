import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { OidcSecurityService, } from 'angular-auth-oidc-client';
import { Observable, map } from 'rxjs';
import { DtoSTGDataType, IDtoWorkItemData, QueryParams, SearchOperator } from './app.models';
import { ConfigService } from './config.service';


@Injectable({ providedIn: 'root' })
export class AppService {

    bearerHeader = 'Bearer ';

    constructor(
        private http: HttpClient,
        private configService: ConfigService,
        private oidcSecurityService: OidcSecurityService) { }


    createRequestHeaders(ts?: string, wi?: IDtoWorkItemData): HttpHeaders {

        let h = new HttpHeaders();
        const token = this.configService.params.token || this.oidcSecurityService.getAccessToken();
        h = h.set('authorization', this.bearerHeader + token);
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

    private paramsToSearchQuery(queryParams: QueryParams): any {
        return {
            ActivityInstancesIds: [queryParams.activityInstanceID],
            WorkItemStatuses: [0],
            DocumentIndexFilter: {
                RootDocumentsOnly: true,
                FilterData: (queryParams.enableDocIndex ? [{
                    Field: queryParams.docIndexName!,
                    Operator: SearchOperator.Equal,
                    Type: DtoSTGDataType.STGString,
                    Value: queryParams.docIndexValue
                }] : [])
            },
            Order: [{ OrderBy: queryParams.orderBy ?? "WorkItemID", Descending: false }],
            PageSize: 1,
        }
    }

    SearchAndLockWorkItem(queryParams: QueryParams): Observable<IDtoWorkItemData | undefined> {
        const url_post = this.configService.wiUrl + '/SearchAndLock'
            + (queryParams.userTracking ? "?userTracking=true" : "");

        return this.http.post(url_post, this.paramsToSearchQuery(queryParams),
            { headers: this.createRequestHeaders() }).pipe(map(response_post => {
                return response_post as IDtoWorkItemData;
            }));
    }

    get token$(): Observable<string> {
        return this.oidcSecurityService.getAccessToken()
    }

    ReleaseWorkItem(workItem: IDtoWorkItemData, userTracking: boolean): Observable<boolean> {
        const url = this.configService.wiUrl + '/move' + (userTracking ? '?userTracking=true' : '');
        return this.http.post(url, workItem, { headers: this.createRequestHeaders(workItem.TimeStamp) }).pipe(map((response) => {
            return response != null;
        }));
    }

    validateParams(): string[] {
        const errors: string[] = [];
        if (!this.configService.params.runtimeUrl) errors.push('Runtime url (rt) is required.');
        if (!this.configService.params.activityInstanceID) errors.push('Activity instance ID (ai) is required.');
        if (!this.configService.params.requestCount || this.configService.params.requestCount <= 0) errors.push('Request count (rc) must be > 0.');
        return errors;
    }
}
