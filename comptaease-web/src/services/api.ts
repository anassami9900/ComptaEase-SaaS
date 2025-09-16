import axios from 'axios';
import { 
  LoginRequest, 
  RegisterRequest, 
  LoginResponse, 
  Employee, 
  CreateEmployee, 
  Payroll, 
  CreatePayroll, 
  PayrollCalculation,
  Cotisation,
  Company
} from '../types';

const API_BASE_URL = process.env.REACT_APP_API_URL || 'http://localhost:5000/api';

// Create axios instance
const api = axios.create({
  baseURL: API_BASE_URL,
  headers: {
    'Content-Type': 'application/json',
  },
});

// Request interceptor to add auth token
api.interceptors.request.use((config) => {
  const token = localStorage.getItem('token');
  if (token) {
    config.headers.Authorization = `Bearer ${token}`;
  }
  return config;
});

// Response interceptor to handle auth errors
api.interceptors.response.use(
  (response) => response,
  (error) => {
    if (error.response?.status === 401) {
      localStorage.removeItem('token');
      localStorage.removeItem('user');
      localStorage.removeItem('company');
      window.location.href = '/login';
    }
    return Promise.reject(error);
  }
);

// Auth API
export const authAPI = {
  login: async (credentials: LoginRequest): Promise<LoginResponse> => {
    const response = await api.post('/auth/login', credentials);
    return response.data;
  },

  register: async (data: RegisterRequest): Promise<LoginResponse> => {
    const response = await api.post('/auth/register', data);
    return response.data;
  },
};

// Employee API
export const employeeAPI = {
  getAll: async (): Promise<Employee[]> => {
    const response = await api.get('/employee');
    return response.data;
  },

  getById: async (id: number): Promise<Employee> => {
    const response = await api.get(`/employee/${id}`);
    return response.data;
  },

  create: async (employee: CreateEmployee): Promise<Employee> => {
    const response = await api.post('/employee', employee);
    return response.data;
  },

  update: async (id: number, employee: Partial<Employee>): Promise<void> => {
    await api.put(`/employee/${id}`, employee);
  },

  delete: async (id: number): Promise<void> => {
    await api.delete(`/employee/${id}`);
  },
};

// Payroll API
export const payrollAPI = {
  getAll: async (year?: number, month?: number): Promise<Payroll[]> => {
    const params = new URLSearchParams();
    if (year) params.append('year', year.toString());
    if (month) params.append('month', month.toString());
    
    const response = await api.get(`/payroll?${params.toString()}`);
    return response.data;
  },

  getById: async (id: number): Promise<Payroll> => {
    const response = await api.get(`/payroll/${id}`);
    return response.data;
  },

  calculate: async (data: {
    employeeId: number;
    workedDays: number;
    allowances: number;
    otherDeductions: number;
  }): Promise<PayrollCalculation> => {
    const response = await api.post('/payroll/calculate', data);
    return response.data;
  },

  create: async (payroll: CreatePayroll): Promise<Payroll> => {
    const response = await api.post('/payroll', payroll);
    return response.data;
  },

  approve: async (id: number): Promise<void> => {
    await api.post(`/payroll/${id}/approve`);
  },

  generateBulletin: async (id: number, sendEmail = false): Promise<any> => {
    const response = await api.post(`/payroll/${id}/generate-bulletin?sendEmail=${sendEmail}`);
    return response.data;
  },

  getBulletin: async (id: number): Promise<Blob> => {
    const response = await api.get(`/payroll/${id}/bulletin`, {
      responseType: 'blob',
    });
    return response.data;
  },

  sendEmail: async (id: number): Promise<void> => {
    await api.post(`/payroll/${id}/send-email`);
  },
};

// Company API
export const companyAPI = {
  get: async (): Promise<Company> => {
    const response = await api.get('/company');
    return response.data;
  },

  update: async (company: Partial<Company>): Promise<void> => {
    await api.put('/company', company);
  },

  getCotisations: async (): Promise<Cotisation[]> => {
    const response = await api.get('/company/cotisations');
    return response.data;
  },

  createCotisation: async (cotisation: Omit<Cotisation, 'id' | 'isActive'>): Promise<Cotisation> => {
    const response = await api.post('/company/cotisations', cotisation);
    return response.data;
  },

  updateCotisation: async (id: number, cotisation: Partial<Cotisation>): Promise<void> => {
    await api.put(`/company/cotisations/${id}`, cotisation);
  },
};

export default api;