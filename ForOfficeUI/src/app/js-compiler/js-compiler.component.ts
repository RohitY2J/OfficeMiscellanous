import { CommonModule } from '@angular/common';
import { HttpClient, HttpClientModule } from '@angular/common/http';
import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, FormsModule, ReactiveFormsModule, Validators } from '@angular/forms';
import {JSLabels, JsCompilerResponse} from '../../Model/JsCompiler';
import { NotificationComponent } from '../notification/notification.component';
import { NotificationParam } from '../../Model/Notification';
import { LoadingSpinnerComponent } from '../loading-spinner/loading-spinner.component';

@Component({
  selector: 'app-js-compiler',
  standalone: true,
  imports: [FormsModule, HttpClientModule, ReactiveFormsModule, CommonModule, NotificationComponent, LoadingSpinnerComponent],
  templateUrl: './js-compiler.component.html',
  styleUrl: './js-compiler.component.css'
})
export class JsCompilerComponent {
  form: FormGroup;
  showServerInput: boolean = false;
  processedJs: string= "";
  labelOptions = [];
  isLoading: boolean = false;

  dropdownOptions = [
    { key: '30860047', value: 'CRP CVCP' },
    // Add more key-value pairs as needed
  ];

  notificationParam: NotificationParam = {
    isSuccess: false,
    isVisible: false,
    message: ""
  };
  //enableOutputField: boolean = true;

  controlNames: any = {
    client: 'Client',
    file_output_path: 'File Output Path',
    label_name: 'Label Name',
    enableOutputField: false,
    server: 'Server',
    user_name: 'User Name',
    pass: 'Password'
  };

  constructor(private formBuilder: FormBuilder, private http: HttpClient) {
    this.form = this.formBuilder.group({
      client: ['', Validators.required],
      file_output_path: [''],
      label_name: ['', Validators.required],
      server: [''],
      pass: [''],
      user_name: [''],
      enable_output: false
    });
  }

  ngOnInit(){
    this.isLoading = true;
    this.http.get<JSLabels>("https://localhost:7064/GetAllLabels")
    .subscribe(response =>{
      this.labelOptions = response.labels;
      this.isLoading =false;
    },error=>{

      this.isLoading = false;
    } )
  }

  mapFormData(formValue: any)
  {
    return {
      client: formValue.client,
      fileOutputPath: formValue.file_output_path,
      labelName: formValue.label_name,
      server: formValue.server,
      serverUserName: formValue.user_name,
      serverPassword: formValue.pass
    }
  }

  onSubmit() {
    if (this.form.valid) {
      const formData = this.mapFormData(this.form.value);
      //const formData = this.form.value;
      this.isLoading = true;
      this.http.post<JsCompilerResponse>('https://localhost:7064/ExecuteJSCompile', formData)
        .subscribe(response => {
          if(response.isSuccess){
            this.notificationParam.isSuccess = true;
            this.notificationParam.message = "Compiled Successfully";
            this.processedJs = response.processedJS;
            console.log(response.processedJS);
            this.isLoading = false;
          }
          else{
            this.notificationParam.isSuccess = false;
            this.notificationParam.message = response.message;
            console.log(response.message);
            this.isLoading = false;
          }
          this.notificationParam.isVisible = true;
        }, error => {
          console.error('Error sending form data:', error);
        });
    }
    else{
      
    }
  }
}
