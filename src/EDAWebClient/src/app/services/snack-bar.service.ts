import { Injectable } from '@angular/core';
import { Subject } from 'rxjs';
import {SnackBarData} from '../models/snack-bar.model';

@Injectable()
export class SnackBarService {
  snackBarSubject = new Subject<SnackBarData>();
}
