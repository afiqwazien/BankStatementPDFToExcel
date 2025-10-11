import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FileUploadModule } from 'primeng/fileupload';
import { ToastModule } from 'primeng/toast';
import { ButtonModule } from 'primeng/button';
import { ProgressBarModule } from 'primeng/progressbar';
import { BadgeModule } from 'primeng/badge';
import { PrimeNG } from 'primeng/config';
import { MessageService } from 'primeng/api';
import { FileService } from '../services/file.service';

@Component({
  selector: 'app-file',
  imports: [ FileUploadModule, ToastModule, ButtonModule, ProgressBarModule, BadgeModule, CommonModule],
  templateUrl: './file.component.html',
  styleUrl: './file.component.css'
})
export class FileComponent {
  files = [];
  loading: boolean = false;

  totalSize : number = 0;

  totalSizePercent : number = 0;

  constructor(private config: PrimeNG, private messageService: MessageService, private fileService: FileService) {}

  choose(event: any, callback: any) {
      callback();
  }

  onRemoveTemplatingFile(event: any, file: any, removeFileCallback: any, index: any) {
      removeFileCallback(event, index);
      this.totalSize -= parseInt(this.formatSize(file.size));
      this.totalSizePercent = this.totalSize / 10;
  }

  onClearTemplatingUpload(clear: any) {
      clear();
      this.totalSize = 0;
      this.totalSizePercent = 0;
  }

  onFileUpload(event: any) {
    console.log(event.files[0]);
    console.log(event);
    if (event){
        this.fileService.uploadTempFile(event.files[0]).subscribe((result) => {
            console.log(result);
        })
        this.messageService.add({ severity: 'info', summary: 'Success', detail: 'File Uploaded', life: 3000 });
    }
  }

  onCustomUpload(event: any){
    const file = event.files[0];
    if (!file) return;

    this.loading = true;

    this.fileService.downloadPdfToExcel(file).subscribe({
      next: (blob) => {
        this.loading = false;
        const downloadUrl = window.URL.createObjectURL(blob);
        const timestamp = new Date().toISOString().replace(/[-:.]/g, '_');
        const filename = file.name.replace('.pdf', `_${timestamp}.xlsx`);

        const link = document.createElement('a');
        link.href = downloadUrl;
        link.download = filename;
        link.click();
        window.URL.revokeObjectURL(downloadUrl);

        this.messageService.add({
          severity: 'success',
          summary: 'Success',
          detail: 'Excel file exported successfully!',
          life: 3000
        });
      },
      error: (err) => {
        this.loading = false;
        console.error('Download failed:', err);
        this.messageService.add({
          severity: 'error',
          summary: 'Error',
          detail: 'Failed to export Excel file.',
          life: 4000
        });
      }
    });
  }

  onSelectedFiles(event: any) {
      this.files = event.currentFiles;
      this.files.forEach((file:any) => {
          this.totalSize += parseInt(this.formatSize(file.size));
      });
      this.totalSizePercent = this.totalSize / 10;
  }

  uploadEvent(callback: any) {
      callback();
  }

  formatSize(bytes: any) {
      const k = 1024;
      const dm = 3;
      const sizes = this.config.translation.fileSizeTypes;
      if (bytes === 0) {
          return `0 ${sizes![0]}`;
      }

      const i = Math.floor(Math.log(bytes) / Math.log(k));
      const formattedSize = parseFloat((bytes / Math.pow(k, i)).toFixed(dm));

      return `${formattedSize} ${sizes![i]}`;
  }
}
