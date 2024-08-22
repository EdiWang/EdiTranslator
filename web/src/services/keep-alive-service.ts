import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';

@Injectable({
  providedIn: 'root'
})
export class KeepAliveService {

  private keepAliveUrl = '/api/keepalive';

  constructor(private http: HttpClient) { }

  keepSessionAlive() {
    return this.http.get(this.keepAliveUrl, { withCredentials: true });
  }
}
