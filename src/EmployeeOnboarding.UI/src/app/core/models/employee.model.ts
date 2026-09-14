export interface Employee {
  id: number;
  employeeCode: string;
  name: string;
  email: string;
  phone: string;
  department: string;
  designation: string;
  dateOfJoining: string;
  resumeUrl?: string;
  welcomeLetterUrl?: string;
  createdDate: string;
  modifiedDate?: string;
}

export interface CreateEmployeeRequest {
  employeeCode: string;
  name: string;
  email: string;
  phone: string;
  department: string;
  designation: string;
  dateOfJoining: string;
}

export interface UpdateEmployeeRequest {
  name: string;
  email: string;
  phone: string;
  department: string;
  designation: string;
  dateOfJoining: string;
}
