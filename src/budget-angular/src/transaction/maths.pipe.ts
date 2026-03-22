import { Pipe, PipeTransform } from '@angular/core';

// TODO: Test if I can remove this pipe!
@Pipe({
  name: 'abs',
})
export class MathAbsPipe implements PipeTransform {
  transform(value: number | null | undefined): number {
    return value ? Math.abs(value) : 0;
  }
}