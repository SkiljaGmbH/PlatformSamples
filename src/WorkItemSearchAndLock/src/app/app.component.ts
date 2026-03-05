
import { Component, OnInit } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { RouterModule } from '@angular/router';
import { FaIconLibrary, FontAwesomeModule, IconDefinition } from '@fortawesome/angular-fontawesome';
import { faCheckCircle, faExclamationCircle, faSpinner, faTimesCircle } from '@fortawesome/free-solid-svg-icons';
import { OidcSecurityService } from 'angular-auth-oidc-client';
import { firstValueFrom } from 'rxjs';
import { DtoProcessingStatus, DtoStepStatus, IDtoStepItem, IDtoWorkItemData, QueryParams } from './app.models';
import { AppService } from './app.service';


@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.css'],
  imports: [
    FormsModule,
    FontAwesomeModule,
    RouterModule
  ]
})
export class AppComponent implements OnInit {
  title = 'WorkItemSearchAndLock';
  steps = new Array<IDtoStepItem>();
  processing = DtoProcessingStatus.Processing;
  queryParams: QueryParams = new QueryParams();

  private iconMap: Record<DtoStepStatus, IconDefinition> = {
    [DtoStepStatus.Waiting]: faExclamationCircle,
    [DtoStepStatus.Loading]: faSpinner,
    [DtoStepStatus.Done]: faCheckCircle,
    [DtoStepStatus.Error]: faTimesCircle
  };

  protected getStepIcon(status: DtoStepStatus): IconDefinition {
    return this.iconMap[status] || faExclamationCircle;
  }


  constructor(
    private appService: AppService,
    private oidcSecurityService: OidcSecurityService,
    private lib: FaIconLibrary
  ) {

    this.lib.addIcons(faExclamationCircle, faSpinner, faCheckCircle, faTimesCircle);
    this.buildDefaultSteps();

  }

  ngOnInit() {
    this.oidcSecurityService.checkAuth().subscribe(({ isAuthenticated, accessToken }) => {
      this.steps[0].status = DtoStepStatus.Done;
      this.queryParams = this.appService.params;

      if (isAuthenticated) {
        this.queryParams.token = accessToken;
        this.steps[1].status = DtoStepStatus.Done;
        this.processing = DtoProcessingStatus.Processing;
      } else {
        this.steps[1].status = DtoStepStatus.Waiting;
        this.processing = DtoProcessingStatus.PreLogIn;
      }
    });
  }


  login() {
    const popupOptions = { width: 600, height: 600 };

    this.oidcSecurityService.authorizeWithPopUp(undefined, popupOptions)
      .subscribe(({ isAuthenticated, accessToken }) => {
        if (isAuthenticated) {
          this.queryParams.token = accessToken;
          this.steps[1].status = DtoStepStatus.Done;
          this.processing = DtoProcessingStatus.Processing;
        }
      });
  }


  private performStep(step: IDtoStepItem, stepFunction: (step: IDtoStepItem) => Promise<void>[], nextFunction: () => void) {
    step.status = DtoStepStatus.Loading;
    step.startTime = new Date();
    let stepPromises: Promise<void>[] = stepFunction(step);

    Promise.allSettled(stepPromises).then(
      _ => {
        //step.durationMs = new Date().valueOf() - step.startTime!.valueOf();
        this.checkSubStepsForStatus(step);
        nextFunction();
      }
    );
  }

  private checkSubStepsForStatus(step: IDtoStepItem) {
    if (step.subSteps!.length == 0 || step.subSteps!.some(x => x.relatedWi) === false) {
      step.status = DtoStepStatus.Done;
    }
    if (step.subSteps!.some(x => x.status != DtoStepStatus.Done) === false) {
      step.status = DtoStepStatus.Done;
    }
    if (step.subSteps!.some(x => x.status == DtoStepStatus.Error)) {
      step.status = DtoStepStatus.Error;
    }
    let durations = step.subSteps!.filter(s => s.durationMs).map(s => s.durationMs!);
    let count = durations.length;
    if (count > 0) {
      step.durationMs = durations.reduce((p, c) => Math.max(p, c), 0);
      step.subStepDurationAvgMs = Math.round(durations.reduce((p, c) => p + c, 0) / count);
      step.subStepDurationStdDevMs = Math.round(Math.sqrt(durations.map(x => Math.pow(x - step.subStepDurationAvgMs!, 2)).reduce((a, b) => a + b) / count));
    }
  }

  private doMultipleWorkItemFunctionsInParallel(
    step: IDtoStepItem, wiFunction: (params: QueryParams) => Promise<IDtoWorkItemData | undefined>,
    stepName: (i: number) => string, errorMsg: string
  ): Promise<void>[] {
    let wiLockPromises: Promise<void>[] = [];
    step.subSteps = [];

    for (let i = 0; i < this.queryParams.requestCount; i++) {
      let subStep: IDtoStepItem = { text: stepName(i), status: DtoStepStatus.Loading, showDetails: false, startTime: new Date() };
      step.subSteps!.push(subStep);

      let wiLockPromise =
        wiFunction(this.queryParams).then(
          (workItem) => {
            subStep.status = DtoStepStatus.Done;
            subStep.durationMs = new Date().valueOf() - subStep.startTime!.valueOf();
            subStep.relatedWi = workItem;
          }, (e) => {
            console.error(e);
            this.reportError(subStep, errorMsg + ' Reason: ' + JSON.stringify(e));
          });

      wiLockPromises.push(wiLockPromise);
    }
    return wiLockPromises;
  }

  private releaseMultipleWorkItemsInParallel(step: IDtoStepItem, workItems: IDtoWorkItemData[]): Promise<void>[] {
    step.status = DtoStepStatus.Loading;
    step.subSteps = [];
    let wiReleasePromises: Promise<void>[] = [];

    workItems.forEach(wi => {
      if (wi) {
        let releaseSubStep: IDtoStepItem = { text: 'Work item release request', status: DtoStepStatus.Loading, showDetails: false, startTime: new Date(), relatedWi: wi };
        step.subSteps!.push(releaseSubStep);

        let wiReleasePromise =
          firstValueFrom(
            this.appService.ReleaseWorkItem(wi, this.appService.params.userTracking)
          )
            .then((wiSaved) => {
              if (wiSaved) {
                releaseSubStep.status = DtoStepStatus.Done;
                releaseSubStep.durationMs = new Date().valueOf() - releaseSubStep.startTime!.valueOf();
              } else {
                this.reportError(releaseSubStep, 'Work Item was not released!');
              }
            }, (e) => {
              console.error(e);
              this.reportError(releaseSubStep, 'Unable to Release work items. Reason: ' + JSON.stringify(e));
            });
        wiReleasePromises.push(wiReleasePromise);
      }
    });
    return wiReleasePromises;
  }

  private reportError(step: IDtoStepItem, errDescription: string) {
    if (!step.errors) {
      step.errors = new Array<string>();
    }
    step.errors.push(errDescription);
    step.status = DtoStepStatus.Error;
    step.durationMs = step.startTime ? new Date().valueOf() - step.startTime.valueOf() : undefined;
  }

  private validateParam(paramName: string, validationFunc: () => string | undefined): IDtoStepItem {
    let subStep: IDtoStepItem = { text: 'Validating param ' + paramName, status: DtoStepStatus.Loading, showDetails: false, errors: [] };
    let err = validationFunc();
    subStep.status = DtoStepStatus.Done;
    if (err) {
      this.reportError(subStep, err);
    }
    return subStep;
  }

  private validateParams(step: IDtoStepItem): Promise<void>[] {
    step.subSteps = [];
    step.subSteps.push(this.validateParam('runtimeUrl', () => !this.queryParams.runtimeUrl ? 'The Runtime url parameter is required' : undefined));
    step.subSteps.push(this.validateParam('activityInstanceID', () => !this.queryParams.activityInstanceID ? 'The activity instance ID parameter is required' : undefined));
    step.subSteps.push(this.validateParam('requestCount', () => !this.queryParams.requestCount ? 'The request count parameter is required' : undefined));
    if (step.subSteps.some(s => s.status != DtoStepStatus.Done) === false) {
      this.appService.Init(this.queryParams);
    }
    return step.subSteps.map(s => Promise.resolve());
  }


  private buildDefaultSteps() {
    this.steps.push({ text: 'Parsing Parameters', status: DtoStepStatus.Waiting, showDetails: false });
    this.steps.push({ text: 'Logging In', status: DtoStepStatus.Waiting, showDetails: false });
  }

  private restoreDefaultSteps() {
    this.steps.length = 2;
  }


  public startSearchAndLock(e: MouseEvent) {
    e.preventDefault();
    this.restoreDefaultSteps();

    let validationStep: IDtoStepItem = { text: 'Validate Parameters', status: DtoStepStatus.Waiting, showDetails: false };
    let lockingStep: IDtoStepItem = { text: 'Locking WorkItems', status: DtoStepStatus.Waiting, showDetails: true };
    let releasingStep: IDtoStepItem = { text: 'Releasing WorkItems', status: DtoStepStatus.Waiting, showDetails: true };

    this.steps.push(validationStep, lockingStep, releasingStep);

    this.performStep(validationStep,
      (s) => {
        return this.validateParams(s);
      },
      () => {
        if (validationStep.status == DtoStepStatus.Done) {
          this.performStep(lockingStep,
            (s) => {
              return this.doMultipleWorkItemFunctionsInParallel(
                s,
                (params) => firstValueFrom(this.appService.SearchAndLockWorkItem(params)),
                (i) => {
                  return 'Work item lock request ' + i;
                },
                "Failed to lock the work items. "
              );
            },
            () => {
              this.performStep(releasingStep, (s) => {
                return this.releaseMultipleWorkItemsInParallel(s, lockingStep.subSteps?.filter(x => x.relatedWi).map(x => x.relatedWi!) ?? []);
              },
                () => { });
            }
          );
        }
      }
    );
  }
}
