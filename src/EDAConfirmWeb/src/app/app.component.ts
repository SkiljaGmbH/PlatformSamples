import { mergeMap, map } from 'rxjs/operators';
import { Component, HostListener, Input, OnInit } from '@angular/core';
import {HttpParams} from '@angular/common/http';
import { QueryParams, IDtoStepItem, DtoStepStatus, IDecisionResult, DtoProcessingStatus } from './app.models';
import { IconDefinition, library } from '@fortawesome/fontawesome-svg-core';
import { faSpinner, faCheckCircle, faTimesCircle, faExclamationCircle  } from '@fortawesome/free-solid-svg-icons';
import { AppService } from './app.service';
import { AsyncSubject, BehaviorSubject } from 'rxjs';
import { OidcSecurityService } from 'angular-auth-oidc-client';


@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.css']
})
export class AppComponent implements OnInit {
  title = 'EDAConfirmWeb';
  message = 'We need your decision in order to continue the process. So what shall it be?';
  yesTitle = 'Yes';
  noTitle = 'No';
  opener = null;
  steps = new Array<IDtoStepItem>();
  processing = DtoProcessingStatus.Communication;
  private decisionSource = new AsyncSubject<IDecisionResult>();
  private tokenSource = new BehaviorSubject<string>('');
  private loggingInSource = new AsyncSubject<boolean>();
  private oAuthInitSource = new AsyncSubject<boolean>();
  private loginPopUp: WindowProxy;

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
    this.buildSteps();
    this.execProcessing();
    
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
  handleKeyDown(e) {
    if(this.loginPopUp){
      this.loginPopUp.close();
      this.loginPopUp = null;
      this. oidcSecurityService.authorizedCallbackWithCode(window.location.toString() + '?' +  e.detail);
    }
  }

  private doCallbackLogicIfRequired() {
    this.oidcSecurityService.authorizedCallbackWithCode(window.location.toString());
  }

  login() {
    this.oidcSecurityService.authorize((authUrl) => {
      // handle the authorrization URL
      this.loginPopUp = window.open(authUrl, '_blank', 'toolbar=0,location=0,menubar=0');
  });
    
  }

  closeMe() {
    window.opener = window;
    window.close();
  }

  doLogin() : Promise<string>{
    return new Promise<string>(resolve => {
      if (this.appService.params.token && this.appService.params.token.length > 0 ) {
        this.tokenSource.next(this.appService.params.token);
        resolve(this.appService.params.token);
      } else {
        this.appService.initOAuth().then( _ => {
          this.oAuthInitSource.toPromise().then(_=> {
            this.processing = DtoProcessingStatus.Login;
            this.loggingInSource.toPromise().then(_ => {
              this.appService.Login().then(token => {
                resolve(token);
              });
            });
          });
        });
        
      }
    })
    
  }

  private execProcessing() {
    this.processing = DtoProcessingStatus.Communication;
    let step = this.steps[0];
    step.status = DtoStepStatus.Loading;
    if (this.extractParams(step)) {
      step.status = DtoStepStatus.Done;
      step = this.steps[1];
      step.status = DtoStepStatus.Loading;
      this.doLogin().then((result) => {
        this.processing = DtoProcessingStatus.Communication;
        step.status = DtoStepStatus.Done;
        step = this.steps[2];
        this.appService.params.token = result;
        this.appService.LockNotification(this.appService.params.notificationId).subscribe((notification) => {
          step.status = DtoStepStatus.Done;
          step = this.steps[3];
          step.status = DtoStepStatus.Loading;
          this.appService.LockWorkItem(notification, this.appService.params.userTracking).subscribe((workItem) => {
            step.status = DtoStepStatus.Done;
            step = this.steps[4];
            step.status = DtoStepStatus.Loading;
            this.appService.LoadConfig(workItem).subscribe((settings) => {
              if (settings.Message && settings.Message.length > 0) {
                this.appService.params.message = settings.Message;
              }
              if (settings.NegativeValue && settings.NegativeValue.length > 0) {
                this.appService.params.denyValue = settings.NegativeValue;
              } else {
                this.appService.params.denyValue = '0'
              }
              if (settings.PositiveValue && settings.PositiveValue.length > 0) {
                this.appService.params.approveValue = settings.PositiveValue;
              } else {
                this.appService.params.approveValue = '1'
              }
              step.status = DtoStepStatus.Done;
              step = this.steps[5];
              step.status = DtoStepStatus.Loading;
              this.appService.LoadDocument(workItem).subscribe((doc) => {
                step.status = DtoStepStatus.Done;
                step = this.steps[6];
                step.status = DtoStepStatus.Loading;
                this.processing = DtoProcessingStatus.Decision;
                this.decisionSource.asObservable().subscribe((decision) => {
                    if (decision.IsDecided) {
                      this.processing = DtoProcessingStatus.Communication;
                      let found = false;
                      if (!doc.CustomValues) {
                        doc.CustomValues = [];
                      }
                      if (!settings ) {
                        settings = { DecisionCustomValue: 'CV_Router',
                          Message: this.appService.params.message,
                          NegativeValue: this.appService.params.approveValue,
                          PositiveValue: this.appService.params.denyValue };
                      } else if (!settings.DecisionCustomValue) {
                          settings.DecisionCustomValue = 'CV_Router';
                      }
                      doc.CustomValues.forEach(cv => {
                        if (cv.Key.trim().toUpperCase() === settings.DecisionCustomValue.trim().toUpperCase()) {
                          found = true;
                          cv.Value = decision.Result;
                        }
                      });
                      if (!found) {
                        doc.CustomValues.push({Key: settings.DecisionCustomValue, Value: decision.Result});
                      }
                      step.status = DtoStepStatus.Done;
                      step = this.steps[7];
                      step.status = DtoStepStatus.Loading;
                      this.appService.SaveDocument(workItem, doc).subscribe((wiEtag) => {
                        if (wiEtag && wiEtag.length > 0) {
                          step.status = DtoStepStatus.Done;
                          step = this.steps[8];
                          step.status = DtoStepStatus.Loading;
                          workItem.TimeStamp = wiEtag;
                          this.appService.ReleaseWorkItem(workItem, this.appService.params.userTracking).subscribe((wiSaved) => {
                            if (wiSaved) {
                              step.status = DtoStepStatus.Done;
                              step = this.steps[9];
                              step.status = DtoStepStatus.Loading;
                              this.appService.AcknowledgeNotification(notification).subscribe((acknowledged) => {
                                if (acknowledged) {
                                  step.status = DtoStepStatus.Done;
                                  this.processing = DtoProcessingStatus.Done;
                                } else {
                                  this.reportError(step, 'Notification was not acknowledged!');
                                  step.status = DtoStepStatus.Error;
                                }
                              }, (e) => {
                                console.error(e);
                                this.reportError(step, 'Unable to acknowledge the notification (' + notification.ID + '). Reason: ' + e);
                                step.status = DtoStepStatus.Error;
                              });
                            } else {
                              this.reportError(step, 'Work Item was not released!');
                              step.status = DtoStepStatus.Error;
                            }
                          }, (e) => {
                            console.error(e);
                            this.reportError(step, 'Unable to Release the work item  (' + workItem.WorkItemID + '). Reason: ' + e);
                            step.status = DtoStepStatus.Error;
                          });
                        } else {
                          this.reportError(step, 'Document was not saved!');
                          step.status = DtoStepStatus.Error;
                        }
                      }, (e) => {
                        console.error(e);
                        this.reportError(step, 'Unable to save document (' + workItem.DocumentID + '). Reason: ' + e);
                        step.status = DtoStepStatus.Error;
                      });
                    } else {
                      this.processing = DtoProcessingStatus.Communication;
                      this.reportError(step, 'Undecided result');
                      step.status = DtoStepStatus.Error;
                    }
                }, (e) => {
                  console.error(e);
                  this.reportError(step, 'Decision failure : ' + e);
                  step.status = DtoStepStatus.Error;
                });
              }, (e) => {
                console.error(e);
                this.reportError(step,
                  'Unable to load the document (' + workItem.DocumentID + ') for the work item ('  +
                  workItem.WorkItemID + '). Reason: ' + e);
                step.status = DtoStepStatus.Error;
              });
            }, (e) => {
              console.error(e);
              this.reportError(step,
                'Unable to load the settings for the activity ('  + workItem.ActivityName + '). Reason: ' + e);
              step.status = DtoStepStatus.Error;
            });
          }, (e) => {
            console.error(e);
            this.reportError(step,
              'Failed to lock the work item ('  + notification.WorkItemID + '). Reason: ' + e);
            step.status = DtoStepStatus.Error;
          });
        }, (e) => {
          console.error(e);
          this.reportError(step,
            'Failed to lock the requested notification notification ('  + this.appService.params.notificationId + '). Reason: ' + e);
          step.status = DtoStepStatus.Error;
        });
      }, (e) => {
        this.processing = DtoProcessingStatus.Communication;
        console.error(e);
        this.reportError(step,
          'Failed to login to the platform. Reason: ' + e);
        step.status = DtoStepStatus.Error;
      });
    } else {
      step.status = DtoStepStatus.Error;
      return;
    }
  }

  private reportError (step: IDtoStepItem, errDescription: string) {
    if (!step.errors) {
      step.errors = new Array<string>();
    }
    step.errors.push(errDescription);
    step.status = DtoStepStatus.Error;
  }

  private validateParams (params: QueryParams): Array<string> {
    const ret = new Array<string>();
    if (!params.runtimeUrl) {
      ret.push('The Runtime url parameter is required');
    }
    if (!params.notificationId && !params.workItemID) {
      ret.push('The notification or work item parameter is required');
    }
    return ret;
  }

  private extractParams(step: IDtoStepItem) {
    const url = window.location.href;
    if (url.includes('?')) {
      const httpParams = new HttpParams({ fromString: url.split('?')[1] });
      const params = this.constructQueryParamObjects(httpParams);
      if (params.activityName) {
        this.title = params.activityName;
      }
      if (params.message) {
        this.message = params.message;
      }
      if (params.approveValue) {
        this.yesTitle = params.approveValue;
      }
      if (params.denyValue) {
        this.noTitle = params.denyValue;
      }

      const errs = this.validateParams(params);
      if (errs && errs.length > 0) {
        errs.forEach(err => {this.reportError(step, err); });
        return false;
      } else {
        this.appService.Init(params);
        step.status = DtoStepStatus.Done;
        return true;
      }
    } else {
      this.reportError(step, 'The application must provide minimal query parameters (runtimeUrl and notification or work item)');
      return false;
    }
  }

  private buildSteps() {
    this.steps.push({ text: 'Parsing Parameters', status: DtoStepStatus.Waiting });
    this.steps.push({ text: 'Logging In', status: DtoStepStatus.Waiting });
    this.steps.push({ text: 'Locking Notification', status: DtoStepStatus.Waiting });
    this.steps.push({ text: 'Locking WorkItem', status: DtoStepStatus.Waiting });
    this.steps.push({ text: 'Loading Settings', status: DtoStepStatus.Waiting });
    this.steps.push({ text: 'Loading Document', status: DtoStepStatus.Waiting });
    this.steps.push({ text: 'Awaiting Confirmation', status: DtoStepStatus.Waiting });
    this.steps.push({ text: 'Saving Document', status: DtoStepStatus.Waiting});
    this.steps.push({ text: 'Releasing WorkItem', status: DtoStepStatus.Waiting});
    this.steps.push({ text: 'Acknowledging Notification', status: DtoStepStatus.Waiting});
  }

  private constructQueryParamObjects(params: HttpParams): QueryParams {
    const ret = new QueryParams;

    if (params.has('an')) {
      ret.activityName = params.get('an');
    }

    if (params.has('msg')) {
      ret.message = params.get('msg');
    }

    if (params.has('cv')) {
      ret.approveValue = params.get('cv');
    }

    if (params.has('dv')) {
      ret.denyValue = params.get('dv');
    }

    if (params.has('t')) {
      ret.token = params.get('t');
    }

    if (params.has('rt')) {
      ret.runtimeUrl = params.get('rt');
    }

    if (params.has('nt')) {
      // tslint:disable-next-line:radix
      ret.notificationId = Number.parseInt(params.get('nt'));
    }

    if (params.has('wi')) {
      // tslint:disable-next-line:radix
      ret.workItemID = Number.parseInt(params.get('wi'));
    }

    if (params.has('ut')){
      ret.userTracking = params.get('ut').toLowerCase() == 'true';
    }

    if (ret.notificationId) {
      ret.fromNotification = true;
    }

    return ret;
  }

  public negativeAnswer(e: MouseEvent) {
    e.preventDefault();
    this.decisionSource.next({IsDecided: true, Result: this.appService.params.denyValue});
    this.decisionSource.complete();
  }

  public positiveAnswer(e: MouseEvent) {
    e.preventDefault();
    this.decisionSource.next({IsDecided: true, Result: this.appService.params.approveValue});
    this.decisionSource.complete();
  }
}
