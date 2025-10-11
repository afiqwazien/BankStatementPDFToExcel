import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { FormGroup, UntypedFormBuilder, UntypedFormGroup } from '@angular/forms';
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class FileService {

  private uploadForm: UntypedFormGroup;
  // masterUrl:string = "http://localhost:5158/";
  masterUrl:string = "https://bankstatementpdftoexcel.onrender.com/";
  // masterUrl:string = "https://localhost:7194/";

  constructor(private http: HttpClient, private formBuilder: UntypedFormBuilder) { }

  uploadTempFile(data:any):Observable<string>
  {
    console.log(data);
    // this.uploadForm = this.formBuilder.group({
    //   file: ['']
    // });

    // this.uploadForm.get('file')!.setValue(data);
    // const formData = new FormData();
    // formData.append('file', this.uploadForm.get('file')!.value);

    // return this.http.post<string>(this.masterUrl+`api/FileUpload/exportPdfToExcel`, formData);
    const formData = new FormData();
    formData.append('file', data);
    console.log('API call to:', this.masterUrl + 'api/FileUpload/exportPdfToExcel');
    return this.http.post<string>(this.masterUrl + `api/FileUpload/exportPdfToExcel`, formData);
  }

  downloadPdfToExcel(file: File): Observable<Blob> {
    const formData = new FormData();
    formData.append('file', file);

    return this.http.post(
      this.masterUrl + 'api/FileUpload/exportPdfToExcelDownload',
      formData,
      { responseType: 'blob' }
    );
  }

}
