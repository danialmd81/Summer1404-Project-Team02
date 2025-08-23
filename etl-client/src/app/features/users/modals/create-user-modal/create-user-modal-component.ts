import {Component} from '@angular/core';
import {Dialog} from 'primeng/dialog';
import {ButtonModule} from 'primeng/button';
import {InputTextModule} from 'primeng/inputtext';

@Component({
  selector: 'app-create-user-modal',
  standalone: true,
  imports: [Dialog, ButtonModule, InputTextModule],
  templateUrl: './create-user-modal-component.html',
  styleUrl: './create-user-modal-component.scss'
})
export class CreateUserModalComponent {
  public visible: boolean = false;
  protected enteredUsername: string = '';
  public enteredPassword: string = '';
  public enteredEmail: string = '';

  private resetFormValuesOnModalClose(): void {
    this.enteredUsername = '';
    this.enteredPassword = '';
    this.enteredEmail = '';
    this.visible = false;
  }

  public enteredUsernameChangeHandler(event: Event) {
    const input = event.target as HTMLInputElement;
    this.enteredUsername = input.value;
  }

  public enteredPasswordChangeHandler(event: Event) {
    const input = event.target as HTMLInputElement;
    this.enteredPassword = input.value;
  }

  public enteredEmailChangeHandler(event: Event) {
    const input = event.target as HTMLInputElement;
    this.enteredEmail = input.value;
  }

  public showDialog() {
    this.visible = true;
  }

  public hideDialog() {
    this.resetFormValuesOnModalClose();
  }

  public submitHandler() {
    const newUser = {
      username: this.enteredUsername,
      password: this.enteredPassword,
      email: this.enteredEmail
    };
    this.resetFormValuesOnModalClose();
  }
}


