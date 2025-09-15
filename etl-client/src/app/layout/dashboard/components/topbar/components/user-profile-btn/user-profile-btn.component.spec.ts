import { ComponentFixture, TestBed } from '@angular/core/testing';

import { UserProfileBtnComponent } from './user-profile-btn.component';

describe('UserProfileBtnComponent', () => {
  let component: UserProfileBtnComponent;
  let fixture: ComponentFixture<UserProfileBtnComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [UserProfileBtnComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(UserProfileBtnComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
