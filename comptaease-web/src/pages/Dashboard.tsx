import React, { useState, useEffect } from 'react';
import {
  Grid,
  Card,
  CardContent,
  Typography,
  Box,
  CircularProgress,
  Alert
} from '@mui/material';
import {
  People,
  AccountBalance,
  TrendingUp,
  Business
} from '@mui/icons-material';
import { employeeAPI, payrollAPI } from '../services/api';
import { Employee, Payroll } from '../types';
import { useAuth } from '../contexts/AuthContext';

interface DashboardStats {
  totalEmployees: number;
  activeEmployees: number;
  totalPayrolls: number;
  currentMonthPayrolls: number;
  totalNetSalary: number;
}

const Dashboard: React.FC = () => {
  const [stats, setStats] = useState<DashboardStats | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');
  const { user, company } = useAuth();

  useEffect(() => {
    const fetchDashboardData = async () => {
      try {
        setLoading(true);
        const [employees, payrolls] = await Promise.all([
          employeeAPI.getAll(),
          payrollAPI.getAll(new Date().getFullYear(), new Date().getMonth() + 1)
        ]);

        const activeEmployees = employees.filter(emp => emp.isActive);
        const totalNetSalary = payrolls.reduce((sum, payroll) => sum + payroll.netSalary, 0);

        setStats({
          totalEmployees: employees.length,
          activeEmployees: activeEmployees.length,
          totalPayrolls: payrolls.length,
          currentMonthPayrolls: payrolls.length,
          totalNetSalary
        });
      } catch (err: any) {
        setError('Erreur lors du chargement du tableau de bord');
        console.error('Dashboard error:', err);
      } finally {
        setLoading(false);
      }
    };

    fetchDashboardData();
  }, []);

  if (loading) {
    return (
      <Box display="flex" justifyContent="center" alignItems="center" minHeight="200px">
        <CircularProgress />
      </Box>
    );
  }

  if (error) {
    return <Alert severity="error">{error}</Alert>;
  }

  const StatCard: React.FC<{
    title: string;
    value: string | number;
    icon: React.ReactNode;
    color: string;
  }> = ({ title, value, icon, color }) => (
    <Card>
      <CardContent>
        <Box display="flex" alignItems="center" justifyContent="space-between">
          <Box>
            <Typography color="textSecondary" gutterBottom variant="body2">
              {title}
            </Typography>
            <Typography variant="h4" component="h2">
              {typeof value === 'number' && title.includes('MAD') 
                ? `${value.toLocaleString('fr-FR', { minimumFractionDigits: 2 })} MAD`
                : value}
            </Typography>
          </Box>
          <Box
            sx={{
              backgroundColor: color,
              borderRadius: '50%',
              padding: 2,
              display: 'flex',
              alignItems: 'center',
              justifyContent: 'center'
            }}
          >
            {icon}
          </Box>
        </Box>
      </CardContent>
    </Card>
  );

  return (
    <Box>
      <Typography variant="h4" gutterBottom>
        Tableau de bord
      </Typography>
      
      <Box mb={3}>
        <Typography variant="h6" color="textSecondary">
          Bienvenue, {user?.firstName} {user?.lastName}
        </Typography>
        <Typography variant="body2" color="textSecondary">
          {company?.name} - {user?.role}
        </Typography>
      </Box>

      <Grid container spacing={3}>
        <Grid item xs={12} sm={6} md={3}>
          <StatCard
            title="Employés actifs"
            value={stats?.activeEmployees || 0}
            icon={<People sx={{ color: 'white' }} />}
            color="#1976d2"
          />
        </Grid>
        
        <Grid item xs={12} sm={6} md={3}>
          <StatCard
            title="Total employés"
            value={stats?.totalEmployees || 0}
            icon={<Business sx={{ color: 'white' }} />}
            color="#388e3c"
          />
        </Grid>
        
        <Grid item xs={12} sm={6} md={3}>
          <StatCard
            title="Paies ce mois"
            value={stats?.currentMonthPayrolls || 0}
            icon={<AccountBalance sx={{ color: 'white' }} />}
            color="#f57c00"
          />
        </Grid>
        
        <Grid item xs={12} sm={6} md={3}>
          <StatCard
            title="Salaire net total MAD"
            value={stats?.totalNetSalary || 0}
            icon={<TrendingUp sx={{ color: 'white' }} />}
            color="#7b1fa2"
          />
        </Grid>
      </Grid>

      <Grid container spacing={3} sx={{ mt: 3 }}>
        <Grid item xs={12} md={6}>
          <Card>
            <CardContent>
              <Typography variant="h6" gutterBottom>
                Aperçu rapide
              </Typography>
              <Typography variant="body2" color="textSecondary" paragraph>
                Période: {new Date().toLocaleDateString('fr-FR', { month: 'long', year: 'numeric' })}
              </Typography>
              <Box>
                <Typography variant="body2" gutterBottom>
                  • {stats?.activeEmployees} employés actifs
                </Typography>
                <Typography variant="body2" gutterBottom>
                  • {stats?.currentMonthPayrolls} bulletins de paie générés
                </Typography>
                <Typography variant="body2" gutterBottom>
                  • Masse salariale: {stats?.totalNetSalary.toLocaleString('fr-FR', { minimumFractionDigits: 2 })} MAD
                </Typography>
              </Box>
            </CardContent>
          </Card>
        </Grid>
        
        <Grid item xs={12} md={6}>
          <Card>
            <CardContent>
              <Typography variant="h6" gutterBottom>
                Actions rapides
              </Typography>
              <Typography variant="body2" color="textSecondary" paragraph>
                Accès rapide aux fonctionnalités principales
              </Typography>
              <Box>
                <Typography variant="body2" gutterBottom>
                  • Gérer les employés
                </Typography>
                <Typography variant="body2" gutterBottom>
                  • Calculer la paie
                </Typography>
                <Typography variant="body2" gutterBottom>
                  • Générer les bulletins
                </Typography>
                <Typography variant="body2" gutterBottom>
                  • Configurer les cotisations
                </Typography>
              </Box>
            </CardContent>
          </Card>
        </Grid>
      </Grid>
    </Box>
  );
};

export default Dashboard;