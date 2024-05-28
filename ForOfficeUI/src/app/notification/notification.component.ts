import { CommonModule } from '@angular/common';
import { Component, Input } from '@angular/core';
import { NotificationParam } from '../../Model/Notification';

@Component({
  selector: 'app-notification',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './notification.component.html',
  styleUrl: './notification.component.css'
})
export class NotificationComponent {
  @Input() notificationParam: NotificationParam = {
    isSuccess: false,
    isVisible: false,
    message: ""
  }

  hide() {
    this.notificationParam.isVisible = false;
  }
}
