import { Component, TemplateRef, ViewChild } from '@angular/core';
import { OidcSecurityService } from 'angular-auth-oidc-client';
import { FaIconLibrary, FontAwesomeModule, IconDefinition } from '@fortawesome/angular-fontawesome';
import { faSpinner, faCheckCircle, faTimesCircle, faExclamationCircle  } from '@fortawesome/free-solid-svg-icons';
import { DtoProcessingStatus, DtoStepStatus, IActivitySettings, IDecisionResult, IDtoDocument, IDtoEventDrivenNotification, IDtoStepItem, IDtoWorkItemData, QueryParams } from './app.model';
import { AsyncSubject, BehaviorSubject, firstValueFrom} from 'rxjs';
import { AppService } from './app.service';

import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [CommonModule,  FontAwesomeModule],
  templateUrl: './app.component.html',
  styleUrl: './app.component.scss'
})
export class AppComponent {
  @ViewChild('communicationTemplate', { static: true }) communicationTemplate?: TemplateRef<any>;
  @ViewChild('loginTemplate', { static: true }) loginTemplate?: TemplateRef<any> ;
  @ViewChild('decisionTemplate', { static: true }) decisionTemplate?: TemplateRef<any> ;
  @ViewChild('doneTemplate', { static: true }) doneTemplate?: TemplateRef<any> ;
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
 
  isAuthenticated = false;

  constructor(library: FaIconLibrary, private appService: AppService, private oidcSecurityService: OidcSecurityService ){
    this.opener = window.opener;
    library.addIcons(faExclamationCircle, faSpinner as IconDefinition, faCheckCircle as IconDefinition, faTimesCircle as IconDefinition);
    this.buildSteps();
    this.execProcessing();
  }

  ngOnInit() {
    this.oidcSecurityService.isAuthenticated$.subscribe(
      ({ isAuthenticated }) => {
        this.isAuthenticated = isAuthenticated;

        console.info('authenticated: ', isAuthenticated);
      }
    );

    this.oidcSecurityService
      .checkAuth()
      .subscribe(({ isAuthenticated, userData, accessToken, errorMessage }) => {
        console.log(isAuthenticated);
        console.log(userData);
        console.log(accessToken);
        console.log(errorMessage);
      });
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

  private execProcessing() {
    this.processing = DtoProcessingStatus.Communication;
    this.updateStepStatus(0, DtoStepStatus.Loading);
  
    if (this.extractParams(this.steps[0])) {
      this.updateStepStatus(0, DtoStepStatus.Done);
      this.doLogin()
        .then(result => this.handleLoginSuccess(result))
        .catch(error => this.handleLoginFailure(error));
    } else {
      this.updateStepStatus(0, DtoStepStatus.Error);
    }
  }

  doLogin() : Promise<string>{
    return new Promise<string>((resolve, reject) => {
      if (this.appService.params?.token && this.appService.params?.token.length > 0 ) {
        this.tokenSource.next(this.appService.params.token);
        resolve(this.appService.params.token);
      } else {
        this.oidcSecurityService.checkAuth().subscribe((authResult) => {
          if (authResult.isAuthenticated) {
            resolve(authResult.accessToken);
          } else {
            this.processing = DtoProcessingStatus.Login;
            firstValueFrom(this.loggingInSource).then(isLogged => {
              if(isLogged){
                resolve(this.tokenSource.value)
              } else {
                this.processing = DtoProcessingStatus.Communication;
                reject('Failed to login');
              }
            })
          }
        });
        /*this.appService.initOAuth().then( _ => {
          this.oAuthInitSource.toPromise().then(_=> {
            this.processing = DtoProcessingStatus.Login;
            this.loggingInSource.toPromise().then(_ => {
              this.appService.Login().then(token => {
                resolve(token);
              });
            });
          });
        });*/
      }
    })
  }
  
  private handleLoginSuccess(result: string) {
    this.processing = DtoProcessingStatus.Communication;
    this.updateStepStatus(1, DtoStepStatus.Done);
    this.appService.params!.token = result;
  
    this.appService.LockNotification(this.appService.params!.notificationId)
      .subscribe({
        next: (notification) => this.handleNotificationLockSuccess(notification),
        error: (error) => this.handleNotificationLockFailure(error)
      });
  }
  
  private handleLoginFailure(error: any) {
    this.processing = DtoProcessingStatus.Communication;
    console.error(error);
    this.reportError(this.steps[1], 'Failed to login to the platform. Reason: ' + error);
    this.updateStepStatus(1, DtoStepStatus.Error);
  }
  
  private handleNotificationLockSuccess(notification: IDtoEventDrivenNotification) {
    this.updateStepStatus(2, DtoStepStatus.Done);
    this.updateStepStatus(3, DtoStepStatus.Loading);
  
    this.appService.LockWorkItem(notification, this.appService.params?.userTracking ?? false)
      .subscribe({
        next: workItem => this.handleWorkItemLockSuccess(workItem, notification),
        error: error => this.handleWorkItemLockFailure(error, notification)
      }
    );
  }
  
  private handleNotificationLockFailure(error: any) {
    console.error(error);
    this.reportError(this.steps[2], 'Failed to lock the requested notification (' + this.appService.params!.notificationId + '). Reason: ' + error);
    this.updateStepStatus(2, DtoStepStatus.Error);
  }
  
  private handleWorkItemLockSuccess(workItem: IDtoWorkItemData, notification: IDtoEventDrivenNotification) {
    this.updateStepStatus(3, DtoStepStatus.Done);
    this.updateStepStatus(4, DtoStepStatus.Loading);
  
    this.appService.LoadConfig(workItem)
      .subscribe({
        next: settings => this.handleConfigLoadSuccess(settings, workItem, notification),
        error: error => this.handleConfigLoadFailure(error, workItem)
      }
    );
  }
  
  private handleWorkItemLockFailure(error: any, notification: IDtoEventDrivenNotification) {
    console.error(error);
    this.reportError(this.steps[3], 'Failed to lock the work item (' + notification.WorkItemID + '). Reason: ' + error);
    this.updateStepStatus(3, DtoStepStatus.Error);
  }
  
  private handleConfigLoadSuccess(settings: IActivitySettings, workItem: IDtoWorkItemData, notification: IDtoEventDrivenNotification) {
    this.updateStepStatus(4, DtoStepStatus.Done);
    this.updateStepStatus(5, DtoStepStatus.Loading);
  
    this.appService.params!.message = settings?.Message || this.message;
    this.appService.params!.denyValue = settings?.NegativeValue || this.noTitle;
    this.appService.params!.approveValue = settings?.PositiveValue || this.noTitle;
  
    this.appService.LoadDocument(workItem)
      .subscribe({
        next: doc => this.handleDocumentLoadSuccess(doc, settings, workItem, notification),
        error: error => this.handleDocumentLoadFailure(error, workItem)
      }
    );
  }
  
  private handleConfigLoadFailure(error: any, workItem: IDtoWorkItemData) {
    console.error(error);
    this.reportError(this.steps[4], 'Unable to load the settings for the activity (' + workItem.ActivityName + '). Reason: ' + error);
    this.updateStepStatus(4, DtoStepStatus.Error);
  }
  
  private handleDocumentLoadSuccess(doc: IDtoDocument, settings: IActivitySettings, workItem: IDtoWorkItemData, notification: IDtoEventDrivenNotification) {
    this.updateStepStatus(5, DtoStepStatus.Done);
    this.updateStepStatus(6, DtoStepStatus.Loading);
    this.processing = DtoProcessingStatus.Decision;
  
    this.decisionSource.asObservable()
      .subscribe({
        next: decision => this.handleDecisionSuccess(decision, doc, settings, workItem, notification),
        error: error => this.handleDecisionFailure(error)
      }
    );
  }
  
  private handleDocumentLoadFailure(error: any, workItem: IDtoWorkItemData) {
    console.error(error);
    this.reportError(this.steps[5], 'Unable to load the document (' + workItem.DocumentID + ') for the work item (' + workItem.WorkItemID + '). Reason: ' + error);
    this.updateStepStatus(5, DtoStepStatus.Error);
  }
  
  private handleDecisionSuccess(decision: IDecisionResult, doc: IDtoDocument, settings: IActivitySettings, workItem: IDtoWorkItemData, notification: IDtoEventDrivenNotification) {
    if (decision.IsDecided) {
      this.processing = DtoProcessingStatus.Communication;
      this.updateDecisionInDocument(decision, doc, settings);
      this.updateStepStatus(6, DtoStepStatus.Done);
      this.updateStepStatus(7, DtoStepStatus.Loading);
  
      this.appService.SaveDocument(workItem, doc)
        .subscribe({
          next: wiEtag => this.handleDocumentSaveSuccess(wiEtag, workItem, notification),
          error: error => this.handleDocumentSaveFailure(error, workItem)
        }
      );
    } else {
      this.processing = DtoProcessingStatus.Communication;
      this.reportError(this.steps[6], 'Undecided result');
      this.updateStepStatus(6, DtoStepStatus.Error);
    }
  }
  
  private handleDecisionFailure(error: any) {
    console.error(error);
    this.reportError(this.steps[6], 'Decision failure : ' + error);
    this.updateStepStatus(6, DtoStepStatus.Error);
  }
  
  private updateDecisionInDocument(decision: IDecisionResult, doc: IDtoDocument, settings: IActivitySettings) {
    let found = false;
    if (!doc.CustomValues) {
      doc.CustomValues = [];
    }
    if (!settings) {
      settings = {
        DecisionCustomValue: 'CV_Router',
        Message: this.appService.params?.message ?? '',
        NegativeValue: this.appService.params?.approveValue ?? this.noTitle,
        PositiveValue: this.appService.params?.denyValue ?? this.yesTitle
      };
    } else if (!settings.DecisionCustomValue) {
      settings.DecisionCustomValue = 'CV_Router';
    }
  
    doc.CustomValues.forEach(cv => {
      if (cv.Key.trim().toUpperCase() === settings.DecisionCustomValue.trim().toUpperCase()) {
        found = true;
        cv.Value = decision.Result ?? '';
      }
    });
  
    if (!found) {
      doc.CustomValues.push({ Key: settings.DecisionCustomValue, Value: decision.Result ?? '' });
    }
  }
  
  private handleDocumentSaveSuccess(wiEtag: string, workItem: IDtoWorkItemData, notification: IDtoEventDrivenNotification) {
    if (wiEtag && wiEtag.length > 0) {
      this.updateStepStatus(7, DtoStepStatus.Done);
      this.updateStepStatus(8, DtoStepStatus.Loading);
      workItem.TimeStamp = wiEtag;
  
      this.appService.ReleaseWorkItem(workItem, this.appService.params?.userTracking ?? false)
        .subscribe({
          next: wiSaved => this.handleWorkItemReleaseSuccess(wiSaved, notification),
          error: error => this.handleWorkItemReleaseFailure(error, workItem)
        }
      );
    } else {
      this.reportError(this.steps[7], 'Document was not saved!');
      this.updateStepStatus(7, DtoStepStatus.Error);
    }
  }
  
  private handleDocumentSaveFailure(error: any, workItem: IDtoWorkItemData) {
    console.error(error);
    this.reportError(this.steps[7], 'Unable to save document (' + workItem.DocumentID + '). Reason: ' + error);
    this.updateStepStatus(7, DtoStepStatus.Error);
  }
  
  private handleWorkItemReleaseSuccess(wiSaved: boolean, notification: IDtoEventDrivenNotification) {
    if (wiSaved) {
      this.updateStepStatus(8, DtoStepStatus.Done);
      this.updateStepStatus(9, DtoStepStatus.Loading);
  
      this.appService.AcknowledgeNotification(notification)
        .subscribe({
          next: acknowledged => this.handleNotificationAcknowledgeSuccess(acknowledged),
          error: error => this.handleNotificationAcknowledgeFailure(error, notification)
        }          
      );
    } else {
      this.reportError(this.steps[8], 'Work Item was not released!');
      this.updateStepStatus(8, DtoStepStatus.Error);
    }
  }
  
  private handleWorkItemReleaseFailure(error: any, workItem: IDtoWorkItemData) {
    console.error(error);
    this.reportError(this.steps[8], 'Unable to Release the work item (' + workItem.WorkItemID + '). Reason: ' + error);
    this.updateStepStatus(8, DtoStepStatus.Error);
  }
  
  private handleNotificationAcknowledgeSuccess(acknowledged: boolean) {
    if (acknowledged) {
      this.updateStepStatus(9, DtoStepStatus.Done);
      this.processing = DtoProcessingStatus.Done;
    } else {
      this.reportError(this.steps[9], 'Notification was not acknowledged!');
      this.updateStepStatus(9, DtoStepStatus.Error);
    }
  }
  
  private handleNotificationAcknowledgeFailure(error: any, notification: IDtoEventDrivenNotification) {
    console.error(error);
    this.reportError(this.steps[9], 'Unable to acknowledge the notification (' + notification.ID + '). Reason: ' + error);
    this.updateStepStatus(9, DtoStepStatus.Error);
  }
  
  private updateStepStatus(index: number, status: DtoStepStatus) {
    if (this.steps[index]) {
      this.steps[index].status = status;
    }
  }
  private extractParams(step: IDtoStepItem) {
    try {
      const params = this.appService.extractParams(step)  
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

      const errs = this.appService.validateParams(params);
      if (errs && errs.length > 0) {
        errs.forEach(err => {this.reportError(step, err); });
        return false;
      } else {
        this.appService.Init(params);
        step.status = DtoStepStatus.Done;
        return true;
      }
      
    } catch (error) {
      if (error instanceof Error) {
        this.reportError(step, error.message);
      }
      return false;
    }
  }

  private reportError (step: IDtoStepItem, errDescription: string) {
    if (!step.errors) {
      step.errors = new Array<string>();
    }
    step.errors.push(errDescription);
    step.status = DtoStepStatus.Error;
  }


  login() { 
    this.oidcSecurityService.authorizeWithPopUp().subscribe(loginResponse => {
      if(loginResponse.isAuthenticated){
        this.tokenSource.next(loginResponse.accessToken);
      }
      this.loggingInSource.next(true);
      this.loggingInSource.complete();
      
    })
  }

  closeMe() {
    window.opener = window;
    window.close();
  }

  public negativeAnswer(e: MouseEvent) {
    e.preventDefault();
    this.decisionSource.next({IsDecided: true, Result: this.appService.params!.denyValue});
    this.decisionSource.complete();
  }

  public positiveAnswer(e: MouseEvent) {
    e.preventDefault();
    this.decisionSource.next({IsDecided: true, Result: this.appService.params!.approveValue});
    this.decisionSource.complete();
  }

  getTemplate(processing: DtoProcessingStatus): TemplateRef<any> {
    switch (processing) {
      case DtoProcessingStatus.Communication:
        return this.communicationTemplate!;
      case DtoProcessingStatus.Login:
        return this.loginTemplate!;
      case DtoProcessingStatus.Decision:
        return this.decisionTemplate!;
      case DtoProcessingStatus.Done:
        return this.doneTemplate!;
    }
  }

}
