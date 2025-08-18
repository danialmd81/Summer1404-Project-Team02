import {NgModule} from "@angular/core";
import {AuthComponent} from './auth.component';
import {LoginComponent} from '../../features/login/login.component';


@NgModule({
  declarations: [AuthComponent],
  imports: [LoginComponent]
})
export class AuthModule {
}
