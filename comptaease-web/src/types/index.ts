export interface User {
  id: string;
  email: string;
  firstName: string;
  lastName: string;
  role: string;
  companyId: number;
}

export interface Company {
  id: number;
  name: string;
  address?: string;
  phone?: string;
  email?: string;
  taxId?: string;
}

export interface LoginRequest {
  email: string;
  password: string;
}

export interface RegisterRequest {
  email: string;
  password: string;
  firstName: string;
  lastName: string;
  companyName: string;
}

export interface LoginResponse {
  token: string;
  user: User;
  company: Company;
}

export interface Employee {
  id: number;
  employeeNumber: string;
  firstName: string;
  lastName: string;
  email?: string;
  phone?: string;
  address?: string;
  dateOfBirth?: string;
  hireDate: string;
  terminationDate?: string;
  position: string;
  department: string;
  baseSalary: number;
  cnssNumber?: string;
  cinNumber?: string;
  isActive: boolean;
}

export interface CreateEmployee {
  employeeNumber: string;
  firstName: string;
  lastName: string;
  email?: string;
  phone?: string;
  address?: string;
  dateOfBirth?: string;
  hireDate: string;
  position: string;
  department: string;
  baseSalary: number;
  cnssNumber?: string;
  cinNumber?: string;
}

export interface Payroll {
  id: number;
  employeeId: number;
  employeeFullName: string;
  employeeNumber: string;
  payrollPeriodYear: number;
  payrollPeriodMonth: number;
  grossSalary: number;
  netSalary: number;
  totalDeductions: number;
  totalAllowances: number;
  cnssEmployee: number;
  cnssEmployer: number;
  amoEmployee: number;
  amoEmployer: number;
  igrTax: number;
  otherDeductions: number;
  workedDays: number;
  paymentDate: string;
  status: string;
  createdAt: string;
}

export interface CreatePayroll {
  employeeId: number;
  payrollPeriodYear: number;
  payrollPeriodMonth: number;
  workedDays: number;
  totalAllowances?: number;
  otherDeductions?: number;
  paymentDate?: string;
}

export interface PayrollCalculation {
  grossSalary: number;
  netSalary: number;
  totalDeductions: number;
  cnssEmployee: number;
  cnssEmployer: number;
  amoEmployee: number;
  amoEmployer: number;
  igrTax: number;
  breakdown: PayrollBreakdown;
}

export interface PayrollBreakdown {
  baseSalary: number;
  allowances: number;
  grossSalary: number;
  deductions: Deduction[];
  totalDeductions: number;
  netSalary: number;
}

export interface Deduction {
  name: string;
  amount: number;
  type: string;
}

export interface Cotisation {
  id: number;
  name: string;
  type: string;
  employeeRate: number;
  employerRate: number;
  maxAmount?: number;
  minAmount?: number;
  isActive: boolean;
}