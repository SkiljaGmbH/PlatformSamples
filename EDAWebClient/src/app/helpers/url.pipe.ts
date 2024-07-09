import {PipeTransform, Pipe} from '@angular/core';

@Pipe({ name: 'urlCustomPipe' })
export class UrlPipe implements PipeTransform {
  transform(text: string): string {
    const regExp = new RegExp('((https?:\\/\\/)(www\\.)?|(https?:\\/\\/)?(www\\.))((([a-z\\d]([a-z\\d-]*[a-z\\d])*)\\.?)+' +
      '[a-z]{2,}|((\\d{1,3}\\.){3}\\d{1,3}))(\\:\\d+)?(\\/[-\\w%_.~+]*)*(\\?[;&\\w%_.\\[\\]~+=-]*)?(\\#[-\\w_\\[\\]=&]*)?', 'i');

    return text.replace(regExp, (match) => `<a href="${match}" target="_blank">${match}</a>`);
  }
}
