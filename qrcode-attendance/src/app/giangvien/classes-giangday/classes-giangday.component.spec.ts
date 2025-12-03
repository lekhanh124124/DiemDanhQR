import { ComponentFixture, TestBed } from '@angular/core/testing';

import { ClassesGiangdayComponent } from './classes-giangday.component';

describe('ClassesGiangdayComponent', () => {
  let component: ClassesGiangdayComponent;
  let fixture: ComponentFixture<ClassesGiangdayComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ClassesGiangdayComponent]
    })
    .compileComponents();
    
    fixture = TestBed.createComponent(ClassesGiangdayComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
