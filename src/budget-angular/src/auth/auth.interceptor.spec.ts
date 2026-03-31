import { HttpClient, provideHttpClient, withInterceptors } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { mock } from '../testing/mock';
import { authInterceptor } from './auth.interceptor';
import { TestBed } from '@angular/core/testing';
import { firstValueFrom } from 'rxjs';
import { FakeAuthService } from './fake-auth.service';

describe('authInterceptor', () => {
  it('should add Authorization header with Bearer token', (done) => {
    const authServiceMock = mock<FakeAuthService>();
    authServiceMock.getAccessToken.mockReturnValue('test-token');

    TestBed.configureTestingModule({
      providers: [
        provideHttpClient(
          withInterceptors([authInterceptor])
        ),
        provideHttpClientTesting(),
        { provide: FakeAuthService, useValue: authServiceMock }
      ]
    });

    const httpTesting = TestBed.inject(HttpTestingController);
    const httpClient = TestBed.inject(HttpClient);

    firstValueFrom(httpClient.get('/api/foo'));

    const call = httpTesting.expectOne('/api/foo', 'Request to load the configuration');

    expect(call.request.method).toBe('GET');
    expect(call.request.headers.has('Authorization')).toBeTruthy();
    expect(call.request.headers.get('Authorization')).toBe('Bearer test-token');
  });
});
