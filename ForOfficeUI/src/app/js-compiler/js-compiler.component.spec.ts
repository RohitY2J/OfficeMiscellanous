import { ComponentFixture, TestBed } from '@angular/core/testing';

import { JsCompilerComponent } from './js-compiler.component';

describe('JsCompilerComponent', () => {
  let component: JsCompilerComponent;
  let fixture: ComponentFixture<JsCompilerComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [JsCompilerComponent]
    })
    .compileComponents();
    
    fixture = TestBed.createComponent(JsCompilerComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
