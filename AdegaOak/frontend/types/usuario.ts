export interface UsuarioDto {
  id: number;
  nome: string;
  username: string;
  role: 'admin' | 'funcionario';
  ativo: boolean;
  criadoEm: string;
}

export interface CreateUsuarioRequest {
  nome: string;
  username: string;
  password: string;
  role: 'admin' | 'funcionario';
}

export interface UpdateUsuarioRequest {
  nome?: string;
  password?: string;
  role?: 'admin' | 'funcionario';
  ativo?: boolean;
}

export interface LoginRequest {
  username: string;
  password: string;
}

export interface LoginResponse {
  token: string;
  username: string;
  nome: string;
  role: string;
}
