import {Component} from '@angular/core';
import {PasswordModule} from 'primeng/password';
import {Divider} from 'primeng/divider';
import {FloatLabel} from 'primeng/floatlabel';
import {InputText} from 'primeng/inputtext';
import {Button} from 'primeng/button';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [PasswordModule, Divider, FloatLabel, InputText, Button],
  templateUrl: './login.component.html',
  styleUrl: './login.component.scss'
})
export class LoginComponent {

}
