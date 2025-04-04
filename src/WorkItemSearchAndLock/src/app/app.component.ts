import { Component, HostListener, OnInit } from '@angular/core';
import { DtoProcessingStatus, DtoSTGDataType, DtoStepStatus, IDtoStepItem, IDtoWorkItemData, QueryParams, SearchOperator } from './app.models';
import { AsyncSubject, BehaviorSubject, map } from 'rxjs';
import { OidcSecurityService } from 'angular-auth-oidc-client';
import { AppService } from './app.service';
import { IconDefinition, library } from '@fortawesome/fontawesome-svg-core';
import { faCheckCircle, faExclamationCircle, faSpinner, faTimesCircle } from '@fortawesome/free-solid-svg-icons';
import { HttpParams } from '@angular/common/http';

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.css']
})
export class AppComponent  implements OnInit {
  title = 'WorkItemSearchAndLock';
  opener = null;
  steps = new Array<IDtoStepItem>();
  processing = DtoProcessingStatus.Processing;
  queryParams = new QueryParams();
  private tokenSource = new BehaviorSubject<string>('');
  private loggingInSource = new AsyncSubject<boolean>();
  private oAuthInitSource = new AsyncSubject<boolean>();
  private loginPopUp: WindowProxy | undefined | null;

  constructor(
    private appService: AppService,
    private oidcSecurityService : OidcSecurityService ,
    ) {
    if (this.oidcSecurityService.moduleSetup) {
      this.doCallbackLogicIfRequired();
    } else {
      this.oidcSecurityService.onModuleSetup.subscribe(() => {
          this.doCallbackLogicIfRequired();
          this.oAuthInitSource.next(true);
          this.oAuthInitSource.complete();
      });
    }
    this.opener = window.opener;
    library.add(faExclamationCircle, faSpinner as IconDefinition, faCheckCircle as IconDefinition, faTimesCircle as IconDefinition);
    this.buildDefaultSteps();
    this.prepareForProcessing();
    
  }

  ngOnInit() {
    this.oidcSecurityService.onAuthorizationResult.subscribe(res => {
      if(res.validationResult == 'Ok' &&  res.authorizationState == 'authorized'){
        this.loggingInSource.next(true);
        this.loggingInSource.complete();
      }
    })
  }

  @HostListener('window:oidc-popup-auth-message', ['$event'])
  handleKeyDown(e: { detail: string; }) {
    if(this.loginPopUp){
      this.loginPopUp.close();
      this.loginPopUp = undefined;
      this. oidcSecurityService.authorizedCallbackWithCode(window.location.toString() + '?' +  e.detail);
    }
  }

  private doCallbackLogicIfRequired() {
    this.oidcSecurityService.authorizedCallbackWithCode(window.location.toString());
  }

  login() {
    if (this.processing == DtoProcessingStatus.LogIn){
    this.oidcSecurityService.authorize((authUrl) => {
      // handle the authorrization URL
      this.loginPopUp = window.open(authUrl, '_blank', 'toolbar=0,location=0,menubar=0');
    }); 
  }
    else if(this.processing == DtoProcessingStatus.PreLogIn){
      this.doLogin();
    }
  }

  closeMe() {
    window.opener = window;
    window.close();
  }

  private doLogin() : Promise<void>{
    let step = this.steps[1];
    step.status = DtoStepStatus.Loading;
    step.errors = [];

    return new Promise<string>((resolve, reject) => {
      if (this.appService.params.token && this.appService.params.token.length > 0 ) {
        this.tokenSource.next(this.appService.params.token);
        resolve(this.appService.params.token);
      } else {
        this.processing = DtoProcessingStatus.PreLogIn;
        if (this.queryParams.runtimeUrl){
        this.appService.Init(this.queryParams);
        this.appService.initOAuth().then( _ => {
          this.oAuthInitSource.toPromise().then(_=> {
            this.processing = DtoProcessingStatus.LogIn;
            this.loggingInSource.toPromise().then(_ => {
              this.appService.Login().then(token => {
                resolve(token);
              });
            });
          });
        }).catch((e) => {
          this.reportError(step, 'initOAuth Error. Reason: ' + JSON.stringify(e));
        });        
      }
        else{
          reject("Runtime URL not defined");
        }
      }
    }).then ((result) => {
      this.processing = DtoProcessingStatus.Processing;
          step.status = DtoStepStatus.Done;
          this.appService.params.token = result;
        }, (e) => {
      this.processing = DtoProcessingStatus.PreLogIn;
          console.error(e);
      this.reportError(step, 'Failed to login to the platform. Reason: ' +  JSON.stringify(e));
        });
      }

  private prepareForProcessing() {
    this.processing = DtoProcessingStatus.Processing;
    let step = this.steps[0];
    step.status = DtoStepStatus.Loading;
    if (this.extractParams(step)) {
      this.doLogin();
    } else {
      step.status = DtoStepStatus.Error;
      return;
    }
  }

  private performStep(step : IDtoStepItem, stepFunction: (step : IDtoStepItem) => Promise<void>[], nextFunction: () => void){
    step.status = DtoStepStatus.Loading;
    step.startTime = new Date();
    let stepPromises : Promise<void>[] = stepFunction(step);

    Promise.allSettled(stepPromises).then(
      _ => {
        //step.durationMs = new Date().valueOf() - step.startTime!.valueOf();
        this.checkSubstepsForStatus(step);
        nextFunction();
  }
    );
  }

  private checkSubstepsForStatus(step: IDtoStepItem){
    if (step.substeps!.length == 0 || step.substeps!.some(x => x.relatedWi) === false){
      step.status = DtoStepStatus.Done;
    }
    if (step.substeps!.some(x => x.status != DtoStepStatus.Done) === false){
      step.status = DtoStepStatus.Done;
    }
    if (step.substeps!.some(x => x.status == DtoStepStatus.Error)){
      step.status = DtoStepStatus.Error;
    }
    let durations = step.substeps!.filter(s => s.durationMs).map(s => s.durationMs!);
    let count = durations.length;
    if (count > 0){
      step.durationMs = durations.reduce((p, c) => Math.max(p, c), 0);
      step.substepDurationAvgMs = Math.round(durations.reduce((p, c) => p + c, 0) / count);
      step.substepDurationStdDevMs = Math.round(Math.sqrt(durations.map(x => Math.pow(x - step.substepDurationAvgMs!, 2)).reduce((a, b) => a + b) / count));
    }
  }

  private doMultipleWorkItemFunctionsInParallel(
    step : IDtoStepItem, wiFunction : (params : QueryParams) => Promise<IDtoWorkItemData | undefined>,
    stepName : (i : number) => string, errorMsg : string
  ) : Promise<void>[]{
    let wiLockPromises : Promise<void>[] = [];
    step.substeps = [];

    for(let i = 0; i < this.queryParams.requestCount; i++){
      let substep : IDtoStepItem = { text: stepName(i), status: DtoStepStatus.Loading, showDetails: false, startTime: new Date() };
      step.substeps!.push(substep);
      
      let wiLockPromise = 
        wiFunction(this.queryParams).then(
          (workItem) => {
            substep.status = DtoStepStatus.Done;
            substep.durationMs = new Date().valueOf() - substep.startTime!.valueOf(); 
            substep.relatedWi = workItem;            
          }, (e) => {
            console.error(e);
            this.reportError(substep, errorMsg + ' Reason: ' + JSON.stringify(e));
          });
        
      wiLockPromises.push(wiLockPromise);
    }
    return wiLockPromises;
      }

  private releaseMultipleWorkItemsInParallel(step : IDtoStepItem, workItems : IDtoWorkItemData[]) : Promise<void>[]{
    step.status = DtoStepStatus.Loading;
    step.substeps = [];
    let wiReleasePromises : Promise<void>[] = [];

    workItems.forEach(wi => {
      if (wi){
        let  releasesubstep : IDtoStepItem = { text: 'Work item release request', status: DtoStepStatus.Loading, showDetails: false, startTime: new Date(), relatedWi: wi };
        step.substeps!.push(releasesubstep);

        let wiReleasePromise = 
          this.appService.ReleaseWorkItem(wi, this.appService.params.userTracking)
          .toPromise()
          .then((wiSaved) => {
            if (wiSaved) {
              releasesubstep.status = DtoStepStatus.Done;
              releasesubstep.durationMs = new Date().valueOf() - releasesubstep.startTime!.valueOf(); 
            } else {
              this.reportError(releasesubstep, 'Work Item was not released!');
            }
          }, (e) => {
            console.error(e);
            this.reportError(releasesubstep, 'Unable to Release work items. Reason: ' + JSON.stringify(e));
          });
        wiReleasePromises.push(wiReleasePromise);
      }
    });
    return wiReleasePromises;
      }

  private reportError (step: IDtoStepItem, errDescription: string) {
    if (!step.errors) {
      step.errors = new Array<string>();
    }
    step.errors.push(errDescription);
    step.status = DtoStepStatus.Error;
    step.durationMs = step.startTime ? new Date().valueOf() - step.startTime.valueOf() : undefined; 
  }

  private validateParam(paramname : string, validationFunc : () => string | undefined) : IDtoStepItem{
    let substep : IDtoStepItem = { text: 'Validating param '+paramname, status: DtoStepStatus.Loading, showDetails: false, errors: [] };
    let err = validationFunc();
    substep.status = DtoStepStatus.Done;
    if (err){
      this.reportError(substep, err);
    }
    return substep;
    }

  private validateParams (step: IDtoStepItem) : Promise<void>[]{
    step.substeps = [];
    step.substeps.push(this.validateParam('runtimeUrl', () => !this.queryParams.runtimeUrl ? 'The Runtime url parameter is required' : undefined ));
    step.substeps.push(this.validateParam('activityInstanceID', () => !this.queryParams.activityInstanceID ? 'The activity instance ID parameter is required' : undefined ));
    step.substeps.push(this.validateParam('requestCount', () => !this.queryParams.requestCount ? 'The request count parameter is required' : undefined ));
    if (step.substeps.some(s => s.status != DtoStepStatus.Done) === false){
      this.appService.Init(this.queryParams);
    }
    return step.substeps.map(s => Promise.resolve());
  }

  private extractParams(step: IDtoStepItem): boolean {
    const url = window.location.href;
    if (url.includes('?')) {
      const httpParams = new HttpParams({ fromString: url.split('?')[1] });
      this.queryParams = this.constructQueryParamObjects(httpParams);
    }
      step.status = DtoStepStatus.Done;
      return true;
    }

  private buildDefaultSteps() {
    this.steps.push({ text: 'Parsing Parameters', status: DtoStepStatus.Waiting, showDetails: false });
    this.steps.push({ text: 'Logging In', status: DtoStepStatus.Waiting, showDetails: false });
  }

  private restoreDefaultSteps(){
    this.steps.length = 2;
  }

  private constructQueryParamObjects(params: HttpParams): QueryParams {
    const ret = new QueryParams;

    if (params.has('ai')) {
      ret.activityInstanceID = parseInt(params.get('ai')!);
    }

    if (params.has('rc')) {
      ret.requestCount = parseInt(params.get('rc')!);
    }

    if (params.has('din')) {
      ret.enableDocIndex = true;
      ret.docIndexName = params.get('din')!;
      ret.docIndexValue = params.get('div')!;
    }

    if (params.has('t')) {
      ret.token = params.get('t')!;
    }

    if (params.has('rt')) {
      ret.runtimeUrl = params.get('rt')!;
    }

    if (params.has('ut')){
      ret.userTracking = params.get('ut')!.toLowerCase() == 'true';
    }

    if (params.has('ord')) {
      ret.orderBy = params.get('ord')!;
    }

    return ret;
  }

  public startSearchAndLock(e: MouseEvent) {
    e.preventDefault();
    this.restoreDefaultSteps();
    let validationStep : IDtoStepItem = { text: 'Validate Parameters', status: DtoStepStatus.Waiting, showDetails: false };
    let lockingStep : IDtoStepItem = { text: 'Locking WorkItems', status: DtoStepStatus.Waiting, showDetails: true };
    let releasingStep : IDtoStepItem = { text: 'Releasing WorkItems', status: DtoStepStatus.Waiting, showDetails: true };
    this.steps.push(validationStep);
    this.steps.push(lockingStep);
    this.steps.push(releasingStep);
    this.performStep(validationStep, (s) => {
      return this.validateParams(s);
    }, () => {
      if (validationStep.status == DtoStepStatus.Done){
    this.performStep(lockingStep, (s) => {
      return this.doMultipleWorkItemFunctionsInParallel(s, (params) => this.appService.SearchAndLockWorkItem(params).toPromise(), (i) => {
        return 'Work item lock request '+i;
      }, "Failed to lock the work items. ");
    }, () => {
      this.performStep(releasingStep, (s) => {
        return this.releaseMultipleWorkItemsInParallel(s, lockingStep.substeps?.filter(x => x.relatedWi).map(x => x.relatedWi!) ?? []);
      }, () => {});
    });
  }
    });
  }
  
}
