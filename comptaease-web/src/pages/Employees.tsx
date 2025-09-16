import React, { useState, useEffect } from 'react';
import {
  Box,
  Button,
  Typography,
  Alert,
  Dialog,
  DialogTitle,
  DialogContent,
  DialogActions,
  TextField,
  Grid,
  FormControlLabel,
  Switch,
  IconButton,
  Card,
  CardContent,
  Chip
} from '@mui/material';
import { DataGrid, GridColDef, GridActionsCellItem } from '@mui/x-data-grid';
import { Add, Edit, Delete, Visibility } from '@mui/icons-material';
import { DatePicker } from '@mui/x-date-pickers/DatePicker';
import { LocalizationProvider } from '@mui/x-date-pickers/LocalizationProvider';
import { AdapterDayjs } from '@mui/x-date-pickers/AdapterDayjs';
import dayjs, { Dayjs } from 'dayjs';
import 'dayjs/locale/fr';
import { employeeAPI } from '../services/api';
import { Employee, CreateEmployee } from '../types';
import { useAuth } from '../contexts/AuthContext';

dayjs.locale('fr');

const Employees: React.FC = () => {
  const [employees, setEmployees] = useState<Employee[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');
  const [openDialog, setOpenDialog] = useState(false);
  const [selectedEmployee, setSelectedEmployee] = useState<Employee | null>(null);
  const [viewMode, setViewMode] = useState<'view' | 'edit' | 'create'>('view');
  const { user } = useAuth();

  const [formData, setFormData] = useState<CreateEmployee>({
    employeeNumber: '',
    firstName: '',
    lastName: '',
    email: '',
    phone: '',
    address: '',
    dateOfBirth: '',
    hireDate: new Date().toISOString().split('T')[0],
    position: '',
    department: '',
    baseSalary: 0,
    cnssNumber: '',
    cinNumber: ''
  });

  useEffect(() => {
    fetchEmployees();
  }, []);

  const fetchEmployees = async () => {
    try {
      setLoading(true);
      const data = await employeeAPI.getAll();
      setEmployees(data);
    } catch (err: any) {
      setError('Erreur lors du chargement des employés');
      console.error('Employees error:', err);
    } finally {
      setLoading(false);
    }
  };

  const handleOpenDialog = (mode: 'view' | 'edit' | 'create', employee?: Employee) => {
    setViewMode(mode);
    if (employee) {
      setSelectedEmployee(employee);
      setFormData({
        employeeNumber: employee.employeeNumber,
        firstName: employee.firstName,
        lastName: employee.lastName,
        email: employee.email || '',
        phone: employee.phone || '',
        address: employee.address || '',
        dateOfBirth: employee.dateOfBirth || '',
        hireDate: employee.hireDate,
        position: employee.position,
        department: employee.department,
        baseSalary: employee.baseSalary,
        cnssNumber: employee.cnssNumber || '',
        cinNumber: employee.cinNumber || ''
      });
    } else {
      setSelectedEmployee(null);
      setFormData({
        employeeNumber: '',
        firstName: '',
        lastName: '',
        email: '',
        phone: '',
        address: '',
        dateOfBirth: '',
        hireDate: new Date().toISOString().split('T')[0],
        position: '',
        department: '',
        baseSalary: 0,
        cnssNumber: '',
        cinNumber: ''
      });
    }
    setOpenDialog(true);
  };

  const handleCloseDialog = () => {
    setOpenDialog(false);
    setSelectedEmployee(null);
    setError('');
  };

  const handleInputChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    const { name, value } = e.target;
    setFormData(prev => ({
      ...prev,
      [name]: name === 'baseSalary' ? parseFloat(value) || 0 : value
    }));
  };

  const handleDateChange = (field: 'dateOfBirth' | 'hireDate') => (date: Dayjs | null) => {
    setFormData(prev => ({
      ...prev,
      [field]: date ? date.format('YYYY-MM-DD') : ''
    }));
  };

  const handleSubmit = async () => {
    try {
      if (viewMode === 'create') {
        await employeeAPI.create(formData);
      } else if (viewMode === 'edit' && selectedEmployee) {
        await employeeAPI.update(selectedEmployee.id, formData);
      }
      
      await fetchEmployees();
      handleCloseDialog();
    } catch (err: any) {
      setError(err.response?.data?.message || 'Erreur lors de la sauvegarde');
    }
  };

  const handleDelete = async (employee: Employee) => {
    if (window.confirm(`Êtes-vous sûr de vouloir supprimer ${employee.firstName} ${employee.lastName} ?`)) {
      try {
        await employeeAPI.delete(employee.id);
        await fetchEmployees();
      } catch (err: any) {
        setError('Erreur lors de la suppression');
      }
    }
  };

  const columns: GridColDef[] = [
    { field: 'employeeNumber', headerName: 'Matricule', width: 120 },
    { field: 'firstName', headerName: 'Prénom', width: 130 },
    { field: 'lastName', headerName: 'Nom', width: 130 },
    { field: 'position', headerName: 'Poste', width: 150 },
    { field: 'department', headerName: 'Département', width: 130 },
    {
      field: 'baseSalary',
      headerName: 'Salaire de base',
      width: 130,
      valueFormatter: (params) => `${params.value.toLocaleString('fr-FR')} MAD`
    },
    {
      field: 'isActive',
      headerName: 'Statut',
      width: 100,
      renderCell: (params) => (
        <Chip
          label={params.value ? 'Actif' : 'Inactif'}
          color={params.value ? 'success' : 'default'}
          size="small"
        />
      )
    },
    {
      field: 'actions',
      type: 'actions',
      headerName: 'Actions',
      width: 120,
      getActions: (params) => [
        <GridActionsCellItem
          icon={<Visibility />}
          label="Voir"
          onClick={() => handleOpenDialog('view', params.row)}
        />,
        ...(user?.role === 'Admin' || user?.role === 'HR' ? [
          <GridActionsCellItem
            icon={<Edit />}
            label="Modifier"
            onClick={() => handleOpenDialog('edit', params.row)}
          />,
          <GridActionsCellItem
            icon={<Delete />}
            label="Supprimer"
            onClick={() => handleDelete(params.row)}
          />
        ] : [])
      ]
    }
  ];

  const canCreateOrEdit = user?.role === 'Admin' || user?.role === 'HR';

  return (
    <LocalizationProvider dateAdapter={AdapterDayjs} adapterLocale="fr">
      <Box>
        <Box display="flex" justifyContent="space-between" alignItems="center" mb={3}>
          <Typography variant="h4">
            Gestion des employés
          </Typography>
          {canCreateOrEdit && (
            <Button
              variant="contained"
              startIcon={<Add />}
              onClick={() => handleOpenDialog('create')}
            >
              Ajouter un employé
            </Button>
          )}
        </Box>

        {error && <Alert severity="error" sx={{ mb: 2 }}>{error}</Alert>}

        <Card>
          <DataGrid
            rows={employees}
            columns={columns}
            loading={loading}
            pageSizeOptions={[10, 25, 50]}
            initialState={{
              pagination: { paginationModel: { pageSize: 10 } }
            }}
            checkboxSelection={false}
            disableRowSelectionOnClick
            autoHeight
            sx={{ border: 0 }}
          />
        </Card>

        <Dialog
          open={openDialog}
          onClose={handleCloseDialog}
          maxWidth="md"
          fullWidth
        >
          <DialogTitle>
            {viewMode === 'create' ? 'Ajouter un employé' :
             viewMode === 'edit' ? 'Modifier l\'employé' : 'Détails de l\'employé'}
          </DialogTitle>
          <DialogContent>
            <Grid container spacing={2} sx={{ mt: 1 }}>
              <Grid item xs={12} sm={6}>
                <TextField
                  fullWidth
                  label="Matricule"
                  name="employeeNumber"
                  value={formData.employeeNumber}
                  onChange={handleInputChange}
                  disabled={viewMode === 'view'}
                  required
                />
              </Grid>
              <Grid item xs={12} sm={6}>
                <TextField
                  fullWidth
                  label="Prénom"
                  name="firstName"
                  value={formData.firstName}
                  onChange={handleInputChange}
                  disabled={viewMode === 'view'}
                  required
                />
              </Grid>
              <Grid item xs={12} sm={6}>
                <TextField
                  fullWidth
                  label="Nom"
                  name="lastName"
                  value={formData.lastName}
                  onChange={handleInputChange}
                  disabled={viewMode === 'view'}
                  required
                />
              </Grid>
              <Grid item xs={12} sm={6}>
                <TextField
                  fullWidth
                  label="Email"
                  name="email"
                  type="email"
                  value={formData.email}
                  onChange={handleInputChange}
                  disabled={viewMode === 'view'}
                />
              </Grid>
              <Grid item xs={12} sm={6}>
                <TextField
                  fullWidth
                  label="Téléphone"
                  name="phone"
                  value={formData.phone}
                  onChange={handleInputChange}
                  disabled={viewMode === 'view'}
                />
              </Grid>
              <Grid item xs={12} sm={6}>
                <DatePicker
                  label="Date de naissance"
                  value={formData.dateOfBirth ? dayjs(formData.dateOfBirth) : null}
                  onChange={handleDateChange('dateOfBirth')}
                  disabled={viewMode === 'view'}
                  slotProps={{ textField: { fullWidth: true } }}
                />
              </Grid>
              <Grid item xs={12}>
                <TextField
                  fullWidth
                  label="Adresse"
                  name="address"
                  value={formData.address}
                  onChange={handleInputChange}
                  disabled={viewMode === 'view'}
                  multiline
                  rows={2}
                />
              </Grid>
              <Grid item xs={12} sm={6}>
                <DatePicker
                  label="Date d'embauche"
                  value={dayjs(formData.hireDate)}
                  onChange={handleDateChange('hireDate')}
                  disabled={viewMode === 'view'}
                  slotProps={{ textField: { fullWidth: true, required: true } }}
                />
              </Grid>
              <Grid item xs={12} sm={6}>
                <TextField
                  fullWidth
                  label="Poste"
                  name="position"
                  value={formData.position}
                  onChange={handleInputChange}
                  disabled={viewMode === 'view'}
                  required
                />
              </Grid>
              <Grid item xs={12} sm={6}>
                <TextField
                  fullWidth
                  label="Département"
                  name="department"
                  value={formData.department}
                  onChange={handleInputChange}
                  disabled={viewMode === 'view'}
                  required
                />
              </Grid>
              <Grid item xs={12} sm={6}>
                <TextField
                  fullWidth
                  label="Salaire de base (MAD)"
                  name="baseSalary"
                  type="number"
                  value={formData.baseSalary}
                  onChange={handleInputChange}
                  disabled={viewMode === 'view'}
                  required
                />
              </Grid>
              <Grid item xs={12} sm={6}>
                <TextField
                  fullWidth
                  label="Numéro CNSS"
                  name="cnssNumber"
                  value={formData.cnssNumber}
                  onChange={handleInputChange}
                  disabled={viewMode === 'view'}
                />
              </Grid>
              <Grid item xs={12} sm={6}>
                <TextField
                  fullWidth
                  label="Numéro CIN"
                  name="cinNumber"
                  value={formData.cinNumber}
                  onChange={handleInputChange}
                  disabled={viewMode === 'view'}
                />
              </Grid>
            </Grid>
          </DialogContent>
          <DialogActions>
            <Button onClick={handleCloseDialog}>
              {viewMode === 'view' ? 'Fermer' : 'Annuler'}
            </Button>
            {viewMode !== 'view' && canCreateOrEdit && (
              <Button onClick={handleSubmit} variant="contained">
                {viewMode === 'create' ? 'Créer' : 'Modifier'}
              </Button>
            )}
          </DialogActions>
        </Dialog>
      </Box>
    </LocalizationProvider>
  );
};

export default Employees;