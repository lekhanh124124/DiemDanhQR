import { ComponentFixture, TestBed } from '@angular/core/testing';

import { ClassSectionDetailComponent } from './class-section-detail.component';

describe('ClassSectionDetailComponent', () => {
  let component: ClassSectionDetailComponent;
  let fixture: ComponentFixture<ClassSectionDetailComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ClassSectionDetailComponent]
    })
    .compileComponents();
    
    fixture = TestBed.createComponent(ClassSectionDetailComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
