import { Component, OnInit } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { MenuItem } from 'primeng/api';
import { ButtonModule } from 'primeng/button';
import { MenubarModule } from 'primeng/menubar';
import { BadgeModule } from 'primeng/badge';
import { CommonModule } from '@angular/common';
import { CardModule } from 'primeng/card';
import { FileComponent } from "./file/file.component";


@Component({
  selector: 'app-root',
  imports: [RouterOutlet, CommonModule, ButtonModule, MenubarModule, BadgeModule, CardModule, FileComponent],
  templateUrl: './app.component.html',
  styleUrl: './app.component.css'
})
export class AppComponent implements OnInit {
  title = 'Angular';
  menuItems:MenuItem[] = [];

  ngOnInit(): void {
    this.menuItems = [
      {
          label: 'Home',
          icon: 'pi pi-home',
          iconClass: 'amber-primary'
      },
      {
        label: 'Features',
        icon: 'pi pi-sliders-h'
      },
      {
        label: 'Support',
        icon: 'pi pi-ticket'
      }
        // {
        //   label: 'Logout',
        //   icon: 'pi pi-sign-out',
        //   command: (event) =>{
        //     this.signOut();
        //   }
        // }  
    ];
  }
}
