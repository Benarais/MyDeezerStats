export interface AuthResponse {
  token?: string;
}

export interface SignInResponse {
  isSuccess?: boolean;
  message?: string; 
}

export interface SignUpResponse {
  success: boolean;
  message: string;
}