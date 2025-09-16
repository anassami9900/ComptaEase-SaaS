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
  Card,
  CardContent,
  Chip,
  MenuItem,
  FormControl,
  InputLabel,
  Select,
  Divider,
  IconButton,
  Accordion,
  AccordionSummary,
  AccordionDetails
} from '@mui/material';
import { DataGrid, GridColDef, GridActionsCellItem } from '@mui/x-data-grid';
import { 
  Add, 
  Calculate, 
  CheckCircle, 
  Receipt, 
  Email,
  ExpandMore,
  Visibility
} from '@mui/icons-material';
import { payrollAPI, employeeAPI } from '../services/api';
import { Payroll, CreatePayroll, Employee, PayrollCalculation } from '../types';
import { useAuth } from '../contexts/AuthContext';

const PayrollPage: React.FC = () => {
  const [payrolls, setPayrolls] = useState<Payroll[]>([]);
  const [employees, setEmployees] = useState<Employee[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');
  const [openDialog, setOpenDialog] = useState(false);
  const [calculation, setCalculation] = useState<PayrollCalculation | null>(null);
  const [calculationLoading, setCalculationLoading] = useState(false);
  const { user } = useAuth();

  const [formData, setFormData] = useState<CreatePayroll>({
    employeeId: 0,
    payrollPeriodYear: new Date().getFullYear(),
    payrollPeriodMonth: new Date().getMonth() + 1,
    workedDays: 30,
    totalAllowances: 0,
    otherDeductions: 0
  });

  const [filters, setFilters] = useState({
    year: new Date().getFullYear(),
    month: new Date().getMonth() + 1
  });

  useEffect(() => {
    fetchData();
  }, [filters]);

  const fetchData = async () => {
    try {
      setLoading(true);
      const [payrollData, employeeData] = await Promise.all([
        payrollAPI.getAll(filters.year, filters.month),
        employeeAPI.getAll()
      ]);
      setPayrolls(payrollData);
      setEmployees(employeeData.filter(emp => emp.isActive));
    } catch (err: any) {
      setError('Erreur lors du chargement des données');
      console.error('Payroll error:', err);
    } finally {
      setLoading(false);
    }
  };

  const handleCalculate = async () => {
    if (formData.employeeId === 0) {
      setError('Veuillez sélectionner un employé');
      return;
    }

    try {
      setCalculationLoading(true);
      const result = await payrollAPI.calculate({
        employeeId: formData.employeeId,
        workedDays: formData.workedDays,
        allowances: formData.totalAllowances || 0,
        otherDeductions: formData.otherDeductions || 0
      });
      setCalculation(result);
    } catch (err: any) {
      setError('Erreur lors du calcul de la paie');
    } finally {
      setCalculationLoading(false);
    }
  };

  const handleCreate = async () => {
    try {
      await payrollAPI.create(formData);
      await fetchData();
      handleCloseDialog();
    } catch (err: any) {
      setError(err.response?.data?.message || 'Erreur lors de la création');
    }
  };

  const handleApprove = async (id: number) => {
    try {
      await payrollAPI.approve(id);
      await fetchData();
    } catch (err: any) {
      setError('Erreur lors de l\'approbation');
    }
  };

  const handleGenerateBulletin = async (id: number, sendEmail = false) => {
    try {
      await payrollAPI.generateBulletin(id, sendEmail);
      await fetchData();
      alert(`Bulletin ${sendEmail ? 'généré et envoyé' : 'généré'} avec succès`);
    } catch (err: any) {
      setError('Erreur lors de la génération du bulletin');
    }
  };

  const handleDownloadBulletin = async (id: number) => {
    try {
      const blob = await payrollAPI.getBulletin(id);
      const url = window.URL.createObjectURL(blob);
      const a = document.createElement('a');
      a.href = url;
      a.download = `bulletin-${id}.pdf`;
      a.click();
      window.URL.revokeObjectURL(url);
    } catch (err: any) {
      setError('Erreur lors du téléchargement du bulletin');
    }
  };

  const handleOpenDialog = () => {
    setOpenDialog(true);
    setCalculation(null);
    setError('');
  };

  const handleCloseDialog = () => {
    setOpenDialog(false);
    setFormData({
      employeeId: 0,
      payrollPeriodYear: new Date().getFullYear(),
      payrollPeriodMonth: new Date().getMonth() + 1,
      workedDays: 30,
      totalAllowances: 0,
      otherDeductions: 0
    });
    setCalculation(null);
    setError('');
  };

  const handleInputChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    const { name, value } = e.target;
    setFormData(prev => ({
      ...prev,
      [name]: ['employeeId', 'payrollPeriodYear', 'payrollPeriodMonth', 'workedDays'].includes(name)
        ? parseInt(value) || 0
        : parseFloat(value) || 0
    }));
  };

  const handleFilterChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    const { name, value } = e.target;
    setFilters(prev => ({
      ...prev,
      [name]: parseInt(value) || 0
    }));
  };

  const columns: GridColDef[] = [
    { field: 'employeeNumber', headerName: 'Matricule', width: 100 },
    { field: 'employeeFullName', headerName: 'Employé', width: 180 },
    { 
      field: 'period', 
      headerName: 'Période', 
      width: 100,
      valueGetter: (params) => `${params.row.payrollPeriodMonth.toString().padStart(2, '0')}/${params.row.payrollPeriodYear}`
    },
    {
      field: 'grossSalary',
      headerName: 'Salaire brut',
      width: 130,
      valueFormatter: (params) => `${params.value.toLocaleString('fr-FR', { minimumFractionDigits: 2 })} MAD`
    },
    {
      field: 'netSalary',
      headerName: 'Salaire net',
      width: 130,
      valueFormatter: (params) => `${params.value.toLocaleString('fr-FR', { minimumFractionDigits: 2 })} MAD`
    },
    {
      field: 'status',
      headerName: 'Statut',
      width: 100,
      renderCell: (params) => (
        <Chip
          label={params.value}
          color={
            params.value === 'Approved' ? 'success' :
            params.value === 'Draft' ? 'default' : 'info'
          }
          size="small"
        />
      )
    },
    {
      field: 'actions',
      type: 'actions',
      headerName: 'Actions',
      width: 200,
      getActions: (params) => [
        <GridActionsCellItem
          icon={<Visibility />}
          label="Voir"
          onClick={() => console.log('View payroll', params.row)}
        />,
        ...(user?.role === 'Admin' || user?.role === 'HR' ? [
          <GridActionsCellItem
            icon={<CheckCircle />}
            label="Approuver"
            onClick={() => handleApprove(params.row.id)}
            disabled={params.row.status !== 'Draft'}
          />,
          <GridActionsCellItem
            icon={<Receipt />}
            label="Bulletin"
            onClick={() => handleGenerateBulletin(params.row.id, false)}
          />,
          <GridActionsCellItem
            icon={<Email />}
            label="Envoyer"
            onClick={() => handleGenerateBulletin(params.row.id, true)}
          />
        ] : [])
      ]
    }
  ];

  const canCreateOrEdit = user?.role === 'Admin' || user?.role === 'HR';

  return (
    <Box>
      <Box display="flex" justifyContent="space-between" alignItems="center" mb={3}>
        <Typography variant="h4">
          Gestion de la paie
        </Typography>
        {canCreateOrEdit && (
          <Button
            variant="contained"
            startIcon={<Add />}
            onClick={handleOpenDialog}
          >
            Calculer une paie
          </Button>
        )}
      </Box>

      {error && <Alert severity="error" sx={{ mb: 2 }}>{error}</Alert>}

      {/* Filters */}
      <Card sx={{ mb: 3 }}>
        <CardContent>
          <Typography variant="h6" gutterBottom>
            Filtres
          </Typography>
          <Grid container spacing={2}>
            <Grid item xs={12} sm={6} md={3}>
              <TextField
                select
                fullWidth
                label="Année"
                name="year"
                value={filters.year}
                onChange={handleFilterChange}
              >
                {[2023, 2024, 2025].map(year => (
                  <MenuItem key={year} value={year}>{year}</MenuItem>
                ))}
              </TextField>
            </Grid>
            <Grid item xs={12} sm={6} md={3}>
              <TextField
                select
                fullWidth
                label="Mois"
                name="month"
                value={filters.month}
                onChange={handleFilterChange}
              >
                {Array.from({ length: 12 }, (_, i) => (
                  <MenuItem key={i + 1} value={i + 1}>
                    {new Date(2024, i).toLocaleDateString('fr-FR', { month: 'long' })}
                  </MenuItem>
                ))}
              </TextField>
            </Grid>
          </Grid>
        </CardContent>
      </Card>

      <Card>
        <DataGrid
          rows={payrolls}
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

      {/* Create Payroll Dialog */}
      <Dialog
        open={openDialog}
        onClose={handleCloseDialog}
        maxWidth="md"
        fullWidth
      >
        <DialogTitle>Calculer et créer une paie</DialogTitle>
        <DialogContent>
          <Grid container spacing={2} sx={{ mt: 1 }}>
            <Grid item xs={12} sm={6}>
              <FormControl fullWidth required>
                <InputLabel>Employé</InputLabel>
                <Select
                  name="employeeId"
                  value={formData.employeeId}
                  label="Employé"
                  onChange={(e) => setFormData(prev => ({ ...prev, employeeId: Number(e.target.value) }))}
                >
                  {employees.map(emp => (
                    <MenuItem key={emp.id} value={emp.id}>
                      {emp.employeeNumber} - {emp.firstName} {emp.lastName}
                    </MenuItem>
                  ))}
                </Select>
              </FormControl>
            </Grid>
            <Grid item xs={12} sm={3}>
              <TextField
                fullWidth
                label="Année"
                name="payrollPeriodYear"
                type="number"
                value={formData.payrollPeriodYear}
                onChange={handleInputChange}
                required
              />
            </Grid>
            <Grid item xs={12} sm={3}>
              <TextField
                fullWidth
                label="Mois"
                name="payrollPeriodMonth"
                type="number"
                inputProps={{ min: 1, max: 12 }}
                value={formData.payrollPeriodMonth}
                onChange={handleInputChange}
                required
              />
            </Grid>
            <Grid item xs={12} sm={4}>
              <TextField
                fullWidth
                label="Jours travaillés"
                name="workedDays"
                type="number"
                inputProps={{ min: 1, max: 31 }}
                value={formData.workedDays}
                onChange={handleInputChange}
                required
              />
            </Grid>
            <Grid item xs={12} sm={4}>
              <TextField
                fullWidth
                label="Primes (MAD)"
                name="totalAllowances"
                type="number"
                inputProps={{ min: 0 }}
                value={formData.totalAllowances}
                onChange={handleInputChange}
              />
            </Grid>
            <Grid item xs={12} sm={4}>
              <TextField
                fullWidth
                label="Autres retenues (MAD)"
                name="otherDeductions"
                type="number"
                inputProps={{ min: 0 }}
                value={formData.otherDeductions}
                onChange={handleInputChange}
              />
            </Grid>
            
            <Grid item xs={12}>
              <Button
                variant="outlined"
                startIcon={<Calculate />}
                onClick={handleCalculate}
                disabled={calculationLoading || formData.employeeId === 0}
                fullWidth
              >
                {calculationLoading ? 'Calcul en cours...' : 'Calculer la paie'}
              </Button>
            </Grid>

            {calculation && (
              <Grid item xs={12}>
                <Accordion>
                  <AccordionSummary expandIcon={<ExpandMore />}>
                    <Typography variant="h6">Résultat du calcul</Typography>
                  </AccordionSummary>
                  <AccordionDetails>
                    <Grid container spacing={2}>
                      <Grid item xs={12} sm={6}>
                        <Card variant="outlined">
                          <CardContent>
                            <Typography variant="h6" color="primary" gutterBottom>
                              Salaire brut
                            </Typography>
                            <Typography variant="h4">
                              {calculation.grossSalary.toLocaleString('fr-FR', { minimumFractionDigits: 2 })} MAD
                            </Typography>
                          </CardContent>
                        </Card>
                      </Grid>
                      <Grid item xs={12} sm={6}>
                        <Card variant="outlined">
                          <CardContent>
                            <Typography variant="h6" color="success.main" gutterBottom>
                              Salaire net
                            </Typography>
                            <Typography variant="h4">
                              {calculation.netSalary.toLocaleString('fr-FR', { minimumFractionDigits: 2 })} MAD
                            </Typography>
                          </CardContent>
                        </Card>
                      </Grid>
                      <Grid item xs={12}>
                        <Divider sx={{ my: 2 }} />
                        <Typography variant="h6" gutterBottom>Détail des retenues</Typography>
                        <Grid container spacing={1}>
                          <Grid item xs={6} sm={3}>
                            <Typography variant="body2">
                              CNSS Employé: {calculation.cnssEmployee.toLocaleString('fr-FR', { minimumFractionDigits: 2 })} MAD
                            </Typography>
                          </Grid>
                          <Grid item xs={6} sm={3}>
                            <Typography variant="body2">
                              AMO Employé: {calculation.amoEmployee.toLocaleString('fr-FR', { minimumFractionDigits: 2 })} MAD
                            </Typography>
                          </Grid>
                          <Grid item xs={6} sm={3}>
                            <Typography variant="body2">
                              IGR: {calculation.igrTax.toLocaleString('fr-FR', { minimumFractionDigits: 2 })} MAD
                            </Typography>
                          </Grid>
                          <Grid item xs={6} sm={3}>
                            <Typography variant="body2" fontWeight="bold">
                              Total: {calculation.totalDeductions.toLocaleString('fr-FR', { minimumFractionDigits: 2 })} MAD
                            </Typography>
                          </Grid>
                        </Grid>
                      </Grid>
                    </Grid>
                  </AccordionDetails>
                </Accordion>
              </Grid>
            )}
          </Grid>
        </DialogContent>
        <DialogActions>
          <Button onClick={handleCloseDialog}>Annuler</Button>
          <Button 
            onClick={handleCreate} 
            variant="contained"
            disabled={!calculation}
          >
            Créer la paie
          </Button>
        </DialogActions>
      </Dialog>
    </Box>
  );
};

export default PayrollPage;