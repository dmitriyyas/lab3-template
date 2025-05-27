import React from 'react';
import logo from './logo.svg';
import {BrowserRouter, Routes, Route, Navigate} from 'react-router-dom';
import './App.css';
import { FlightsPage } from './components/pages/FlightsPage/FlightsPage';
import { AuthPage } from './components/pages/AuthPage/AuthPage';
import { UserPage } from './components/pages/UserPage/UserPage';
import { RegisterPage } from './components/pages/RegisterPage/RegisterPage';
import { StatsPage } from './components/pages/StatsPage/StatsPage';

function App() {
  return (
    <BrowserRouter>
      <Routes>
        <Route path='/stats' Component={StatsPage} />
        <Route path='/me' Component={UserPage} />
        <Route path='/auth' Component={AuthPage} />
        <Route path='/register' Component={RegisterPage} />
        <Route path='/flights' Component={FlightsPage} />
        <Route path="/" element={<Navigate to='/flights' />} />
      </Routes>
    </BrowserRouter>
  );
}

export default App;
