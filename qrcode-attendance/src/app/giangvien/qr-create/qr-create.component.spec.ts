import { ComponentFixture, TestBed } from '@angular/core/testing';

import { QrCreateComponent } from './qr-create.component';

describe('QrCreateComponent', () => {
  let component: QrCreateComponent;
  let fixture: ComponentFixture<QrCreateComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [QrCreateComponent]
    })
    .compileComponents();
    
    fixture = TestBed.createComponent(QrCreateComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
