import { Component, inject, OnInit } from '@angular/core';
import { AdminService } from '../../_services/admin.service';
import { User } from '../../_models/user';
import { BsModalRef, BsModalService, ModalOptions } from 'ngx-bootstrap/modal';
import { RolesModalComponent } from '../../modals/roles-modal/roles-modal.component';
import { ConfirmService } from '../../_services/confirm.service';
import { TitleCasePipe } from '@angular/common';

@Component({
  selector: 'app-user-management',
  standalone: true,
  imports: [],
  templateUrl: './user-management.component.html',
  styleUrl: './user-management.component.css'
})
export class UserManagementComponent implements OnInit {
  private adminService = inject(AdminService);
  private modalService = inject(BsModalService);
  private confirmService = inject(ConfirmService);
  private titleCasePipe = new TitleCasePipe();
  users: User[] = [];
  bsModalRef: BsModalRef<RolesModalComponent> = new BsModalRef<RolesModalComponent>();

  ngOnInit(): void {
    this.getUsersWithRoles();
  }

  openRolesModal(user: User) {
    const initialState: ModalOptions = {
      class: 'modal-lg',
      initialState: {
        title: 'User roles',
        username: user.username,
        selectedRoles: [...user.roles],
        users: this.users,
        availableRoles: ['Admin', 'Moderator', 'Member'],
        rolesUpdated: false
      }
    };

    this.bsModalRef = this.modalService.show(RolesModalComponent, initialState);
    this.bsModalRef.onHide?.subscribe({
      next: () => {
        if (this.bsModalRef.content && this.bsModalRef.content.rolesUpdated) {
          const selectedRoles = this.bsModalRef.content.selectedRoles;
          this.adminService.updateUserRoles(user.username, selectedRoles).subscribe({
            next: roles => user.roles = roles
          });
        }
      }
    });
  }
  
  getUsersWithRoles() {
    this.adminService.getUserWithRoles().subscribe({
      next: users => this.users = users
    });
  }

  deleteUser(username: string) {
    this.confirmService.confirm('Confirmation', 
      'Are you sure you want to delete user - ' + this.titleCasePipe.transform(username) + '?')?.subscribe({
        next: result => {
          if (result === true) {
            // Delete user
            this.adminService.deleteUser(username).subscribe({
              next: () => {
                 this.users = this.users.filter(x => x.username !== username)
              }
            });
          }
        } 
      });

        
      
      
    
    
  }
}
