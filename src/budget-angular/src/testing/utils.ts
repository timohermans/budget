import { ApplicationRef } from "@angular/core";
import { TestBed } from '@angular/core/testing';

export async function tickHttpResources() {
    await TestBed.inject(ApplicationRef).whenStable();
}