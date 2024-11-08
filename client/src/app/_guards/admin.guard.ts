import { inject } from '@angular/core';
import { CanActivateFn } from '@angular/router';
import { AccountService } from '../_services/account.service';
import { ToastrService } from 'ngx-toastr';

export const adminGuard: CanActivateFn = (route, state) => {
  const accountService = inject(AccountService);
  const toastr = inject(ToastrService);
  
  if (accountService.userRoles().includes('Admin') || accountService.userRoles().includes('Moderator')) {
    return true;
  } else {
    toastr.error('You cannot enter this area!');
    return false;
  }
};
